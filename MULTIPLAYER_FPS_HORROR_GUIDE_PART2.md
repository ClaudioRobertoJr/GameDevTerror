# 🎮 Guia Completo: FPS Horror Multiplayer - PARTE 2

## Continuação de MULTIPLAYER_FPS_HORROR_GUIDE.md

---

## 🏃 Player Controller Multiplayer com New Input System

### Arquitetura do Network Player

```
NetworkPlayer
├── NetworkObject (Netcode)
├── NetworkTransform (posição sync)
├── NetworkAnimator (animações sync)
├── CharacterController (física)
├── PlayerInputHandler (New Input System)
├── NetworkPlayerController (lógica multiplayer)
├── FirstPersonController (movimento)
├── PlayerHealth (vida)
└── Camera (apenas local player)
```

### Implementação Completa

```csharp
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorGame.Player
{
    /// <summary>
    /// Controlador de player multiplayer usando New Input System
    /// Suporta 4-5 jogadores simultâneos
    /// </summary>
    public class NetworkPlayerController : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListener;

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float crouchSpeed = 1.5f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float gravity = -19.62f;

        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;

        [Header("Network Sync")]
        private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
        private NetworkVariable<int> networkHealth = new NetworkVariable<int>(100);
        private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>();

        // Estado local
        private Vector3 velocity;
        private float xRotation = 0f;
        private bool isGrounded;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                // Este é o player local
                SetupLocalPlayer();
            }
            else
            {
                // Este é um player remoto
                SetupRemotePlayer();
            }

            // Setup nome
            if (IsOwner)
            {
                SetPlayerNameServerRpc(AuthenticationManager.Instance.PlayerName);
            }

            // Eventos
            networkHealth.OnValueChanged += OnHealthChanged;
        }

        private void SetupLocalPlayer()
        {
            Debug.Log("[NetworkPlayer] Configurando player local");

            // Habilitar input
            if (inputHandler != null)
                inputHandler.enabled = true;

            // Habilitar câmera
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                playerCamera.tag = "MainCamera";
            }

            // Habilitar audio listener
            if (audioListener != null)
                audioListener.enabled = true;

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Layer para não renderizar próprio modelo (opcional)
            // SetLayerRecursively(transform, LayerMask.NameToLayer("LocalPlayer"));
        }

        private void SetupRemotePlayer()
        {
            Debug.Log("[NetworkPlayer] Configurando player remoto");

            // Desabilitar input
            if (inputHandler != null)
                inputHandler.enabled = false;

            // Desabilitar câmera
            if (playerCamera != null)
                playerCamera.enabled = false;

            // Desabilitar audio listener
            if (audioListener != null)
                audioListener.enabled = false;

            // Desabilitar CharacterController (apenas visual)
            if (characterController != null)
                characterController.enabled = false;
        }

        private void Update()
        {
            if (IsOwner)
            {
                HandleLocalPlayer();
            }
            else
            {
                HandleRemotePlayer();
            }
        }

        private void HandleLocalPlayer()
        {
            // Ground check
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Input
            Vector2 moveInput = inputHandler.MoveInput;
            Vector2 lookInput = inputHandler.LookInput;

            // Movimento
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

            float currentSpeed = walkSpeed;
            if (inputHandler.SprintHeld)
                currentSpeed = sprintSpeed;

            characterController.Move(move * currentSpeed * Time.deltaTime);

            // Pulo
            if (inputHandler.JumpPressed && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            // Gravidade
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);

            // Rotação da câmera (mouse look)
            float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);

            // Sincronizar posição/rotação com servidor
            if (Vector3.Distance(transform.position, networkPosition.Value) > 0.1f)
            {
                UpdatePositionServerRpc(transform.position, transform.rotation);
            }
        }

        private void HandleRemotePlayer()
        {
            // Interpolação suave de posição
            transform.position = Vector3.Lerp(
                transform.position,
                networkPosition.Value,
                Time.deltaTime * 15f
            );

            // Interpolação suave de rotação
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                networkRotation.Value,
                Time.deltaTime * 15f
            );
        }

        [ServerRpc]
        private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
        {
            networkPosition.Value = position;
            networkRotation.Value = rotation;
        }

        [ServerRpc]
        private void SetPlayerNameServerRpc(string name)
        {
            playerName.Value = name;
        }

        /// <summary>
        /// Receber dano (validado pelo servidor)
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (!IsOwner) return;
            TakeDamageServerRpc(damage);
        }

        [ServerRpc]
        private void TakeDamageServerRpc(int damage)
        {
            networkHealth.Value -= damage;
            networkHealth.Value = Mathf.Max(0, networkHealth.Value);

            if (networkHealth.Value <= 0)
            {
                HandlePlayerDeathClientRpc();
            }
        }

        [ClientRpc]
        private void HandlePlayerDeathClientRpc()
        {
            Debug.Log($"[NetworkPlayer] {playerName.Value} morreu!");

            if (IsOwner)
            {
                // Desabilitar controles
                inputHandler.enabled = false;

                // Mostrar tela de morte
                GameEvents.OnPlayerDied?.Invoke();
            }

            // Tocar animação/efeitos de morte
            PlayDeathEffects();
        }

        private void OnHealthChanged(int oldHealth, int newHealth)
        {
            if (IsOwner)
            {
                // Atualizar HUD
                GameEvents.OnPlayerDamaged?.Invoke(oldHealth - newHealth, newHealth);
            }
        }

        private void PlayDeathEffects()
        {
            // Implementar efeitos visuais/sonoros
        }

        public int GetHealth() => networkHealth.Value;
        public string GetPlayerName() => playerName.Value.ToString();
    }
}
```

---

## 👾 Sistema de Inimigos em Rede

### Arquitetura

```
NetworkEnemy
├── NetworkObject
├── NetworkTransform (posição sync)
├── NetworkAnimator (animações sync)
├── NavMeshAgent (IA pathfinding)
├── EnemyAI (lógica IA - APENAS SERVIDOR)
├── NetworkEnemyController (sync multiplayer)
└── EnemyStats (vida, dano, etc)
```

### Implementação

```csharp
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorGame.Enemy
{
    /// <summary>
    /// Controlador de inimigo multiplayer
    /// IA roda APENAS no servidor, clientes apenas visualizam
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

        // Estado local (apenas servidor)
        private Transform targetPlayer;
        private int currentPatrolIndex = 0;
        private float lastAttackTime;
        private float patrolTimer;

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
            }
            else
            {
                // Clientes apenas visualizam
                navAgent.enabled = false;
            }

            // Todos ouvem mudanças de estado
            currentState.OnValueChanged += OnStateChanged;
            networkHealth.OnValueChanged += OnHealthChanged;
        }

        private void Update()
        {
            if (!IsServer) return;
            if (currentState.Value == EnemyState.Dead) return;

            UpdateAI();
        }

        private void UpdateAI()
        {
            // Encontrar player mais próximo
            targetPlayer = GetClosestPlayer();

            if (targetPlayer == null)
            {
                // Sem players, patrulhar
                if (currentState.Value != EnemyState.Patrol)
                    currentState.Value = EnemyState.Patrol;

                HandlePatrol();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            // Máquina de estados
            switch (currentState.Value)
            {
                case EnemyState.Patrol:
                    if (distanceToPlayer < detectionRange)
                    {
                        currentState.Value = EnemyState.Chase;
                        Debug.Log($"[Enemy] Detectou player a {distanceToPlayer}m");
                    }
                    else
                    {
                        HandlePatrol();
                    }
                    break;

                case EnemyState.Chase:
                    if (distanceToPlayer > detectionRange * 1.5f)
                    {
                        currentState.Value = EnemyState.Patrol;
                    }
                    else if (distanceToPlayer < attackRange)
                    {
                        currentState.Value = EnemyState.Attack;
                    }
                    else
                    {
                        HandleChase();
                    }
                    break;

                case EnemyState.Attack:
                    if (distanceToPlayer > attackRange * 1.2f)
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

            // Chegou no waypoint?
            if (Vector3.Distance(transform.position, targetWaypoint.position) < 1f)
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

            Debug.Log($"[Enemy] Atacando player!");

            // Aplicar dano ao player
            var playerController = targetPlayer.GetComponent<NetworkPlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage((int)damage);
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
        /// Receber dano (apenas servidor pode chamar)
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float damageAmount, ulong attackerId)
        {
            if (!IsServer) return;
            if (currentState.Value == EnemyState.Dead) return;

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
        /// Encontra o player mais próximo que está vivo
        /// </summary>
        private Transform GetClosestPlayer()
        {
            Transform closest = null;
            float closestDistance = Mathf.Infinity;

            // Iterar por todos os clientes conectados
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var client = NetworkManager.Singleton.ConnectedClients[clientId];
                if (client.PlayerObject == null) continue;

                var playerController = client.PlayerObject.GetComponent<NetworkPlayerController>();
                if (playerController == null) continue;

                // Apenas considerar players vivos
                if (playerController.GetHealth() <= 0) continue;

                float distance = Vector3.Distance(transform.position, client.PlayerObject.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = client.PlayerObject.transform;
                }
            }

            return closest;
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
    }
}
```

---

## 🧠 IA Avançada para Multiplayer (GOAP)

### Por que GOAP em Multiplayer?

✅ **Adaptável** - Inimigos reagem a múltiplos jogadores dinamicamente
✅ **Imprevisível** - Cada partida é diferente
✅ **Coordenação** - Inimigos podem trabalhar em grupo
✅ **Emergente** - Comportamentos complexos surgem naturalmente

### Implementação GOAP Básica

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace HorrorGame.AI
{
    /// <summary>
    /// Sistema GOAP para inimigos inteligentes
    /// Baseado no repositório Unity-GOAP do Adam Myhre
    /// </summary>
    public class GOAPEnemyAI : MonoBehaviour
    {
        [Header("GOAP Settings")]
        [SerializeField] private List<GOAPAction> availableActions;
        [SerializeField] private List<GOAPGoal> availableGoals;

        private GOAPPlanner planner;
        private Queue<GOAPAction> currentPlan;
        private GOAPAction currentAction;
        private Dictionary<string, object> worldState;

        private void Start()
        {
            planner = new GOAPPlanner();
            worldState = new Dictionary<string, object>();

            SetupActions();
            SetupGoals();
        }

        private void Update()
        {
            UpdateWorldState();

            // Se não tem plano ou plano completou, criar novo
            if (currentPlan == null || currentPlan.Count == 0)
            {
                CreateNewPlan();
            }

            // Executar ação atual
            if (currentAction != null)
            {
                bool actionComplete = currentAction.Perform(gameObject);

                if (actionComplete)
                {
                    currentAction.OnActionComplete();
                    currentAction = null;

                    if (currentPlan.Count > 0)
                    {
                        currentAction = currentPlan.Dequeue();
                        currentAction.OnActionStart();
                    }
                }
            }
        }

        private void UpdateWorldState()
        {
            // Atualizar estado do mundo baseado em observações
            var closestPlayer = GetClosestPlayer();

            worldState["hasTarget"] = closestPlayer != null;
            worldState["targetDistance"] = closestPlayer != null ?
                Vector3.Distance(transform.position, closestPlayer.position) : 999f;
            worldState["playerVisible"] = CanSeePlayer(closestPlayer);
            worldState["healthLow"] = GetComponent<NetworkEnemyController>().GetHealth() < 30f;
            worldState["hasBackup"] = GetNearbyAllies().Count > 0;
        }

        private void CreateNewPlan()
        {
            // Escolher goal com maior prioridade
            GOAPGoal selectedGoal = SelectBestGoal();

            if (selectedGoal == null) return;

            // Criar plano para atingir goal
            currentPlan = planner.Plan(gameObject, availableActions, worldState, selectedGoal);

            if (currentPlan != null && currentPlan.Count > 0)
            {
                currentAction = currentPlan.Dequeue();
                currentAction.OnActionStart();

                Debug.Log($"[GOAP] Novo plano criado para goal: {selectedGoal.goalName}");
            }
        }

        private GOAPGoal SelectBestGoal()
        {
            GOAPGoal best = null;
            float highestPriority = 0f;

            foreach (var goal in availableGoals)
            {
                if (goal.IsGoalRelevant(worldState))
                {
                    float priority = goal.GetPriority(worldState);
                    if (priority > highestPriority)
                    {
                        highestPriority = priority;
                        best = goal;
                    }
                }
            }

            return best;
        }

        private void SetupActions()
        {
            availableActions = new List<GOAPAction>
            {
                new MoveToTargetAction(),
                new AttackTargetAction(),
                new FlankTargetAction(),
                new CallForBackupAction(),
                new HideAndAmbushAction(),
                new FleeToSafetyAction()
            };
        }

        private void SetupGoals()
        {
            availableGoals = new List<GOAPGoal>
            {
                new KillPlayerGoal { priority = 10f },
                new SurviveGoal { priority = 8f },
                new AmbushPlayerGoal { priority = 7f }
            };
        }

        // Métodos auxiliares
        private Transform GetClosestPlayer() { /* implementação */ return null; }
        private bool CanSeePlayer(Transform player) { /* implementação */ return false; }
        private List<GameObject> GetNearbyAllies() { /* implementação */ return new List<GameObject>(); }
    }

    // ===== GOAP Classes Base =====

    public abstract class GOAPAction
    {
        public string actionName;
        public float cost = 1f;
        public Dictionary<string, object> preconditions = new Dictionary<string, object>();
        public Dictionary<string, object> effects = new Dictionary<string, object>();

        public virtual void OnActionStart() { }
        public virtual void OnActionComplete() { }
        public abstract bool Perform(GameObject agent);
    }

    public abstract class GOAPGoal
    {
        public string goalName;
        public float priority;
        public Dictionary<string, object> desiredState = new Dictionary<string, object>();

        public virtual bool IsGoalRelevant(Dictionary<string, object> worldState) { return true; }
        public virtual float GetPriority(Dictionary<string, object> worldState) { return priority; }
    }

    public class GOAPPlanner
    {
        public Queue<GOAPAction> Plan(GameObject agent, List<GOAPAction> actions,
            Dictionary<string, object> worldState, GOAPGoal goal)
        {
            // Implementação de A* para encontrar sequência de ações
            // que leva do estado atual ao goal desejado

            // Simplificado para exemplo
            Queue<GOAPAction> plan = new Queue<GOAPAction>();

            foreach (var action in actions)
            {
                // Checar se ação é aplicável
                if (CheckPreconditions(action.preconditions, worldState))
                {
                    plan.Enqueue(action);
                }
            }

            return plan;
        }

        private bool CheckPreconditions(Dictionary<string, object> preconditions,
            Dictionary<string, object> worldState)
        {
            foreach (var kvp in preconditions)
            {
                if (!worldState.ContainsKey(kvp.Key)) return false;
                if (!worldState[kvp.Key].Equals(kvp.Value)) return false;
            }
            return true;
        }
    }

    // ===== Exemplos de Actions =====

    public class MoveToTargetAction : GOAPAction
    {
        public MoveToTargetAction()
        {
            actionName = "Move to Target";
            cost = 1f;
            preconditions["hasTarget"] = true;
            effects["atTarget"] = true;
        }

        public override bool Perform(GameObject agent)
        {
            // Mover em direção ao target usando NavMesh
            return false; // Return true quando chegou
        }
    }

    public class AttackTargetAction : GOAPAction
    {
        public AttackTargetAction()
        {
            actionName = "Attack Target";
            cost = 1f;
            preconditions["atTarget"] = true;
            effects["targetDamaged"] = true;
        }

        public override bool Perform(GameObject agent)
        {
            // Executar ataque
            return true;
        }
    }

    // ===== Exemplos de Goals =====

    public class KillPlayerGoal : GOAPGoal
    {
        public KillPlayerGoal()
        {
            goalName = "Kill Player";
            priority = 10f;
            desiredState["targetDead"] = true;
        }

        public override float GetPriority(Dictionary<string, object> worldState)
        {
            // Maior prioridade se player está perto
            if (worldState.ContainsKey("targetDistance"))
            {
                float distance = (float)worldState["targetDistance"];
                if (distance < 10f)
                    return priority + 5f;
            }
            return priority;
        }
    }
}
```

### Integração GOAP com Multiplayer

```csharp
using Unity.Netcode;

namespace HorrorGame.AI
{
    /// <summary>
    /// Enemy com GOAP que funciona em multiplayer
    /// IA roda APENAS no servidor
    /// </summary>
    public class NetworkGOAPEnemy : NetworkBehaviour
    {
        private GOAPEnemyAI goapAI;
        private NetworkEnemyController networkController;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Apenas servidor roda GOAP
                goapAI = gameObject.AddComponent<GOAPEnemyAI>();
                goapAI.enabled = true;
            }

            networkController = GetComponent<NetworkEnemyController>();
        }

        // GOAP pode chamar métodos do NetworkEnemyController
        // para sincronizar ações com clientes
    }
}
```

---

## 🎯 Sistema de Partidas (Match System)

### Flow Completo

```
1. Lobby → 2. Countdown → 3. Match Start → 4. Gameplay → 5. Match End → 6. Results → 7. Return Lobby
```

### Implementação

```csharp
using Unity.Netcode;
using UnityEngine;
using System.Collections;

namespace HorrorGame.Networking
{
    /// <summary>
    /// Gerencia o fluxo de partida multiplayer
    /// </summary>
    public class MatchManager : NetworkBehaviour
    {
        public static MatchManager Instance { get; private set; }

        [Header("Match Settings")]
        [SerializeField] private float matchDuration = 600f; // 10 minutos
        [SerializeField] private int minPlayers = 2;
        [SerializeField] private int maxPlayers = 5;

        [Header("Objectives")]
        [SerializeField] private int objectivesToComplete = 3;

        // Network Variables
        private NetworkVariable<MatchState> currentState = new NetworkVariable<MatchState>();
        private NetworkVariable<float> matchTimer = new NetworkVariable<float>();
        private NetworkVariable<int> objectivesCompleted = new NetworkVariable<int>();
        private NetworkVariable<int> playersAlive = new NetworkVariable<int>();

        public enum MatchState
        {
            Waiting,
            Countdown,
            InProgress,
            Victory,
            Defeat,
            Ended
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                currentState.Value = MatchState.Waiting;
                NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeft;
            }

            currentState.OnValueChanged += OnStateChanged;
        }

        private void Update()
        {
            if (!IsServer) return;

            switch (currentState.Value)
            {
                case MatchState.InProgress:
                    UpdateMatchTimer();
                    CheckWinConditions();
                    CheckLoseConditions();
                    break;
            }
        }

        private void OnPlayerJoined(ulong clientId)
        {
            Debug.Log($"[Match] Player {clientId} entrou");

            playersAlive.Value++;

            // Se tem players suficientes, começar countdown
            if (currentState.Value == MatchState.Waiting &&
                NetworkManager.Singleton.ConnectedClients.Count >= minPlayers)
            {
                StartCountdown();
            }
        }

        private void OnPlayerLeft(ulong clientId)
        {
            Debug.Log($"[Match] Player {clientId} saiu");

            playersAlive.Value--;

            // Se muito poucos players, encerrar
            if (currentState.Value == MatchState.InProgress &&
                playersAlive.Value < minPlayers)
            {
                EndMatch(MatchState.Ended);
            }
        }

        private void StartCountdown()
        {
            if (!IsServer) return;

            currentState.Value = MatchState.Countdown;
            StartCountdownClientRpc();
        }

        [ClientRpc]
        private void StartCountdownClientRpc()
        {
            Debug.Log("[Match] Iniciando countdown...");
            StartCoroutine(CountdownRoutine());
        }

        private IEnumerator CountdownRoutine()
        {
            for (int i = 3; i > 0; i--)
            {
                Debug.Log($"[Match] {i}...");
                GameEvents.OnCountdownTick?.Invoke(i);
                yield return new WaitForSeconds(1f);
            }

            if (IsServer)
            {
                StartMatch();
            }
        }

        private void StartMatch()
        {
            if (!IsServer) return;

            currentState.Value = MatchState.InProgress;
            matchTimer.Value = matchDuration;
            objectivesCompleted.Value = 0;

            Debug.Log("[Match] Partida iniciada!");

            StartMatchClientRpc();
        }

        [ClientRpc]
        private void StartMatchClientRpc()
        {
            GameEvents.OnMatchStarted?.Invoke();
        }

        private void UpdateMatchTimer()
        {
            matchTimer.Value -= Time.deltaTime;

            if (matchTimer.Value <= 0)
            {
                matchTimer.Value = 0;
                // Tempo acabou = derrota
                EndMatch(MatchState.Defeat);
            }
        }

        private void CheckWinConditions()
        {
            // Vitória: completar todos objetivos
            if (objectivesCompleted.Value >= objectivesToComplete)
            {
                EndMatch(MatchState.Victory);
            }
        }

        private void CheckLoseConditions()
        {
            // Derrota: todos players morreram
            if (playersAlive.Value <= 0)
            {
                EndMatch(MatchState.Defeat);
            }
        }

        public void OnObjectiveCompleted()
        {
            if (!IsServer) return;

            objectivesCompleted.Value++;
            Debug.Log($"[Match] Objetivo completado! ({objectivesCompleted.Value}/{objectivesToComplete})");

            ObjectiveCompletedClientRpc();
        }

        [ClientRpc]
        private void ObjectiveCompletedClientRpc()
        {
            GameEvents.OnObjectiveCompleted?.Invoke(objectivesCompleted.Value, objectivesToComplete);
        }

        public void OnPlayerDied()
        {
            if (!IsServer) return;

            playersAlive.Value--;
            Debug.Log($"[Match] Player morreu. Restantes: {playersAlive.Value}");
        }

        private void EndMatch(MatchState endState)
        {
            if (!IsServer) return;
            if (currentState.Value == MatchState.Ended) return;

            currentState.Value = endState;

            Debug.Log($"[Match] Partida encerrada: {endState}");

            EndMatchClientRpc(endState);
        }

        [ClientRpc]
        private void EndMatchClientRpc(MatchState endState)
        {
            if (endState == MatchState.Victory)
            {
                GameEvents.OnMatchVictory?.Invoke();
            }
            else if (endState == MatchState.Defeat)
            {
                GameEvents.OnMatchDefeat?.Invoke();
            }

            // Mostrar tela de resultados
            ShowResults();
        }

        private void ShowResults()
        {
            // Implementar tela de resultados
            Debug.Log("[Match] Mostrando resultados...");

            // Retornar ao lobby após delay
            Invoke(nameof(ReturnToLobby), 10f);
        }

        private void ReturnToLobby()
        {
            if (IsServer)
            {
                // Desconectar todos
                NetworkManager.Singleton.Shutdown();
            }

            // Carregar cena do lobby
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }

        private void OnStateChanged(MatchState oldState, MatchState newState)
        {
            Debug.Log($"[Match] Estado: {oldState} -> {newState}");
        }

        // Getters públicos
        public MatchState GetCurrentState() => currentState.Value;
        public float GetRemainingTime() => matchTimer.Value;
        public int GetObjectivesCompleted() => objectivesCompleted.Value;
        public int GetPlayersAlive() => playersAlive.Value;
    }
}
```

---

## ⚡ Otimizações e Performance

### 1. Redução de Bandwidth

```csharp
// Usar NetworkVariable apenas quando necessário
// Evitar atualizar toda frame

// ❌ Ruim
void Update()
{
    networkPosition.Value = transform.position; // Toda frame!
}

// ✅ Bom
void Update()
{
    if (Vector3.Distance(transform.position, networkPosition.Value) > 0.1f)
    {
        networkPosition.Value = transform.position; // Apenas se mudou significativamente
    }
}
```

### 2. Interest Management

```csharp
// Não sincronizar objetos muito longe dos players
public class NetworkInterestManager : NetworkBehaviour
{
    [SerializeField] private float interestRange = 50f;

    private void Update()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.Value.PlayerObject == null) continue;

            Vector3 playerPos = client.Value.PlayerObject.transform.position;

            // Esconder objetos distantes
            HideDistantObjects(client.Key, playerPos);
        }
    }

    private void HideDistantObjects(ulong clientId, Vector3 playerPos)
    {
        var allNetworkObjects = FindObjectsOfType<NetworkObject>();

        foreach (var obj in allNetworkObjects)
        {
            float distance = Vector3.Distance(playerPos, obj.transform.position);

            if (distance > interestRange)
            {
                obj.NetworkHide(clientId);
            }
            else
            {
                obj.NetworkShow(clientId);
            }
        }
    }
}
```

### 3. Object Pooling para Networked Objects

```csharp
public class NetworkObjectPool : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitializePool();
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject SpawnFromPool(Vector3 position, Quaternion rotation)
    {
        if (!IsServer) return null;

        GameObject obj = pool.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        obj.GetComponent<NetworkObject>().Spawn();

        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (!IsServer) return;

        obj.GetComponent<NetworkObject>().Despawn(false);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

---

## 🗺️ Roadmap de Implementação Completo

### **Semana 1-2: Setup e Fundamentos**

#### Dias 1-3: Configuração Inicial
- [ ] Criar projeto Unity 2022.3 LTS
- [ ] Instalar todos os packages (Netcode, Input System, etc)
- [ ] Configurar Unity Gaming Services
- [ ] Criar estrutura de pastas
- [ ] Setup Git/Version Control

#### Dias 4-7: Input System e Player Base
- [ ] Criar Input Actions Asset
- [ ] Implementar PlayerInputHandler
- [ ] Criar FirstPersonController com New Input System
- [ ] Testar movimento local

### **Semana 3-4: Multiplayer Core**

#### Dias 8-10: Authentication e Lobby
- [ ] Implementar AuthenticationManager
- [ ] Implementar LobbyManager
- [ ] Criar UI de Lobby
- [ ] Testar criação/entrada em lobbies

#### Dias 11-14: Relay e Networking
- [ ] Implementar RelayManager
- [ ] Configurar NetworkManager
- [ ] Converter Player para NetworkObject
- [ ] Implementar NetworkPlayerController
- [ ] Testar 2 players conectados

### **Semana 5-6: Enemies e Combat**

#### Dias 15-18: Enemy Networking
- [ ] Criar NetworkEnemyController
- [ ] Implementar IA básica (Patrol/Chase/Attack)
- [ ] Sincronizar inimigos pela rede
- [ ] Testar spawning de inimigos

#### Dias 19-21: Combat System
- [ ] Sistema de dano (validado por servidor)
- [ ] Sistema de vida
- [ ] Efeitos visuais/sonoros
- [ ] Testar combate multiplayer

### **Semana 7-8: IA Avançada**

#### Dias 22-25: GOAP Implementation
- [ ] Estudar repositório Unity-GOAP
- [ ] Implementar GOAP básico
- [ ] Criar Actions e Goals
- [ ] Integrar com NetworkEnemy

#### Dias 26-28: GOAP Polishing
- [ ] Balancear comportamentos
- [ ] Adicionar mais Actions
- [ ] Testar com múltiplos players
- [ ] Debugar IA

### **Semana 9-10: Match System e Game Loop**

#### Dias 29-32: Match Flow
- [ ] Implementar MatchManager
- [ ] Sistema de countdown
- [ ] Win/Lose conditions
- [ ] Sistema de objetivos

#### Dias 33-35: UI e Polish
- [ ] HUD multiplayer
- [ ] Tela de resultados
- [ ] Sistema de respawn (se aplicável)
- [ ] Transições de cena

### **Semana 11-12: Otimização e Testing**

#### Dias 36-39: Performance
- [ ] Implementar Interest Management
- [ ] Object Pooling
- [ ] Reduzir bandwidth
- [ ] Profiling e otimização

#### Dias 40-42: Testing e Bugfixes
- [ ] Testar com 5 players
- [ ] Testar latência
- [ ] Testar edge cases (disconnect, etc)
- [ ] Corrigir bugs críticos

---

## 🎯 Checklist Final de Funcionalidades

### Core Multiplayer
- [ ] 4-5 jogadores simultâneos
- [ ] Sistema de Lobby funcional
- [ ] Matchmaking
- [ ] Relay (sem port forwarding)
- [ ] Sincronização estável

### Gameplay
- [ ] Movimento FPS responsivo
- [ ] New Input System integrado
- [ ] Sistema de combate
- [ ] Inimigos com IA inteligente
- [ ] Sistema de objetivos

### Polish
- [ ] UI completa
- [ ] Efeitos visuais/sonoros
- [ ] Atmosfera de terror
- [ ] Performance otimizada (60+ FPS)
- [ ] Sem bugs críticos

---

## 📚 Recursos Finais

### Repositórios Essenciais (Adam Myhre)
```
git clone https://github.com/adammyhre/Unity-GOAP
git clone https://github.com/adammyhre/Unity-Event-Bus
git clone https://github.com/adammyhre/Unity-Multiplayer-Kart
git clone https://github.com/adammyhre/Unity-Improved-Timers
```

### Canais YouTube
- Adam Myhre (@git-amend)
- Dapper Dino
- Code Monkey

### Documentação
- [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Gaming Services](https://docs.unity.com/ugs/)
- [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)

---

## 🎓 Próximos Passos

1. **Comece pelo Lobby** - Sistema de matchmaking primeiro
2. **Implemente Players** - Movimento e sincronização
3. **Adicione Inimigos** - IA básica primeiro, depois GOAP
4. **Implemente Game Loop** - Match system
5. **Otimize** - Performance e bandwidth
6. **Polish** - UI, sons, efeitos

**Boa sorte com seu jogo de terror multiplayer!** 🎮👻

---

**IMPORTANTE:** Este guia assume conhecimento básico de Unity e C#. Para dúvidas específicas, consulte a documentação oficial ou os canais recomendados.
