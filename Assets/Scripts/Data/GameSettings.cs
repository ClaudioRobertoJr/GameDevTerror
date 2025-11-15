using UnityEngine;

namespace HorrorGame.Data
{
    /// <summary>
    /// ScriptableObject para configurações globais do jogo
    /// </summary>
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Horror Game/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Graphics")]
        [Tooltip("Qualidade gráfica padrão")]
        public int defaultQualityLevel = 2;

        [Tooltip("Taxa de frames alvo")]
        public int targetFrameRate = 60;

        [Tooltip("VSync ativado")]
        public bool vSyncEnabled = false;

        [Header("Audio")]
        [Range(0f, 1f)]
        public float defaultMasterVolume = 1f;

        [Range(0f, 1f)]
        public float defaultMusicVolume = 0.7f;

        [Range(0f, 1f)]
        public float defaultSFXVolume = 1f;

        [Header("Gameplay")]
        [Tooltip("Dificuldade padrão")]
        public GameDifficulty defaultDifficulty = GameDifficulty.Normal;

        [Tooltip("Multiplcador de dano recebido pelo player")]
        public float playerDamageMultiplier = 1f;

        [Tooltip("Multiplicador de dano causado aos inimigos")]
        public float enemyDamageMultiplier = 1f;

        [Header("Controls")]
        [Range(1f, 10f)]
        public float defaultMouseSensitivity = 3f;

        public bool invertYAxis = false;

        [Header("UI")]
        [Tooltip("Mostrar FPS counter")]
        public bool showFPSCounter = false;

        [Tooltip("Mostrar dicas de tutorial")]
        public bool showTutorialHints = true;
    }

    public enum GameDifficulty
    {
        Easy,
        Normal,
        Hard,
        Nightmare
    }
}
