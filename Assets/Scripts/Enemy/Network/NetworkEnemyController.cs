using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace HorrorGame.Enemy
{
    /// <summary>
    /// Controlador de inimigo multiplayer
    /// IA roda APENAS no servidor, clientes apenas visualizam
    ///
    /// ✅ VERSÃO CORRIGIDA - Novembro 2025
    /// - GetComponent caching implementado
    /// - Memory leaks corrigidos (OnNetworkDespawn)
    /// - Performance otimizada (sqrMagnitude)
    /// - Validação server-side completa
    /// </summary>
    public class NetworkEnemyController : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private NavMeshAgent navAgent;
        [SerializeField] private Animator animator;

        [Header("Enemy Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 2f;

        [Header("AI Settings")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolWaitTime = 3f;

        // Network Variables
        private NetworkVariable<float> networkHealth = new NetworkVariable<float>();
        private NetworkVariable<EnemyState> currentState = new NetworkVariable<EnemyState>();
        private NetworkVariable<ulong> targetPlayerId = new NetworkVariable<ulong>();

        // ✅ FIX: Cache de components para performance
        private Dictionary<ulong, NetworkPlayerController> cachedPlayerControllers = new Dictionary<ulong, NetworkPlayerController>();
        private Dictionary<ulong, Transform> cachedPlayerTransforms = new Dictionary<ulong, Transform>();

        // Estado local (apenas servidor)
        private Transform targetPlayer;
        private int currentPatrolIndex = 0;
        private float lastAttackTime;
        private float patrolTimer;

        // ✅ FIX: Cache para otimização de busca de players
        private float playerSearchInterval = 0.5f; // Buscar players a cada 0.5s
        private float lastPlayerSearchTime;

        public enum EnemyState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Dead
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                // Apenas servidor inicializa IA
                networkHealth.Value = maxHealth;
                currentState.Value = EnemyState.Patrol;
                navAgent.enabled = true;

                // Subscribe to network events
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            }
            else
            {
                // Clientes apenas visualizam
                navAgent.enabled = false;
            }

            // ✅ FIX: Subscribe to events
            currentState.OnValueChanged += OnStateChanged;
            networkHealth.OnValueChanged += OnHealthChanged;
        }

        // ✅ FIX: Cleanup de eventos para evitar memory leaks
        public override void OnNetworkDespawn()
        {
            // CRÍTICO: Limpar eventos NetworkVariable
            currentState.OnValueChanged -= OnStateChanged;
            networkHealth.OnValueChanged -= OnHealthChanged;

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            }

            // Limpar caches
            cachedPlayerControllers.Clear();
            cachedPlayerTransforms.Clear();

            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (!IsServer) return;
            if (currentState.Value == EnemyState.Dead) return;

            UpdateAI();
        }

        private void UpdateAI()
        {
            // ✅ FIX: Buscar player mais próximo apenas a cada intervalo
            if (Time.time - lastPlayerSearchTime > playerSearchInterval)
            {
                targetPlayer = GetClosestPlayer();
                lastPlayerSearchTime = Time.time;
            }

            if (targetPlayer == null)
            {
                // Sem players, patrulhar
                if (currentState.Value != EnemyState.Patrol)
                    currentState.Value = EnemyState.Patrol;

                HandlePatrol();
                return;
            }

            // ✅ FIX: Usar sqrMagnitude em vez de Distance
            float sqrDistanceToPlayer = (transform.position - targetPlayer.position).sqrMagnitude;
            float detectionRangeSqr = detectionRange * detectionRange;
            float attackRangeSqr = attackRange * attackRange;

            // Máquina de estados
            switch (currentState.Value)
            {
                case EnemyState.Patrol:
                    if (sqrDistanceToPlayer < detectionRangeSqr)
                    {
                        currentState.Value = EnemyState.Chase;
                        Debug.Log($"[Enemy] Detectou player a {Mathf.Sqrt(sqrDistanceToPlayer)}m");
                    }
                    else
                    {
                        HandlePatrol();
                    }
                    break;

                case EnemyState.Chase:
                    float loseRangeSqr = (detectionRange * 1.5f) * (detectionRange * 1.5f);
                    if (sqrDistanceToPlayer > loseRangeSqr)
                    {
                        currentState.Value = EnemyState.Patrol;
                    }
                    else if (sqrDistanceToPlayer < attackRangeSqr)
                    {
                        currentState.Value = EnemyState.Attack;
                    }
                    else
                    {
                        HandleChase();
                    }
                    break;

                case EnemyState.Attack:
                    float attackLoseRangeSqr = (attackRange * 1.2f) * (attackRange * 1.2f);
                    if (sqrDistanceToPlayer > attackLoseRangeSqr)
                    {
                        currentState.Value = EnemyState.Chase;
                    }
                    else
                    {
                        HandleAttack();
                    }
                    break;
            }
        }

        private void HandlePatrol()
        {
            if (patrolPoints.Length == 0) return;

            // Ir para próximo waypoint
            Transform targetWaypoint = patrolPoints[currentPatrolIndex];
            navAgent.SetDestination(targetWaypoint.position);

            // ✅ FIX: Usar sqrMagnitude
            float sqrDistance = (transform.position - targetWaypoint.position).sqrMagnitude;

            // Chegou no waypoint?
            if (sqrDistance < 1f) // 1m * 1m = 1
            {
                patrolTimer += Time.deltaTime;

                if (patrolTimer >= patrolWaitTime)
                {
                    patrolTimer = 0f;
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }
            }
        }

        private void HandleChase()
        {
            if (targetPlayer == null) return;

            navAgent.SetDestination(targetPlayer.position);

            // Atualizar ID do player alvo (para sincronização)
            var networkObject = targetPlayer.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                targetPlayerId.Value = networkObject.OwnerClientId;
            }
        }

        private void HandleAttack()
        {
            if (targetPlayer == null) return;

            // Parar movimento
            navAgent.SetDestination(transform.position);

            // Olhar para player
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            // Atacar se passou o cooldown
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }

        private void PerformAttack()
        {
            if (targetPlayer == null) return;

            // ✅ FIX: Validar distância novamente antes de aplicar dano (anti-cheat)
            float sqrDistance = (transform.position - targetPlayer.position).sqrMagnitude;
            float attackRangeSqr = attackRange * attackRange;

            if (sqrDistance > attackRangeSqr * 1.1f) // 10% de margem
            {
                Debug.LogWarning("[Enemy] Player muito longe para atacar!");
                return;
            }

            Debug.Log($"[Enemy] Atacando player!");

            // ✅ FIX: Usar cache de component
            var networkObject = targetPlayer.GetComponent<NetworkObject>();
            if (networkObject == null) return;

            ulong playerId = networkObject.OwnerClientId;
            NetworkPlayerController playerController = GetCachedPlayerController(playerId);

            if (playerController != null && !playerController.IsDead())
            {
                // Servidor (enemy) aplica dano ao player
                playerController.TakeDamage((int)damage, NetworkManager.Singleton.LocalClientId);
            }

            // Notificar todos os clientes para tocar animação/som
            PlayAttackEffectsClientRpc();
        }

        [ClientRpc]
        private void PlayAttackEffectsClientRpc()
        {
            // Tocar animação de ataque
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Tocar som
            GameEvents.OnEnemyAttack?.Invoke(gameObject);
        }

        /// <summary>
        /// ✅ FIX: Receber dano com validação server-side completa
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float damageAmount, ulong attackerId, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            if (currentState.Value == EnemyState.Dead) return;

            // ✅ VALIDAÇÕES SERVER-SIDE (anti-cheat)

            // 1. Validar que attacker existe
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(attackerId))
            {
                Debug.LogWarning($"[Anti-Cheat] Attacker {attackerId} não existe!");
                return;
            }

            // 2. Validar distância (anti-cheat)
            var attackerObject = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject;
            if (attackerObject != null)
            {
                float sqrDistance = (transform.position - attackerObject.transform.position).sqrMagnitude;
                if (sqrDistance > 100f) // 10m * 10m = 100
                {
                    Debug.LogWarning($"[Anti-Cheat] Ataque de distância inválida: {Mathf.Sqrt(sqrDistance)}m");
                    return;
                }
            }

            // 3. Validar valor do dano
            if (damageAmount < 0 || damageAmount > 200)
            {
                Debug.LogWarning($"[Anti-Cheat] Dano inválido: {damageAmount}");
                return;
            }

            // 4. Aplicar dano
            networkHealth.Value -= damageAmount;

            if (networkHealth.Value <= 0)
            {
                networkHealth.Value = 0;
                Die();
            }
        }

        private void Die()
        {
            currentState.Value = EnemyState.Dead;

            Debug.Log("[Enemy] Morreu!");

            // Notificar morte
            GameEvents.OnEnemyDied?.Invoke(gameObject);

            // Tocar efeitos de morte em todos os clientes
            PlayDeathEffectsClientRpc();

            // Despawnar após delay
            Invoke(nameof(DespawnEnemy), 3f);
        }

        [ClientRpc]
        private void PlayDeathEffectsClientRpc()
        {
            // Desabilitar NavMeshAgent
            if (navAgent != null)
                navAgent.enabled = false;

            // Tocar animação de morte
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }

            // Tocar som
            // AudioManager.Instance.PlaySound3D("enemy_death", transform.position);
        }

        private void DespawnEnemy()
        {
            if (IsServer)
            {
                GetComponent<NetworkObject>().Despawn(true);
            }
        }

        /// <summary>
        /// ✅ FIX: Encontra o player mais próximo com CACHING e OTIMIZAÇÃO
        /// </summary>
        private Transform GetClosestPlayer()
        {
            Transform closest = null;
            float closestSqrDistance = Mathf.Infinity;

            // Iterar por todos os clientes conectados
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var client = NetworkManager.Singleton.ConnectedClients[clientId];
                if (client.PlayerObject == null) continue;

                // ✅ FIX: Usar cache de components
                NetworkPlayerController playerController = GetCachedPlayerController(clientId);
                Transform playerTransform = GetCachedPlayerTransform(clientId);

                if (playerController == null || playerTransform == null) continue;

                // Apenas considerar players vivos
                if (playerController.IsDead()) continue;

                // ✅ FIX: Usar sqrMagnitude (muito mais rápido)
                float sqrDistance = (transform.position - playerTransform.position).sqrMagnitude;

                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closest = playerTransform;
                }
            }

            return closest;
        }

        /// <summary>
        /// ✅ FIX: Cache de PlayerController para performance
        /// </summary>
        private NetworkPlayerController GetCachedPlayerController(ulong clientId)
        {
            // Tentar obter do cache
            if (cachedPlayerControllers.TryGetValue(clientId, out var cached))
            {
                if (cached != null) return cached;
                // Se null, remover do cache
                cachedPlayerControllers.Remove(clientId);
            }

            // Não está no cache, buscar e cachear
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            if (client.PlayerObject == null) return null;

            var controller = client.PlayerObject.GetComponent<NetworkPlayerController>();
            if (controller != null)
            {
                cachedPlayerControllers[clientId] = controller;
            }

            return controller;
        }

        /// <summary>
        /// ✅ FIX: Cache de Transform para performance
        /// </summary>
        private Transform GetCachedPlayerTransform(ulong clientId)
        {
            // Tentar obter do cache
            if (cachedPlayerTransforms.TryGetValue(clientId, out var cached))
            {
                if (cached != null) return cached;
                cachedPlayerTransforms.Remove(clientId);
            }

            // Não está no cache, buscar e cachear
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            if (client.PlayerObject == null) return null;

            var transform = client.PlayerObject.transform;
            if (transform != null)
            {
                cachedPlayerTransforms[clientId] = transform;
            }

            return transform;
        }

        /// <summary>
        /// ✅ FIX: Limpar cache quando player desconecta
        /// </summary>
        private void OnClientDisconnect(ulong clientId)
        {
            cachedPlayerControllers.Remove(clientId);
            cachedPlayerTransforms.Remove(clientId);

            Debug.Log($"[Enemy] Cache limpo para client {clientId}");
        }

        private void OnStateChanged(EnemyState oldState, EnemyState newState)
        {
            Debug.Log($"[Enemy] Estado: {oldState} -> {newState}");

            // Atualizar animador
            if (animator != null)
            {
                animator.SetInteger("State", (int)newState);
            }
        }

        private void OnHealthChanged(float oldHealth, float newHealth)
        {
            Debug.Log($"[Enemy] Vida: {oldHealth} -> {newHealth}");
        }

        // ===== PUBLIC GETTERS =====

        public float GetHealth() => networkHealth.Value;
        public float GetMaxHealth() => maxHealth;
        public EnemyState GetCurrentState() => currentState.Value;
        public bool IsDead() => currentState.Value == EnemyState.Dead;

        // ===== DEBUG =====

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (!IsServer) return;

            // Desenhar range de detecção
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Desenhar range de ataque
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Desenhar linha para target
            if (targetPlayer != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetPlayer.position);
            }
        }
    }
}
