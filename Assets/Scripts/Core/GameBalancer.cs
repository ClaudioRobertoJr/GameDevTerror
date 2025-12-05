using UnityEngine;
using Unity.Netcode;

namespace SurvivalHorror.Core
{
    /// <summary>
    /// Sistema de balanceamento dinâmico Solo vs Multiplayer
    /// Ajusta dificuldade, recursos e mecânicas baseado no número de jogadores
    /// </summary>
    public class GameBalancer : NetworkBehaviour
    {
        public static GameBalancer Instance { get; private set; }

        [Header("Multiplayer Scaling")]
        [SerializeField, Range(0f, 1f)] private float enemyCountPerPlayer = 0.3f;
        [SerializeField, Range(0f, 1f)] private float enemyHealthPerPlayer = 0.15f;
        [SerializeField, Range(0f, 1f)] private float resourceBonusPerPlayer = 0.2f;

        [Header("Solo Bonuses")]
        [SerializeField, Range(0f, 1f)] private float soloEnemyReduction = 0.5f;
        [SerializeField, Range(0f, 1f)] private float soloEnemyHealthReduction = 0.25f;
        [SerializeField, Range(0f, 1f)] private float soloResourceBonus = 0.3f;
        [SerializeField, Range(0f, 1f)] private float soloSurvivalDrainReduction = 0.2f;
        [SerializeField, Range(0f, 1f)] private float soloCraftingCostReduction = 0.2f;

        // Estado
        private NetworkVariable<int> playerCount = new NetworkVariable<int>(
            1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<bool> isSoloMode = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Multipliers calculados
        private float currentEnemyCountMultiplier = 1f;
        private float currentEnemyHealthMultiplier = 1f;
        private float currentResourceMultiplier = 1f;
        private float currentSurvivalDrainMultiplier = 1f;
        private float currentCraftingCostMultiplier = 1f;

        // Properties
        public int PlayerCount => playerCount.Value;
        public bool IsSoloMode => isSoloMode.Value;

        public float EnemyCountMultiplier => currentEnemyCountMultiplier;
        public float EnemyHealthMultiplier => currentEnemyHealthMultiplier;
        public float ResourceMultiplier => currentResourceMultiplier;
        public float SurvivalDrainMultiplier => currentSurvivalDrainMultiplier;
        public float CraftingCostMultiplier => currentCraftingCostMultiplier;

        // Events
        public event System.Action<int> OnPlayerCountChanged;
        public event System.Action<bool> OnSoloModeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                UpdatePlayerCount();
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }

            // Subscribe em mudanças
            playerCount.OnValueChanged += (oldVal, newVal) =>
            {
                UpdateMultipliers();
                OnPlayerCountChanged?.Invoke(newVal);
            };

            isSoloMode.OnValueChanged += (oldVal, newVal) =>
            {
                OnSoloModeChanged?.Invoke(newVal);
            };

            UpdateMultipliers();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        #region Player Count Tracking

        private void OnClientConnected(ulong clientId)
        {
            UpdatePlayerCount();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            UpdatePlayerCount();
        }

        private void UpdatePlayerCount()
        {
            if (!IsServer) return;

            int count = NetworkManager.Singleton.ConnectedClientsList.Count;
            playerCount.Value = Mathf.Max(1, count);
            isSoloMode.Value = count == 1;

            Debug.Log($"[GameBalancer] Player count updated: {count} (Solo: {isSoloMode.Value})");
        }

        #endregion

        #region Multiplier Calculation

        private void UpdateMultipliers()
        {
            int players = playerCount.Value;

            if (isSoloMode.Value)
            {
                // Solo mode bonuses
                currentEnemyCountMultiplier = 1f - soloEnemyReduction;
                currentEnemyHealthMultiplier = 1f - soloEnemyHealthReduction;
                currentResourceMultiplier = 1f + soloResourceBonus;
                currentSurvivalDrainMultiplier = 1f - soloSurvivalDrainReduction;
                currentCraftingCostMultiplier = 1f - soloCraftingCostReduction;

                Debug.Log($"[GameBalancer] Solo mode active - Enemies: x{currentEnemyCountMultiplier:F2}, Resources: x{currentResourceMultiplier:F2}");
            }
            else
            {
                // Multiplayer scaling
                currentEnemyCountMultiplier = 1f + (players - 1) * enemyCountPerPlayer;
                currentEnemyHealthMultiplier = 1f + (players - 1) * enemyHealthPerPlayer;
                currentResourceMultiplier = 1f + (players - 1) * resourceBonusPerPlayer;
                currentSurvivalDrainMultiplier = 1f; // Normal em MP
                currentCraftingCostMultiplier = 1f; // Normal em MP

                Debug.Log($"[GameBalancer] Multiplayer ({players} players) - Enemies: x{currentEnemyCountMultiplier:F2}, HP: x{currentEnemyHealthMultiplier:F2}");
            }
        }

        #endregion

        #region Public Helper Methods

        /// <summary>
        /// Calcula quantidade de inimigos baseado no número de jogadores
        /// </summary>
        public int GetScaledEnemyCount(int baseCount)
        {
            return Mathf.RoundToInt(baseCount * currentEnemyCountMultiplier);
        }

        /// <summary>
        /// Calcula HP de inimigo baseado no número de jogadores
        /// </summary>
        public float GetScaledEnemyHealth(float baseHealth)
        {
            return baseHealth * currentEnemyHealthMultiplier;
        }

        /// <summary>
        /// Calcula quantidade de recursos dropados
        /// </summary>
        public int GetScaledResourceDrop(int baseAmount)
        {
            return Mathf.RoundToInt(baseAmount * currentResourceMultiplier);
        }

        /// <summary>
        /// Calcula custo de crafting
        /// </summary>
        public int GetScaledCraftingCost(int baseCost)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseCost * currentCraftingCostMultiplier));
        }

        /// <summary>
        /// Calcula taxa de drenagem de sobrevivência
        /// </summary>
        public float GetScaledSurvivalDrain(float baseDrain)
        {
            return baseDrain * currentSurvivalDrainMultiplier;
        }

        /// <summary>
        /// Verifica se está em modo solo
        /// </summary>
        public bool IsSolo()
        {
            return isSoloMode.Value;
        }

        /// <summary>
        /// Retorna descrição dos modificadores ativos
        /// </summary>
        public string GetBalanceDescription()
        {
            if (IsSoloMode)
            {
                return $"SOLO MODE ACTIVE:\n" +
                       $"• Enemies: -{soloEnemyReduction:P0}\n" +
                       $"• Enemy HP: -{soloEnemyHealthReduction:P0}\n" +
                       $"• Resources: +{soloResourceBonus:P0}\n" +
                       $"• Survival Drain: -{soloSurvivalDrainReduction:P0}\n" +
                       $"• Crafting Cost: -{soloCraftingCostReduction:P0}";
            }
            else
            {
                return $"MULTIPLAYER ({PlayerCount} players):\n" +
                       $"• Enemies: x{currentEnemyCountMultiplier:F2}\n" +
                       $"• Enemy HP: x{currentEnemyHealthMultiplier:F2}\n" +
                       $"• Resources: x{currentResourceMultiplier:F2}";
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!Debug.isDebugBuild) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = IsSoloMode ? Color.yellow : Color.cyan;

            int y = 250;
            int lineHeight = 20;

            GUI.Label(new Rect(10, y, 400, lineHeight), $"👥 Players: {PlayerCount} {(IsSoloMode ? "(SOLO)" : "")}", style);
            y += lineHeight;

            style.fontSize = 10;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(10, y, 400, lineHeight), $"Enemies: x{EnemyCountMultiplier:F2} HP: x{EnemyHealthMultiplier:F2}", style);
            y += lineHeight;
            GUI.Label(new Rect(10, y, 400, lineHeight), $"Resources: x{ResourceMultiplier:F2} Crafting: x{CraftingCostMultiplier:F2}", style);
            y += lineHeight;
            GUI.Label(new Rect(10, y, 400, lineHeight), $"Survival Drain: x{SurvivalDrainMultiplier:F2}", style);
        }

        #endregion
    }
}
