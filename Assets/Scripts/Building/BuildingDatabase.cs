using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SurvivalHorror.Building
{
    /// <summary>
    /// Banco de dados central de todas as estruturas construtíveis
    /// Singleton que carrega ScriptableObjects de estruturas
    /// </summary>
    public class BuildingDatabase : MonoBehaviour
    {
        public static BuildingDatabase Instance { get; private set; }

        [Header("Estruturas Disponíveis")]
        [SerializeField] private BuildableStructure[] allStructures;

        private Dictionary<int, BuildableStructure> structureByID;
        private Dictionary<StructureCategory, List<BuildableStructure>> structuresByCategory;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            structureByID = new Dictionary<int, BuildableStructure>();
            structuresByCategory = new Dictionary<StructureCategory, List<BuildableStructure>>();

            // Inicializar categorias
            foreach (StructureCategory category in System.Enum.GetValues(typeof(StructureCategory)))
            {
                structuresByCategory[category] = new List<BuildableStructure>();
            }

            // Validar e indexar estruturas
            foreach (var structure in allStructures)
            {
                if (structure == null)
                {
                    Debug.LogWarning("[BuildingDB] Null structure in database!");
                    continue;
                }

                if (!structure.IsValid())
                {
                    Debug.LogWarning($"[BuildingDB] Invalid structure: {structure.name}");
                    continue;
                }

                // Adicionar ao dicionário por ID
                if (structureByID.ContainsKey(structure.StructureID))
                {
                    Debug.LogError($"[BuildingDB] Duplicate ID {structure.StructureID} for {structure.StructureName}!");
                    continue;
                }

                structureByID[structure.StructureID] = structure;

                // Adicionar à categoria
                structuresByCategory[structure.Category].Add(structure);
            }

            Debug.Log($"[BuildingDB] Loaded {structureByID.Count} structures across {structuresByCategory.Count} categories");
        }

        #region Public Methods

        /// <summary>
        /// Retorna estrutura por ID
        /// </summary>
        public BuildableStructure GetStructureByID(int id)
        {
            return structureByID.ContainsKey(id) ? structureByID[id] : null;
        }

        /// <summary>
        /// Retorna todas estruturas de uma categoria
        /// </summary>
        public List<BuildableStructure> GetStructuresByCategory(StructureCategory category)
        {
            return structuresByCategory.ContainsKey(category)
                ? structuresByCategory[category]
                : new List<BuildableStructure>();
        }

        /// <summary>
        /// Retorna todas as estruturas
        /// </summary>
        public BuildableStructure[] GetAllStructures()
        {
            return structureByID.Values.ToArray();
        }

        /// <summary>
        /// Busca estrutura por nome
        /// </summary>
        public BuildableStructure GetStructureByName(string name)
        {
            return structureByID.Values.FirstOrDefault(s => s.StructureName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}
