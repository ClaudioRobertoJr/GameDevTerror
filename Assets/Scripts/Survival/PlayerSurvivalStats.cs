using UnityEngine;
using Unity.Netcode;

namespace SurvivalHorror.Survival
{
    /// <summary>
    /// Sistema completo de sobrevivência do jogador
    /// Gerencia: Fome, Sede, Temperatura, Sanidade
    /// Compatível com multiplayer (Netcode)
    /// </summary>
    public class PlayerSurvivalStats : NetworkBehaviour
    {
        [Header("Stats Máximos")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float maxTemperature = 100f;
        [SerializeField] private float maxSanity = 100f;

        [Header("Taxa de Drenagem (por segundo)")]
        [SerializeField] private float hungerDrainRate = 0.5f;
        [SerializeField] private float thirstDrainRate = 0.8f;
        [SerializeField] private float temperatureDrainRate = 0.3f;
        [SerializeField] private float sanityDrainInDark = 0.5f;

        [Header("Multiplicadores")]
        [SerializeField] private float runningHungerMultiplier = 2f;
        [SerializeField] private float coldTemperatureMultiplier = 1.5f;

        [Header("Dano por Stats Baixos")]
        [SerializeField] private float lowHungerDamage = 1f;
        [SerializeField] private float lowThirstDamage = 2f;
        [SerializeField] private float damageInterval = 5f;

        // Network Variables (sincronizadas automaticamente)
        private NetworkVariable<float> hunger = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> thirst = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> temperature = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> sanity = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Estados
        private bool isRunning;
        private bool isInDarkness;
        private bool isNearFire;
        private float damageTimer;

        // Referências
        private HorrorGame.Player.Health.PlayerHealth playerHealth;

        // Properties públicas (apenas leitura)
        public float Hunger => hunger.Value;
        public float Thirst => thirst.Value;
        public float Temperature => temperature.Value;
        public float Sanity => sanity.Value;

        public float HungerPercentage => Hunger / maxHunger;
        public float ThirstPercentage => Thirst / maxThirst;
        public float TemperaturePercentage => Temperature / maxTemperature;
        public float SanityPercentage => Sanity / maxSanity;

        // Events
        public event System.Action<float> OnHungerChanged;
        public event System.Action<float> OnThirstChanged;
        public event System.Action<float> OnTemperatureChanged;
        public event System.Action<float> OnSanityChanged;

        private void Awake()
        {
            playerHealth = GetComponent<HorrorGame.Player.Health.PlayerHealth>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Inicializar stats se for o owner
            if (IsOwner)
            {
                InitializeStats();
            }

            // Subscribe em mudanças (para todos os clientes)
            hunger.OnValueChanged += (oldValue, newValue) => OnHungerChanged?.Invoke(newValue);
            thirst.OnValueChanged += (oldValue, newValue) => OnThirstChanged?.Invoke(newValue);
            temperature.OnValueChanged += (oldValue, newValue) => OnTemperatureChanged?.Invoke(newValue);
            sanity.OnValueChanged += (oldValue, newValue) => OnSanityChanged?.Invoke(newValue);
        }

        private void Update()
        {
            if (!IsOwner) return; // Só o owner processa

            UpdateStats();
            CheckCriticalStats();
        }

        private void InitializeStats()
        {
            hunger.Value = maxHunger;
            thirst.Value = maxThirst;
            temperature.Value = maxTemperature;
            sanity.Value = maxSanity;
        }

        private void UpdateStats()
        {
            float deltaTime = Time.deltaTime;

            // Drenagem de Fome
            float hungerDrain = hungerDrainRate * deltaTime;
            if (isRunning)
                hungerDrain *= runningHungerMultiplier;

            hunger.Value = Mathf.Max(0, hunger.Value - hungerDrain);

            // Drenagem de Sede
            float thirstDrain = thirstDrainRate * deltaTime;
            thirst.Value = Mathf.Max(0, thirst.Value - thirstDrain);

            // Temperatura
            UpdateTemperature(deltaTime);

            // Sanidade
            UpdateSanity(deltaTime);
        }

        private void UpdateTemperature(float deltaTime)
        {
            float tempChange = 0f;

            if (isNearFire)
            {
                // Perto de fogo: aumenta temperatura
                tempChange = 2f * deltaTime;
            }
            else
            {
                // Frio natural (especialmente à noite)
                tempChange = -temperatureDrainRate * deltaTime;

                // Multiplica se estiver de noite
                if (DayNightCycle.Instance != null && DayNightCycle.Instance.IsNight)
                {
                    tempChange *= coldTemperatureMultiplier;
                }
            }

            temperature.Value = Mathf.Clamp(temperature.Value + tempChange, 0, maxTemperature);
        }

        private void UpdateSanity(float deltaTime)
        {
            if (isInDarkness)
            {
                // No escuro: perde sanidade
                sanity.Value = Mathf.Max(0, sanity.Value - sanityDrainInDark * deltaTime);
            }
            else
            {
                // Na luz: recupera sanidade lentamente
                sanity.Value = Mathf.Min(maxSanity, sanity.Value + 0.2f * deltaTime);
            }

            // Sanidade também cai perto de monstros (implementar com trigger detection)
        }

        private void CheckCriticalStats()
        {
            damageTimer -= Time.deltaTime;

            if (damageTimer <= 0f)
            {
                damageTimer = damageInterval;

                // Fome crítica
                if (HungerPercentage < 0.1f)
                {
                    DealDamage(lowHungerDamage, "Starvation");
                }

                // Sede crítica
                if (ThirstPercentage < 0.1f)
                {
                    DealDamage(lowThirstDamage, "Dehydration");
                }
            }
        }

        private void DealDamage(float amount, string cause)
        {
            if (playerHealth != null)
            {
                Debug.Log($"[Survival] Taking {amount} damage from {cause}");
                // playerHealth.TakeDamage(amount); // Descomentar quando integrar
            }
        }

        #region Public Methods - Modificar Stats

        /// <summary>
        /// Consome comida para restaurar fome
        /// </summary>
        public void Eat(float amount)
        {
            if (!IsOwner) return;
            hunger.Value = Mathf.Min(maxHunger, hunger.Value + amount);
            Debug.Log($"[Survival] Ate food. Hunger: {Hunger:F1}");
        }

        /// <summary>
        /// Bebe água para restaurar sede
        /// </summary>
        public void Drink(float amount)
        {
            if (!IsOwner) return;
            thirst.Value = Mathf.Min(maxThirst, thirst.Value + amount);
            Debug.Log($"[Survival] Drank water. Thirst: {Thirst:F1}");
        }

        /// <summary>
        /// Aquece o jogador
        /// </summary>
        public void Warm(float amount)
        {
            if (!IsOwner) return;
            temperature.Value = Mathf.Min(maxTemperature, temperature.Value + amount);
        }

        /// <summary>
        /// Restaura sanidade
        /// </summary>
        public void RestoreSanity(float amount)
        {
            if (!IsOwner) return;
            sanity.Value = Mathf.Min(maxSanity, sanity.Value + amount);
        }

        /// <summary>
        /// Reduz sanidade (usado quando vê monstros)
        /// </summary>
        public void ReduceSanity(float amount)
        {
            if (!IsOwner) return;
            sanity.Value = Mathf.Max(0, sanity.Value - amount);
        }

        #endregion

        #region State Setters

        public void SetRunning(bool running)
        {
            isRunning = running;
        }

        public void SetInDarkness(bool inDarkness)
        {
            isInDarkness = inDarkness;
        }

        public void SetNearFire(bool nearFire)
        {
            isNearFire = nearFire;
        }

        #endregion

        #region Status Checks

        public bool IsStarving() => HungerPercentage < 0.3f;
        public bool IsDehydrated() => ThirstPercentage < 0.3f;
        public bool IsCold() => TemperaturePercentage < 0.3f;
        public bool IsInsane() => SanityPercentage < 0.2f;

        public bool CanRun() => HungerPercentage > 0.1f && ThirstPercentage > 0.1f;

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!IsOwner) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.white;

            int y = 120;
            int lineHeight = 20;

            GUI.Label(new Rect(10, y, 300, lineHeight), $"🍖 Hunger: {Hunger:F1} ({HungerPercentage:P0})", style);
            y += lineHeight;
            GUI.Label(new Rect(10, y, 300, lineHeight), $"💧 Thirst: {Thirst:F1} ({ThirstPercentage:P0})", style);
            y += lineHeight;
            GUI.Label(new Rect(10, y, 300, lineHeight), $"🥶 Temperature: {Temperature:F1} ({TemperaturePercentage:P0})", style);
            y += lineHeight;
            GUI.Label(new Rect(10, y, 300, lineHeight), $"😰 Sanity: {Sanity:F1} ({SanityPercentage:P0})", style);
        }

        #endregion
    }
}
