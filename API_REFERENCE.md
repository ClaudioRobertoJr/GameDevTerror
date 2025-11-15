# 📖 API Reference - Horror Game Framework

Referência rápida de todas as APIs públicas do framework.

## 📋 Índice

- [Core Systems](#core-systems)
- [Player Systems](#player-systems)
- [Enemy Systems](#enemy-systems)
- [Environment](#environment)
- [UI Systems](#ui-systems)
- [Audio System](#audio-system)
- [Events](#events)
- [Utilities](#utilities)

---

## 🎮 Core Systems

### GameManager

**Namespace:** `HorrorGame.Core`

**Singleton Access:**
```csharp
GameManager.Instance
```

**Properties:**
```csharp
GameState CurrentState { get; }      // Estado atual do jogo
bool IsPaused { get; }                // Se está pausado
```

**Methods:**
```csharp
void ChangeState(GameState newState)  // Muda estado do jogo
void PauseGame()                      // Pausa o jogo
void ResumeGame()                     // Resume o jogo
void TogglePause()                    // Alterna pause
void StartNewGame()                   // Inicia novo jogo
void ReturnToMainMenu()               // Volta ao menu
void GameOver()                       // Ativa game over
void QuitGame()                       // Sai do jogo
```

**Example:**
```csharp
// Pausar o jogo
GameManager.Instance.PauseGame();

// Verificar se está pausado
if (GameManager.Instance.IsPaused)
{
    // Não atualiza gameplay
}

// Mudar para game over
GameManager.Instance.GameOver();
```

---

### SceneController

**Namespace:** `HorrorGame.Core`

**Singleton Access:**
```csharp
SceneController.Instance
```

**Properties:**
```csharp
bool IsLoading { get; }              // Se está carregando
float LoadingProgress { get; }       // Progresso 0-1
```

**Methods:**
```csharp
void LoadMainMenu()                  // Carrega menu principal
void LoadGameScene()                 // Carrega cena do jogo
void LoadScene(string sceneName)     // Carrega cena específica
void ReloadCurrentScene()            // Recarrega cena atual
```

**Example:**
```csharp
// Carregar próximo nível
SceneController.Instance.LoadScene("Level02");

// Verificar progresso
float progress = SceneController.Instance.LoadingProgress;
```

---

### GameEvents

**Namespace:** `HorrorGame.Core`

**Static Class - Todos os eventos são estáticos**

#### Game State Events
```csharp
Action<GameState, GameState> OnGameStateChanged  // (prev, new)
Action OnGamePaused
Action OnGameResumed
```

#### Player Events
```csharp
Action<float, float> OnPlayerDamaged    // (damage, currentHealth)
Action OnPlayerDied
Action<float, float> OnPlayerHealed     // (healAmount, currentHealth)
Action<float, float> OnStaminaChanged   // (current, max)
Action<GameObject> OnPlayerInteract     // (target)
```

#### Enemy Events
```csharp
Action<GameObject> OnEnemySpawned
Action<GameObject> OnEnemyDied
Action<GameObject> OnEnemyDetectedPlayer
Action<GameObject> OnEnemyLostPlayer
Action<GameObject> OnEnemyAttack
```

#### Audio Events
```csharp
Action<string> OnPlaySound2D                    // (soundName)
Action<string, Vector3> OnPlaySound3D           // (soundName, position)
Action<string> OnChangeMusicTrack               // (trackName)
Action<float> OnTensionLevelChanged             // (level 0-1)
```

#### Environment Events
```csharp
Action<GameObject, bool> OnDoorToggled          // (door, isOpen)
Action<GameObject> OnItemCollected              // (item)
Action<Vector3> OnCheckpointReached             // (position)
```

#### UI Events
```csharp
Action<string, float> OnShowMessage             // (message, duration)
Action<string> OnObjectiveUpdated               // (objective)
```

**Example:**
```csharp
// Inscrever em evento
void Start()
{
    GameEvents.OnPlayerDamaged += HandlePlayerDamage;
}

// Desinscrever (IMPORTANTE!)
void OnDestroy()
{
    GameEvents.OnPlayerDamaged -= HandlePlayerDamage;
}

// Handler
void HandlePlayerDamage(float damage, float health)
{
    Debug.Log($"Player took {damage} damage! Health: {health}");
}

// Disparar evento
GameEvents.OnPlayerDamaged?.Invoke(10f, 90f);
```

---

## 👤 Player Systems

### FirstPersonController

**Namespace:** `HorrorGame.Player`

**Properties:**
```csharp
bool IsMoving { get; }              // Se está se movendo
bool IsGrounded { get; }            // Se está no chão
float CurrentStamina { get; }       // Stamina atual
```

**Methods:**
```csharp
void Teleport(Vector3 position)     // Teleporta player
void AddForce(Vector3 force)        // Adiciona força (knockback)
```

**Example:**
```csharp
FirstPersonController controller = player.GetComponent<FirstPersonController>();

// Teleportar
controller.Teleport(new Vector3(10, 0, 10));

// Knockback
controller.AddForce(Vector3.back * 5f);

// Verificar se está no chão
if (controller.IsGrounded)
{
    // Player pode pular
}
```

---

### MouseLook

**Namespace:** `HorrorGame.Player`

**Methods:**
```csharp
void ShakeCamera(float intensity, float duration)  // Camera shake
void SetSensitivity(float sensitivity)             // Muda sensibilidade
void SetInvertY(bool invert)                       // Inverte Y
```

**Example:**
```csharp
MouseLook mouseLook = player.GetComponent<MouseLook>();

// Camera shake quando toma dano
mouseLook.ShakeCamera(0.5f, 0.3f);

// Mudar sensibilidade via settings
mouseLook.SetSensitivity(newSensitivity);
```

---

### PlayerHealth

**Namespace:** `HorrorGame.Player`

**Properties:**
```csharp
float MaxHealth { get; }            // Vida máxima
float CurrentHealth { get; }        // Vida atual
bool IsDead { get; }                // Se está morto
float HealthPercent { get; }        // Percentual de vida (0-1)
```

**Methods:**
```csharp
void TakeDamage(float damage, Vector3 source = default)  // Recebe dano
void TakeFallDamage(float fallDistance)                  // Dano de queda
void Heal(float amount)                                  // Cura
void FullHeal()                                          // Cura completa
void SetMaxHealth(float newMaxHealth)                    // Define vida máx
bool CanTakeDamage()                                     // Se pode tomar dano
void Revive(Vector3 respawnPosition)                     // Revive player
```

**Example:**
```csharp
PlayerHealth health = player.GetComponent<PlayerHealth>();

// Aplicar dano
health.TakeDamage(25f, enemyPosition);

// Curar
health.Heal(50f);

// Verificar se vivo
if (!health.IsDead)
{
    // Player vivo
}

// Obter percentual de vida
float healthPercent = health.HealthPercent; // 0.0 - 1.0
```

---

### PlayerInteraction

**Namespace:** `HorrorGame.Player`

**Note:** Sistema automático baseado em raycast. Objetos devem implementar `IInteractable`.

**IInteractable Interface:**
```csharp
public interface IInteractable
{
    string GetInteractionPrompt();   // Texto mostrado
    bool CanInteract();              // Se pode interagir
    void Interact(GameObject player); // Executar interação
}
```

**Example:**
```csharp
public class CustomInteractable : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt()
    {
        return "Press E to activate";
    }

    public bool CanInteract()
    {
        return true; // Ou sua lógica
    }

    public void Interact(GameObject player)
    {
        Debug.Log("Interacted!");
        // Sua lógica aqui
    }
}
```

---

### Flashlight

**Namespace:** `HorrorGame.Player`

**Properties:**
```csharp
bool IsOn { get; }                  // Se está ligada
float BatteryPercent { get; }       // Bateria 0-1
```

**Methods:**
```csharp
void Toggle()                       // Liga/desliga
void TurnOn()                       // Liga
void TurnOff()                      // Desliga
void RechargeBattery(float amount)  // Recarrega bateria
void FullyRecharge()                // Recarga total
```

**Example:**
```csharp
Flashlight flashlight = player.GetComponentInChildren<Flashlight>();

// Ligar lanterna
flashlight.TurnOn();

// Verificar bateria
if (flashlight.BatteryPercent < 0.2f)
{
    Debug.Log("Battery low!");
}

// Recarregar (item coletado)
flashlight.RechargeBattery(50f);
```

---

## 👾 Enemy Systems

### EnemyBase (Abstract)

**Namespace:** `HorrorGame.Enemy`

**Properties:**
```csharp
bool IsDead { get; }                // Se está morto
Transform Target { get; }           // Alvo atual (player)
float HealthPercent { get; }        // Vida percentual (0-1)
```

**Methods:**
```csharp
void TakeDamage(float damage, Vector3 source = default)  // Recebe dano

// Métodos protegidos (para subclasses)
protected abstract void UpdateAI()                        // Lógica de IA
protected abstract void OnAttack()                        // Lógica de ataque
protected bool DetectPlayer()                             // Detecta player
protected bool HasLineOfSight(Transform target)           // Linha de visão
protected void SetTarget(Transform newTarget)             // Define alvo
protected bool HasLostTarget()                            // Perdeu alvo
protected void LoseTarget()                               // Perde alvo
protected bool IsInAttackRange()                          // No alcance
protected virtual void OnDamageTaken(float, Vector3)      // Ao tomar dano
protected virtual void OnPlayerDetected()                 // Ao detectar
protected virtual void Die()                              // Ao morrer
protected virtual void OnDeath()                          // Override morte
```

**Example (Creating Custom Enemy):**
```csharp
public class FastEnemy : EnemyBase
{
    protected override void UpdateAI()
    {
        // Detecta player
        if (DetectPlayer())
        {
            // Move em direção ao player
            navAgent.SetDestination(target.position);

            // Ataca se no alcance
            if (IsInAttackRange())
            {
                Attack();
            }
        }
    }

    protected override void OnAttack()
    {
        // Dano ao player
        PlayerHealth health = target.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage, transform.position);
        }
    }

    protected override void OnDamageTaken(float damage, Vector3 source)
    {
        base.OnDamageTaken(damage, source);
        // Reação customizada ao dano
    }
}
```

---

### PatrolEnemy

**Namespace:** `HorrorGame.Enemy`

**Inherits from:** `EnemyBase`

**Inspector Fields:**
```csharp
Transform[] patrolPoints            // Pontos de patrulha
float waypointWaitTime              // Tempo em cada waypoint
bool loopPatrol                     // Se volta ao início
float investigateTime               // Tempo investigando
```

**States:**
- Idle
- Patrol (entre waypoints)
- Investigate (última posição conhecida)
- Chase (perseguindo player)
- Attack (atacando)

**Example:**
```csharp
// Setup no Inspector:
// 1. Criar waypoints vazios
// 2. Arrastar para Patrol Points
// 3. Configurar tempos

// O script funciona automaticamente!
```

---

## 🌍 Environment

### Door

**Namespace:** `HorrorGame.Environment`

**Implements:** `IInteractable`

**Properties:**
```csharp
bool IsOpen { get; }                // Se está aberta
bool IsLocked { get; }              // Se está trancada
```

**Methods:**
```csharp
void OpenDoor()                     // Abre
void CloseDoor()                    // Fecha
void Lock()                         // Tranca
void Unlock()                       // Destranca
void SetRequiredKey(string keyID)   // Define chave necessária
bool TryUnlockWithKey(string keyID) // Tenta destrancar com chave
```

**Example:**
```csharp
Door door = GetComponent<Door>();

// Abrir porta
door.OpenDoor();

// Trancar porta
door.Lock();

// Tentar abrir com chave
bool success = door.TryUnlockWithKey("red_key");
```

---

### Collectible

**Namespace:** `HorrorGame.Environment`

**Implements:** `IInteractable`

**Types:**
```csharp
enum CollectibleType
{
    HealthKit,
    Ammo,
    Key,
    Note,
    Battery,
    Other
}
```

**Inspector Fields:**
```csharp
CollectibleType type
string itemName
float value                         // Valor (vida/munição)
bool autoCollectOnTrigger           // Coleta ao tocar
```

**Example:**
```csharp
// Setup no Inspector:
Collectible collectible = gameObject.AddComponent<Collectible>();
collectible.type = CollectibleType.HealthKit;
collectible.itemName = "Med Kit";
collectible.value = 50f;
collectible.autoCollectOnTrigger = true;
```

---

### EnemySpawner

**Namespace:** `HorrorGame.Environment`

**Properties:**
```csharp
int ActiveEnemiesCount { get; }     // Inimigos ativos
int CurrentWave { get; }            // Wave atual
```

**Methods:**
```csharp
GameObject SpawnEnemy()             // Spawna um inimigo
void SpawnAll()                     // Spawna todos
void SpawnWave()                    // Spawna uma wave
void StopSpawning()                 // Para spawning
void RestartSpawning()              // Reinicia spawning
void ClearAllEnemies()              // Destrói todos
```

**Spawn Modes:**
```csharp
enum SpawnMode
{
    Manual,     // Spawn via script
    Interval,   // Spawn em intervalos
    All         // Spawn todos de uma vez
}
```

**Example:**
```csharp
EnemySpawner spawner = GetComponent<EnemySpawner>();

// Spawnar uma wave
spawner.SpawnWave();

// Spawnar inimigo único
GameObject enemy = spawner.SpawnEnemy();

// Limpar todos
spawner.ClearAllEnemies();

// Verificar quantidade
int count = spawner.ActiveEnemiesCount;
```

---

### AtmosphereController

**Namespace:** `HorrorGame.Environment`

**Properties:**
```csharp
float CurrentTensionLevel { get; }  // Nível de tensão 0-1
```

**Methods:**
```csharp
void SetTensionLevel(float level)   // Define tensão (0-1)
void IncreaseTension(float amount)  // Aumenta tensão
void DecreaseTension(float amount)  // Diminui tensão
void ResetToNormal()                // Volta ao normal
```

**Example:**
```csharp
AtmosphereController atmosphere = FindObjectOfType<AtmosphereController>();

// Aumentar tensão quando inimigo detecta
atmosphere.SetTensionLevel(0.8f);

// Diminuir gradualmente
atmosphere.DecreaseTension(0.1f);

// Resetar
atmosphere.ResetToNormal();
```

---

## 🎨 UI Systems

### PlayerHUD

**Namespace:** `HorrorGame.UI`

**Methods:**
```csharp
void UpdateHealth(float current, float max)           // Atualiza vida
void UpdateStamina(float current, float max)          // Atualiza stamina
void ShowInteractionPrompt(string text)               // Mostra prompt
void HideInteractionPrompt()                          // Esconde prompt
void ShowMessage(string message, float duration)      // Mostra mensagem
```

**Note:** Maioria dos updates são automáticos via eventos!

**Example:**
```csharp
PlayerHUD hud = FindObjectOfType<PlayerHUD>();

// Mostrar mensagem customizada
hud.ShowMessage("Door unlocked!", 3f);

// Mostrar prompt customizado
hud.ShowInteractionPrompt("Press E to activate terminal");
```

---

### PauseMenu

**Namespace:** `HorrorGame.UI`

**Note:** Funciona automaticamente com ESC!

**Callbacks públicos (se precisar):**
```csharp
void ShowPauseMenu()
void HidePauseMenu()
```

---

## 🔊 Audio System

### AudioManager

**Namespace:** `HorrorGame.Audio`

**Singleton Access:**
```csharp
AudioManager.Instance
```

**Methods:**
```csharp
// Música
void PlayMusic(string musicName, bool fadeIn = true)
void StopMusic(bool fadeOut = true)

// SFX
void PlaySound2D(string soundName, float volumeMultiplier = 1f)
void PlaySound3D(string soundName, Vector3 position, float volumeMultiplier = 1f)

// Ambiente
void PlayAmbient(string ambientName)
void SetTensionLevel(float level)  // 0-1

// Volume
void SetMasterVolume(float volume)  // 0-1
void SetMusicVolume(float volume)   // 0-1
void SetSFXVolume(float volume)     // 0-1
```

**Example:**
```csharp
// Tocar música
AudioManager.Instance.PlayMusic("horror_theme");

// SFX 2D
AudioManager.Instance.PlaySound2D("door_open");

// SFX 3D em posição
AudioManager.Instance.PlaySound3D("enemy_roar", enemyPosition);

// Ambiente
AudioManager.Instance.PlayAmbient("wind_howling");

// Tensão
AudioManager.Instance.SetTensionLevel(0.8f);
```

**Note:** É mais comum usar via GameEvents:
```csharp
GameEvents.OnPlaySound2D?.Invoke("door_open");
GameEvents.OnPlaySound3D?.Invoke("scream", transform.position);
GameEvents.OnChangeMusicTrack?.Invoke("chase_theme");
```

---

## 🔧 Utilities

### Checkpoint

**Namespace:** `HorrorGame.Utilities`

**Methods:**
```csharp
void ActivateCheckpoint(GameObject player)
Vector3 GetRespawnPosition()
```

**Example:**
```csharp
Checkpoint checkpoint = GetComponent<Checkpoint>();

// Ativar manualmente
checkpoint.ActivateCheckpoint(playerGameObject);

// Obter posição de respawn
Vector3 respawnPos = checkpoint.GetRespawnPosition();
```

---

### FPSCounter

**Namespace:** `HorrorGame.Utilities`

**Methods:**
```csharp
void ToggleDisplay()                // Liga/desliga
void SetVisible(bool visible)       // Define visibilidade
```

**Example:**
```csharp
FPSCounter fpsCounter = FindObjectOfType<FPSCounter>();

// Toggle
fpsCounter.ToggleDisplay();

// Mostrar/esconder
fpsCounter.SetVisible(true);
```

---

## 📊 ScriptableObjects

### EnemyData

**Namespace:** `HorrorGame.Data`

**Create:** Right-click > Create > Horror Game > Enemy Data

**Fields:**
```csharp
// Basic
string enemyName
string description

// Stats
float maxHealth
float damage
float attackRange
float attackCooldown

// Movement
float moveSpeed
float chaseSpeed

// Detection
float detectionRange
float loseTargetDistance
float fieldOfView

// Audio
string detectedSound
string attackSound
string deathSound
string idleSound

// Behavior
float investigateTime
bool canPatrol
float aggression
```

**Example:**
```csharp
[SerializeField] private EnemyData enemyData;

void Start()
{
    maxHealth = enemyData.maxHealth;
    damage = enemyData.damage;
    // etc...
}
```

---

### GameSettings

**Namespace:** `HorrorGame.Data`

**Create:** Right-click > Create > Horror Game > Game Settings

**Fields:**
```csharp
// Graphics
int defaultQualityLevel
int targetFrameRate
bool vSyncEnabled

// Audio
float defaultMasterVolume
float defaultMusicVolume
float defaultSFXVolume

// Gameplay
GameDifficulty defaultDifficulty
float playerDamageMultiplier
float enemyDamageMultiplier

// Controls
float defaultMouseSensitivity
bool invertYAxis

// UI
bool showFPSCounter
bool showTutorialHints
```

---

## 📝 Common Patterns

### Check if Game is Paused

```csharp
if (GameManager.Instance != null && GameManager.Instance.IsPaused)
    return; // Não atualiza
```

### Subscribe/Unsubscribe Events

```csharp
void Start()
{
    GameEvents.OnPlayerDamaged += HandleDamage;
}

void OnDestroy()
{
    GameEvents.OnPlayerDamaged -= HandleDamage;
}
```

### Play Sound at Position

```csharp
GameEvents.OnPlaySound3D?.Invoke("explosion", transform.position);
```

### Get Player Reference

```csharp
GameObject player = GameObject.FindGameObjectWithTag("Player");
PlayerHealth health = player.GetComponent<PlayerHealth>();
```

### Damage Player

```csharp
PlayerHealth health = player.GetComponent<PlayerHealth>();
if (health != null && health.CanTakeDamage())
{
    health.TakeDamage(damage, transform.position);
}
```

### Teleport Player

```csharp
FirstPersonController controller = player.GetComponent<FirstPersonController>();
controller.Teleport(newPosition);
```

### Change Atmosphere Tension

```csharp
AtmosphereController atmosphere = FindObjectOfType<AtmosphereController>();
atmosphere.SetTensionLevel(0.8f); // 0-1
```

### Show UI Message

```csharp
GameEvents.OnShowMessage?.Invoke("Key collected!", 3f);
```

---

## 🎯 Best Practices

### Always Check for Null

```csharp
if (GameManager.Instance != null)
{
    GameManager.Instance.PauseGame();
}
```

### Use Null-Conditional with Events

```csharp
GameEvents.OnPlayerDamaged?.Invoke(damage, health);
```

### Cache Component References

```csharp
// ❌ BAD - Toda frame
void Update()
{
    GetComponent<PlayerHealth>().CurrentHealth;
}

// ✅ GOOD - Cache no Start
private PlayerHealth health;
void Start()
{
    health = GetComponent<PlayerHealth>();
}
void Update()
{
    float hp = health.CurrentHealth;
}
```

### Unsubscribe from Events

```csharp
// SEMPRE faça isso!
void OnDestroy()
{
    GameEvents.OnPlayerDamaged -= HandleDamage;
}
```

---

## 🔍 Quick Lookup

**Need to...**

**Play sound?**
```csharp
GameEvents.OnPlaySound2D?.Invoke("sound_name");
```

**Damage player?**
```csharp
player.GetComponent<PlayerHealth>().TakeDamage(damage);
```

**Pause game?**
```csharp
GameManager.Instance.PauseGame();
```

**Load scene?**
```csharp
SceneController.Instance.LoadScene("SceneName");
```

**Show message?**
```csharp
GameEvents.OnShowMessage?.Invoke("Message", 3f);
```

**Spawn enemy?**
```csharp
spawner.SpawnEnemy();
```

**Change atmosphere?**
```csharp
atmosphere.SetTensionLevel(0.8f);
```

---

Esta referência cobre os principais sistemas do framework. Para exemplos mais detalhados, veja EXAMPLES.md! 📚
