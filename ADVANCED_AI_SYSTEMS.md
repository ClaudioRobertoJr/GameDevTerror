# 🤖 Sistemas de IA Avançados para Horror Game

Guia detalhado de implementação de GOAP, Behavior Trees e Utility AI para criar inimigos inteligentes e aterrorizantes.

## 📚 Índice

- [Comparação dos Sistemas](#comparação-dos-sistemas)
- [GOAP Implementation](#goap-implementation)
- [Behavior Trees Implementation](#behavior-trees-implementation)
- [Utility AI Implementation](#utility-ai-implementation)
- [Sistemas Híbridos](#sistemas-híbridos)
- [Exemplos de Inimigos](#exemplos-de-inimigos)
- [Debugging e Testing](#debugging-e-testing)

---

## 🎯 Comparação dos Sistemas

### Quando usar cada sistema?

| Sistema | Melhor Para | Complexidade | Performance | Debugging |
|---------|-------------|--------------|-------------|-----------|
| **GOAP** | Inimigos que planejam (flanqueiam, coordenam) | 🔴 Alta | 🟡 Média | 🔴 Difícil |
| **Behavior Trees** | Comportamentos hierárquicos complexos | 🟡 Média | 🟢 Boa | 🟢 Fácil |
| **Utility AI** | Decisões baseadas em contexto | 🟢 Baixa | 🟢 Boa | 🟡 Média |
| **FSM (atual)** | Comportamentos simples e previsíveis | 🟢 Baixa | 🟢 Excelente | 🟢 Fácil |

### Matriz de Decisão

```
Use GOAP se:
  ✓ Inimigos precisam planejar múltiplos passos
  ✓ Coordenação entre inimigos
  ✓ Comportamentos emergentes são desejados
  ✓ Imprevisibilidade é importante

Use Behavior Trees se:
  ✓ Comportamentos complexos mas estruturados
  ✓ Fácil visualização e debugging são necessários
  ✓ Reutilização de sub-comportamentos
  ✓ Designers não-programadores vão ajustar

Use Utility AI se:
  ✓ Múltiplos fatores influenciam decisões
  ✓ Comportamentos precisam ser contextuais
  ✓ Balanceamento fino é importante
  ✓ Simplicidade de implementação

Use FSM (atual) se:
  ✓ Comportamentos são simples
  ✓ Estados são claramente definidos
  ✓ Performance máxima é crítica
  ✓ Rapidez de implementação
```

---

## 🎯 GOAP Implementation

### Conceitos Fundamentais

```
GOAP = Goal + Actions + World State + Planner

Goal: "Hunt Player"
Actions: [Move, Hide, Attack, CallBackup]
World State: {playerVisible: true, hasWeapon: false, alliesNearby: 2}
Planner: Encontra sequência de ações que atingem o goal
```

### Arquitetura Completa

```csharp
namespace HorrorGame.AI.GOAP
{
    // 1. World State - Estado do mundo
    public class WorldState
    {
        private Dictionary<string, object> state = new Dictionary<string, object>();

        public void SetState(string key, object value)
        {
            state[key] = value;
        }

        public T GetState<T>(string key)
        {
            if (state.ContainsKey(key))
                return (T)state[key];
            return default(T);
        }

        public bool HasState(string key)
        {
            return state.ContainsKey(key);
        }

        public WorldState Clone()
        {
            var clone = new WorldState();
            foreach (var kvp in state)
            {
                clone.SetState(kvp.Key, kvp.Value);
            }
            return clone;
        }
    }

    // 2. Goal - Objetivo
    public class GOAPGoal
    {
        public string name;
        public float priority;
        public Dictionary<string, object> desiredState;

        public GOAPGoal(string name, float priority)
        {
            this.name = name;
            this.priority = priority;
            this.desiredState = new Dictionary<string, object>();
        }

        public void AddDesiredState(string key, object value)
        {
            desiredState[key] = value;
        }

        public bool IsAchieved(WorldState worldState)
        {
            foreach (var kvp in desiredState)
            {
                if (!worldState.HasState(kvp.Key))
                    return false;

                var stateValue = worldState.GetState<object>(kvp.Key);
                if (!stateValue.Equals(kvp.Value))
                    return false;
            }
            return true;
        }
    }

    // 3. Action - Ação
    public abstract class GOAPAction
    {
        public string actionName;
        public float cost;

        // Pré-condições (o que precisa ser verdade para executar)
        public Dictionary<string, object> preconditions = new Dictionary<string, object>();

        // Efeitos (o que muda no mundo após executar)
        public Dictionary<string, object> effects = new Dictionary<string, object>();

        protected GameObject agent;
        protected bool isRunning = false;

        public GOAPAction(GameObject agent)
        {
            this.agent = agent;
        }

        // Pode executar esta ação?
        public bool CheckPreconditions(WorldState worldState)
        {
            foreach (var precondition in preconditions)
            {
                if (!worldState.HasState(precondition.Key))
                    return false;

                var value = worldState.GetState<object>(precondition.Key);
                if (!value.Equals(precondition.Value))
                    return false;
            }
            return true;
        }

        // Aplicar efeitos ao estado do mundo
        public void ApplyEffects(WorldState worldState)
        {
            foreach (var effect in effects)
            {
                worldState.SetState(effect.Key, effect.Value);
            }
        }

        // Executar ação
        public abstract bool Execute();

        // Ação completou?
        public abstract bool IsComplete();

        // Resetar ação
        public virtual void Reset()
        {
            isRunning = false;
        }
    }

    // 4. Planner - Planejador (Greedy DFS)
    public class GOAPPlanner
    {
        public Queue<GOAPAction> CreatePlan(
            GameObject agent,
            List<GOAPAction> availableActions,
            GOAPGoal goal,
            WorldState worldState)
        {
            // Resetar ações
            foreach (var action in availableActions)
            {
                action.Reset();
            }

            // Criar plano usando Greedy DFS
            var plan = new Queue<GOAPAction>();
            var currentState = worldState.Clone();

            if (BuildPlan(availableActions, goal, currentState, plan))
            {
                return plan;
            }

            return null; // Nenhum plano encontrado
        }

        private bool BuildPlan(
            List<GOAPAction> actions,
            GOAPGoal goal,
            WorldState currentState,
            Queue<GOAPAction> plan)
        {
            // Goal já atingido?
            if (goal.IsAchieved(currentState))
            {
                return true;
            }

            // Tentar cada ação
            foreach (var action in actions)
            {
                // Pode executar esta ação?
                if (action.CheckPreconditions(currentState))
                {
                    // Simular execução
                    var newState = currentState.Clone();
                    action.ApplyEffects(newState);

                    // Recursão
                    var remainingActions = new List<GOAPAction>(actions);
                    remainingActions.Remove(action);

                    if (BuildPlan(remainingActions, goal, newState, plan))
                    {
                        plan.Enqueue(action);
                        return true;
                    }
                }
            }

            return false;
        }
    }

    // 5. Agent - Agente GOAP
    public class GOAPAgent : MonoBehaviour
    {
        [Header("GOAP Components")]
        private GOAPPlanner planner;
        private WorldState worldState;
        private Queue<GOAPAction> currentPlan;
        private GOAPAction currentAction;

        [Header("Configuration")]
        [SerializeField] private float replanInterval = 1f;
        private float replanTimer;

        public List<GOAPAction> availableActions = new List<GOAPAction>();
        public List<GOAPGoal> availableGoals = new List<GOAPGoal>();

        private void Start()
        {
            planner = new GOAPPlanner();
            worldState = new WorldState();

            InitializeActions();
            InitializeGoals();
        }

        private void Update()
        {
            // Atualizar world state
            UpdateWorldState();

            // Replan periodicamente ou se não há plano
            replanTimer -= Time.deltaTime;
            if (currentPlan == null || currentPlan.Count == 0 || replanTimer <= 0)
            {
                replanTimer = replanInterval;
                CreateNewPlan();
            }

            // Executar ação atual
            if (currentAction != null)
            {
                bool actionComplete = currentAction.Execute();

                if (actionComplete)
                {
                    currentAction.Reset();
                    currentAction = null;

                    // Próxima ação
                    if (currentPlan != null && currentPlan.Count > 0)
                    {
                        currentAction = currentPlan.Dequeue();
                    }
                }
            }
            else if (currentPlan != null && currentPlan.Count > 0)
            {
                currentAction = currentPlan.Dequeue();
            }
        }

        private void CreateNewPlan()
        {
            // Escolher goal de maior prioridade
            GOAPGoal currentGoal = GetHighestPriorityGoal();

            if (currentGoal != null)
            {
                currentPlan = planner.CreatePlan(
                    gameObject,
                    availableActions,
                    currentGoal,
                    worldState
                );

                if (currentPlan != null)
                {
                    Debug.Log($"[GOAP] Novo plano criado para goal: {currentGoal.name}");
                    Debug.Log($"[GOAP] Ações: {string.Join(" -> ", currentPlan.Select(a => a.actionName))}");
                }
                else
                {
                    Debug.LogWarning($"[GOAP] Nenhum plano encontrado para goal: {currentGoal.name}");
                }
            }
        }

        private GOAPGoal GetHighestPriorityGoal()
        {
            GOAPGoal best = null;
            float bestPriority = 0f;

            foreach (var goal in availableGoals)
            {
                if (goal.priority > bestPriority && !goal.IsAchieved(worldState))
                {
                    best = goal;
                    bestPriority = goal.priority;
                }
            }

            return best;
        }

        protected virtual void InitializeActions()
        {
            // Subclasses implementam
        }

        protected virtual void InitializeGoals()
        {
            // Subclasses implementam
        }

        protected virtual void UpdateWorldState()
        {
            // Subclasses implementam
        }

        private void OnDrawGizmos()
        {
            if (currentAction != null)
            {
                // Desenhar informação da ação atual
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.5f);

                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 2.5f,
                    $"GOAP: {currentAction.actionName}"
                );
                #endif
            }
        }
    }
}
```

### Exemplo: Inimigo Caçador com GOAP

```csharp
using HorrorGame.AI.GOAP;
using UnityEngine;

public class HunterEnemy : GOAPAgent
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent navAgent;

    [Header("Stats")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float health = 100f;

    // Actions
    private MoveToPlayerAction moveToPlayer;
    private AttackPlayerAction attackPlayer;
    private HideAction hide;
    private CallBackupAction callBackup;
    private FlankPlayerAction flankPlayer;

    protected override void InitializeActions()
    {
        moveToPlayer = new MoveToPlayerAction(gameObject, player, navAgent);
        attackPlayer = new AttackPlayerAction(gameObject, player);
        hide = new HideAction(gameObject, navAgent);
        callBackup = new CallBackupAction(gameObject);
        flankPlayer = new FlankPlayerAction(gameObject, player, navAgent);

        availableActions.Add(moveToPlayer);
        availableActions.Add(attackPlayer);
        availableActions.Add(hide);
        availableActions.Add(callBackup);
        availableActions.Add(flankPlayer);
    }

    protected override void InitializeGoals()
    {
        // Goal 1: Hunt Player (prioridade alta)
        var huntGoal = new GOAPGoal("HuntPlayer", priority: 10f);
        huntGoal.AddDesiredState("playerDead", true);
        availableGoals.Add(huntGoal);

        // Goal 2: Survive (prioridade baixa, mas aumenta se saúde baixa)
        var surviveGoal = new GOAPGoal("Survive", priority: 5f);
        surviveGoal.AddDesiredState("isSafe", true);
        availableGoals.Add(surviveGoal);

        // Goal 3: Coordinate (se há aliados)
        var coordinateGoal = new GOAPGoal("Coordinate", priority: 7f);
        coordinateGoal.AddDesiredState("groupFormed", true);
        availableGoals.Add(coordinateGoal);
    }

    protected override void UpdateWorldState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Atualizar estados
        worldState.SetState("playerVisible", CanSeePlayer());
        worldState.SetState("playerDistance", distanceToPlayer);
        worldState.SetState("inAttackRange", distanceToPlayer <= attackRange);
        worldState.SetState("healthLow", health < 30f);
        worldState.SetState("hasLineOfSight", HasLineOfSight());
        worldState.SetState("alliesNearby", CountNearbyAllies());

        // Ajustar prioridades baseado em contexto
        if (health < 30f)
        {
            // Se saúde baixa, priorizar sobrevivência
            availableGoals.Find(g => g.name == "Survive").priority = 15f;
            availableGoals.Find(g => g.name == "HuntPlayer").priority = 3f;
        }
        else
        {
            // Saúde ok, priorizar caça
            availableGoals.Find(g => g.name == "Survive").priority = 5f;
            availableGoals.Find(g => g.name == "HuntPlayer").priority = 10f;
        }
    }

    private bool CanSeePlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= detectionRange;
    }

    private bool HasLineOfSight()
    {
        Vector3 direction = player.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, detectionRange))
        {
            return hit.transform == player;
        }

        return false;
    }

    private int CountNearbyAllies()
    {
        Collider[] allies = Physics.OverlapSphere(transform.position, 10f);
        int count = 0;

        foreach (var collider in allies)
        {
            if (collider.CompareTag("Enemy") && collider.gameObject != gameObject)
            {
                count++;
            }
        }

        return count;
    }
}

// Implementação de Actions

public class MoveToPlayerAction : GOAPAction
{
    private Transform player;
    private NavMeshAgent navAgent;
    private float acceptableDistance = 3f;

    public MoveToPlayerAction(GameObject agent, Transform player, NavMeshAgent navAgent)
        : base(agent)
    {
        this.player = player;
        this.navAgent = navAgent;

        actionName = "MoveToPlayer";
        cost = 1f;

        // Precondições
        preconditions["playerVisible"] = true;

        // Efeitos
        effects["nearPlayer"] = true;
    }

    public override bool Execute()
    {
        if (!isRunning)
        {
            navAgent.SetDestination(player.position);
            isRunning = true;
        }

        // Atualizar destino
        if (navAgent.remainingDistance > acceptableDistance)
        {
            navAgent.SetDestination(player.position);
            return false;
        }

        return true; // Chegou perto o suficiente
    }

    public override bool IsComplete()
    {
        return navAgent.remainingDistance <= acceptableDistance;
    }
}

public class AttackPlayerAction : GOAPAction
{
    private Transform player;
    private float lastAttackTime;
    private float attackCooldown = 1.5f;

    public AttackPlayerAction(GameObject agent, Transform player)
        : base(agent)
    {
        this.player = player;

        actionName = "AttackPlayer";
        cost = 1f;

        // Precondições
        preconditions["nearPlayer"] = true;
        preconditions["inAttackRange"] = true;

        // Efeitos
        effects["playerDead"] = true; // Idealizado (pode não matar de primeira)
    }

    public override bool Execute()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // Executar ataque
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(20f);
                Debug.Log("[GOAP] Atacou player!");
            }

            lastAttackTime = Time.time;
            return true;
        }

        return false;
    }

    public override bool IsComplete()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }
}

public class HideAction : GOAPAction
{
    private NavMeshAgent navAgent;
    private Vector3 hidePosition;
    private bool foundHideSpot = false;

    public HideAction(GameObject agent, NavMeshAgent navAgent)
        : base(agent)
    {
        this.navAgent = navAgent;

        actionName = "Hide";
        cost = 2f;

        // Precondições
        preconditions["healthLow"] = true;

        // Efeitos
        effects["isSafe"] = true;
        effects["hidden"] = true;
    }

    public override bool Execute()
    {
        if (!isRunning)
        {
            // Encontrar lugar para esconder
            hidePosition = FindHideSpot();
            foundHideSpot = true;
            navAgent.SetDestination(hidePosition);
            isRunning = true;
            Debug.Log("[GOAP] Procurando lugar para esconder");
        }

        return navAgent.remainingDistance <= 1f;
    }

    public override bool IsComplete()
    {
        return foundHideSpot && navAgent.remainingDistance <= 1f;
    }

    private Vector3 FindHideSpot()
    {
        // Encontrar ponto longe do player
        Vector3 directionAway = (agent.transform.position - GameObject.FindGameObjectWithTag("Player").transform.position).normalized;
        return agent.transform.position + directionAway * 15f;
    }

    public override void Reset()
    {
        base.Reset();
        foundHideSpot = false;
    }
}

public class FlankPlayerAction : GOAPAction
{
    private Transform player;
    private NavMeshAgent navAgent;
    private Vector3 flankPosition;

    public FlankPlayerAction(GameObject agent, Transform player, NavMeshAgent navAgent)
        : base(agent)
    {
        this.player = player;
        this.navAgent = navAgent;

        actionName = "FlankPlayer";
        cost = 3f;

        // Precondições
        preconditions["playerVisible"] = true;
        preconditions["alliesNearby"] = true; // Precisa de aliados para flankear

        // Efeitos
        effects["flanking"] = true;
        effects["nearPlayer"] = true;
    }

    public override bool Execute()
    {
        if (!isRunning)
        {
            // Calcular posição de flank (perpendicular ao player)
            Vector3 toPlayer = player.position - agent.transform.position;
            Vector3 perpendicular = Vector3.Cross(toPlayer, Vector3.up).normalized;

            flankPosition = player.position + perpendicular * 5f;
            navAgent.SetDestination(flankPosition);
            isRunning = true;

            Debug.Log("[GOAP] Flanqueando player!");
        }

        return navAgent.remainingDistance <= 2f;
    }

    public override bool IsComplete()
    {
        return navAgent.remainingDistance <= 2f;
    }
}

public class CallBackupAction : GOAPAction
{
    private bool calledBackup = false;

    public CallBackupAction(GameObject agent)
        : base(agent)
    {
        actionName = "CallBackup";
        cost = 2f;

        // Precondições
        preconditions["playerVisible"] = true;

        // Efeitos
        effects["backupCalled"] = true;
    }

    public override bool Execute()
    {
        if (!calledBackup)
        {
            // Notificar outros inimigos
            GameEvents.OnEnemyDetectedPlayer?.Invoke(agent);
            calledBackup = true;
            Debug.Log("[GOAP] Chamou reforços!");

            return true;
        }

        return false;
    }

    public override bool IsComplete()
    {
        return calledBackup;
    }

    public override void Reset()
    {
        base.Reset();
        calledBackup = false;
    }
}
```

---

## 🌳 Behavior Trees Implementation

### Conceitos Fundamentais

```
Behavior Tree = Hierarquia de Nós

Tipos de Nós:
1. Composite (controle de fluxo)
   - Sequence: AND lógico
   - Selector: OR lógico
   - Parallel: Executa múltiplos

2. Decorator (modificadores)
   - Inverter
   - Repeater
   - Cooldown

3. Leaf (ações e condições)
   - Actions: Executam algo
   - Conditions: Testam algo
```

### Arquitetura Completa

```csharp
namespace HorrorGame.AI.BehaviorTree
{
    // Resultado de execução de nó
    public enum NodeState
    {
        Running,  // Ainda executando
        Success,  // Sucesso
        Failure   // Falha
    }

    // Nó base abstrato
    public abstract class BTNode
    {
        protected NodeState state;

        public NodeState State => state;

        public abstract NodeState Evaluate();
    }

    // Composite Node - nó com filhos
    public abstract class BTComposite : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        public void AddChild(BTNode child)
        {
            children.Add(child);
        }

        public void RemoveChild(BTNode child)
        {
            children.Remove(child);
        }

        public void ClearChildren()
        {
            children.Clear();
        }
    }

    // Sequence - executa filhos em ordem (AND)
    public class BTSequence : BTComposite
    {
        public override NodeState Evaluate()
        {
            bool anyChildRunning = false;

            foreach (var child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Failure:
                        state = NodeState.Failure;
                        return state;

                    case NodeState.Success:
                        continue;

                    case NodeState.Running:
                        anyChildRunning = true;
                        continue;

                    default:
                        state = NodeState.Success;
                        return state;
                }
            }

            state = anyChildRunning ? NodeState.Running : NodeState.Success;
            return state;
        }
    }

    // Selector - executa até um suceder (OR)
    public class BTSelector : BTComposite
    {
        public override NodeState Evaluate()
        {
            foreach (var child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Failure:
                        continue;

                    case NodeState.Success:
                        state = NodeState.Success;
                        return state;

                    case NodeState.Running:
                        state = NodeState.Running;
                        return state;

                    default:
                        continue;
                }
            }

            state = NodeState.Failure;
            return state;
        }
    }

    // Parallel - executa múltiplos simultaneamente
    public class BTParallel : BTComposite
    {
        public override NodeState Evaluate()
        {
            int successCount = 0;
            int failureCount = 0;

            foreach (var child in children)
            {
                switch (child.Evaluate())
                {
                    case NodeState.Success:
                        successCount++;
                        break;

                    case NodeState.Failure:
                        failureCount++;
                        break;

                    case NodeState.Running:
                        break;
                }
            }

            // Se todos sucederam
            if (successCount == children.Count)
            {
                state = NodeState.Success;
                return state;
            }

            // Se algum falhou
            if (failureCount > 0)
            {
                state = NodeState.Failure;
                return state;
            }

            // Ainda executando
            state = NodeState.Running;
            return state;
        }
    }

    // Decorator - modifica comportamento de um filho
    public abstract class BTDecorator : BTNode
    {
        protected BTNode child;

        public void SetChild(BTNode child)
        {
            this.child = child;
        }
    }

    // Inverter - inverte resultado do filho
    public class BTInverter : BTDecorator
    {
        public override NodeState Evaluate()
        {
            switch (child.Evaluate())
            {
                case NodeState.Failure:
                    state = NodeState.Success;
                    return state;

                case NodeState.Success:
                    state = NodeState.Failure;
                    return state;

                case NodeState.Running:
                    state = NodeState.Running;
                    return state;
            }

            state = NodeState.Failure;
            return state;
        }
    }

    // Repeater - repete filho N vezes
    public class BTRepeater : BTDecorator
    {
        private int repeatCount;
        private int currentCount = 0;

        public BTRepeater(int repeatCount)
        {
            this.repeatCount = repeatCount;
        }

        public override NodeState Evaluate()
        {
            if (currentCount < repeatCount)
            {
                var result = child.Evaluate();

                if (result == NodeState.Success || result == NodeState.Failure)
                {
                    currentCount++;
                }

                state = NodeState.Running;
                return state;
            }

            currentCount = 0;
            state = NodeState.Success;
            return state;
        }
    }

    // Cooldown - adiciona tempo de espera
    public class BTCooldown : BTDecorator
    {
        private float cooldownTime;
        private float lastExecutionTime = -999f;

        public BTCooldown(float cooldownTime)
        {
            this.cooldownTime = cooldownTime;
        }

        public override NodeState Evaluate()
        {
            if (Time.time - lastExecutionTime < cooldownTime)
            {
                state = NodeState.Failure;
                return state;
            }

            state = child.Evaluate();

            if (state == NodeState.Success || state == NodeState.Failure)
            {
                lastExecutionTime = Time.time;
            }

            return state;
        }
    }

    // Blackboard - memória compartilhada
    public class Blackboard
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public void SetValue<T>(string key, T value)
        {
            data[key] = value;
        }

        public T GetValue<T>(string key)
        {
            if (data.ContainsKey(key))
                return (T)data[key];

            return default(T);
        }

        public bool HasValue(string key)
        {
            return data.ContainsKey(key);
        }

        public void RemoveValue(string key)
        {
            if (data.ContainsKey(key))
                data.Remove(key);
        }
    }

    // Behavior Tree raiz
    public class BehaviorTree
    {
        private BTNode rootNode;
        private Blackboard blackboard;

        public Blackboard Blackboard => blackboard;

        public BehaviorTree(BTNode rootNode)
        {
            this.rootNode = rootNode;
            this.blackboard = new Blackboard();
        }

        public NodeState Tick()
        {
            return rootNode != null ? rootNode.Evaluate() : NodeState.Failure;
        }
    }
}
```

### Exemplo: Inimigo Stalker com Behavior Tree

```csharp
using HorrorGame.AI.BehaviorTree;
using UnityEngine;

public class StalkerEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshAgent navAgent;

    [Header("Stats")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float health = 100f;

    private BehaviorTree behaviorTree;

    private void Start()
    {
        behaviorTree = BuildTree();
    }

    private void Update()
    {
        // Atualizar blackboard
        UpdateBlackboard();

        // Executar árvore
        behaviorTree.Tick();
    }

    private BehaviorTree BuildTree()
    {
        var tree = new BehaviorTree(
            // Root Selector - tenta comportamentos em ordem
            new BTSelector()
        );

        var rootSelector = (BTSelector)tree.Blackboard;

        // Comportamento 1: Player está morto? Celebrar!
        var celebrateSequence = new BTSequence();
        celebrateSequence.AddChild(new IsPlayerDeadCondition(tree.Blackboard));
        celebrateSequence.AddChild(new CelebrateAction(gameObject));
        rootSelector.AddChild(celebrateSequence);

        // Comportamento 2: Atacar se possível
        var attackSequence = new BTSequence();
        attackSequence.AddChild(new CanSeePlayerCondition(tree.Blackboard));
        attackSequence.AddChild(new IsInRangeCondition(tree.Blackboard, attackRange));
        attackSequence.AddChild(new AttackAction(gameObject, player));
        rootSelector.AddChild(attackSequence);

        // Comportamento 3: Perseguir se vê player
        var chaseSequence = new BTSequence();
        chaseSequence.AddChild(new CanSeePlayerCondition(tree.Blackboard));
        chaseSequence.AddChild(new ChaseAction(gameObject, navAgent, player));
        rootSelector.AddChild(chaseSequence);

        // Comportamento 4: Investigar última posição conhecida
        var investigateSequence = new BTSequence();
        investigateSequence.AddChild(new HasLastKnownPositionCondition(tree.Blackboard));
        investigateSequence.AddChild(new MoveToPositionAction(gameObject, navAgent, tree.Blackboard, "lastKnownPosition"));
        rootSelector.AddChild(investigateSequence);

        // Comportamento 5: Patrulhar (fallback)
        rootSelector.AddChild(new PatrolAction(gameObject, navAgent));

        return tree;
    }

    private void UpdateBlackboard()
    {
        var bb = behaviorTree.Blackboard;

        // Atualizar informações
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSee = CanSeePlayer();

        bb.SetValue("player", player);
        bb.SetValue("distanceToPlayer", distanceToPlayer);
        bb.SetValue("canSeePlayer", canSee);
        bb.SetValue("health", health);

        // Salvar última posição conhecida
        if (canSee)
        {
            bb.SetValue("lastKnownPosition", player.position);
            bb.SetValue("timeSinceLastSeen", 0f);
        }
        else
        {
            float timeSinceSeen = bb.GetValue<float>("timeSinceLastSeen");
            bb.SetValue("timeSinceLastSeen", timeSinceSeen + Time.deltaTime);
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 direction = player.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, detectionRange))
        {
            return hit.transform == player;
        }

        return false;
    }
}

// Implementação de Nodes

// Condição: Player está morto?
public class IsPlayerDeadCondition : BTNode
{
    private Blackboard blackboard;

    public IsPlayerDeadCondition(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }

    public override NodeState Evaluate()
    {
        var player = blackboard.GetValue<Transform>("player");
        var playerHealth = player.GetComponent<PlayerHealth>();

        if (playerHealth != null && playerHealth.IsDead)
        {
            state = NodeState.Success;
        }
        else
        {
            state = NodeState.Failure;
        }

        return state;
    }
}

// Condição: Pode ver player?
public class CanSeePlayerCondition : BTNode
{
    private Blackboard blackboard;

    public CanSeePlayerCondition(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }

    public override NodeState Evaluate()
    {
        bool canSee = blackboard.GetValue<bool>("canSeePlayer");
        state = canSee ? NodeState.Success : NodeState.Failure;
        return state;
    }
}

// Condição: Está em range?
public class IsInRangeCondition : BTNode
{
    private Blackboard blackboard;
    private float range;

    public IsInRangeCondition(Blackboard blackboard, float range)
    {
        this.blackboard = blackboard;
        this.range = range;
    }

    public override NodeState Evaluate()
    {
        float distance = blackboard.GetValue<float>("distanceToPlayer");
        state = distance <= range ? NodeState.Success : NodeState.Failure;
        return state;
    }
}

// Condição: Tem última posição conhecida?
public class HasLastKnownPositionCondition : BTNode
{
    private Blackboard blackboard;

    public HasLastKnownPositionCondition(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }

    public override NodeState Evaluate()
    {
        bool hasPosition = blackboard.HasValue("lastKnownPosition");
        float timeSinceSeen = blackboard.GetValue<float>("timeSinceLastSeen");

        // Só investigar se viu recentemente (últimos 10 segundos)
        if (hasPosition && timeSinceSeen < 10f)
        {
            state = NodeState.Success;
        }
        else
        {
            state = NodeState.Failure;
        }

        return state;
    }
}

// Ação: Atacar
public class AttackAction : BTNode
{
    private GameObject agent;
    private Transform target;
    private float lastAttackTime;
    private float attackCooldown = 1.5f;

    public AttackAction(GameObject agent, Transform target)
    {
        this.agent = agent;
        this.target = target;
    }

    public override NodeState Evaluate()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // Executar ataque
            var playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(15f);
                Debug.Log("[BT] Atacou player!");
            }

            lastAttackTime = Time.time;
            state = NodeState.Success;
        }
        else
        {
            state = NodeState.Running;
        }

        return state;
    }
}

// Ação: Perseguir
public class ChaseAction : BTNode
{
    private GameObject agent;
    private NavMeshAgent navAgent;
    private Transform target;

    public ChaseAction(GameObject agent, NavMeshAgent navAgent, Transform target)
    {
        this.agent = agent;
        this.navAgent = navAgent;
        this.target = target;
    }

    public override NodeState Evaluate()
    {
        navAgent.SetDestination(target.position);
        state = NodeState.Running;
        return state;
    }
}

// Ação: Mover para posição
public class MoveToPositionAction : BTNode
{
    private GameObject agent;
    private NavMeshAgent navAgent;
    private Blackboard blackboard;
    private string positionKey;

    public MoveToPositionAction(GameObject agent, NavMeshAgent navAgent, Blackboard blackboard, string positionKey)
    {
        this.agent = agent;
        this.navAgent = navAgent;
        this.blackboard = blackboard;
        this.positionKey = positionKey;
    }

    public override NodeState Evaluate()
    {
        Vector3 targetPosition = blackboard.GetValue<Vector3>(positionKey);
        navAgent.SetDestination(targetPosition);

        float distance = Vector3.Distance(agent.transform.position, targetPosition);

        if (distance <= 2f)
        {
            // Chegou - limpar última posição conhecida
            blackboard.RemoveValue(positionKey);
            state = NodeState.Success;
        }
        else
        {
            state = NodeState.Running;
        }

        return state;
    }
}

// Ação: Patrulhar
public class PatrolAction : BTNode
{
    private GameObject agent;
    private NavMeshAgent navAgent;
    private Vector3[] patrolPoints;
    private int currentPoint = 0;

    public PatrolAction(GameObject agent, NavMeshAgent navAgent)
    {
        this.agent = agent;
        this.navAgent = navAgent;

        // Gerar pontos de patrulha aleatórios
        patrolPoints = GeneratePatrolPoints(4, 10f);
    }

    public override NodeState Evaluate()
    {
        if (navAgent.remainingDistance <= 1f)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
        }

        navAgent.SetDestination(patrolPoints[currentPoint]);
        state = NodeState.Running;
        return state;
    }

    private Vector3[] GeneratePatrolPoints(int count, float radius)
    {
        Vector3[] points = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * radius;
            randomDir += agent.transform.position;
            randomDir.y = agent.transform.position.y;

            points[i] = randomDir;
        }

        return points;
    }
}

// Ação: Celebrar
public class CelebrateAction : BTNode
{
    private GameObject agent;

    public CelebrateAction(GameObject agent)
    {
        this.agent = agent;
    }

    public override NodeState Evaluate()
    {
        Debug.Log("[BT] Vitória! Player morreu!");
        // Tocar animação de vitória, etc
        state = NodeState.Success;
        return state;
    }
}
```

---

**Continuação nos próximos arquivos...**

Este documento será continuado com Utility AI, Sistemas Híbridos, Exemplos Avançados e Debugging.

---

## 🎯 Resumo Rápido

### GOAP
- ✅ Use para planejamento inteligente
- ✅ Inimigos que coordenam
- ✅ Comportamento emergente
- ❌ Mais complexo de implementar

### Behavior Trees
- ✅ Hierarquia visual clara
- ✅ Fácil debugging
- ✅ Modular e reutilizável
- ❌ Pode ficar verboso

### Utility AI
- ✅ Decisões contextuais
- ✅ Fácil balancear
- ✅ Performance boa
- ❌ Menos "inteligente" que GOAP

---

Consulte **COOP_ROADMAP.md** para plano de implementação e **INTEGRATION_GUIDE.md** para integração no projeto.
