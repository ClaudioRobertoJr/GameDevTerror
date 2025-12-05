using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace SurvivalHorror.Building
{
    /// <summary>
    /// Sistema de construção de base
    /// Permite jogadores construírem estruturas defensivas e utilitárias
    /// Sincronizado em multiplayer
    /// </summary>
    public class BuildingSystem : NetworkBehaviour
    {
        [Header("Configuração")]
        [SerializeField] private LayerMask placementLayer;
        [SerializeField] private float maxPlacementDistance = 5f;
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;

        [Header("Referências")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform previewHolder;

        // Estado atual
        private BuildableStructure currentStructure;
        private GameObject currentPreview;
        private bool isInBuildMode;
        private bool canPlace;

        // Sistema de inventário de recursos (simplificado)
        private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

        // Lista de estruturas construídas
        private List<PlacedStructure> placedStructures = new List<PlacedStructure>();

        // Events
        public event System.Action OnBuildModeEntered;
        public event System.Action OnBuildModeExited;
        public event System.Action<BuildableStructure> OnStructurePlaced;

        private void Awake()
        {
            InitializeResources();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwner)
            {
                enabled = false; // Só o owner constrói
                return;
            }

            if (playerCamera == null)
                playerCamera = Camera.main;

            if (previewHolder == null)
            {
                previewHolder = new GameObject("PreviewHolder").transform;
                previewHolder.SetParent(transform);
            }
        }

        private void Update()
        {
            if (!IsOwner || !isInBuildMode) return;

            UpdatePreviewPosition();
            HandleInput();
        }

        #region Build Mode

        /// <summary>
        /// Entra no modo de construção com uma estrutura
        /// </summary>
        public void EnterBuildMode(BuildableStructure structure)
        {
            if (isInBuildMode) ExitBuildMode();

            currentStructure = structure;
            isInBuildMode = true;

            // Criar preview
            if (structure.PreviewPrefab != null)
            {
                currentPreview = Instantiate(structure.PreviewPrefab, previewHolder);
                currentPreview.layer = LayerMask.NameToLayer("Ignore Raycast");
                DisablePreviewColliders(currentPreview);
            }

            OnBuildModeEntered?.Invoke();
            Debug.Log($"[Building] Entered build mode: {structure.StructureName}");
        }

        /// <summary>
        /// Sai do modo de construção
        /// </summary>
        public void ExitBuildMode()
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }

            currentStructure = null;
            isInBuildMode = false;
            canPlace = false;

            OnBuildModeExited?.Invoke();
            Debug.Log("[Building] Exited build mode");
        }

        #endregion

        #region Preview & Placement

        private void UpdatePreviewPosition()
        {
            if (currentPreview == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, placementLayer))
            {
                // Calcular posição no grid
                Vector3 targetPosition = hit.point;
                if (currentStructure.SnapToGrid)
                {
                    targetPosition = SnapToGrid(targetPosition);
                }

                currentPreview.transform.position = targetPosition;

                // Calcular rotação alinhada à superfície
                if (currentStructure.AlignToSurface)
                {
                    Vector3 forward = Vector3.ProjectOnPlane(playerCamera.transform.forward, hit.normal).normalized;
                    currentPreview.transform.rotation = Quaternion.LookRotation(forward, hit.normal);
                }
                else
                {
                    currentPreview.transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
                }

                // Verificar se pode colocar
                canPlace = CanPlaceStructure(targetPosition, currentPreview.transform.rotation);

                // Atualizar material do preview
                UpdatePreviewMaterial(canPlace);

                currentPreview.SetActive(true);
            }
            else
            {
                currentPreview.SetActive(false);
                canPlace = false;
            }
        }

        private bool CanPlaceStructure(Vector3 position, Quaternion rotation)
        {
            // Verificar recursos
            if (!HasResources(currentStructure.Cost))
                return false;

            // Verificar colisão
            Collider[] colliders = Physics.OverlapBox(
                position + Vector3.up * 0.5f,
                currentStructure.Size * 0.5f,
                rotation,
                ~placementLayer
            );

            if (colliders.Length > 0)
                return false;

            // Verificar distância de outras estruturas do mesmo tipo
            foreach (var placed in placedStructures)
            {
                if (placed.Structure == currentStructure)
                {
                    float distance = Vector3.Distance(position, placed.Position);
                    if (distance < currentStructure.MinDistanceBetween)
                        return false;
                }
            }

            return true;
        }

        private void UpdatePreviewMaterial(bool valid)
        {
            if (currentPreview == null) return;

            Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();
            Material mat = valid ? validPlacementMaterial : invalidPlacementMaterial;

            foreach (var renderer in renderers)
            {
                renderer.material = mat;
            }
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // Confirmar construção (E ou Mouse1)
            if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                if (canPlace)
                {
                    PlaceStructure();
                }
            }

            // Cancelar (ESC ou Mouse2)
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                ExitBuildMode();
            }

            // Rotacionar (R)
            if (Input.GetKeyDown(KeyCode.R) && currentPreview != null)
            {
                currentPreview.transform.Rotate(Vector3.up, 90f);
            }
        }

        #endregion

        #region Structure Placement

        private void PlaceStructure()
        {
            if (!canPlace || currentPreview == null) return;

            Vector3 position = currentPreview.transform.position;
            Quaternion rotation = currentPreview.transform.rotation;

            // Consumir recursos
            ConsumeResources(currentStructure.Cost);

            // Spawnar estrutura na rede
            SpawnStructureServerRpc(
                currentStructure.StructureID,
                position,
                rotation
            );

            OnStructurePlaced?.Invoke(currentStructure);
            ExitBuildMode();
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnStructureServerRpc(int structureID, Vector3 position, Quaternion rotation)
        {
            BuildableStructure structure = BuildingDatabase.Instance.GetStructureByID(structureID);
            if (structure == null)
            {
                Debug.LogError($"[Building] Structure ID {structureID} not found!");
                return;
            }

            // Spawnar o prefab real
            GameObject structureObj = Instantiate(structure.Prefab, position, rotation);

            NetworkObject netObj = structureObj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }

            // Registrar estrutura
            PlacedStructure placed = new PlacedStructure
            {
                Structure = structure,
                GameObject = structureObj,
                Position = position,
                Rotation = rotation,
                PlacedTime = Time.time
            };

            placedStructures.Add(placed);

            Debug.Log($"[Building] Placed {structure.StructureName} at {position}");
        }

        #endregion

        #region Resource Management

        private void InitializeResources()
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                resources[type] = 0;
            }

            // Debug: Dar recursos iniciais
            AddResource(ResourceType.Wood, 100);
            AddResource(ResourceType.Stone, 50);
            AddResource(ResourceType.Fiber, 30);
        }

        public void AddResource(ResourceType type, int amount)
        {
            if (resources.ContainsKey(type))
            {
                resources[type] += amount;
                Debug.Log($"[Building] +{amount} {type}. Total: {resources[type]}");
            }
        }

        public bool HasResources(ResourceCost[] costs)
        {
            foreach (var cost in costs)
            {
                if (!resources.ContainsKey(cost.Type) || resources[cost.Type] < cost.Amount)
                    return false;
            }
            return true;
        }

        private void ConsumeResources(ResourceCost[] costs)
        {
            foreach (var cost in costs)
            {
                if (resources.ContainsKey(cost.Type))
                {
                    resources[cost.Type] -= cost.Amount;
                    Debug.Log($"[Building] -{cost.Amount} {cost.Type}. Remaining: {resources[cost.Type]}");
                }
            }
        }

        public int GetResourceAmount(ResourceType type)
        {
            return resources.ContainsKey(type) ? resources[type] : 0;
        }

        #endregion

        #region Utilities

        private Vector3 SnapToGrid(Vector3 position)
        {
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize
            );
        }

        private void DisablePreviewColliders(GameObject obj)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }
        }

        #endregion

        #region Public Properties

        public bool IsInBuildMode => isInBuildMode;
        public BuildableStructure CurrentStructure => currentStructure;

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class PlacedStructure
    {
        public BuildableStructure Structure;
        public GameObject GameObject;
        public Vector3 Position;
        public Quaternion Rotation;
        public float PlacedTime;
    }

    public enum ResourceType
    {
        Wood,
        Stone,
        Fiber,
        Metal,
        Oil,
        Medicine,
        Battery
    }

    [System.Serializable]
    public class ResourceCost
    {
        public ResourceType Type;
        public int Amount;
    }

    #endregion
}
