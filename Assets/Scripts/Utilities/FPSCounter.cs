using UnityEngine;
using TMPro;

namespace HorrorGame.Utilities
{
    /// <summary>
    /// Contador de FPS simples para debug
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private float updateInterval = 0.5f;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private Color goodFPSColor = Color.green;
        [SerializeField] private Color averageFPSColor = Color.yellow;
        [SerializeField] private Color badFPSColor = Color.red;

        [Header("Thresholds")]
        [SerializeField] private int goodFPSThreshold = 50;
        [SerializeField] private int averageFPSThreshold = 30;

        private float deltaTime = 0f;
        private float updateTimer = 0f;
        private int frameCount = 0;
        private float currentFPS = 0f;

        private void Start()
        {
            if (fpsText != null)
            {
                fpsText.gameObject.SetActive(showOnStart);
            }
        }

        private void Update()
        {
            deltaTime += Time.unscaledDeltaTime;
            frameCount++;
            updateTimer += Time.unscaledDeltaTime;

            if (updateTimer >= updateInterval)
            {
                currentFPS = frameCount / deltaTime;
                UpdateDisplay();

                deltaTime = 0f;
                frameCount = 0;
                updateTimer = 0f;
            }
        }

        private void UpdateDisplay()
        {
            if (fpsText == null) return;

            fpsText.text = $"FPS: {Mathf.RoundToInt(currentFPS)}";

            // Muda cor baseado no FPS
            if (currentFPS >= goodFPSThreshold)
            {
                fpsText.color = goodFPSColor;
            }
            else if (currentFPS >= averageFPSThreshold)
            {
                fpsText.color = averageFPSColor;
            }
            else
            {
                fpsText.color = badFPSColor;
            }
        }

        public void ToggleDisplay()
        {
            if (fpsText != null)
            {
                fpsText.gameObject.SetActive(!fpsText.gameObject.activeSelf);
            }
        }

        public void SetVisible(bool visible)
        {
            if (fpsText != null)
            {
                fpsText.gameObject.SetActive(visible);
            }
        }
    }
}
