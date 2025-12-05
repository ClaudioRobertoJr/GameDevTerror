using UnityEngine;

namespace SurvivalHorror.Building
{
    /// <summary>
    /// ScriptableObject que define uma estrutura construtível
    /// </summary>
    [CreateAssetMenu(fileName = "New Structure", menuName = "Survival Horror/Building/Structure")]
    public class BuildableStructure : ScriptableObject
    {
        [Header("Identificação")]
        [SerializeField] private int structureID;
        [SerializeField] private string structureName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        [Header("Categoria")]
        [SerializeField] private StructureCategory category;

        [Header("Prefabs")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private GameObject previewPrefab;

        [Header("Placement")]
        [SerializeField] private Vector3 size = Vector3.one;
        [SerializeField] private bool snapToGrid = true;
        [SerializeField] private bool alignToSurface = false;
        [SerializeField] private float minDistanceBetween = 2f;

        [Header("Custo de Construção")]
        [SerializeField] private ResourceCost[] cost;

        [Header("Stats (se aplicável)")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool isDestructible = true;

        // Properties públicas
        public int StructureID => structureID;
        public string StructureName => structureName;
        public string Description => description;
        public Sprite Icon => icon;
        public StructureCategory Category => category;
        public GameObject Prefab => prefab;
        public GameObject PreviewPrefab => previewPrefab;
        public Vector3 Size => size;
        public bool SnapToGrid => snapToGrid;
        public bool AlignToSurface => alignToSurface;
        public float MinDistanceBetween => minDistanceBetween;
        public ResourceCost[] Cost => cost;
        public float MaxHealth => maxHealth;
        public bool IsDestructible => isDestructible;

        /// <summary>
        /// Valida se tem todas as informações necessárias
        /// </summary>
        public bool IsValid()
        {
            return structureID >= 0
                && !string.IsNullOrEmpty(structureName)
                && prefab != null
                && cost != null
                && cost.Length > 0;
        }
    }

    public enum StructureCategory
    {
        Defense,      // Paredes, portas, torres
        Utility,      // Fogueira, baú, bancada
        Agriculture,  // Plantação, armadilha de pesca
        Light         // Tochas, lanternas
    }
}
