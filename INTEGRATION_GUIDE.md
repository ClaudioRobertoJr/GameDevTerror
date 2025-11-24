# 🔧 Guia de Integração - Sistemas Essenciais

Guia prático de como integrar todas as tecnologias avançadas ao seu framework de horror game.

## 📚 Índice

- [Preparação](#preparação)
- [Integração 1: Event Bus](#integração-1-event-bus)
- [Integração 2: Improved Timers](#integração-2-improved-timers)
- [Integração 3: Netcode for GameObjects](#integração-3-netcode-for-gameobjects)
- [Integração 4: Behavior Trees](#integração-4-behavior-trees)
- [Integração 5: GOAP (Alternativa)](#integração-5-goap-alternativa)
- [Integração 6: Utility AI](#integração-6-utility-ai)
- [Integração 7: Performance Systems](#integração-7-performance-systems)
- [Testing e Debugging](#testing-e-debugging)

---

## 🎯 Preparação

### Backup do Projeto
```bash
# Faça backup antes de começar!
git add .
git commit -m "Backup antes das integrações avançadas"
git push
```

### Checklist Pré-Integração
- [ ] Unity 6 instalado
- [ ] Projeto atual funcionando
- [ ] Git configurado
- [ ] Espaço em disco (>5GB)

---

## 🔌 Integração 1: Event Bus

### Objetivo
Substituir `GameEvents.cs` por sistema Event Bus type-safe.

### Tempo Estimado
4-6 horas

### Passo 1: Download e Import (30min)

```bash
# Clone o repositório
cd ~/Projects
git clone https://github.com/adammyhre/Unity-Event-Bus.git

# Copie para seu projeto
cp -r Unity-Event-Bus/Assets/EventBus ~/GameDevTerror/Assets/Scripts/Core/
```

### Passo 2: Criar Estruturas de Eventos (1h)

Crie `Assets/Scripts/Core/Events/GameEventStructs.cs`:

```csharp
using HorrorGame.Core;

namespace HorrorGame.Events
{
    // Player Events
    public struct PlayerDamagedEvent : IEvent
    {
        public int playerId;
        public float damage;
        public float currentHealth;
        public Vector3 damageSource;
    }

    public struct PlayerDiedEvent : IEvent
    {
        public int playerId;
        public Vector3 deathPosition;
    }

    public struct PlayerHealedEvent : IEvent
    {
        public int playerId;
        public float amount;
        public float currentHealth;
    }

    public struct StaminaChangedEvent : IEvent
    {
        public int playerId;
        public float current;
        public float max;
    }

    // Enemy Events
    public struct EnemySpawnedEvent : IEvent
    {
        public GameObject enemy;
        public Vector3 position;
        public string enemyType;
    }

    public struct EnemyDiedEvent : IEvent
    {
        public GameObject enemy;
        public Vector3 position;
        public int killerId;
    }

    public struct EnemyDetectedPlayerEvent : IEvent
    {
        public GameObject enemy;
        public int detectedPlayerId;
    }

    // Environment Events
    public struct DoorToggledEvent : IEvent
    {
        public GameObject door;
        public bool isOpen;
    }

    public struct ItemCollectedEvent : IEvent
    {
        public GameObject item;
        public int collecterId;
        public string itemType;
    }

    // Audio Events
    public struct PlaySound2DEvent : IEvent
    {
        public string soundName;
        public float volume;
    }

    public struct PlaySound3DEvent : IEvent
    {
        public string soundName;
        public Vector3 position;
        public float volume;
    }

    public struct ChangeMusicTrackEvent : IEvent
    {
        public string trackName;
        public float fadeTime;
    }

    public struct TensionLevelChangedEvent : IEvent
    {
        public float tensionLevel;
    }

    // UI Events
    public struct ShowMessageEvent : IEvent
    {
        public string message;
        public float duration;
    }
}
```

### Passo 3: Atualizar PlayerHealth (1h)

Modificar `Assets/Scripts/Player/Health/PlayerHealth.cs`:

```csharp
using HorrorGame.Events;
using UnityEngine;

namespace HorrorGame.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        // Event bindings
        private EventBinding<PlayerDamagedEvent> damagedBinding;
        private EventBinding<PlayerHealedEvent> healedBinding;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        // Substituir método TakeDamage
        public void TakeDamage(float damage)
        {
            int damageInt = Mathf.RoundToInt(damage);
            currentHealth -= damageInt;
            currentHealth = Mathf.Max(0, currentHealth);

            // Disparar evento
            EventBus<PlayerDamagedEvent>.Raise(new PlayerDamagedEvent
            {
                playerId = GetPlayerId(),
                damage = damage,
                currentHealth = currentHealth,
                damageSource = transform.position // Ou posição do atacante
            });

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            int healInt = Mathf.RoundToInt(amount);
            currentHealth += healInt;
            currentHealth = Mathf.Min(maxHealth, currentHealth);

            EventBus<PlayerHealedEvent>.Raise(new PlayerHealedEvent
            {
                playerId = GetPlayerId(),
                amount = amount,
                currentHealth = currentHealth
            });
        }

        private void Die()
        {
            EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent
            {
                playerId = GetPlayerId(),
                deathPosition = transform.position
            });
        }

        private int GetPlayerId()
        {
            // Se single-player, retorna 0
            // Se multiplayer, retorna networkId
            return 0;
        }
    }
}
```

### Passo 4: Atualizar PlayerHUD (1h)

Modificar `Assets/Scripts/UI/HUD/PlayerHUD.cs`:

```csharp
using HorrorGame.Events;
using UnityEngine;

namespace HorrorGame.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private Image healthBar;

        private EventBinding<PlayerDamagedEvent> damagedBinding;
        private EventBinding<PlayerHealedEvent> healedBinding;

        private void OnEnable()
        {
            // Registrar eventos
            damagedBinding = new EventBinding<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus<PlayerDamagedEvent>.Register(damagedBinding);

            healedBinding = new EventBinding<PlayerHealedEvent>(OnPlayerHealed);
            EventBus<PlayerHealedEvent>.Register(healedBinding);
        }

        private void OnDisable()
        {
            // Desregistrar eventos
            EventBus<PlayerDamagedEvent>.Deregister(damagedBinding);
            EventBus<PlayerHealedEvent>.Deregister(healedBinding);
        }

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            // Atualizar barra de vida
            UpdateHealthBar(evt.currentHealth);
        }

        private void OnPlayerHealed(PlayerHealedEvent evt)
        {
            UpdateHealthBar(evt.currentHealth);
        }

        private void UpdateHealthBar(float health)
        {
            if (healthBar != null)
            {
                healthBar.fillAmount = health / 100f;
            }
        }
    }
}
```

### Passo 5: Atualizar AudioManager (1h)

```csharp
using HorrorGame.Events;

namespace HorrorGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private EventBinding<PlaySound2DEvent> sound2DBinding;
        private EventBinding<PlaySound3DEvent> sound3DBinding;
        private EventBinding<ChangeMusicTrackEvent> musicBinding;

        private void OnEnable()
        {
            sound2DBinding = new EventBinding<PlaySound2DEvent>(OnPlaySound2D);
            EventBus<PlaySound2DEvent>.Register(sound2DBinding);

            sound3DBinding = new EventBinding<PlaySound3DEvent>(OnPlaySound3D);
            EventBus<PlaySound3DEvent>.Register(sound3DBinding);

            musicBinding = new EventBinding<ChangeMusicTrackEvent>(OnChangeMusic);
            EventBus<ChangeMusicTrackEvent>.Register(musicBinding);
        }

        private void OnDisable()
        {
            EventBus<PlaySound2DEvent>.Deregister(sound2DBinding);
            EventBus<PlaySound3DEvent>.Deregister(sound3DBinding);
            EventBus<ChangeMusicTrackEvent>.Deregister(musicBinding);
        }

        private void OnPlaySound2D(PlaySound2DEvent evt)
        {
            // Tocar som 2D
            PlaySound2D(evt.soundName, evt.volume);
        }

        private void OnPlaySound3D(PlaySound3DEvent evt)
        {
            PlaySound3D(evt.soundName, evt.position, evt.volume);
        }

        private void OnChangeMusic(ChangeMusicTrackEvent evt)
        {
            ChangeTrack(evt.trackName, evt.fadeTime);
        }

        // Métodos existentes...
    }
}
```

### Passo 6: Remover GameEvents.cs (30min)

1. Comentar `GameEvents.cs`
2. Compilar e ver erros
3. Substituir todas chamadas por Event Bus
4. Testar
5. Deletar `GameEvents.cs`

### Checkpoint ✅
- [ ] Event Bus compilando
- [ ] PlayerHealth usando eventos
- [ ] PlayerHUD respondendo a eventos
- [ ] AudioManager usando eventos
- [ ] Sem erros de compilação

---

## ⏱️ Integração 2: Improved Timers

### Objetivo
Substituir lógica de timer manual pelo sistema Player Loop.

### Tempo Estimado
2-4 horas

### Passo 1: Download e Import (20min)

```bash
cd ~/Projects
git clone https://github.com/adammyhre/Unity-Improved-Timers.git
cp -r Unity-Improved-Timers/Assets/Timers ~/GameDevTerror/Assets/Scripts/Utilities/
```

### Passo 2: Atualizar Flashlight (30min)

Modificar `Assets/Scripts/Player/Flashlight.cs`:

```csharp
using UnityTimer;
using UnityEngine;

namespace HorrorGame.Player
{
    public class Flashlight : MonoBehaviour
    {
        [SerializeField] private Light flashlightLight;
        [SerializeField] private float maxBattery = 100f;
        [SerializeField] private float drainRate = 1f;
        [SerializeField] private float rechargeRate = 0.5f;

        private float currentBattery;
        private bool isOn = false;

        // Timers
        private CountdownTimer batteryTimer;
        private CountdownTimer rechargeTimer;

        private void Start()
        {
            currentBattery = maxBattery;

            // Setup battery drain timer
            batteryTimer = new CountdownTimer(maxBattery / drainRate);
            batteryTimer.OnTimerStop += OnBatteryDepleted;

            // Setup recharge timer (inverte lógica para countdown)
            rechargeTimer = new CountdownTimer(0f);
        }

        private void Update()
        {
            // Input
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleFlashlight();
            }

            // Atualizar bateria visual
            if (isOn)
            {
                currentBattery = Mathf.Max(0, maxBattery - (batteryTimer.Progress * maxBattery));
            }
            else if (currentBattery < maxBattery)
            {
                currentBattery = Mathf.Min(maxBattery, currentBattery + (rechargeRate * Time.deltaTime));
            }

            UpdateFlashlightIntensity();
        }

        private void ToggleFlashlight()
        {
            isOn = !isOn;
            flashlightLight.enabled = isOn;

            if (isOn && currentBattery > 0)
            {
                batteryTimer.Start();
            }
            else
            {
                batteryTimer.Pause();
            }
        }

        private void OnBatteryDepleted()
        {
            isOn = false;
            flashlightLight.enabled = false;
            Debug.Log("Bateria esgotada!");
        }

        private void UpdateFlashlightIntensity()
        {
            float batteryPercent = currentBattery / maxBattery;
            flashlightLight.intensity = Mathf.Lerp(0.2f, 1f, batteryPercent);
        }

        private void OnDestroy()
        {
            batteryTimer?.Stop();
            rechargeTimer?.Stop();
        }
    }
}
```

### Passo 3: Atualizar EnemySpawner (30min)

```csharp
using UnityTimer;

namespace HorrorGame.Environment
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float spawnInterval = 5f;

        private FrequencyTimer spawnTimer;

        private void Start()
        {
            // Spawnar a cada X segundos
            float spawnFrequency = 1f / spawnInterval;
            spawnTimer = new FrequencyTimer(spawnFrequency);
            spawnTimer.OnTimerStop += SpawnEnemy;
            spawnTimer.Start();
        }

        private void SpawnEnemy()
        {
            // Lógica de spawn
            Vector3 spawnPos = GetRandomSpawnPosition();
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // Implementar lógica
            return transform.position;
        }

        private void OnDestroy()
        {
            spawnTimer?.Stop();
        }
    }
}
```

### Passo 4: Criar Sistema de Cooldowns (1h)

Criar `Assets/Scripts/Utilities/CooldownManager.cs`:

```csharp
using UnityTimer;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorGame.Utilities
{
    public class CooldownManager : MonoBehaviour
    {
        private Dictionary<string, CountdownTimer> cooldowns = new Dictionary<string, CountdownTimer>();

        public bool IsOnCooldown(string abilityName)
        {
            if (cooldowns.ContainsKey(abilityName))
            {
                return !cooldowns[abilityName].IsFinished;
            }
            return false;
        }

        public void StartCooldown(string abilityName, float duration)
        {
            if (cooldowns.ContainsKey(abilityName))
            {
                cooldowns[abilityName].Reset(duration);
                cooldowns[abilityName].Start();
            }
            else
            {
                var timer = new CountdownTimer(duration);
                timer.Start();
                cooldowns[abilityName] = timer;
            }
        }

        public float GetCooldownRemaining(string abilityName)
        {
            if (cooldowns.ContainsKey(abilityName))
            {
                return cooldowns[abilityName].TimeRemaining;
            }
            return 0f;
        }

        private void OnDestroy()
        {
            foreach (var timer in cooldowns.Values)
            {
                timer.Stop();
            }
            cooldowns.Clear();
        }
    }
}
```

### Checkpoint ✅
- [ ] Improved Timers instalado
- [ ] Flashlight usando timers
- [ ] EnemySpawner usando timers
- [ ] CooldownManager funcionando
- [ ] Performance melhorada

---

## 🌐 Integração 3: Netcode for GameObjects

### Objetivo
Adicionar multiplayer ao projeto.

### Tempo Estimado
8-12 horas (veja MULTIPLAYER_ARCHITECTURE.md para detalhes completos)

### Quick Start (2h)

#### 1. Instalar Packages (20min)
```
Window > Package Manager > Add from Git URL
https://github.com/Unity-Technologies/com.unity.netcode.gameobjects.git
```

#### 2. Setup NetworkManager (30min)
- Criar GameObject "NetworkManager"
- Adicionar componente `NetworkManager`
- Adicionar `UnityTransport`
- Configurar no Inspector

#### 3. Converter Player (1h)
```csharp
using Unity.Netcode;

public class NetworkPlayerController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Enable local controls
        }
        else
        {
            // Disable remote controls
        }
    }
}
```

#### 4. Menu de Conexão (30min)
```csharp
public class NetworkUI : MonoBehaviour
{
    public void OnHostButton()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void OnJoinButton()
    {
        NetworkManager.Singleton.StartClient();
    }
}
```

### Para integração completa, siga MULTIPLAYER_ARCHITECTURE.md e COOP_ROADMAP.md

---

## 🌳 Integração 4: Behavior Trees

### Objetivo
Implementar IA avançada usando Behavior Trees.

### Tempo Estimado
12-16 horas

### Passo 1: Download Framework (30min)

```bash
cd ~/Projects
git clone https://github.com/adammyhre/Unity-Behaviour-Trees.git
cp -r Unity-Behaviour-Trees/Assets/BehaviorTree ~/GameDevTerror/Assets/Scripts/AI/
```

### Passo 2: Criar Inimigo BT (4-6h)

Veja exemplos completos em `ADVANCED_AI_SYSTEMS.md`.

Quick example:

```csharp
using HorrorGame.AI.BehaviorTree;

public class BTEnemy : MonoBehaviour
{
    private BehaviorTree behaviorTree;

    void Start()
    {
        behaviorTree = new BehaviorTree(
            new BTSelector() // Root
        );

        var root = behaviorTree.RootNode as BTSelector;

        // Attack if in range
        root.AddChild(new BTSequence(
            new CanSeePlayerCondition(),
            new IsInRangeCondition(2f),
            new AttackAction()
        ));

        // Chase if visible
        root.AddChild(new BTSequence(
            new CanSeePlayerCondition(),
            new ChaseAction()
        ));

        // Patrol otherwise
        root.AddChild(new PatrolAction());
    }

    void Update()
    {
        behaviorTree.Tick();
    }
}
```

### Checkpoint ✅
- [ ] BT framework instalado
- [ ] Pelo menos 1 inimigo com BT
- [ ] Comportamento funcionando
- [ ] Sem erros

---

## 🎯 Integração 5: GOAP (Alternativa ao BT)

### Objetivo
Implementar IA com planejamento usando GOAP.

### Tempo Estimado
16-20 horas

Veja `ADVANCED_AI_SYSTEMS.md` seção GOAP para implementação completa.

---

## 📊 Integração 6: Utility AI

### Objetivo
Adicionar sistema de decisão baseado em scoring.

### Tempo Estimado
8-10 horas

Quick example:

```csharp
public class UtilityAIEnemy : MonoBehaviour
{
    private List<UtilityAction> actions;

    void Start()
    {
        actions = new List<UtilityAction>
        {
            new AttackAction(),
            new FleeAction(),
            new HideAction()
        };
    }

    void Update()
    {
        float bestScore = 0f;
        UtilityAction bestAction = null;

        foreach (var action in actions)
        {
            float score = action.CalculateScore(gameObject);
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        bestAction?.Execute();
    }
}
```

---

## ⚡ Integração 7: Performance Systems

### Octrees (4-6h)

```bash
git clone https://github.com/adammyhre/Unity-Octrees.git
cp -r Unity-Octrees/Assets/Octree ~/GameDevTerror/Assets/Scripts/Utilities/
```

Uso:
```csharp
private Octree<EnemyBase> enemyOctree;

void Start()
{
    enemyOctree = new Octree<EnemyBase>(worldBounds, 5);
}

void RegisterEnemy(EnemyBase enemy)
{
    enemyOctree.Insert(enemy, enemy.transform.position);
}

List<EnemyBase> GetNearby(Vector3 position, float radius)
{
    return enemyOctree.GetNearby(position, radius);
}
```

### Batch Raycasting (2-4h)

```csharp
using Unity.Jobs;
using Unity.Collections;

public class BatchVision : MonoBehaviour
{
    void CheckMultipleVision()
    {
        var commands = new NativeArray<RaycastCommand>(enemyCount, Allocator.TempJob);
        var results = new NativeArray<RaycastHit>(enemyCount, Allocator.TempJob);

        // Setup commands
        for (int i = 0; i < enemyCount; i++)
        {
            commands[i] = new RaycastCommand(origin, direction, maxDist);
        }

        // Batch execute
        JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 32);
        handle.Complete();

        // Process results
        for (int i = 0; i < enemyCount; i++)
        {
            if (results[i].collider != null)
            {
                // Player found
            }
        }

        commands.Dispose();
        results.Dispose();
    }
}
```

---

## 🧪 Testing e Debugging

### Testing Checklist

#### Event Bus
- [ ] Eventos disparam corretamente
- [ ] Listeners recebem eventos
- [ ] Unregister funciona (sem memory leaks)

#### Timers
- [ ] Timers completam no tempo certo
- [ ] Pause/Resume funciona
- [ ] Sem timers órfãos

#### Networking
- [ ] 2 players conectam
- [ ] Movement sincronizado
- [ ] Sem duplicate AudioListeners

#### AI
- [ ] Comportamentos executam
- [ ] Transições funcionam
- [ ] Performance aceitável

### Debugging Tools

#### Network Debugging
```csharp
// Habilitar logs de rede
NetworkManager.Singleton.LogLevel = LogLevel.Developer;
```

#### AI Debugging
```csharp
void OnDrawGizmos()
{
    // Visualizar estado da IA
    Gizmos.color = GetStateColor();
    Gizmos.DrawWireSphere(transform.position + Vector3.up, 1f);

    #if UNITY_EDITOR
    UnityEditor.Handles.Label(
        transform.position + Vector3.up * 2f,
        $"State: {currentState}"
    );
    #endif
}
```

#### Performance Profiling
```
Window > Analysis > Profiler
```
- CPU Usage
- Memory
- Network Stats

---

## ✅ Checklist Final de Integração

### Event Bus
- [ ] Instalado e funcionando
- [ ] GameEvents.cs removido
- [ ] Todos sistemas usando Event Bus
- [ ] Sem memory leaks

### Timers
- [ ] Instalado e funcionando
- [ ] Flashlight usando timers
- [ ] Spawners usando timers
- [ ] Cooldowns implementados

### Networking
- [ ] Netcode instalado
- [ ] Players sincronizando
- [ ] Enemies sincronizando
- [ ] UI multiplayer funcional

### AI
- [ ] BT ou GOAP implementado
- [ ] Múltiplos tipos de inimigos
- [ ] Comportamentos variados
- [ ] Performance OK

### Performance
- [ ] Octrees implementado
- [ ] Batch raycasting
- [ ] 60 FPS mantido
- [ ] Memory estável

---

## 🚨 Troubleshooting Comum

### Event Bus não funciona
- ✅ Verificar namespace correto
- ✅ Register em OnEnable
- ✅ Deregister em OnDisable
- ✅ EventBinding correto

### Timers não param
- ✅ Chamar Stop() no OnDestroy
- ✅ Guardar referência ao timer
- ✅ Verificar se timer foi Started

### Network não conecta
- ✅ Firewall permite conexão
- ✅ Porta não está em uso
- ✅ NetworkManager na cena
- ✅ Transport configurado

### AI não funciona
- ✅ NavMesh baked
- ✅ NavMeshAgent no GameObject
- ✅ Player tem tag "Player"
- ✅ Layers configurados

---

## 🎉 Conclusão

Parabéns! Se chegou até aqui, seu projeto agora tem:

✅ Event Bus type-safe e performático
✅ Sistema de timers moderno
✅ Multiplayer funcionando
✅ IA avançada inteligente
✅ Performance otimizada

### Próximos Passos

1. **Testar extensivamente**
2. **Adicionar conteúdo**
3. **Balancear gameplay**
4. **Polish e efeitos**
5. **Playtest com outras pessoas**

### Recursos Adicionais

- [README.md](README.md) - Visão geral
- [ADVANCED_TECHNOLOGIES.md](ADVANCED_TECHNOLOGIES.md) - Tecnologias detalhadas
- [MULTIPLAYER_ARCHITECTURE.md](MULTIPLAYER_ARCHITECTURE.md) - Networking completo
- [ADVANCED_AI_SYSTEMS.md](ADVANCED_AI_SYSTEMS.md) - IA avançada
- [COOP_ROADMAP.md](COOP_ROADMAP.md) - Roadmap completo

---

**Você está pronto para criar um incrível jogo de terror cooperativo! 👻🎮**

Bom desenvolvimento! 🚀
