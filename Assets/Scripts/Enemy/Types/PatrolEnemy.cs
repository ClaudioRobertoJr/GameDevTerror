using UnityEngine;
using HorrorGame.Player;

namespace HorrorGame.Enemy
{
    /// <summary>
    /// Inimigo que patrulha entre waypoints e persegue o player quando detectado
    /// </summary>
    public class PatrolEnemy : EnemyBase
    {
        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float waypointWaitTime = 2f;
        [SerializeField] private bool loopPatrol = true;

        [Header("AI Settings")]
        [SerializeField] private float investigateTime = 5f;

        private EnemyAIState currentState = EnemyAIState.Patrol;
        private int currentPatrolIndex = 0;
        private float waypointTimer = 0f;
        private Vector3 lastKnownPlayerPosition;
        private float investigateTimer = 0f;

        protected override void Start()
        {
            base.Start();

            if (patrolPoints.Length > 0)
            {
                ChangeState(EnemyAIState.Patrol);
            }
            else
            {
                ChangeState(EnemyAIState.Idle);
            }
        }

        protected override void UpdateAI()
        {
            switch (currentState)
            {
                case EnemyAIState.Idle:
                    UpdateIdle();
                    break;

                case EnemyAIState.Patrol:
                    UpdatePatrol();
                    break;

                case EnemyAIState.Investigate:
                    UpdateInvestigate();
                    break;

                case EnemyAIState.Chase:
                    UpdateChase();
                    break;

                case EnemyAIState.Attack:
                    UpdateAttackState();
                    break;
            }

            // Sempre verifica detecção (exceto se morto)
            if (currentState != EnemyAIState.Dead && currentState != EnemyAIState.Attack)
            {
                if (DetectPlayer())
                {
                    if (currentState != EnemyAIState.Chase && currentState != EnemyAIState.Attack)
                    {
                        ChangeState(EnemyAIState.Chase);
                    }
                }
            }
        }

        #region State Machine

        private void ChangeState(EnemyAIState newState)
        {
            if (currentState == newState) return;

            OnStateExit(currentState);
            currentState = newState;
            OnStateEnter(newState);
        }

        private void OnStateEnter(EnemyAIState state)
        {
            switch (state)
            {
                case EnemyAIState.Idle:
                    navAgent.isStopped = true;
                    break;

                case EnemyAIState.Patrol:
                    navAgent.isStopped = false;
                    navAgent.speed = moveSpeed;
                    if (patrolPoints.Length > 0)
                    {
                        MoveToNextPatrolPoint();
                    }
                    break;

                case EnemyAIState.Investigate:
                    navAgent.isStopped = false;
                    navAgent.speed = moveSpeed;
                    navAgent.SetDestination(lastKnownPlayerPosition);
                    investigateTimer = investigateTime;
                    break;

                case EnemyAIState.Chase:
                    navAgent.isStopped = false;
                    navAgent.speed = chaseSpeed;
                    break;

                case EnemyAIState.Attack:
                    navAgent.isStopped = true;
                    break;
            }

            Debug.Log($"[PatrolEnemy] State changed to: {state}");
        }

        private void OnStateExit(EnemyAIState state)
        {
            // Limpeza ao sair de estados
        }

        #endregion

        #region State Updates

        private void UpdateIdle()
        {
            // Apenas espera, detecção é feita no UpdateAI
        }

        private void UpdatePatrol()
        {
            if (patrolPoints.Length == 0)
            {
                ChangeState(EnemyAIState.Idle);
                return;
            }

            // Verifica se chegou no waypoint
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                waypointTimer += Time.deltaTime;

                if (waypointTimer >= waypointWaitTime)
                {
                    MoveToNextPatrolPoint();
                }
            }
        }

        private void UpdateInvestigate()
        {
            investigateTimer -= Time.deltaTime;

            // Verifica se chegou na última posição conhecida
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                // Espera um tempo investigando
                if (investigateTimer <= 0f)
                {
                    // Volta a patrulhar
                    ChangeState(EnemyAIState.Patrol);
                }
            }
        }

        private void UpdateChase()
        {
            if (target == null)
            {
                // Perdeu o alvo, vai investigar última posição conhecida
                ChangeState(EnemyAIState.Investigate);
                return;
            }

            // Verifica se perdeu o alvo
            if (HasLostTarget())
            {
                lastKnownPlayerPosition = target.position;
                LoseTarget();
                ChangeState(EnemyAIState.Investigate);
                return;
            }

            // Atualiza destino para posição do player
            navAgent.SetDestination(target.position);
            lastKnownPlayerPosition = target.position;

            // Verifica se está no alcance de ataque
            if (IsInAttackRange())
            {
                ChangeState(EnemyAIState.Attack);
            }
        }

        private void UpdateAttackState()
        {
            if (target == null)
            {
                ChangeState(EnemyAIState.Investigate);
                return;
            }

            // Olha para o alvo
            Vector3 lookDirection = (target.position - transform.position).normalized;
            lookDirection.y = 0f;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDirection),
                    Time.deltaTime * 5f
                );
            }

            // Verifica se ainda está no alcance
            if (!IsInAttackRange())
            {
                ChangeState(EnemyAIState.Chase);
                return;
            }

            // Ataca
            Attack();
        }

        #endregion

        #region Patrol

        private void MoveToNextPatrolPoint()
        {
            if (patrolPoints.Length == 0) return;

            navAgent.SetDestination(patrolPoints[currentPatrolIndex].position);
            waypointTimer = 0f;

            currentPatrolIndex++;

            if (currentPatrolIndex >= patrolPoints.Length)
            {
                if (loopPatrol)
                {
                    currentPatrolIndex = 0;
                }
                else
                {
                    currentPatrolIndex = patrolPoints.Length - 1;
                }
            }
        }

        #endregion

        #region Combat Override

        protected override void OnAttack()
        {
            if (target == null) return;

            // Aplica dano ao player
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                Debug.Log($"[PatrolEnemy] Attacked player for {damage} damage");
            }
        }

        protected override void OnPlayerDetected()
        {
            base.OnPlayerDetected();
            lastKnownPlayerPosition = target.position;
        }

        protected override void OnDamageTaken(float damage, Vector3 damageSource)
        {
            base.OnDamageTaken(damage, damageSource);

            // Se não estava perseguindo, agora vai
            if (currentState != EnemyAIState.Chase && currentState != EnemyAIState.Attack)
            {
                // Tenta encontrar quem atacou
                if (DetectPlayer())
                {
                    ChangeState(EnemyAIState.Chase);
                }
            }
        }

        #endregion

        #region Gizmos

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (patrolPoints == null || patrolPoints.Length == 0) return;

            // Desenha rota de patrulha
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;

                Vector3 currentPos = patrolPoints[i].position;
                Gizmos.DrawWireSphere(currentPos, 0.5f);

                // Desenha linha para o próximo ponto
                if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(currentPos, patrolPoints[i + 1].position);
                }
                else if (loopPatrol && patrolPoints[0] != null)
                {
                    Gizmos.DrawLine(currentPos, patrolPoints[0].position);
                }
            }
        }

        #endregion
    }
}
