# 🏗️ Estrutura Detalhada do Projeto

Este documento descreve a arquitetura e organização do código do Horror Game Framework.

## 📐 Arquitetura

### Padrões de Design Utilizados

#### 1. Singleton Pattern
**Onde:** GameManager, AudioManager, SceneController

**Por quê:** Garante única instância e acesso global a sistemas críticos.

```csharp
public static GameManager Instance { get; private set; }

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
```

#### 2. Observer Pattern (Event System)
**Onde:** GameEvents

**Por quê:** Permite comunicação desacoplada entre sistemas.

```csharp
// Publisher
GameEvents.OnPlayerDamaged?.Invoke(damage, currentHealth);

// Subscriber
GameEvents.OnPlayerDamaged += HandleDamage;
```

#### 3. State Machine
**Onde:** Enemy AI (EnemyAIState)

**Por quê:** Gerencia comportamentos complexos de IA de forma organizada.

```csharp
private void ChangeState(EnemyAIState newState)
{
    OnStateExit(currentState);
    currentState = newState;
    OnStateEnter(newState);
}
```

#### 4. Object Pooling
**Onde:** AudioManager (AudioSources)

**Por quê:** Reduz alocação/garbage collection para objetos frequentemente criados.

```csharp
private Queue<AudioSource> sfxSourcePool = new Queue<AudioSource>();
```

#### 5. Template Method Pattern
**Onde:** EnemyBase (classe abstrata)

**Por quê:** Define esqueleto do algoritmo, permitindo subclasses implementarem detalhes.

```csharp
protected abstract void UpdateAI();
protected abstract void OnAttack();
```

---

## 🔄 Fluxo de Dados

### Inicialização do Jogo

```
Awake()
├── GameManager.Instance criado
├── AudioManager.Instance criado
├── SceneController.Instance criado
└── Componentes do Player inicializados

Start()
├── GameManager.Initialize()
├── Eventos registrados
└── Estado inicial definido (MainMenu ou Playing)
```

### Loop de Jogo

```
Update()
├── Input Processing
│   ├── PlayerMovement (WASD)
│   ├── MouseLook (Mouse)
│   ├── Actions (E, F, Space, etc)
│   └── Pause (ESC)
│
├── AI Updates
│   ├── Enemy Detection
│   ├── State Machine
│   └── NavMesh Movement
│
├── Systems Updates
│   ├── Stamina regeneration
│   ├── Health regeneration
│   ├── Battery drain
│   └── Atmosphere tension
│
└── Rendering
    ├── Camera effects
    ├── UI updates
    └── Visual feedback
```

### Ciclo de Eventos

```
Ação → Evento Disparado → Subscribers Notificados → Reação

Exemplo:
Player toma dano
    → GameEvents.OnPlayerDamaged(damage, health)
    → PlayerHUD atualiza barra de vida
    → AudioManager toca som de dano
    → Camera shake aplicado
    → Damage vignette mostrada
```

---

## 📦 Dependências Entre Sistemas

### Grafo de Dependências

```
GameManager (root)
├── SceneController
├── GameEvents (static)
└── Cursor/Time control

AudioManager
├── GameEvents (listener)
└── Resources/Audio/

Player Systems
├── FirstPersonController
│   ├── CharacterController (Unity)
│   ├── GameManager (pause check)
│   └── GameEvents (stamina events)
│
├── MouseLook
│   ├── Camera
│   └── GameManager (pause check)
│
├── PlayerHealth
│   ├── MouseLook (camera shake)
│   ├── GameEvents (damage/heal events)
│   └── GameManager (game over)
│
└── PlayerInteraction
    ├── IInteractable (interface)
    ├── Camera (raycast)
    └── GameEvents (interact events)

Enemy Systems
├── EnemyBase (abstract)
│   ├── NavMeshAgent (Unity)
│   ├── GameEvents (spawn/death events)
│   └── Player (target reference)
│
└── PatrolEnemy : EnemyBase
    ├── Waypoints
    └── State Machine

Environment Systems
├── Door : IInteractable
│   └── GameEvents (door toggle)
│
├── Collectible : IInteractable
│   ├── PlayerHealth (healing)
│   └── GameEvents (collect events)
│
├── EnemySpawner
│   ├── Enemy Prefabs
│   ├── Spawn Points
│   └── GameEvents (enemy spawn)
│
└── AtmosphereController
    ├── Light
    ├── RenderSettings (fog)
    ├── GameEvents (tension events)
    └── Physics.OverlapSphere (enemy detection)

UI Systems
├── PlayerHUD
│   ├── GameEvents (health/stamina listeners)
│   └── UI Elements
│
└── PauseMenu
    ├── GameManager (pause/resume)
    └── UI Elements
```

---

## 🔌 Interfaces

### IInteractable
```csharp
public interface IInteractable
{
    string GetInteractionPrompt();
    bool CanInteract();
    void Interact(GameObject player);
}
```

**Implementações:**
- Door
- Collectible
- Checkpoint (parcialmente)
- *Você pode criar mais!*

---

## 📨 Sistema de Eventos

### Eventos Disponíveis

#### Game State Events
- `OnGameStateChanged(GameState prev, GameState new)` - Estado do jogo mudou
- `OnGamePaused()` - Jogo pausado
- `OnGameResumed()` - Jogo retomado

#### Player Events
- `OnPlayerDamaged(float damage, float currentHealth)` - Player tomou dano
- `OnPlayerDied()` - Player morreu
- `OnPlayerHealed(float amount, float currentHealth)` - Player curado
- `OnStaminaChanged(float current, float max)` - Stamina mudou
- `OnPlayerInteract(GameObject target)` - Player interagiu

#### Enemy Events
- `OnEnemySpawned(GameObject enemy)` - Inimigo spawnado
- `OnEnemyDied(GameObject enemy)` - Inimigo morreu
- `OnEnemyDetectedPlayer(GameObject enemy)` - Inimigo detectou player
- `OnEnemyLostPlayer(GameObject enemy)` - Inimigo perdeu player
- `OnEnemyAttack(GameObject enemy)` - Inimigo atacou

#### Audio Events
- `OnPlaySound2D(string soundName)` - Tocar som 2D
- `OnPlaySound3D(string soundName, Vector3 position)` - Tocar som 3D
- `OnChangeMusicTrack(string trackName)` - Mudar música
- `OnTensionLevelChanged(float level)` - Nível de tensão mudou

#### Environment Events
- `OnDoorToggled(GameObject door, bool isOpen)` - Porta aberta/fechada
- `OnItemCollected(GameObject item)` - Item coletado
- `OnCheckpointReached(Vector3 position)` - Checkpoint atingido

#### UI Events
- `OnShowMessage(string message, float duration)` - Mostrar mensagem
- `OnObjectiveUpdated(string objective)` - Objetivo atualizado

---

## 🎮 Estados do Jogo

### GameState Enum

```csharp
public enum GameState
{
    MainMenu,   // Menu principal
    Loading,    // Carregando cena
    Playing,    // Jogando
    Paused,     // Pausado
    GameOver,   // Game over
    Cutscene    // Cutscene ativa
}
```

### Transições de Estado

```
MainMenu
    ↓ (StartNewGame)
Loading
    ↓ (Scene loaded)
Playing
    ↓ (ESC)          ↓ (Player died)
Paused          GameOver
    ↓ (Resume)       ↓ (Retry)
Playing          Loading
```

---

## 🤖 Estados da IA

### EnemyAIState Enum

```csharp
public enum EnemyAIState
{
    Idle,        // Parado
    Patrol,      // Patrulhando
    Investigate, // Investigando
    Chase,       // Perseguindo
    Attack,      // Atacando
    Stunned,     // Atordoado
    Retreat,     // Recuando
    Dead         // Morto
}
```

### Máquina de Estados do PatrolEnemy

```
Idle/Patrol
    ↓ (Player detected)
Chase
    ↓ (In range)          ↓ (Lost target)
Attack              Investigate
    ↓ (Out of range)      ↓ (Timeout)
Chase               Patrol
```

---

## 📊 ScriptableObjects

### EnemyData
Configuração reutilizável de inimigos.

**Campos:**
- Stats (health, damage, ranges)
- Movement (speeds)
- Detection (FOV, ranges)
- Audio (sound names)
- Behavior (aggression, patrol)

**Uso:**
```csharp
[SerializeField] private EnemyData enemyData;

void Start()
{
    maxHealth = enemyData.maxHealth;
    damage = enemyData.damage;
    // etc...
}
```

### GameSettings
Configurações globais do jogo.

**Campos:**
- Graphics (quality, FPS)
- Audio (volumes)
- Gameplay (difficulty, multipliers)
- Controls (sensitivity, invert Y)
- UI (show FPS, hints)

---

## 🔊 Sistema de Áudio

### Estrutura de Recursos

```
Resources/
└── Audio/
    ├── Music/
    │   ├── main_theme.mp3
    │   └── chase_theme.mp3
    │
    ├── SFX/
    │   ├── footstep_walk.wav
    │   ├── door_open.wav
    │   └── enemy_attack.wav
    │
    └── Ambient/
        ├── ambient_calm.wav
        └── wind.wav
```

### AudioManager Architecture

```
AudioManager
├── MusicSource (looping)
├── AmbientSource (looping)
├── TensionSource (looping, volume controlled)
└── SFX Pool (10 sources, one-shot)
```

---

## 🎨 UI Structure

```
Canvas
├── PlayerHUD
│   ├── Health Bar
│   ├── Stamina Bar
│   ├── Interaction Prompt
│   ├── Damage Vignette
│   └── Message Text
│
└── PauseMenu
    ├── Panel (background)
    └── Buttons
        ├── Resume
        ├── Settings
        ├── Main Menu
        └── Quit
```

---

## 🧩 Extensibilidade

### Como Adicionar Novo Sistema

1. **Criar classe no namespace apropriado**
```csharp
namespace HorrorGame.YourNamespace
{
    public class YourSystem : MonoBehaviour
    {
        // Seu código
    }
}
```

2. **Registrar eventos necessários**
```csharp
private void Start()
{
    GameEvents.OnSomeEvent += HandleEvent;
}

private void OnDestroy()
{
    GameEvents.OnSomeEvent -= HandleEvent;
}
```

3. **Adicionar novos eventos se necessário**
```csharp
// Em GameEvents.cs
public static Action<YourType> OnYourNewEvent;
```

4. **Documentar no README**

---

## 🔐 Camadas e Tags

### Tags Necessárias
- `Player` - GameObject do player
- `Enemy` - Inimigos (opcional, pode usar layers)

### Layers Sugeridas
```
Layer 6: Player
Layer 7: Enemy
Layer 8: Interactable
Layer 9: Ground
```

### Layer Masks
```csharp
// Detectar apenas player
[SerializeField] private LayerMask playerLayer = 1 << 6;

// Detectar interagíveis
[SerializeField] private LayerMask interactableLayer = 1 << 8;
```

---

## 🚀 Performance Considerations

### NavMesh
- Bake apenas superfícies walkable
- Use Agent Radius adequado (0.5f para humanoides)
- Configure max slope apropriadamente

### Audio
- Limite 3D audio distance
- Use pooling (já implementado)
- Comprima arquivos de áudio

### Spawning
- Respeite maxEnemies
- Use minPlayerDistance
- Considere usar waves para controle

### UI
- Use TextMesh Pro (melhor performance)
- Desative elementos não visíveis
- Use Canvas Groups para fade

---

## 📝 Checklist de Setup

### Cena Básica
- [ ] Criar GameManager GameObject
- [ ] Criar SceneController GameObject
- [ ] Criar AudioManager GameObject
- [ ] Configurar Player com todos componentes
- [ ] Adicionar UI Canvas com HUD e PauseMenu
- [ ] Bake NavMesh
- [ ] Configurar iluminação
- [ ] Adicionar AtmosphereController

### Player Setup
- [ ] CharacterController configurado
- [ ] FirstPersonController com valores ajustados
- [ ] MouseLook com sensibilidade correta
- [ ] PlayerHealth com vida máxima
- [ ] PlayerInteraction com layer correto
- [ ] Flashlight na câmera
- [ ] Tag "Player" aplicada

### Enemy Setup
- [ ] NavMeshAgent adicionado
- [ ] Script de enemy configurado
- [ ] Waypoints criados (se patrol)
- [ ] Parâmetros ajustados
- [ ] Layer/Tag configurados

### Audio Setup
- [ ] Arquivos em Resources/Audio/
- [ ] Nomes corretos configurados
- [ ] AudioManager na cena
- [ ] Volumes ajustados

---

Este framework foi projetado para ser modular, extensível e fácil de manter.
Siga estas diretrizes e você terá uma base sólida para seu jogo de terror! 👻
