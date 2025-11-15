using System;
using UnityEngine;

namespace HorrorGame.Core
{
    /// <summary>
    /// Sistema centralizado de eventos do jogo.
    /// Permite comunicação desacoplada entre sistemas.
    /// </summary>
    public static class GameEvents
    {
        #region Game State Events

        /// <summary>
        /// Disparado quando o estado do jogo muda
        /// </summary>
        public static Action<GameState, GameState> OnGameStateChanged;

        /// <summary>
        /// Disparado quando o jogo é pausado
        /// </summary>
        public static Action OnGamePaused;

        /// <summary>
        /// Disparado quando o jogo é retomado
        /// </summary>
        public static Action OnGameResumed;

        #endregion

        #region Player Events

        /// <summary>
        /// Disparado quando o player recebe dano (float = dano, float = vida atual)
        /// </summary>
        public static Action<float, float> OnPlayerDamaged;

        /// <summary>
        /// Disparado quando o player morre
        /// </summary>
        public static Action OnPlayerDied;

        /// <summary>
        /// Disparado quando a vida do player é curada
        /// </summary>
        public static Action<float, float> OnPlayerHealed;

        /// <summary>
        /// Disparado quando a stamina muda
        /// </summary>
        public static Action<float, float> OnStaminaChanged;

        /// <summary>
        /// Disparado quando o player interage com algo
        /// </summary>
        public static Action<GameObject> OnPlayerInteract;

        #endregion

        #region Enemy Events

        /// <summary>
        /// Disparado quando um inimigo é gerado
        /// </summary>
        public static Action<GameObject> OnEnemySpawned;

        /// <summary>
        /// Disparado quando um inimigo morre
        /// </summary>
        public static Action<GameObject> OnEnemyDied;

        /// <summary>
        /// Disparado quando um inimigo detecta o player
        /// </summary>
        public static Action<GameObject> OnEnemyDetectedPlayer;

        /// <summary>
        /// Disparado quando um inimigo perde o player
        /// </summary>
        public static Action<GameObject> OnEnemyLostPlayer;

        /// <summary>
        /// Disparado quando um inimigo ataca
        /// </summary>
        public static Action<GameObject> OnEnemyAttack;

        #endregion

        #region Audio Events

        /// <summary>
        /// Disparado para tocar um som 2D (string = nome do som)
        /// </summary>
        public static Action<string> OnPlaySound2D;

        /// <summary>
        /// Disparado para tocar um som 3D (string = nome do som, Vector3 = posição)
        /// </summary>
        public static Action<string, Vector3> OnPlaySound3D;

        /// <summary>
        /// Disparado para mudar a música de fundo
        /// </summary>
        public static Action<string> OnChangeMusicTrack;

        /// <summary>
        /// Disparado para ajustar intensidade do áudio de tensão
        /// </summary>
        public static Action<float> OnTensionLevelChanged;

        #endregion

        #region Environment Events

        /// <summary>
        /// Disparado quando uma porta é aberta/fechada
        /// </summary>
        public static Action<GameObject, bool> OnDoorToggled;

        /// <summary>
        /// Disparado quando um item é coletado
        /// </summary>
        public static Action<GameObject> OnItemCollected;

        /// <summary>
        /// Disparado quando um checkpoint é atingido
        /// </summary>
        public static Action<Vector3> OnCheckpointReached;

        #endregion

        #region UI Events

        /// <summary>
        /// Disparado para mostrar uma mensagem na UI
        /// </summary>
        public static Action<string, float> OnShowMessage;

        /// <summary>
        /// Disparado para atualizar um objetivo
        /// </summary>
        public static Action<string> OnObjectiveUpdated;

        #endregion

        /// <summary>
        /// Limpa todas as inscrições de eventos (útil ao carregar cenas)
        /// </summary>
        public static void ClearAllEvents()
        {
            OnGameStateChanged = null;
            OnGamePaused = null;
            OnGameResumed = null;

            OnPlayerDamaged = null;
            OnPlayerDied = null;
            OnPlayerHealed = null;
            OnStaminaChanged = null;
            OnPlayerInteract = null;

            OnEnemySpawned = null;
            OnEnemyDied = null;
            OnEnemyDetectedPlayer = null;
            OnEnemyLostPlayer = null;
            OnEnemyAttack = null;

            OnPlaySound2D = null;
            OnPlaySound3D = null;
            OnChangeMusicTrack = null;
            OnTensionLevelChanged = null;

            OnDoorToggled = null;
            OnItemCollected = null;
            OnCheckpointReached = null;

            OnShowMessage = null;
            OnObjectiveUpdated = null;

            Debug.Log("[GameEvents] All events cleared");
        }
    }
}
