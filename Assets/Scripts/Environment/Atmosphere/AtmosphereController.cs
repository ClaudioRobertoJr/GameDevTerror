using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Environment
{
    /// <summary>
    /// Controla a atmosfera do ambiente (iluminação, névoa, efeitos)
    /// Essencial para criar tensão em jogos de terror
    /// </summary>
    public class AtmosphereController : MonoBehaviour
    {
        [Header("Lighting")]
        [SerializeField] private Light mainLight;
        [SerializeField] private float normalLightIntensity = 0.5f;
        [SerializeField] private float tenseLightIntensity = 0.2f;
        [SerializeField] private Color normalLightColor = Color.white;
        [SerializeField] private Color tenseLightColor = new Color(0.8f, 0.3f, 0.3f);

        [Header("Fog")]
        [SerializeField] private bool useFog = true;
        [SerializeField] private float normalFogDensity = 0.01f;
        [SerializeField] private float tenseFogDensity = 0.03f;
        [SerializeField] private Color normalFogColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color tenseFogColor = new Color(0.3f, 0.1f, 0.1f);

        [Header("Transition")]
        [SerializeField] private float transitionSpeed = 1f;

        [Header("Enemy Detection")]
        [SerializeField] private bool reactToEnemyProximity = true;
        [SerializeField] private float enemyDetectionRadius = 15f;
        [SerializeField] private LayerMask enemyLayer;

        private float currentTensionLevel = 0f; // 0 = calmo, 1 = tenso
        private float targetTensionLevel = 0f;
        private Transform playerTransform;

        private void Start()
        {
            if (mainLight == null)
            {
                mainLight = FindObjectOfType<Light>();
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            ApplyFogSettings();

            // Inscreve em eventos
            GameEvents.OnEnemyDetectedPlayer += OnEnemyDetectedPlayer;
            GameEvents.OnEnemyLostPlayer += OnEnemyLostPlayer;
        }

        private void OnDestroy()
        {
            GameEvents.OnEnemyDetectedPlayer -= OnEnemyDetectedPlayer;
            GameEvents.OnEnemyLostPlayer -= OnEnemyLostPlayer;
        }

        private void Update()
        {
            if (reactToEnemyProximity)
            {
                UpdateEnemyProximity();
            }

            UpdateAtmosphere();
        }

        #region Tension Control

        /// <summary>
        /// Define o nível de tensão manualmente (0-1)
        /// </summary>
        public void SetTensionLevel(float level)
        {
            targetTensionLevel = Mathf.Clamp01(level);
        }

        /// <summary>
        /// Aumenta tensão gradualmente
        /// </summary>
        public void IncreaseTension(float amount)
        {
            targetTensionLevel = Mathf.Clamp01(targetTensionLevel + amount);
        }

        /// <summary>
        /// Diminui tensão gradualmente
        /// </summary>
        public void DecreaseTension(float amount)
        {
            targetTensionLevel = Mathf.Clamp01(targetTensionLevel - amount);
        }

        /// <summary>
        /// Reseta para atmosfera calma
        /// </summary>
        public void ResetToNormal()
        {
            targetTensionLevel = 0f;
        }

        #endregion

        #region Updates

        private void UpdateEnemyProximity()
        {
            if (playerTransform == null) return;

            // Verifica inimigos próximos
            Collider[] nearbyEnemies = Physics.OverlapSphere(
                playerTransform.position,
                enemyDetectionRadius,
                enemyLayer
            );

            if (nearbyEnemies.Length > 0)
            {
                // Calcula tensão baseada na distância do inimigo mais próximo
                float closestDistance = float.MaxValue;

                foreach (Collider enemyCol in nearbyEnemies)
                {
                    float distance = Vector3.Distance(playerTransform.position, enemyCol.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                    }
                }

                // Quanto mais perto, mais tenso (inversamente proporcional)
                float proximityTension = 1f - (closestDistance / enemyDetectionRadius);
                targetTensionLevel = Mathf.Max(targetTensionLevel, proximityTension);
            }
            else
            {
                // Sem inimigos por perto, reduz tensão gradualmente
                targetTensionLevel = Mathf.Lerp(targetTensionLevel, 0f, Time.deltaTime * 0.5f);
            }
        }

        private void UpdateAtmosphere()
        {
            // Suaviza transição de tensão
            currentTensionLevel = Mathf.Lerp(currentTensionLevel, targetTensionLevel, Time.deltaTime * transitionSpeed);

            // Atualiza iluminação
            UpdateLighting();

            // Atualiza névoa
            UpdateFog();

            // Notifica sistema de áudio
            GameEvents.OnTensionLevelChanged?.Invoke(currentTensionLevel);
        }

        private void UpdateLighting()
        {
            if (mainLight == null) return;

            // Interpola intensidade
            mainLight.intensity = Mathf.Lerp(normalLightIntensity, tenseLightIntensity, currentTensionLevel);

            // Interpola cor
            mainLight.color = Color.Lerp(normalLightColor, tenseLightColor, currentTensionLevel);
        }

        private void UpdateFog()
        {
            if (!useFog) return;

            // Interpola densidade da névoa
            RenderSettings.fogDensity = Mathf.Lerp(normalFogDensity, tenseFogDensity, currentTensionLevel);

            // Interpola cor da névoa
            RenderSettings.fogColor = Color.Lerp(normalFogColor, tenseFogColor, currentTensionLevel);
        }

        #endregion

        #region Event Handlers

        private void OnEnemyDetectedPlayer(GameObject enemy)
        {
            // Aumenta tensão quando inimigo detecta player
            IncreaseTension(0.5f);
        }

        private void OnEnemyLostPlayer(GameObject enemy)
        {
            // Diminui tensão quando inimigo perde player
            DecreaseTension(0.3f);
        }

        #endregion

        #region Utility

        private void ApplyFogSettings()
        {
            RenderSettings.fog = useFog;
            if (useFog)
            {
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogDensity = normalFogDensity;
                RenderSettings.fogColor = normalFogColor;
            }
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!reactToEnemyProximity) return;

            Transform playerPos = playerTransform;
            if (playerPos == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerPos = player.transform;
            }

            if (playerPos != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawWireSphere(playerPos.position, enemyDetectionRadius);
            }
        }

        #endregion
    }
}
