using UnityEngine;

namespace HorrorGame.Data
{
    /// <summary>
    /// ScriptableObject para configurar dados de inimigos.
    /// Permite criar diferentes tipos de inimigos sem modificar código.
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy Data", menuName = "Horror Game/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyName = "Enemy";
        [TextArea(3, 5)]
        public string description = "";

        [Header("Stats")]
        [Tooltip("Vida máxima do inimigo")]
        public float maxHealth = 100f;

        [Tooltip("Dano causado por ataque")]
        public float damage = 10f;

        [Tooltip("Alcance do ataque")]
        public float attackRange = 2f;

        [Tooltip("Tempo entre ataques")]
        public float attackCooldown = 1.5f;

        [Header("Movement")]
        [Tooltip("Velocidade de patrulha/caminhada")]
        public float moveSpeed = 3.5f;

        [Tooltip("Velocidade ao perseguir o player")]
        public float chaseSpeed = 6f;

        [Header("Detection")]
        [Tooltip("Distância de detecção do player")]
        public float detectionRange = 10f;

        [Tooltip("Distância para perder o player")]
        public float loseTargetDistance = 15f;

        [Tooltip("Campo de visão em graus")]
        [Range(0f, 360f)]
        public float fieldOfView = 120f;

        [Header("Audio")]
        public string detectedSound = "enemy_detected";
        public string attackSound = "enemy_attack";
        public string deathSound = "enemy_death";
        public string idleSound = "enemy_idle";

        [Header("Behavior")]
        [Tooltip("Tempo investigando última posição conhecida")]
        public float investigateTime = 5f;

        [Tooltip("Pode patrulhar")]
        public bool canPatrol = true;

        [Tooltip("Agressividade (0 = defensivo, 1 = muito agressivo)")]
        [Range(0f, 1f)]
        public float aggression = 0.5f;
    }
}
