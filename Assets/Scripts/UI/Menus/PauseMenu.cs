using UnityEngine;
using UnityEngine.UI;
using HorrorGame.Core;

namespace HorrorGame.UI
{
    /// <summary>
    /// Controla o menu de pausa
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        [Header("Settings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        private void Start()
        {
            // Inicialmente oculto
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            // Configura botões
            SetupButtons();

            // Inscreve-se em eventos
            GameEvents.OnGamePaused += ShowPauseMenu;
            GameEvents.OnGameResumed += HidePauseMenu;
        }

        private void OnDestroy()
        {
            GameEvents.OnGamePaused -= ShowPauseMenu;
            GameEvents.OnGameResumed -= HidePauseMenu;
        }

        private void Update()
        {
            // Detecta tecla de pause
            if (Input.GetKeyDown(pauseKey))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TogglePause();
                }
            }
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
        }

        private void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }

        #region Button Callbacks

        private void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        private void OnSettingsClicked()
        {
            // TODO: Abrir menu de configurações
            Debug.Log("[PauseMenu] Settings clicked - implement settings menu");
        }

        private void OnMainMenuClicked()
        {
            GameManager.Instance?.ReturnToMainMenu();
        }

        private void OnQuitClicked()
        {
            GameManager.Instance?.QuitGame();
        }

        #endregion
    }
}
