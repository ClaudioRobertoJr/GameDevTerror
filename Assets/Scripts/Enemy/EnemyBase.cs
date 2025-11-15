using UnityEngine;
using UnityEngine.AI;
using HorrorGame.Core;

namespace HorrorGame.Enemy
{
    /// <summary>
    /// Classe base para todos os inimigos do jogo
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float attackCooldown = 1.5f;

        [Header("Movement")]
        [SerializeField] protected float moveSpeed = 3.5f;
        [SerializeField] protected float chaseSpeed = 5f;

        [Header("Detection")]
        [SerializeField] protected float detectionRange = 10f;
        [SerializeField] protected float loseTargetDistance = 15f;
        [SerializeField] protected float fieldOfView = 120f;
        [SerializeField] protected LayerMask detectionLayers;

        [Header("Audio")]
        [SerializeField] protected string detectedSound = "enemy_detected";
        [SerializeField] protected string attackSound = "enemy_attack";
        [SerializeField] protected string deathSound = "enemy_death";

        // Components
        protected NavMeshAgent navAgent;
        protected Animator animator;

        // State
        protected float currentHealth;
        protected Transform target;
        protected bool isDead = false;
        protected float lastAttackTime;

        // Properties
        public bool IsDead => isDead;
        public Transform Target => target;
        public float HealthPercent => currentHealth / maxHealth;

        protected virtual void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            currentHealth = maxHealth;
            navAgent.speed = moveSpeed;
        }

        protected virtual void Start()
        {
            GameEvents.OnEnemySpawned?.Invoke(gameObject);
        }

        protected virtual void Update()
        {
            if (isDead) return;

            UpdateAI();
        }

        /// <summary>
        /// Atualiza a lógica de IA do inimigo (implementado por subclasses)
        /// </summary>
        protected abstract void UpdateAI();

        #region Combat

        /// <summary>
        /// Aplica dano ao inimigo
        /// </summary>
        public virtual void TakeDamage(float damageAmount, Vector3 damageSource = default)
        {
            if (isDead) return;

            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(currentHealth, 0f);

            OnDamageTaken(damageAmount, damageSource);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Chamado quando o inimigo recebe dano
        /// </summary>
        protected virtual void OnDamageTaken(float damage, Vector3 damageSource)
        {
            // Implementado por subclasses para reações específicas
            Debug.Log($"[Enemy] {name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        }

        /// <summary>
        /// Ataca o alvo
        /// </summary>
        protected virtual void Attack()
        {
            if (Time.time < lastAttackTime + attackCooldown) return;

            lastAttackTime = Time.time;

            OnAttack();

            // Toca som de ataque
            if (!string.IsNullOrEmpty(attackSound))
            {
                GameEvents.OnPlaySound3D?.Invoke(attackSound, transform.position);
            }

            GameEvents.OnEnemyAttack?.Invoke(gameObject);
        }

        /// <summary>
        /// Lógica de ataque (implementado por subclasses)
        /// </summary>
        protected abstract void OnAttack();

        /// <summary>
        /// Verifica se está no alcance de ataque
        /// </summary>
        protected bool IsInAttackRange()
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.position) <= attackRange;
        }

        #endregion

        #region Detection

        /// <summary>
        /// Detecta o player
        /// </summary>
        protected bool DetectPlayer()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, detectionLayers);

            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    // Verifica campo de visão
                    Vector3 directionToPlayer = (col.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToPlayer);

                    if (angle < fieldOfView / 2f)
                    {
                        // Raycast para verificar se há obstáculos
                        if (HasLineOfSight(col.transform))
                        {
                            SetTarget(col.transform);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica linha de visão para o alvo
        /// </summary>
        protected bool HasLineOfSight(Transform targetTransform)
        {
            Vector3 direction = targetTransform.position - transform.position;
            RaycastHit hit;

            if (Physics.Raycast(transform.position + Vector3.up, direction.normalized, out hit, detectionRange))
            {
                return hit.transform == targetTransform || hit.transform.CompareTag("Player");
            }

            return false;
        }

        /// <summary>
        /// Define o alvo
        /// </summary>
        protected virtual void SetTarget(Transform newTarget)
        {
            if (target == null && newTarget != null)
            {
                // Detectou pela primeira vez
                OnPlayerDetected();
            }

            target = newTarget;
        }

        /// <summary>
        /// Chamado quando detecta o player pela primeira vez
        /// </summary>
        protected virtual void OnPlayerDetected()
        {
            if (!string.IsNullOrEmpty(detectedSound))
            {
                GameEvents.OnPlaySound3D?.Invoke(detectedSound, transform.position);
            }

            GameEvents.OnEnemyDetectedPlayer?.Invoke(gameObject);

            Debug.Log($"[Enemy] {name} detected player!");
        }

        /// <summary>
        /// Verifica se perdeu o alvo
        /// </summary>
        protected bool HasLostTarget()
        {
            if (target == null) return true;

            float distance = Vector3.Distance(transform.position, target.position);
            return distance > loseTargetDistance;
        }

        /// <summary>
        /// Perde o alvo atual
        /// </summary>
        protected virtual void LoseTarget()
        {
            target = null;
            GameEvents.OnEnemyLostPlayer?.Invoke(gameObject);
            Debug.Log($"[Enemy] {name} lost player");
        }

        #endregion

        #region Death

        /// <summary>
        /// Mata o inimigo
        /// </summary>
        protected virtual void Die()
        {
            if (isDead) return;

            isDead = true;

            // Para movimento
            if (navAgent != null)
            {
                navAgent.isStopped = true;
                navAgent.enabled = false;
            }

            // Toca som de morte
            if (!string.IsNullOrEmpty(deathSound))
            {
                GameEvents.OnPlaySound3D?.Invoke(deathSound, transform.position);
            }

            GameEvents.OnEnemyDied?.Invoke(gameObject);

            Debug.Log($"[Enemy] {name} died");

            OnDeath();

            // Destroy após delay
            Destroy(gameObject, 3f);
        }

        /// <summary>
        /// Chamado quando o inimigo morre (implementado por subclasses)
        /// </summary>
        protected virtual void OnDeath()
        {
            // Implementado por subclasses
        }

        #endregion

        #region Gizmos

        protected virtual void OnDrawGizmosSelected()
        {
            // Desenha alcance de detecção
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Desenha alcance de ataque
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Desenha campo de visão
            Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfView / 2f, transform.up) * transform.forward * detectionRange;
            Vector3 fovLine2 = Quaternion.AngleAxis(-fieldOfView / 2f, transform.up) * transform.forward * detectionRange;

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, fovLine1);
            Gizmos.DrawRay(transform.position, fovLine2);
        }

        #endregion
    }
}
