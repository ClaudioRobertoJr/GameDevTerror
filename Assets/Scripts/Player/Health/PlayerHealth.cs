using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    /// <summary>
    /// Sistema de saúde do player
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Regeneration")]
        [SerializeField] private bool canRegenerate = false;
        [SerializeField] private float regenRate = 5f;
        [SerializeField] private float regenDelay = 5f;

        [Header("Damage Settings")]
        [SerializeField] private float damageInvulnerabilityDuration = 0.5f;
        [SerializeField] private bool canDieFromFalling = true;
        [SerializeField] private float fallDamageThreshold = 10f;

        [Header("Death Settings")]
        [SerializeField] private float deathRespawnDelay = 3f;

        private float regenTimer;
        private float invulnerabilityTimer;
        private bool isDead = false;

        // Referencias
        private MouseLook mouseLook;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsDead => isDead;
        public float HealthPercent => currentHealth / maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
            mouseLook = GetComponent<MouseLook>();
        }

        private void Start()
        {
            // Notifica UI da vida inicial
            GameEvents.OnPlayerHealed?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (isDead) return;

            UpdateInvulnerability();
            UpdateRegeneration();
        }

        private void UpdateInvulnerability()
        {
            if (invulnerabilityTimer > 0f)
            {
                invulnerabilityTimer -= Time.deltaTime;
            }
        }

        private void UpdateRegeneration()
        {
            if (!canRegenerate || currentHealth >= maxHealth) return;

            if (regenTimer > 0f)
            {
                regenTimer -= Time.deltaTime;
            }
            else
            {
                Heal(regenRate * Time.deltaTime);
            }
        }

        #region Damage System

        /// <summary>
        /// Aplica dano ao player
        /// </summary>
        public void TakeDamage(float damage, Vector3 damageSource = default)
        {
            if (isDead || invulnerabilityTimer > 0f) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0f);

            // Reset regen timer
            regenTimer = regenDelay;

            // Ativa invulnerabilidade temporária
            invulnerabilityTimer = damageInvulnerabilityDuration;

            // Shake na câmera
            if (mouseLook != null)
            {
                float shakeIntensity = Mathf.Clamp(damage / 50f, 0.1f, 0.5f);
                mouseLook.ShakeCamera(shakeIntensity, 0.2f);
            }

            // Notifica eventos
            GameEvents.OnPlayerDamaged?.Invoke(damage, currentHealth);

            Debug.Log($"[PlayerHealth] Took {damage} damage. Current health: {currentHealth}/{maxHealth}");

            // Verifica morte
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Dano de queda
        /// </summary>
        public void TakeFallDamage(float fallDistance)
        {
            if (!canDieFromFalling || fallDistance < fallDamageThreshold) return;

            float damage = (fallDistance - fallDamageThreshold) * 10f;
            TakeDamage(damage);
        }

        #endregion

        #region Healing System

        /// <summary>
        /// Cura o player
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            GameEvents.OnPlayerHealed?.Invoke(amount, currentHealth);

            Debug.Log($"[PlayerHealth] Healed {amount}. Current health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Restaura vida completamente
        /// </summary>
        public void FullHeal()
        {
            Heal(maxHealth - currentHealth);
        }

        #endregion

        #region Death System

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            currentHealth = 0f;

            Debug.Log("[PlayerHealth] Player died");

            // Notifica morte
            GameEvents.OnPlayerDied?.Invoke();

            // Game over
            GameManager.Instance?.GameOver();
        }

        /// <summary>
        /// Revive o player (útil para checkpoints)
        /// </summary>
        public void Revive(Vector3 respawnPosition)
        {
            isDead = false;
            currentHealth = maxHealth;

            // Teleporta para posição de respawn
            FirstPersonController controller = GetComponent<FirstPersonController>();
            if (controller != null)
            {
                controller.Teleport(respawnPosition);
            }

            GameEvents.OnPlayerHealed?.Invoke(currentHealth, maxHealth);

            Debug.Log("[PlayerHealth] Player revived");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Define a vida máxima
        /// </summary>
        public void SetMaxHealth(float newMaxHealth)
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        /// <summary>
        /// Verifica se pode tomar dano
        /// </summary>
        public bool CanTakeDamage()
        {
            return !isDead && invulnerabilityTimer <= 0f;
        }

        #endregion
    }
}
