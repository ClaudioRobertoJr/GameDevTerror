using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HorrorGame.Core
{
    /// <summary>
    /// Controla o carregamento e transição entre cenas
    /// </summary>
    public class SceneController : MonoBehaviour
    {
        public static SceneController Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameSceneName = "GameLevel01";
        [SerializeField] private string loadingSceneName = "Loading";

        [Header("Loading Settings")]
        [SerializeField] private float minimumLoadingTime = 1f;
        [SerializeField] private bool useAsyncLoading = true;

        public bool IsLoading { get; private set; }
        public float LoadingProgress { get; private set; }

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

        #region Public Methods

        public void LoadMainMenu()
        {
            LoadScene(mainMenuSceneName);
        }

        public void LoadGameScene()
        {
            LoadScene(gameSceneName);
        }

        public void LoadScene(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneController] Already loading a scene. Ignoring request to load {sceneName}");
                return;
            }

            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void ReloadCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            LoadScene(currentScene.name);
        }

        #endregion

        #region Private Methods

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            IsLoading = true;
            LoadingProgress = 0f;

            Debug.Log($"[SceneController] Loading scene: {sceneName}");

            float startTime = Time.realtimeSinceStartup;

            if (useAsyncLoading)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
                asyncLoad.allowSceneActivation = false;

                // Espera até que a cena esteja quase carregada
                while (!asyncLoad.isDone)
                {
                    // Progress vai de 0 a 0.9 quando está carregando
                    LoadingProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                    // Quando chegar a 0.9, a cena está pronta mas não ativada
                    if (asyncLoad.progress >= 0.9f)
                    {
                        LoadingProgress = 1f;

                        // Garante tempo mínimo de loading para evitar flashes
                        float elapsedTime = Time.realtimeSinceStartup - startTime;
                        if (elapsedTime < minimumLoadingTime)
                        {
                            yield return new WaitForSecondsRealtime(minimumLoadingTime - elapsedTime);
                        }

                        asyncLoad.allowSceneActivation = true;
                    }

                    yield return null;
                }
            }
            else
            {
                // Loading síncrono (não recomendado para jogos grandes)
                SceneManager.LoadScene(sceneName);
            }

            IsLoading = false;
            LoadingProgress = 0f;

            Debug.Log($"[SceneController] Scene {sceneName} loaded successfully");

            // Notifica que a cena foi carregada
            OnSceneLoadedComplete(sceneName);
        }

        private void OnSceneLoadedComplete(string sceneName)
        {
            // Determina o novo estado baseado na cena carregada
            if (sceneName == mainMenuSceneName)
            {
                GameManager.Instance?.ChangeState(GameState.MainMenu);
            }
            else if (sceneName == gameSceneName)
            {
                GameManager.Instance?.ChangeState(GameState.Playing);
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
