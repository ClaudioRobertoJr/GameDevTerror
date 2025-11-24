# 🚀 Tecnologias Avançadas para Horror Game

Este documento apresenta tecnologias avançadas baseadas nos repositórios de **Adam Myhre** (@adammyhre) que podem elevar significativamente a qualidade do seu jogo de terror cooperativo.

## 📚 Índice

- [Visão Geral das Tecnologias](#visão-geral-das-tecnologias)
- [Sistemas de IA Avançados](#sistemas-de-ia-avançados)
- [Sistemas de Performance](#sistemas-de-performance)
- [Sistemas de Arquitetura](#sistemas-de-arquitetura)
- [Multiplayer e Networking](#multiplayer-e-networking)
- [Priorização para Horror Coop](#priorização-para-horror-coop)

---

## 🎯 Visão Geral das Tecnologias

### Repositórios-Chave Analisados

| Tecnologia | Stars | Prioridade | Uso em Horror Coop |
|------------|-------|------------|-------------------|
| **Unity-GOAP** | 135 | 🔴 Alta | IA adaptativa para inimigos inteligentes |
| **Unity-Event-Bus** | 213 | 🔴 Alta | Comunicação desacoplada (multiplayer) |
| **Unity-Multiplayer-Kart** | - | 🔴 Alta | Base para networking cooperativo |
| **Unity-Improved-Timers** | 185 | 🟡 Média | Sistema de timers para mecânicas |
| **Unity-Behaviour-Trees** | 75 | 🟡 Média | IA hierárquica complexa |
| **Unity-Utility-AI** | 72 | 🟡 Média | Decisões contextuais |
| **Unity-Octrees** | 76 | 🟢 Baixa | Otimização espacial |
| **Unity-Batch-Raycasting** | 71 | 🟢 Baixa | Performance de raycasts |
| **Unity-Utils** | 685 | 🟡 Média | Utilitários gerais |
| **Unity-Inventory-System** | 238 | 🟡 Média | Sistema de inventário |

---

## 🤖 Sistemas de IA Avançados

### 1. GOAP (Goal-Oriented Action Planning)

#### O que é?
Sistema de IA que permite NPCs planejarem sequências de ações para atingir objetivos, tornando-os imprevisíveis e inteligentes.

#### Por que usar em Horror?
- **Comportamento emergente**: Inimigos criam estratégias únicas
- **Adaptabilidade**: IA reage dinamicamente às ações do player
- **Imprevisibilidade**: Aumenta tensão ao evitar padrões repetitivos
- **Coordenação**: Múltiplos inimigos podem coordenar ataques

#### Arquitetura

```
GOAP System
├── Goals (Objetivos)
│   ├── HuntPlayer
│   ├── DefendTerritory
│   ├── SeekResource
│   └── CallForBackup
│
├── Actions (Ações)
│   ├── MoveToPosition
│   ├── AttackTarget
│   ├── HideInShadows
│   ├── InvestigateNoise
│   └── FlankPlayer
│
├── World State (Estado do Mundo)
│   ├── PlayerVisible: bool
│   ├── PlayerHealth: float
│   ├── AllyCount: int
│   ├── InDanger: bool
│   └── HasWeapon: bool
│
└── Planner (Planejador)
    └── Greedy DFS Search
```

#### Implementação para Horror

**Exemplo: Inimigo Caçador**
```csharp
// Goals
Goal huntPlayer = new Goal("HuntPlayer", priority: 10);
Goal defendNest = new Goal("DefendNest", priority: 7);
Goal ambushPlayer = new Goal("AmbushPlayer", priority: 9);

// Actions
Action moveToLastKnownPosition = new Action("MoveToPLP")
{
    Preconditions = { {"playerDetected", true} },
    Effects = { {"atPlayerLocation", true} },
    Cost = 1.0f
};

Action hideAndWait = new Action("HideAndWait")
{
    Preconditions = { {"hasHidingSpot", true} },
    Effects = { {"isHidden", true}, {"playerUnaware", true} },
    Cost = 2.0f
};

Action ambushAttack = new Action("AmbushAttack")
{
    Preconditions = { {"isHidden", true}, {"playerNear", true} },
    Effects = { {"playerDamaged", true}, {"isHidden", false} },
    Cost = 1.5f
};
```

#### Casos de Uso

1. **Inimigo Inteligente que Aprende**
   - Detecta padrões do player (sempre esconde em armários)
   - Ajusta comportamento (checa armários mais frequentemente)

2. **Coordenação de Grupo**
   - Um inimigo distrai enquanto outro flanqueia
   - Chamam reforços quando player está forte

3. **Comportamento Contextual**
   - Se player está ferido → comportamento mais agressivo
   - Se player tem arma → comportamento mais cauteloso

#### Integração com Framework Atual

```csharp
// Substituir EnemyBase.UpdateAI() com GOAP
public class GOAPEnemy : EnemyBase
{
    private GOAPPlanner planner;
    private GOAPAgent agent;

    protected override void UpdateAI()
    {
        // Atualizar estado do mundo
        UpdateWorldState();

        // Planejar próxima ação
        var plan = planner.CreatePlan(agent, availableActions, currentGoal);

        // Executar plano
        if (plan != null && plan.Count > 0)
        {
            ExecuteAction(plan.Dequeue());
        }
    }

    private void UpdateWorldState()
    {
        worldState.SetState("playerVisible", CanSeePlayer());
        worldState.SetState("playerHealth", player.GetComponent<PlayerHealth>().CurrentHealth);
        worldState.SetState("isInDanger", currentHealth < maxHealth * 0.3f);
    }
}
```

---

### 2. Behavior Trees (Árvores de Comportamento)

#### O que é?
Estrutura hierárquica de decisões que organiza comportamentos de IA de forma visual e modular.

#### Por que usar em Horror?
- **Hierarquia clara**: Comportamentos organizados em árvore
- **Modularidade**: Fácil adicionar/remover comportamentos
- **Debugging visual**: Veja decisões em tempo real
- **Reutilização**: Subárvores podem ser compartilhadas

#### Estrutura de Nós

```
Behavior Tree Nodes
├── Composite Nodes (Controle)
│   ├── Sequence (AND) - Executa filhos em sequência
│   ├── Selector (OR) - Executa até um suceder
│   ├── Parallel - Executa múltiplos simultaneamente
│   └── Random - Escolhe filho aleatório
│
├── Decorator Nodes (Modificadores)
│   ├── Inverter - Inverte resultado
│   ├── Repeater - Repete N vezes
│   ├── UntilFail - Repete até falhar
│   └── Cooldown - Adiciona tempo de espera
│
└── Leaf Nodes (Ações/Condições)
    ├── Actions - MoveToTarget, Attack, Hide
    └── Conditions - IsPlayerVisible, IsHealthLow
```

#### Blackboard System

Sistema de memória compartilhada entre nós da árvore:

```csharp
public class Blackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public void SetValue<T>(string key, T value)
    {
        data[key] = value;
    }

    public T GetValue<T>(string key)
    {
        return (T)data[key];
    }
}

// Uso
blackboard.SetValue("targetPlayer", playerTransform);
blackboard.SetValue("isAggressive", true);
blackboard.SetValue("lastKnownPosition", Vector3.zero);
```

#### Exemplo: Inimigo Stalker

```
Root (Selector)
├── Is Player Dead? → Celebrate
├── Sequence: Hunt Player
│   ├── Can See Player?
│   ├── Selector: Choose Tactic
│   │   ├── Sequence: Direct Attack
│   │   │   ├── Is Player Weak?
│   │   │   └── Charge At Player
│   │   └── Sequence: Stealth Approach
│   │       ├── Find Cover
│   │       ├── Move To Cover
│   │       └── Wait For Opportunity
│   └── Attack
└── Sequence: Search Mode
    ├── Move To Last Known Position
    ├── Look Around
    └── Patrol Area
```

#### Implementação

```csharp
public class BTEnemy : EnemyBase
{
    private BehaviorTree behaviorTree;
    private Blackboard blackboard;

    private void Start()
    {
        blackboard = new Blackboard();
        behaviorTree = BuildTree();
    }

    protected override void UpdateAI()
    {
        blackboard.SetValue("playerTransform", player.transform);
        blackboard.SetValue("currentHealth", currentHealth);

        behaviorTree.Tick();
    }

    private BehaviorTree BuildTree()
    {
        // Root selector - tenta comportamentos em ordem
        return new BehaviorTree(
            new Selector(
                // Comportamento 1: Atacar se possível
                new Sequence(
                    new ConditionNode(() => CanSeePlayer()),
                    new ConditionNode(() => IsInAttackRange()),
                    new ActionNode(Attack)
                ),
                // Comportamento 2: Perseguir
                new Sequence(
                    new ConditionNode(() => CanSeePlayer()),
                    new ActionNode(ChasePlayer)
                ),
                // Comportamento 3: Investigar
                new Sequence(
                    new ConditionNode(() => HasLastKnownPosition()),
                    new ActionNode(Investigate)
                ),
                // Comportamento 4: Patrulhar
                new ActionNode(Patrol)
            )
        );
    }
}
```

---

### 3. Utility AI

#### O que é?
Sistema baseado em scoring onde ações são avaliadas por múltiplos fatores e a melhor pontuação é escolhida.

#### Por que usar em Horror?
- **Decisões nuanceadas**: Considera múltiplos fatores simultaneamente
- **Comportamento realista**: IA "pesa" opções como humanos
- **Fácil balanceamento**: Ajuste pesos para tuning
- **Responsivo**: Reage rapidamente a mudanças

#### Arquitetura

```
Utility AI System
├── Considerations (Fatores)
│   ├── PlayerDistanceConsideration
│   ├── HealthConsideration
│   ├── NoiseLevelConsideration
│   └── AllyProximityConsideration
│
├── Actions (Ações Pontuadas)
│   ├── AttackAction
│   │   ├── Score = Distance * Health * Aggression
│   │   └── Execute()
│   ├── FleeAction
│   │   ├── Score = (1 - Health) * PlayerDistance
│   │   └── Execute()
│   └── HideAction
│       ├── Score = NearbyHidingSpots * (1 - Distance)
│       └── Execute()
│
└── AI Brain (Decisor)
    └── Select Highest Score
```

#### Considerations (Fatores de Decisão)

```csharp
public abstract class Consideration
{
    public AnimationCurve responseCurve;

    public abstract float Evaluate(GameObject agent);
}

// Exemplo: Distância do player
public class DistanceConsideration : Consideration
{
    public override float Evaluate(GameObject agent)
    {
        float distance = Vector3.Distance(agent.transform.position, player.position);
        float normalizedDistance = distance / maxDetectionRange;

        // Use curva para mapear distância para score (0-1)
        return responseCurve.Evaluate(normalizedDistance);
    }
}

// Exemplo: Nível de saúde
public class HealthConsideration : Consideration
{
    public override float Evaluate(GameObject agent)
    {
        var health = agent.GetComponent<EnemyBase>();
        float healthPercent = health.currentHealth / health.maxHealth;

        return responseCurve.Evaluate(healthPercent);
    }
}
```

#### Actions com Scoring

```csharp
public class UtilityAction
{
    public string actionName;
    public List<Consideration> considerations;

    public float CalculateScore(GameObject agent)
    {
        float score = 1.0f;

        foreach (var consideration in considerations)
        {
            float considerationScore = consideration.Evaluate(agent);
            score *= considerationScore;
        }

        return score;
    }

    public abstract void Execute(GameObject agent);
}

// Exemplo: Ação de Ataque
public class AttackAction : UtilityAction
{
    public AttackAction()
    {
        actionName = "Attack";
        considerations = new List<Consideration>
        {
            new DistanceConsideration(),  // Perto = alto score
            new HealthConsideration(),    // Alta vida = alto score
            new LineOfSightConsideration() // Visível = alto score
        };
    }

    public override void Execute(GameObject agent)
    {
        agent.GetComponent<EnemyBase>().Attack();
    }
}
```

#### AI Brain (Cérebro da IA)

```csharp
public class UtilityAIBrain : MonoBehaviour
{
    public List<UtilityAction> availableActions;

    private void Update()
    {
        // Calcular score de todas as ações
        UtilityAction bestAction = null;
        float bestScore = 0f;

        foreach (var action in availableActions)
        {
            float score = action.CalculateScore(gameObject);

            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        // Executar melhor ação
        if (bestAction != null && bestScore > 0.1f)
        {
            bestAction.Execute(gameObject);
        }
    }
}
```

#### Exemplo de Uso em Horror

```csharp
public class UtilityHorrorEnemy : EnemyBase
{
    private UtilityAIBrain brain;

    protected override void Start()
    {
        base.Start();
        SetupUtilityAI();
    }

    private void SetupUtilityAI()
    {
        brain = gameObject.AddComponent<UtilityAIBrain>();

        // Ação: Ataque Direto
        var attackAction = new AttackAction();
        attackAction.considerations.Add(
            new DistanceConsideration { responseCurve = closeDistanceCurve }
        );
        attackAction.considerations.Add(
            new HealthConsideration { responseCurve = highHealthCurve }
        );

        // Ação: Emboscar
        var ambushAction = new AmbushAction();
        ambushAction.considerations.Add(
            new PlayerAwarenessConsideration() // Player não sabe de nós
        );
        ambushAction.considerations.Add(
            new CoverAvailabilityConsideration() // Tem cobertura próxima
        );

        // Ação: Fugir
        var fleeAction = new FleeAction();
        fleeAction.considerations.Add(
            new HealthConsideration { responseCurve = lowHealthCurve }
        );
        fleeAction.considerations.Add(
            new PlayerStrengthConsideration() // Player está forte
        );

        brain.availableActions = new List<UtilityAction>
        {
            attackAction,
            ambushAction,
            fleeAction
        };
    }
}
```

---

## ⚡ Sistemas de Performance

### 1. Improved Timers

#### O que é?
Sistema de timers integrado ao Unity Player Loop, eliminando necessidade de Update() manual.

#### Vantagens
- **Auto-gerenciado**: Não precisa chamar Update()
- **Performance**: Integrado ao engine loop
- **Tipos múltiplos**: Countdown, Frequency, Stopwatch

#### Uso em Horror

```csharp
using UnityTimer;

public class HorrorMechanics : MonoBehaviour
{
    // Timer de countdown para habilidades
    private CountdownTimer attackCooldown;

    // Timer de frequência para checagens periódicas
    private FrequencyTimer detectionCheck;

    // Timer de cronômetro para tracking
    private StopwatchTimer survivalTime;

    private void Start()
    {
        // Cooldown de ataque (5 segundos)
        attackCooldown = new CountdownTimer(5f);
        attackCooldown.OnTimerStop += () => Debug.Log("Pode atacar!");

        // Checar por inimigos 2x por segundo
        detectionCheck = new FrequencyTimer(2);
        detectionCheck.OnTimerStop += CheckForEnemies;

        // Tempo de sobrevivência
        survivalTime = new StopwatchTimer();
        survivalTime.Start();

        attackCooldown.Start();
        detectionCheck.Start();
    }

    public void TryAttack()
    {
        if (attackCooldown.IsFinished)
        {
            Attack();
            attackCooldown.Reset();
            attackCooldown.Start();
        }
    }

    private void CheckForEnemies()
    {
        // Lógica de detecção
    }
}
```

#### Exemplos Práticos

**1. Sistema de Bateria da Lanterna**
```csharp
private CountdownTimer batteryTimer;

void Start()
{
    batteryTimer = new CountdownTimer(60f); // 60 segundos de bateria
    batteryTimer.OnTimerStop += OnBatteryDepleted;
}

void ToggleFlashlight(bool on)
{
    if (on && !batteryTimer.IsRunning)
        batteryTimer.Start();
    else if (!on && batteryTimer.IsRunning)
        batteryTimer.Pause();
}
```

**2. Sistema de Spawn de Inimigos**
```csharp
private FrequencyTimer spawnTimer;

void Start()
{
    spawnTimer = new FrequencyTimer(0.5f); // 1 spawn a cada 2 segundos
    spawnTimer.OnTimerStop += SpawnEnemy;
    spawnTimer.Start();
}
```

**3. Efeito de Tensão Temporário**
```csharp
void OnEnemyDetected()
{
    var tensionTimer = new CountdownTimer(10f);
    tensionTimer.OnTimerStart += () => SetTension(1.0f);
    tensionTimer.OnTimerStop += () => SetTension(0.3f);
    tensionTimer.Start();
}
```

---

### 2. Octrees para Otimização Espacial

#### O que é?
Estrutura de dados que divide espaço 3D em octantes para buscas espaciais eficientes.

#### Uso em Horror

**1. Busca de Inimigos Próximos**
```csharp
public class EnemyOctree : MonoBehaviour
{
    private Octree<EnemyBase> enemyOctree;

    void Start()
    {
        // Criar octree cobrindo área do nível
        Bounds worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000f);
        enemyOctree = new Octree<EnemyBase>(worldBounds, maxDepth: 5);
    }

    // Adicionar inimigo ao octree
    public void RegisterEnemy(EnemyBase enemy)
    {
        enemyOctree.Insert(enemy, enemy.transform.position);
    }

    // Buscar inimigos perto do player (muito mais rápido)
    public List<EnemyBase> GetNearbyEnemies(Vector3 position, float radius)
    {
        return enemyOctree.GetNearby(position, radius);
    }
}
```

**2. Sistema de Atmosfera Otimizado**
```csharp
// Antes (O(n) - checa todos inimigos)
void UpdateTension()
{
    foreach (var enemy in allEnemies) // LENTO se muitos inimigos
    {
        float distance = Vector3.Distance(player.position, enemy.position);
        if (distance < detectionRadius)
            tension += 0.1f;
    }
}

// Depois (O(log n) - usa octree)
void UpdateTension()
{
    var nearbyEnemies = enemyOctree.GetNearby(player.position, detectionRadius);
    tension = nearbyEnemies.Count * 0.1f;
}
```

---

### 3. Batch Raycasting

#### O que é?
Sistema que agrupa múltiplos raycasts em um único job, melhorando performance drasticamente.

#### Uso em Horror

```csharp
using Unity.Jobs;
using Unity.Collections;

public class BatchedVisionSystem : MonoBehaviour
{
    private NativeArray<RaycastCommand> raycastCommands;
    private NativeArray<RaycastHit> raycastResults;

    void CheckMultipleEnemiesVision()
    {
        int enemyCount = enemies.Count;

        // Alocar arrays
        raycastCommands = new NativeArray<RaycastCommand>(enemyCount, Allocator.TempJob);
        raycastResults = new NativeArray<RaycastHit>(enemyCount, Allocator.TempJob);

        // Setup raycasts
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 origin = enemies[i].transform.position;
            Vector3 direction = (player.position - origin).normalized;

            raycastCommands[i] = new RaycastCommand(origin, direction, maxDistance);
        }

        // Executar todos de uma vez
        JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 32);
        handle.Complete();

        // Processar resultados
        for (int i = 0; i < enemyCount; i++)
        {
            if (raycastResults[i].collider != null)
            {
                if (raycastResults[i].collider.CompareTag("Player"))
                {
                    enemies[i].OnPlayerDetected();
                }
            }
        }

        // Limpar
        raycastCommands.Dispose();
        raycastResults.Dispose();
    }
}
```

---

## 🏗️ Sistemas de Arquitetura

### 1. Event Bus Pattern

#### Por que substituir GameEvents atual?

**Framework Atual:**
```csharp
// GameEvents.cs - eventos estáticos
public static Action<float, float> OnPlayerDamaged;
```

**Problemas:**
- Não é type-safe com eventos complexos
- Dificulta debugging
- Não suporta bem multiplayer (precisa distinguir qual player)

**Event Bus do adammyhre:**
```csharp
// 1. Definir evento
public struct PlayerDamagedEvent : IEvent
{
    public int playerId;
    public float damage;
    public float currentHealth;
    public Vector3 damageSource;
}

// 2. Registrar listener
void OnEnable()
{
    var binding = new EventBinding<PlayerDamagedEvent>(HandlePlayerDamage);
    EventBus<PlayerDamagedEvent>.Register(binding);
}

// 3. Disparar evento
EventBus<PlayerDamagedEvent>.Raise(new PlayerDamagedEvent
{
    playerId = GetPlayerNetworkId(),
    damage = 25f,
    currentHealth = 75f,
    damageSource = enemy.position
});

// 4. Handler
void HandlePlayerDamage(PlayerDamagedEvent evt)
{
    if (evt.playerId == localPlayerId)
    {
        UpdateHealthBar(evt.currentHealth);
    }
}
```

#### Vantagens para Multiplayer

```csharp
// Evento local vs rede
public struct NetworkPlayerEvent : IEvent
{
    public int networkId;
    public bool isLocal;
    public Vector3 position;
}

// Sistema pode filtrar facilmente
void HandlePlayerEvent(NetworkPlayerEvent evt)
{
    if (evt.isLocal)
    {
        // Processar localmente
    }
    else
    {
        // Replicar para rede
        SyncToNetwork(evt);
    }
}
```

---

### 2. Unity Utils (Extensões)

Repositório com 685 stars contendo métodos de extensão úteis:

```csharp
// Exemplos úteis para horror game
transform.LookAt2D(target); // Olhar em 2D (útil para sprites)
vector.RandomVariation(0.2f); // Adiciona variação aleatória
color.WithAlpha(0.5f); // Modificar alpha
gameObject.GetOrAddComponent<T>(); // Get ou add componente

// Útil para efeitos
Color damageColor = Color.red.WithAlpha(0.3f);
Vector3 spawnPos = basePosition.RandomVariation(2f);
```

---

## 🌐 Multiplayer e Networking

### Unity-Multiplayer-Kart Analysis

#### Networking Solution
- **Netcode for GameObjects** (NGO) - Solução oficial Unity
- **Server Authoritative** - Servidor controla tudo
- **Client Prediction** - Clientes predizem movimentos

#### Arquitetura para Horror Coop

```
Multiplayer Architecture
├── Network Manager
│   ├── Connection Management
│   ├── Player Spawning
│   └── Session Management
│
├── Server Authority
│   ├── Enemy AI (server-side only)
│   ├── Physics (server calculates)
│   ├── Health/Damage (server validates)
│   └── Game State (server is source of truth)
│
├── Client Prediction
│   ├── Movement (predict locally)
│   ├── Camera (fully local)
│   ├── Audio (local trigger)
│   └── VFX (local spawn)
│
└── Reconciliation
    ├── Server corrections
    ├── State synchronization
    └── Lag compensation
```

#### Exemplo: Multiplayer Enemy

```csharp
using Unity.Netcode;

public class MultiplayerEnemy : NetworkBehaviour
{
    // Sincronizado pela rede
    private NetworkVariable<float> networkHealth = new NetworkVariable<float>();
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Apenas servidor roda IA
            StartAI();
        }

        // Todos clientes visualizam
        networkHealth.OnValueChanged += OnHealthChanged;
    }

    void Update()
    {
        if (IsServer)
        {
            // Servidor atualiza lógica
            UpdateAI();
            networkPosition.Value = transform.position;
        }
        else
        {
            // Clientes interpolam posição
            transform.position = Vector3.Lerp(
                transform.position,
                networkPosition.Value,
                Time.deltaTime * 10f
            );
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, int attackerId)
    {
        // Servidor valida e aplica dano
        if (IsServer)
        {
            networkHealth.Value -= damage;

            if (networkHealth.Value <= 0)
            {
                DieClientRpc();
            }
        }
    }

    [ClientRpc]
    void DieClientRpc()
    {
        // Todos clientes reproduzem morte
        PlayDeathAnimation();
        PlayDeathSound();
    }
}
```

---

## 🎯 Priorização para Horror Coop

### Fase 1: Fundamentos (Semanas 1-2)
**Prioridade Crítica**

1. **Event Bus** (substitui GameEvents)
   - Necessário para multiplayer
   - Melhora arquitetura existente
   - Base para comunicação desacoplada

2. **Improved Timers**
   - Substitui lógica de timer manual
   - Performance melhorada
   - Mais limpo e manutenível

3. **Netcode for GameObjects**
   - Setup inicial de multiplayer
   - Player spawning
   - Basic networking

### Fase 2: IA Inteligente (Semanas 3-4)
**Prioridade Alta**

4. **GOAP ou Behavior Trees** (escolha um)
   - **GOAP** se quer inimigos super inteligentes e imprevisíveis
   - **Behavior Trees** se quer controle visual e debugging fácil
   - Implementar em 1-2 tipos de inimigos primeiro

5. **Multiplayer Enemy Sync**
   - Sincronizar IA pela rede
   - Server-side AI com client-side visuals
   - Otimização de bandwidth

### Fase 3: Otimização (Semanas 5-6)
**Prioridade Média**

6. **Octrees**
   - Otimizar busca de inimigos
   - Melhorar performance com muitos objetos
   - Spatial queries eficientes

7. **Batch Raycasting**
   - Otimizar detecção de visão
   - Line of sight em batch
   - Performance com múltiplos inimigos

### Fase 4: Polish (Semanas 7+)
**Prioridade Baixa**

8. **Utility AI** (opcional, se não usou)
   - Sistema complementar
   - Decisões nuanceadas
   - Pode combinar com GOAP/BT

9. **Inventory System**
   - Se precisar de inventário multiplayer
   - UI Toolkit based
   - Sincronização de rede

---

## 📦 Repositórios para Download

### Instalação Recomendada

1. **Unity-Event-Bus**
   ```
   https://github.com/adammyhre/Unity-Event-Bus
   ```
   Instale PRIMEIRO - substitui GameEvents.cs

2. **Unity-Improved-Timers**
   ```
   https://github.com/adammyhre/Unity-Improved-Timers
   ```
   Instale cedo - substitui lógica de timers

3. **Unity-GOAP** OU **Unity-Behaviour-Trees**
   ```
   https://github.com/adammyhre/Unity-GOAP
   https://github.com/adammyhre/Unity-Behaviour-Trees
   ```
   Escolha um para IA avançada

4. **Unity-Multiplayer-Kart** (para referência)
   ```
   https://github.com/adammyhre/Unity-Multiplayer-Kart
   ```
   Estude arquitetura de networking

5. **Unity-Utils** (útil sempre)
   ```
   https://github.com/adammyhre/Unity-Utils
   ```
   Extensões gerais

---

## 🎓 Recursos de Aprendizado

### YouTube @git-amend
Adam Myhre tem tutoriais detalhados de cada sistema:
- GOAP AI Tutorial
- Behavior Trees Explained
- Event Bus Architecture
- Netcode for GameObjects

### Documentação Oficial
- [Netcode for GameObjects Docs](https://docs-multiplayer.unity3d.com/)
- [Unity ML-Agents](https://unity.com/products/machine-learning-agents)

---

## 🚀 Próximo Passo

Leia o **MULTIPLAYER_ARCHITECTURE.md** para entender como implementar o sistema cooperativo completo.

Depois, consulte **ADVANCED_AI_SYSTEMS.md** para guias detalhados de implementação de IA.

Por fim, siga o **COOP_ROADMAP.md** para um plano de implementação passo a passo.

---

**Nota**: Todos esses sistemas são modulares e podem ser implementados incrementalmente. Não precisa fazer tudo de uma vez!
