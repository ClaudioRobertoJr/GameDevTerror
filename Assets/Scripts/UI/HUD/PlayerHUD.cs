using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HorrorGame.Core;

namespace HorrorGame.UI
{
    /// <summary>
    /// HUD do player mostrando vida, stamina e informações importantes
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Health UI")]
        [SerializeField] private Image healthBar;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image damageVignette;

        [Header("Stamina UI")]
        [SerializeField] private Image staminaBar;
        [SerializeField] private CanvasGroup staminaGroup;

        [Header("Interaction UI")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private TextMeshProUGUI interactionText;

        [Header("Message UI")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup messageGroup;

        [Header("Effects")]
        [SerializeField] private float damageVignetteDuration = 0.5f;
        [SerializeField] private float messageFadeDuration = 0.3f;

        private float damageVignetteTimer = 0f;
        private float messageTimer = 0f;
        private float messageDuration = 0f;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            UpdateDamageVignette();
            UpdateMessageFade();
        }

        private void InitializeUI()
        {
            if (damageVignette != null)
            {
                Color c = damageVignette.color;
                c.a = 0f;
                damageVignette.color = c;
            }

            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            if (messageGroup != null)
                messageGroup.alpha = 0f;
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnPlayerDamaged += UpdateHealth;
            GameEvents.OnPlayerHealed += UpdateHealth;
            GameEvents.OnStaminaChanged += UpdateStamina;
            GameEvents.OnShowMessage += ShowMessage;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnPlayerDamaged -= UpdateHealth;
            GameEvents.OnPlayerHealed -= UpdateHealth;
            GameEvents.OnStaminaChanged -= UpdateStamina;
            GameEvents.OnShowMessage -= ShowMessage;
        }

        #region Health

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            float healthPercent = currentHealth / maxHealth;

            if (healthBar != null)
                healthBar.fillAmount = healthPercent;

            if (healthText != null)
                healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";

            // Mostra vignette de dano
            if (damageVignette != null)
            {
                damageVignetteTimer = damageVignetteDuration;
            }
        }

        private void UpdateDamageVignette()
        {
            if (damageVignette == null) return;

            if (damageVignetteTimer > 0f)
            {
                damageVignetteTimer -= Time.deltaTime;
                float alpha = Mathf.Clamp01(damageVignetteTimer / damageVignetteDuration);
                Color c = damageVignette.color;
                c.a = alpha;
                damageVignette.color = c;
            }
        }

        #endregion

        #region Stamina

        public void UpdateStamina(float currentStamina, float maxStamina)
        {
            float staminaPercent = currentStamina / maxStamina;

            if (staminaBar != null)
                staminaBar.fillAmount = staminaPercent;

            // Mostra/esconde barra de stamina baseado no uso
            if (staminaGroup != null)
            {
                staminaGroup.alpha = staminaPercent < 0.99f ? 1f : 0f;
            }
        }

        #endregion

        #region Interaction

        public void ShowInteractionPrompt(string text)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);

            if (interactionText != null)
                interactionText.text = text;
        }

        public void HideInteractionPrompt()
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }

        #endregion

        #region Messages

        public void ShowMessage(string message, float duration)
        {
            if (messageText != null)
                messageText.text = message;

            messageDuration = duration;
            messageTimer = duration;

            if (messageGroup != null)
                messageGroup.alpha = 1f;
        }

        private void UpdateMessageFade()
        {
            if (messageGroup == null || messageTimer <= 0f) return;

            messageTimer -= Time.deltaTime;

            // Fade out nos últimos segundos
            if (messageTimer < messageFadeDuration)
            {
                messageGroup.alpha = messageTimer / messageFadeDuration;
            }
        }

        #endregion
    }
}
