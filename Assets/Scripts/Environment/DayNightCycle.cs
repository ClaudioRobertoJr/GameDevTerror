using UnityEngine;
using Unity.Netcode;
using System;

namespace SurvivalHorror.Environment
{
    /// <summary>
    /// Sistema de ciclo dia/noite sincronizado em multiplayer
    /// Controla iluminação, clima e eventos baseados no tempo
    /// 24 minutos reais = 1 dia in-game
    /// </summary>
    public class DayNightCycle : NetworkBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        [Header("Configuração de Tempo")]
        [SerializeField] private float dayLengthInMinutes = 24f; // 24 minutos real = 1 dia
        [SerializeField] private float startTime = 7f; // Começa às 7h da manhã

        [Header("Referências")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Material skyboxDay;
        [SerializeField] private Material skyboxNight;
        [SerializeField] private GameObject starsObject;

        [Header("Iluminação")]
        [SerializeField] private Gradient lightColor;
        [SerializeField] private AnimationCurve lightIntensity;
        [SerializeField] private AnimationCurve ambientIntensity;

        [Header("Fog")]
        [SerializeField] private bool enableFog = true;
        [SerializeField] private AnimationCurve fogDensity;
        [SerializeField] private Gradient fogColor;

        // Network Variables
        private NetworkVariable<float> currentTime = new NetworkVariable<float>(
            7f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<int> currentDay = new NetworkVariable<int>(
            1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // Estado
        private float timeScale;
        private TimeOfDay previousTimeOfDay;
        private bool isNight;

        // Properties públicas
        public float CurrentTime => currentTime.Value;
        public int CurrentDay => currentDay.Value;
        public bool IsNight => isNight;
        public TimeOfDay CurrentTimeOfDay => GetTimeOfDay(CurrentTime);
        public float DayProgress => CurrentTime / 24f;

        // Events
        public event Action<TimeOfDay> OnTimeOfDayChanged;
        public event Action<int> OnNewDay;
        public event Action OnNightStarted;
        public event Action OnDayStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Calcular time scale (quanto passar por segundo)
            timeScale = 24f / (dayLengthInMinutes * 60f);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                currentTime.Value = startTime;
                currentDay.Value = 1;
            }

            // Subscribe em mudanças de tempo
            currentTime.OnValueChanged += OnTimeChanged;
            currentDay.OnValueChanged += OnDayChanged;

            // Configurações iniciais
            previousTimeOfDay = GetTimeOfDay(CurrentTime);
            isNight = IsNightTime(CurrentTime);

            if (directionalLight == null)
                directionalLight = FindObjectOfType<Light>();

            UpdateEnvironment(CurrentTime);
        }

        private void Update()
        {
            if (IsServer)
            {
                UpdateTime();
            }

            UpdateEnvironment(CurrentTime);
            CheckTimeOfDayTransitions();
        }

        #region Time Management

        private void UpdateTime()
        {
            currentTime.Value += timeScale * Time.deltaTime;

            // Novo dia
            if (currentTime.Value >= 24f)
            {
                currentTime.Value = 0f;
                currentDay.Value++;
            }
        }

        private void OnTimeChanged(float previousValue, float newValue)
        {
            UpdateEnvironment(newValue);
        }

        private void OnDayChanged(int previousValue, int newValue)
        {
            OnNewDay?.Invoke(newValue);
            Debug.Log($"[DayNight] Day {newValue} started!");
        }

        #endregion

        #region Environment Updates

        private void UpdateEnvironment(float time)
        {
            float normalizedTime = time / 24f;

            UpdateSunRotation(time);
            UpdateLighting(normalizedTime);
            UpdateFog(normalizedTime);
            UpdateSkybox(normalizedTime);
        }

        private void UpdateSunRotation(float time)
        {
            if (directionalLight == null) return;

            // Sol roda 360° em 24 horas
            // Meia-noite (0h) = -90° (sol abaixo do horizonte)
            // Meio-dia (12h) = 90° (sol acima)
            float angle = (time / 24f) * 360f - 90f;
            directionalLight.transform.rotation = Quaternion.Euler(angle, 0f, 0f);
        }

        private void UpdateLighting(float normalizedTime)
        {
            if (directionalLight == null) return;

            // Cor da luz
            directionalLight.color = lightColor.Evaluate(normalizedTime);

            // Intensidade da luz
            directionalLight.intensity = lightIntensity.Evaluate(normalizedTime);

            // Luz ambiente
            RenderSettings.ambientIntensity = ambientIntensity.Evaluate(normalizedTime);
        }

        private void UpdateFog(float normalizedTime)
        {
            if (!enableFog) return;

            RenderSettings.fog = true;
            RenderSettings.fogDensity = fogDensity.Evaluate(normalizedTime);
            RenderSettings.fogColor = fogColor.Evaluate(normalizedTime);
        }

        private void UpdateSkybox(float normalizedTime)
        {
            if (skyboxDay == null || skyboxNight == null) return;

            // Blend entre skybox dia e noite
            if (normalizedTime < 0.25f || normalizedTime > 0.75f)
            {
                RenderSettings.skybox = skyboxNight;
            }
            else
            {
                RenderSettings.skybox = skyboxDay;
            }

            // Controlar estrelas
            if (starsObject != null)
            {
                starsObject.SetActive(isNight);
            }
        }

        #endregion

        #region Time of Day Transitions

        private void CheckTimeOfDayTransitions()
        {
            TimeOfDay currentTOD = GetTimeOfDay(CurrentTime);

            if (currentTOD != previousTimeOfDay)
            {
                OnTimeOfDayChanged?.Invoke(currentTOD);
                Debug.Log($"[DayNight] Time of day changed to: {currentTOD}");

                previousTimeOfDay = currentTOD;
            }

            // Check night transitions
            bool wasNight = isNight;
            isNight = IsNightTime(CurrentTime);

            if (isNight && !wasNight)
            {
                OnNightStarted?.Invoke();
                Debug.Log($"[DayNight] Night {CurrentDay} started!");
            }
            else if (!isNight && wasNight)
            {
                OnDayStarted?.Invoke();
                Debug.Log($"[DayNight] Day {CurrentDay} started!");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Define tempo manualmente (apenas Server)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetTimeServerRpc(float time)
        {
            currentTime.Value = Mathf.Clamp(time, 0f, 24f);
        }

        /// <summary>
        /// Avança para próximo dia (apenas Server)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void AdvanceDayServerRpc()
        {
            currentDay.Value++;
            currentTime.Value = startTime;
        }

        /// <summary>
        /// Pausa/resume tempo (apenas Server)
        /// </summary>
        public void SetTimeScale(float scale)
        {
            if (!IsServer) return;
            timeScale = scale * (24f / (dayLengthInMinutes * 60f));
        }

        #endregion

        #region Time Utilities

        public static TimeOfDay GetTimeOfDay(float time)
        {
            if (time >= 5f && time < 7f)
                return TimeOfDay.Dawn;
            else if (time >= 7f && time < 12f)
                return TimeOfDay.Morning;
            else if (time >= 12f && time < 17f)
                return TimeOfDay.Afternoon;
            else if (time >= 17f && time < 19f)
                return TimeOfDay.Dusk;
            else if (time >= 19f && time < 23f)
                return TimeOfDay.Night;
            else
                return TimeOfDay.Midnight;
        }

        public static bool IsNightTime(float time)
        {
            return time >= 19f || time < 5f;
        }

        public string GetFormattedTime()
        {
            int hours = Mathf.FloorToInt(CurrentTime);
            int minutes = Mathf.FloorToInt((CurrentTime - hours) * 60f);
            return $"{hours:00}:{minutes:00}";
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!IsOwner) return;

            GUIStyle style = new GUIStyle();
            style.fontSize = 16;
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;

            string timeStr = GetFormattedTime();
            string dayStr = $"Day {CurrentDay}";
            string todStr = CurrentTimeOfDay.ToString();

            GUI.Label(new Rect(10, 10, 300, 30), $"🌍 {dayStr} - {timeStr}", style);
            GUI.Label(new Rect(10, 35, 300, 30), $"⏰ {todStr} {(isNight ? "🌙" : "☀️")}", style);

            // Debug controls (apenas em development)
            if (Debug.isDebugBuild && IsServer)
            {
                if (GUI.Button(new Rect(10, 70, 100, 30), "Skip to Night"))
                {
                    SetTimeServerRpc(19f);
                }
                if (GUI.Button(new Rect(120, 70, 100, 30), "Skip to Day"))
                {
                    SetTimeServerRpc(7f);
                }
            }
        }

        #endregion
    }

    #region Enums

    public enum TimeOfDay
    {
        Dawn,       // 05:00 - 07:00
        Morning,    // 07:00 - 12:00
        Afternoon,  // 12:00 - 17:00
        Dusk,       // 17:00 - 19:00
        Night,      // 19:00 - 23:00
        Midnight    // 23:00 - 05:00
    }

    #endregion
}
