using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Audio
{
    /// <summary>
    /// Gerenciador de áudio do jogo. Controla música, SFX e sons ambientes.
    /// Essencial para criar atmosfera em jogos de terror.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource tensionSource;
        [SerializeField] private AudioSource sfxSourcePrefab;

        [Header("Settings")]
        [SerializeField] private int maxSFXSources = 10;
        [SerializeField] private float musicFadeDuration = 2f;
        [SerializeField] private float tensionFadeDuration = 1f;

        [Header("Volume Settings")]
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.6f;

        private Queue<AudioSource> sfxSourcePool = new Queue<AudioSource>();
        private List<AudioSource> activeSFXSources = new List<AudioSource>();
        private float currentTensionLevel = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
            CreateSFXPool();
            SubscribeToEvents();
        }

        private void InitializeAudioSources()
        {
            // Cria AudioSources se não existirem
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("AmbientSource");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }

            if (tensionSource == null)
            {
                GameObject tensionObj = new GameObject("TensionSource");
                tensionObj.transform.SetParent(transform);
                tensionSource = tensionObj.AddComponent<AudioSource>();
                tensionSource.loop = true;
                tensionSource.playOnAwake = false;
                tensionSource.volume = 0f;
            }

            UpdateVolumes();
        }

        private void CreateSFXPool()
        {
            for (int i = 0; i < maxSFXSources; i++)
            {
                GameObject sfxObj = new GameObject($"SFXSource_{i}");
                sfxObj.transform.SetParent(transform);
                AudioSource source = sfxObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxSourcePool.Enqueue(source);
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnPlaySound2D += PlaySound2D;
            GameEvents.OnPlaySound3D += PlaySound3D;
            GameEvents.OnChangeMusicTrack += PlayMusic;
            GameEvents.OnTensionLevelChanged += SetTensionLevel;
        }

        private void OnDestroy()
        {
            GameEvents.OnPlaySound2D -= PlaySound2D;
            GameEvents.OnPlaySound3D -= PlaySound3D;
            GameEvents.OnChangeMusicTrack -= PlayMusic;
            GameEvents.OnTensionLevelChanged -= SetTensionLevel;
        }

        #region Music Control

        public void PlayMusic(string musicName, bool fadeIn = true)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/Music/{musicName}");

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] Music clip not found: {musicName}");
                return;
            }

            if (fadeIn)
            {
                StartCoroutine(CrossfadeMusic(clip));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
        }

        public void StopMusic(bool fadeOut = true)
        {
            if (fadeOut)
            {
                StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicSource.Stop();
            }
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip)
        {
            // Fade out da música atual
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (musicFadeDuration / 2f));
                yield return null;
            }

            // Troca o clip
            musicSource.clip = newClip;
            musicSource.Play();

            // Fade in da nova música
            elapsed = 0f;
            while (elapsed < musicFadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, elapsed / (musicFadeDuration / 2f));
                yield return null;
            }
        }

        private IEnumerator FadeOutMusic()
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeDuration);
                yield return null;
            }

            musicSource.Stop();
        }

        #endregion

        #region SFX Control

        public void PlaySound2D(string soundName, float volumeMultiplier = 1f)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{soundName}");

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX clip not found: {soundName}");
                return;
            }

            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.spatialBlend = 0f; // 2D
                source.volume = sfxVolume * masterVolume * volumeMultiplier;
                source.PlayOneShot(clip);
            }
        }

        public void PlaySound3D(string soundName, Vector3 position, float volumeMultiplier = 1f)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/SFX/{soundName}");

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] SFX clip not found: {soundName}");
                return;
            }

            AudioSource source = GetAvailableSFXSource();
            if (source != null)
            {
                source.transform.position = position;
                source.spatialBlend = 1f; // 3D
                source.volume = sfxVolume * masterVolume * volumeMultiplier;
                source.PlayOneShot(clip);
            }
        }

        private AudioSource GetAvailableSFXSource()
        {
            // Tenta pegar uma source do pool
            if (sfxSourcePool.Count > 0)
            {
                AudioSource source = sfxSourcePool.Dequeue();
                activeSFXSources.Add(source);
                return source;
            }

            // Se não houver disponível, tenta reutilizar uma que não está tocando
            foreach (var source in activeSFXSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            Debug.LogWarning("[AudioManager] No available SFX sources");
            return null;
        }

        #endregion

        #region Ambient & Tension

        public void PlayAmbient(string ambientName)
        {
            AudioClip clip = Resources.Load<AudioClip>($"Audio/Ambient/{ambientName}");

            if (clip == null)
            {
                Debug.LogWarning($"[AudioManager] Ambient clip not found: {ambientName}");
                return;
            }

            ambientSource.clip = clip;
            ambientSource.volume = ambientVolume * masterVolume;
            ambientSource.Play();
        }

        /// <summary>
        /// Define o nível de tensão (0-1). Usado para aumentar música de tensão quando inimigos estão perto.
        /// </summary>
        public void SetTensionLevel(float level)
        {
            level = Mathf.Clamp01(level);
            currentTensionLevel = level;
            StartCoroutine(FadeTension(level));
        }

        private IEnumerator FadeTension(float targetVolume)
        {
            float startVolume = tensionSource.volume;
            float elapsed = 0f;

            while (elapsed < tensionFadeDuration)
            {
                elapsed += Time.deltaTime;
                tensionSource.volume = Mathf.Lerp(startVolume, targetVolume * masterVolume, elapsed / tensionFadeDuration);
                yield return null;
            }
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;

            if (ambientSource != null)
                ambientSource.volume = ambientVolume * masterVolume;
        }

        #endregion
    }
}
