# ⚡ Quick Reference - Cheat Sheet

Referência rápida para desenvolvimento do horror game cooperativo. Consulte quando precisar de snippets ou comandos específicos.

## 📚 Índice

- [Decisão Rápida: Qual Tecnologia Usar?](#decisão-rápida-qual-tecnologia-usar)
- [Git Commands](#git-commands)
- [Code Snippets](#code-snippets)
- [Network Patterns](#network-patterns)
- [AI Patterns](#ai-patterns)
- [Performance Tips](#performance-tips)
- [Debugging](#debugging)

---

## 🎯 Decisão Rápida: Qual Tecnologia Usar?

### Sistema de IA

```
Precisa de inimigos que PLANEJAM? (flanqueiam, coordenam, estratégias)
  → Use GOAP

Precisa de comportamentos HIERÁRQUICOS organizados?
  → Use Behavior Trees

Precisa de decisões baseadas em MÚLTIPLOS FATORES?
  → Use Utility AI

Comportamentos SIMPLES são suficientes?
  → Use FSM (já tem no framework)
```

### Quando Otimizar?

```
Muitos inimigos (10+) detectando player?
  → Use Octrees + Batch Raycasting

Vários timers/delays no código?
  → Use Improved Timers

Eventos entre sistemas desacoplados?
  → Use Event Bus

Latência/bandwidth problema?
  → Network optimization (Interest Management)
```

---

## 🔧 Git Commands

### Setup Inicial
```bash
# Clone do projeto
git clone https://github.com/ClaudioRobertoJr/GameDevTerror.git
cd GameDevTerror

# Criar branch para feature
git checkout -b feature/minha-feature

# Ver status
git status
```

### Workflow Diário
```bash
# Ver mudanças
git status
git diff

# Add e commit
git add .
git commit -m "feat: Descrição da feature"

# Push
git push -u origin feature/minha-feature

# Pull latest
git pull origin main
```

### Comandos Úteis
```bash
# Desfazer último commit (mantém arquivos)
git reset --soft HEAD~1

# Desfazer mudanças em arquivo
git restore arquivo.cs

# Ver histórico
git log --oneline --graph

# Limpar untracked files
git clean -fd
```

### Convenção de Commits
```
feat: Nova feature
fix: Bug fix
docs: Documentação
refactor: Refatoração
perf: Melhoria de performance
test: Testes
chore: Manutenção
```

---

## 💻 Code Snippets

### Event Bus

#### Criar Evento
```csharp
public struct MinhaEventoCustom : IEvent
{
    public int playerId;
    public float valor;
    public Vector3 posicao;
}
```

#### Disparar Evento
```csharp
EventBus<MinhaEventoCustom>.Raise(new MinhaEventoCustom
{
    playerId = 0,
    valor = 100f,
    posicao = transform.position
});
```

#### Escutar Evento
```csharp
private EventBinding<MinhaEventoCustom> binding;

void OnEnable()
{
    binding = new EventBinding<MinhaEventoCustom>(OnEvento);
    EventBus<MinhaEventoCustom>.Register(binding);
}

void OnDisable()
{
    EventBus<MinhaEventoCustom>.Deregister(binding);
}

void OnEvento(MinhaEventoCustom evt)
{
    Debug.Log($"Recebido: {evt.valor}");
}
```

---

### Improved Timers

#### Countdown Timer
```csharp
using UnityTimer;

private CountdownTimer cooldownTimer;

void Start()
{
    cooldownTimer = new CountdownTimer(5f); // 5 segundos
    cooldownTimer.OnTimerStop += OnCooldownComplete;
    cooldownTimer.Start();
}

void OnCooldownComplete()
{
    Debug.Log("Cooldown completo!");
}

void OnDestroy()
{
    cooldownTimer?.Stop();
}
```

#### Frequency Timer
```csharp
private FrequencyTimer checkTimer;

void Start()
{
    checkTimer = new FrequencyTimer(2); // 2x por segundo
    checkTimer.OnTimerStop += CheckSomething;
    checkTimer.Start();
}

void CheckSomething()
{
    // Executado 2x por segundo
}
```

#### Stopwatch Timer
```csharp
private StopwatchTimer survivalTimer;

void Start()
{
    survivalTimer = new StopwatchTimer();
    survivalTimer.Start();
}

void Update()
{
    float timeElapsed = survivalTimer.ElapsedTime;
    Debug.Log($"Sobreviveu por {timeElapsed}s");
}
```

---

### Netcode for GameObjects

#### NetworkBehaviour Básico
```csharp
using Unity.Netcode;

public class MeuScript : NetworkBehaviour
{
    private NetworkVariable<int> netHealth = new NetworkVariable<int>(100);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Este é o player local
        }
        else
        {
            // Este é um player remoto
        }

        netHealth.OnValueChanged += OnHealthChanged;
    }

    void OnHealthChanged(int oldVal, int newVal)
    {
        Debug.Log($"Health: {oldVal} -> {newVal}");
    }
}
```

#### ServerRpc
```csharp
// Cliente chama, servidor executa
[ServerRpc]
void DoSomethingServerRpc(int value)
{
    Debug.Log($"[Server] Recebido: {value}");
}

// Chamar
void Update()
{
    if (IsOwner && Input.GetKeyDown(KeyCode.E))
    {
        DoSomethingServerRpc(42);
    }
}
```

#### ClientRpc
```csharp
// Servidor chama, todos clientes executam
[ClientRpc]
void NotifyClientsClientRpc(string message)
{
    Debug.Log($"[Client] {message}");
}

// Chamar (apenas no servidor)
void SomeServerMethod()
{
    if (IsServer)
    {
        NotifyClientsClientRpc("Evento importante!");
    }
}
```

#### NetworkVariable
```csharp
// Auto-sincronizada
private NetworkVariable<float> netPosition = new NetworkVariable<float>();

// Apenas servidor pode escrever (padrão)
private NetworkVariable<int> serverOnly = new NetworkVariable<int>(
    writePerm: NetworkVariableWritePermission.Server
);

// Owner pode escrever
private NetworkVariable<string> ownerCanWrite = new NetworkVariable<string>(
    writePerm: NetworkVariableWritePermission.Owner
);
```

---

### Behavior Trees

#### Estrutura Básica
```csharp
using HorrorGame.AI.BehaviorTree;

private BehaviorTree behaviorTree;

void Start()
{
    behaviorTree = new BehaviorTree(
        new BTSelector( // Root: tenta em ordem
            // Comportamento 1
            new BTSequence(
                new Condition1(),
                new Action1()
            ),
            // Comportamento 2 (fallback)
            new Action2()
        )
    );
}

void Update()
{
    behaviorTree.Tick();
}
```

#### Condition Node
```csharp
public class CanSeePlayerCondition : BTNode
{
    private Transform player;

    public CanSeePlayerCondition(Transform player)
    {
        this.player = player;
    }

    public override NodeState Evaluate()
    {
        bool canSee = CheckLineOfSight();
        state = canSee ? NodeState.Success : NodeState.Failure;
        return state;
    }

    bool CheckLineOfSight()
    {
        // Implementar lógica
        return true;
    }
}
```

#### Action Node
```csharp
public class ChasePlayerAction : BTNode
{
    private NavMeshAgent agent;
    private Transform target;

    public ChasePlayerAction(NavMeshAgent agent, Transform target)
    {
        this.agent = agent;
        this.target = target;
    }

    public override NodeState Evaluate()
    {
        agent.SetDestination(target.position);
        state = NodeState.Running;
        return state;
    }
}
```

---

### GOAP

#### World State
```csharp
using HorrorGame.AI.GOAP;

WorldState worldState = new WorldState();

// Set states
worldState.SetState("playerVisible", true);
worldState.SetState("healthLow", false);
worldState.SetState("hasWeapon", true);

// Get states
bool playerVisible = worldState.GetState<bool>("playerVisible");
```

#### Goal
```csharp
GOAPGoal huntGoal = new GOAPGoal("HuntPlayer", priority: 10f);
huntGoal.AddDesiredState("playerDead", true);
```

#### Action
```csharp
public class AttackAction : GOAPAction
{
    public AttackAction(GameObject agent) : base(agent)
    {
        actionName = "Attack";
        cost = 1f;

        // Precondições
        preconditions["nearPlayer"] = true;
        preconditions["hasWeapon"] = true;

        // Efeitos
        effects["playerDead"] = true;
    }

    public override bool Execute()
    {
        // Executar ataque
        return true; // Completou
    }

    public override bool IsComplete()
    {
        return true;
    }
}
```

---

## 🌐 Network Patterns

### Pattern: Client Prediction + Server Authority

```csharp
public class NetworkPlayer : NetworkBehaviour
{
    private Vector3 serverPosition;

    void Update()
    {
        if (IsOwner)
        {
            // Mover localmente (predição)
            transform.position += GetInput() * speed * Time.deltaTime;

            // Enviar para servidor
            UpdatePositionServerRpc(transform.position);
        }
        else
        {
            // Interpolar posição de outros players
            transform.position = Vector3.Lerp(
                transform.position,
                serverPosition,
                Time.deltaTime * 10f
            );
        }
    }

    [ServerRpc]
    void UpdatePositionServerRpc(Vector3 pos)
    {
        // Servidor valida e aceita
        serverPosition = pos;

        // Atualiza para outros clientes
        UpdatePositionClientRpc(pos);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(Vector3 pos)
    {
        if (!IsOwner)
        {
            serverPosition = pos;
        }
    }
}
```

### Pattern: Server Validates Damage

```csharp
// ERRADO - Cliente pode hackear
public void TakeDamage(int damage)
{
    health -= damage; // ❌ Cliente decide
}

// CORRETO - Servidor valida
[ServerRpc(RequireOwnership = false)]
public void TakeDamageServerRpc(int damage, ulong attackerId)
{
    if (!IsServer) return;

    // Servidor valida
    if (IsValidAttack(attackerId))
    {
        health -= damage;

        // Notifica todos
        UpdateHealthClientRpc(health);
    }
}

[ClientRpc]
void UpdateHealthClientRpc(int newHealth)
{
    health = newHealth;
    // Atualizar UI
}
```

---

## 🤖 AI Patterns

### Pattern: State Machine (Simples)

```csharp
public enum EnemyState { Idle, Patrol, Chase, Attack }

private EnemyState currentState = EnemyState.Idle;

void Update()
{
    switch (currentState)
    {
        case EnemyState.Idle:
            IdleLogic();
            break;
        case EnemyState.Patrol:
            PatrolLogic();
            break;
        case EnemyState.Chase:
            ChaseLogic();
            break;
        case EnemyState.Attack:
            AttackLogic();
            break;
    }
}

void ChangeState(EnemyState newState)
{
    OnStateExit(currentState);
    currentState = newState;
    OnStateEnter(newState);
}
```

### Pattern: Singleton Manager

```csharp
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

// Uso
EnemyManager.Instance.DoSomething();
```

---

## ⚡ Performance Tips

### Object Pooling

```csharp
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

### Evitar no Update()

```csharp
// ❌ ERRADO - muito caro
void Update()
{
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    EnemyBase enemy = GetComponent<EnemyBase>();
}

// ✅ CORRETO - cachear
private GameObject player;
private EnemyBase enemy;

void Start()
{
    player = GameObject.FindGameObjectWithTag("Player");
    enemy = GetComponent<EnemyBase>();
}

void Update()
{
    // Usar as variáveis cacheadas
}
```

### Octree para Busca Espacial

```csharp
// ❌ ERRADO - O(n)
List<Enemy> GetNearbyEnemies(Vector3 position, float radius)
{
    List<Enemy> nearby = new List<Enemy>();
    foreach (var enemy in allEnemies) // Checa TODOS
    {
        if (Vector3.Distance(position, enemy.position) < radius)
            nearby.Add(enemy);
    }
    return nearby;
}

// ✅ CORRETO - O(log n) com Octree
List<Enemy> GetNearbyEnemies(Vector3 position, float radius)
{
    return enemyOctree.GetNearby(position, radius);
}
```

---

## 🐛 Debugging

### Network Debugging

```csharp
// Habilitar logs de rede
NetworkManager.Singleton.LogLevel = LogLevel.Developer;

// Log detalhado
Debug.Log($"[{(IsServer ? "Server" : "Client")}] IsOwner: {IsOwner}");
```

### Gizmos para Visualização

```csharp
void OnDrawGizmos()
{
    // Range de detecção
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRange);

    // FOV
    Gizmos.color = Color.red;
    Vector3 forward = transform.forward * detectionRange;
    Gizmos.DrawLine(transform.position, transform.position + forward);
}

void OnDrawGizmosSelected()
{
    // Apenas quando selecionado
    Gizmos.color = Color.blue;
    Gizmos.DrawWireSphere(transform.position, attackRange);
}
```

### Labels em Gizmos

```csharp
void OnDrawGizmos()
{
    #if UNITY_EDITOR
    UnityEditor.Handles.Label(
        transform.position + Vector3.up * 2f,
        $"State: {currentState}\nHealth: {health}"
    );
    #endif
}
```

### Profiling

```csharp
using UnityEngine.Profiling;

void ExpensiveMethod()
{
    Profiler.BeginSample("My Expensive Method");

    // Código caro aqui

    Profiler.EndSample();
}
```

---

## 🎮 Input Handling

### Novo Input System

```csharp
using UnityEngine.InputSystem;

// No Inspector, configurar Input Action Asset
[SerializeField] private InputActionReference moveAction;
[SerializeField] private InputActionReference jumpAction;

void OnEnable()
{
    jumpAction.action.performed += OnJump;
}

void OnDisable()
{
    jumpAction.action.performed -= OnJump;
}

void Update()
{
    Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
}

void OnJump(InputAction.CallbackContext context)
{
    Debug.Log("Jump!");
}
```

### Old Input System

```csharp
void Update()
{
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");

    if (Input.GetKeyDown(KeyCode.Space))
    {
        Jump();
    }

    if (Input.GetButtonDown("Fire1"))
    {
        Shoot();
    }
}
```

---

## 🔍 Common Patterns

### Singleton Pattern

```csharp
public class MyManager : MonoBehaviour
{
    public static MyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

### Observer Pattern (com Event Bus)

```csharp
// Publisher
EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent { playerId = 0 });

// Subscriber
private EventBinding<PlayerDiedEvent> binding;

void OnEnable()
{
    binding = new EventBinding<PlayerDiedEvent>(OnPlayerDied);
    EventBus<PlayerDiedEvent>.Register(binding);
}

void OnPlayerDied(PlayerDiedEvent evt)
{
    // React
}
```

### Coroutine Pattern

```csharp
// Iniciar
StartCoroutine(MyCoroutine());

// Coroutine
IEnumerator MyCoroutine()
{
    Debug.Log("Início");

    yield return new WaitForSeconds(2f);

    Debug.Log("Depois de 2 segundos");

    yield return new WaitUntil(() => condition);

    Debug.Log("Quando condition for true");
}

// Parar
StopCoroutine(MyCoroutine());
StopAllCoroutines();
```

---

## 📱 UI Quick Tips

### Atualizar UI de forma performática

```csharp
// ❌ ERRADO - todo frame
void Update()
{
    healthText.text = $"HP: {health}";
}

// ✅ CORRETO - apenas quando muda
private int lastHealth;

void Update()
{
    if (health != lastHealth)
    {
        healthText.text = $"HP: {health}";
        lastHealth = health;
    }
}

// ✅ AINDA MELHOR - com eventos
void OnEnable()
{
    EventBus<PlayerDamagedEvent>.Register(binding);
}

void OnHealthChanged(PlayerDamagedEvent evt)
{
    healthText.text = $"HP: {evt.currentHealth}";
}
```

---

## 🎨 Shader/Material Tips

### Modificar material em runtime

```csharp
// IMPORTANTE: Criar instância para não modificar asset
private Material materialInstance;

void Start()
{
    materialInstance = GetComponent<Renderer>().material;
}

void Update()
{
    // Modificar cor
    materialInstance.color = Color.Lerp(Color.white, Color.red, tension);

    // Modificar propriedade
    materialInstance.SetFloat("_Intensity", intensity);
}
```

---

## 🔧 Unity Editor Extensions

### MenuItem Customizado

```csharp
using UnityEditor;

public class MyEditorTools
{
    [MenuItem("Tools/Do Something")]
    static void DoSomething()
    {
        Debug.Log("Tool executada!");
    }

    [MenuItem("Tools/Do Something", true)]
    static bool ValidateDoSomething()
    {
        // Retorna true se menu deve estar habilitado
        return Selection.activeGameObject != null;
    }
}
```

---

## 📋 Checklist de Build

### Antes de fazer Build

```
Performance:
□ FPS >= 60 em cenas principais
□ Memory usage < 2GB
□ No memory leaks (Profiler)

Networking (se multiplayer):
□ Server authority funcionando
□ Nenhum cheat possível
□ Testado com latência simulada

Quality:
□ Sem warnings no Console
□ Sem TODOs críticos
□ Todos assets têm referências corretas

Testing:
□ Todas features testadas
□ Multiplayer testado (2-4 players)
□ Testar build (não só Editor)
```

---

## 🚀 Comandos Unity Úteis

### Via Script

```csharp
// Carregar cena
UnityEngine.SceneManagement.SceneManager.LoadScene("NomeDaCena");

// Quit
Application.Quit();

// Pausar
Time.timeScale = 0f; // Pausado
Time.timeScale = 1f; // Normal

// Cursor
Cursor.visible = false;
Cursor.lockState = CursorLockMode.Locked;

// Quality Settings
QualitySettings.vSyncCount = 0;
Application.targetFrameRate = 60;
```

---

## 💾 PlayerPrefs Quick Reference

```csharp
// Save
PlayerPrefs.SetInt("HighScore", 100);
PlayerPrefs.SetFloat("MasterVolume", 0.8f);
PlayerPrefs.SetString("PlayerName", "John");
PlayerPrefs.Save(); // Força salvar imediatamente

// Load
int score = PlayerPrefs.GetInt("HighScore", defaultValue: 0);
float volume = PlayerPrefs.GetFloat("MasterVolume", defaultValue: 1f);
string name = PlayerPrefs.GetString("PlayerName", defaultValue: "Player");

// Check
if (PlayerPrefs.HasKey("HighScore"))
{
    // Existe
}

// Delete
PlayerPrefs.DeleteKey("HighScore");
PlayerPrefs.DeleteAll(); // Cuidado!
```

---

## 🎯 Quick Decision Trees

### "Meu código está lento, o que fazer?"

```
1. Abrir Profiler (Ctrl+7)
2. Identificar hotspot
3. Se for:
   - GetComponent no Update → Cachear
   - Find/FindObjectOfType → Cachear ou usar manager
   - Instantiate → Object pooling
   - Raycasts múltiplos → Batch raycasting
   - Busca espacial → Octree
   - Muitas coroutines → Timers ou Job System
```

### "Network não sincroniza, o que checar?"

```
1. NetworkManager na cena?
2. NetworkObject no prefab?
3. Prefab está em NetworkPrefabsList?
4. Usando NetworkVariable ou RPC?
5. Server está validando?
6. Logs de rede habilitados?
```

### "IA não funciona, o que checar?"

```
1. NavMesh baked?
2. NavMeshAgent no GameObject?
3. Player tem tag "Player"?
4. Layer masks configurados?
5. Distância de detecção OK?
6. Gizmos visualizando corretamente?
```

---

## 📚 Comandos de Consulta Rápida

### Achar rapidamente nos documentos

```bash
# No terminal/VSCode
grep -r "NetworkVariable" *.md
grep -r "GOAP" *.md

# Ou use Ctrl+Shift+F no VSCode
```

### Links Diretos

- **GOAP detalhado**: ADVANCED_AI_SYSTEMS.md linha 100+
- **Behavior Trees**: ADVANCED_AI_SYSTEMS.md linha 800+
- **Multiplayer setup**: MULTIPLAYER_ARCHITECTURE.md linha 50+
- **Event Bus**: ADVANCED_TECHNOLOGIES.md linha 600+
- **Roadmap**: COOP_ROADMAP.md

---

## 🎓 Recursos Rápidos

### Documentação Oficial
- [Unity Docs](https://docs.unity3d.com/)
- [Netcode Docs](https://docs-multiplayer.unity3d.com/)
- [Unity Learn](https://learn.unity.com/)

### Adam Myhre
- YouTube: [@git-amend](https://www.youtube.com/@git-amend)
- GitHub: [adammyhre](https://github.com/adammyhre)

---

**📌 Dica**: Imprima ou mantenha este documento aberto enquanto desenvolve!

**🔖 Favorito**: Marque esta página para consulta rápida!
