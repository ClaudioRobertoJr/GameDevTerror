using UnityEngine;
using HorrorGame.Core;
using HorrorGame.UI;

namespace HorrorGame.Player
{
    /// <summary>
    /// Sistema de interação do player com objetos do mundo
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Raycast Settings")]
        [SerializeField] private Transform raycastOrigin;
        [SerializeField] private bool drawDebugRay = true;

        [Header("UI")]
        [SerializeField] private PlayerHUD playerHUD;

        private IInteractable currentInteractable;
        private Camera playerCamera;

        private void Awake()
        {
            if (raycastOrigin == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                raycastOrigin = playerCamera.transform;
            }

            if (playerHUD == null)
            {
                playerHUD = FindObjectOfType<PlayerHUD>();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
                return;

            CheckForInteractable();
            HandleInteraction();
        }

        private void CheckForInteractable()
        {
            Ray ray = new Ray(raycastOrigin.position, raycastOrigin.forward);
            RaycastHit hit;

            if (drawDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.yellow);
            }

            if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    // Novo objeto interagível
                    if (currentInteractable != interactable)
                    {
                        currentInteractable = interactable;
                        ShowInteractionPrompt(interactable.GetInteractionPrompt());
                    }
                }
                else
                {
                    ClearCurrentInteractable();
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }

        private void HandleInteraction()
        {
            if (Input.GetKeyDown(interactKey) && currentInteractable != null)
            {
                if (currentInteractable.CanInteract())
                {
                    currentInteractable.Interact(gameObject);
                    GameEvents.OnPlayerInteract?.Invoke(((MonoBehaviour)currentInteractable).gameObject);

                    Debug.Log($"[PlayerInteraction] Interacted with {((MonoBehaviour)currentInteractable).name}");
                }
            }
        }

        private void ShowInteractionPrompt(string prompt)
        {
            if (playerHUD != null)
            {
                playerHUD.ShowInteractionPrompt(prompt);
            }
        }

        private void ClearCurrentInteractable()
        {
            if (currentInteractable != null)
            {
                currentInteractable = null;

                if (playerHUD != null)
                {
                    playerHUD.HideInteractionPrompt();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (raycastOrigin != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * interactionRange);
            }
        }
    }

    /// <summary>
    /// Interface para objetos interagíveis
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Texto mostrado quando o player pode interagir
        /// </summary>
        string GetInteractionPrompt();

        /// <summary>
        /// Verifica se pode interagir neste momento
        /// </summary>
        bool CanInteract();

        /// <summary>
        /// Executa a interação
        /// </summary>
        void Interact(GameObject player);
    }
}
