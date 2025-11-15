using UnityEngine;
using UnityEngine.SceneManagement;

namespace HorrorGame.Core
{
    /// <summary>
    /// Gerenciador principal do jogo. Controla estados, inicialização e sistemas globais.
    /// Singleton persistente entre cenas.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        [Header("Settings")]
        [SerializeField] private bool startPaused = false;
        [SerializeField] private float timeScale = 1f;

        public GameState CurrentState => currentState;
        public bool IsPaused { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Start()
        {
            if (startPaused)
            {
                PauseGame();
            }
        }

        private void Initialize()
        {
            // Configurações iniciais
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            // Cursor inicialmente travado (para FPS)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("[GameManager] Initialized successfully");
        }

        #region Game State Management

        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            GameState previousState = currentState;
            currentState = newState;

            // Notificar mudança de estado
            GameEvents.OnGameStateChanged?.Invoke(previousState, newState);

            Debug.Log($"[GameManager] State changed: {previousState} -> {newState}");

            HandleStateChange(newState);
        }

        private void HandleStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameState.Playing:
                    Time.timeScale = timeScale;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }
        }

        #endregion

        #region Pause System

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;

            IsPaused = true;
            ChangeState(GameState.Paused);
            GameEvents.OnGamePaused?.Invoke();
        }

        public void ResumeGame()
        {
            if (!IsPaused) return;

            IsPaused = false;
            ChangeState(GameState.Playing);
            GameEvents.OnGameResumed?.Invoke();
        }

        public void TogglePause()
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }

        #endregion

        #region Game Flow

        public void StartNewGame()
        {
            ChangeState(GameState.Loading);
            SceneController.Instance.LoadGameScene();
        }

        public void ReturnToMainMenu()
        {
            ChangeState(GameState.Loading);
            SceneController.Instance.LoadMainMenu();
        }

        public void GameOver()
        {
            ChangeState(GameState.GameOver);
            GameEvents.OnPlayerDied?.Invoke();
        }

        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        #endregion

        private void OnApplicationQuit()
        {
            Debug.Log("[GameManager] Application quit");
        }
    }

    /// <summary>
    /// Estados possíveis do jogo
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver,
        Cutscene
    }
}
