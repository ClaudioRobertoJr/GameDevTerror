# 📊 Benchmarks e Performance Targets

Metas de performance, como medir, e comparações antes/depois das otimizações.

## 📋 Índice

- [Targets de Performance](#targets-de-performance)
- [Como Medir](#como-medir)
- [Benchmarks por Fase](#benchmarks-por-fase)
- [Comparações: Com vs Sem Otimizações](#comparações-com-vs-sem-otimizações)
- [Bottlenecks Comuns](#bottlenecks-comuns)
- [Testes de Stress](#testes-de-stress)

---

## 🎯 Targets de Performance

### FPS (Frames Per Second)

| Plataforma | Mínimo | Target | Ideal |
|------------|--------|--------|-------|
| **PC (Desktop)** | 30 FPS | 60 FPS | 120+ FPS |
| **Laptop (Médio)** | 30 FPS | 60 FPS | 60 FPS |
| **PC (Low-end)** | 30 FPS | 30-45 FPS | 60 FPS |

**Contexto Horror Cooperativo**:
- ✅ **60 FPS**: Target para experiência suave
- ✅ **30 FPS mínimo**: Aceitável em momentos intensos
- ❌ **< 30 FPS**: Inaceitável, quebra imersão

---

### Frame Time

| Métrica | Target | Explicação |
|---------|--------|------------|
| **Frame Time** | ≤ 16.6ms | 60 FPS = 16.6ms por frame |
| **CPU Time** | ≤ 10ms | Tempo de processamento CPU |
| **GPU Time** | ≤ 12ms | Tempo de renderização |
| **Wait Time** | ≤ 2ms | Tempo ocioso |

**Como calcular FPS de Frame Time**:
```
FPS = 1000ms / Frame Time
60 FPS = 1000ms / 16.6ms
30 FPS = 1000ms / 33.3ms
```

---

### Memory (Memória)

| Tipo | Target | Máximo |
|------|--------|--------|
| **Total Memory** | < 1.5GB | 2GB |
| **Managed Memory** | < 500MB | 800MB |
| **GC Allocations/Frame** | 0 KB | 50 KB |
| **Texture Memory** | < 800MB | 1.2GB |

**Red flags**:
- ❌ Alocações constantes no Update (GC spikes)
- ❌ Memory leaks (memória crescendo constantemente)
- ❌ Textura resolution excessiva

---

### Networking (Multiplayer)

| Métrica | Single Player | 2 Players | 4 Players |
|---------|---------------|-----------|-----------|
| **Latency** | N/A | < 50ms | < 100ms |
| **Bandwidth (per player)** | N/A | < 30 KB/s | < 50 KB/s |
| **Packet Loss** | N/A | < 1% | < 2% |
| **Tick Rate** | N/A | 30 Hz | 20-30 Hz |

**Como medir latency**:
```csharp
float ping = NetworkManager.Singleton.LocalTime - NetworkManager.Singleton.ServerTime;
Debug.Log($"Ping: {ping * 1000}ms");
```

---

### AI Performance

| Cenário | Target | Máximo |
|---------|--------|--------|
| **AI Update Time (1 enemy)** | < 0.5ms | 1ms |
| **AI Update Time (10 enemies)** | < 3ms | 5ms |
| **Pathfinding (per enemy)** | < 0.2ms | 0.5ms |
| **Raycasts (per enemy)** | < 0.1ms | 0.3ms |

**Otimizações críticas**:
- ✅ Não rodar IA todo frame (stagger updates)
- ✅ LOD para IA (IA longe atualiza menos)
- ✅ Octree para busca espacial

---

## 📏 Como Medir

### Unity Profiler

#### Abrir Profiler
```
Window > Analysis > Profiler
Shortcut: Ctrl+7 (Windows) / Cmd+7 (Mac)
```

#### Métricas Principais

**CPU Usage**
- Total: Tempo total de CPU por frame
- Scripts: Tempo em código C#
- Rendering: Tempo de draw calls
- Physics: Tempo de física

**Memory**
- Total Allocated: Memória total
- GC Allocated: Memória gerenciada (C#)
- Texture Memory: Texturas

**Rendering**
- Draw Calls: Quantos calls para GPU
- Batches: Quantos batches (menor = melhor)
- Triangles: Quantidade de polígonos

**Physics**
- Active Rigidbodies: Quantos ativos
- Contacts: Colisões ativas

---

### Profiling em Build

**IMPORTANTE**: Sempre profile em build, não só no Editor!

```csharp
// Habilitar profiling em build
Build Settings > Development Build ✓
Build Settings > Autoconnect Profiler ✓
```

Diferenças Editor vs Build:
- Editor: ~30% mais lento (overhead)
- Build: Performance real

---

### FPS Counter Script

```csharp
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private float fps;
    private float accumulator = 0f;
    private int frames = 0;
    private float timeLeft;

    void Start()
    {
        timeLeft = updateInterval;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        accumulator += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeLeft <= 0f)
        {
            fps = accumulator / frames;
            fpsText.text = $"FPS: {fps:F1}";

            // Color code
            if (fps >= 60)
                fpsText.color = Color.green;
            else if (fps >= 30)
                fpsText.color = Color.yellow;
            else
                fpsText.color = Color.red;

            accumulator = 0f;
            frames = 0;
            timeLeft = updateInterval;
        }
    }
}
```

---

### Network Stats Display

```csharp
using Unity.Netcode;
using TMPro;

public class NetworkStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;

    void Update()
    {
        if (!NetworkManager.Singleton.IsConnectedClient)
            return;

        var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;

        statsText.text = $"Ping: {GetPing()}ms\n" +
                         $"Bandwidth: {GetBandwidth()} KB/s";
    }

    float GetPing()
    {
        // Implementação depende do transport
        return 0f; // Placeholder
    }

    float GetBandwidth()
    {
        // Bytes enviados/recebidos por segundo
        return 0f; // Placeholder
    }
}
```

---

## 📈 Benchmarks por Fase

### Fase 1: Single Player Base (Atual)

**FPS Target**: 60 FPS
**Cenário**: 1 Player, 5 inimigos, cena média

| Métrica | Atual | Target | Status |
|---------|-------|--------|--------|
| FPS | 60+ | 60 | ✅ OK |
| Frame Time | ~12ms | <16ms | ✅ OK |
| Memory | ~800MB | <1.5GB | ✅ OK |
| Draw Calls | ~150 | <300 | ✅ OK |

**Gargalos**:
- Nenhum significativo

---

### Fase 2: Multiplayer Básico (2 Players)

**FPS Target**: 60 FPS
**Cenário**: 2 Players, 5 inimigos, networking

| Métrica | Esperado | Target | Ação |
|---------|----------|--------|------|
| FPS | 55-60 | 60 | Otimizar se < 60 |
| Latency | 30-50ms | <50ms | ✅ OK |
| Bandwidth/player | 20-30 KB/s | <50KB/s | ✅ OK |
| Memory | ~1GB | <1.5GB | ✅ OK |

**Otimizações necessárias**:
- ✅ NetworkTransform com threshold
- ✅ Interest management (opcional)

---

### Fase 3: Multiplayer + IA Avançada (4 Players)

**FPS Target**: 60 FPS
**Cenário**: 4 Players, 10 inimigos com GOAP/BT

| Métrica | Antes | Depois | Target | Status |
|---------|-------|--------|--------|--------|
| FPS | 45 | 60 | 60 | ⚠️ Precisa otimizar |
| AI Update Time | 8ms | 3ms | <5ms | ✅ Após otimizar |
| Latency | 60ms | 50ms | <100ms | ✅ OK |
| Memory | 1.4GB | 1.2GB | <2GB | ✅ OK |

**Otimizações necessárias**:
- 🔴 **CRÍTICO**: Stagger AI updates
- 🟡 **Importante**: LOD para IA
- 🟡 **Importante**: Octree para busca

---

### Fase 4: Polido e Otimizado

**FPS Target**: 60 FPS constante
**Cenário**: 4 Players, 15+ inimigos, efeitos visuais

| Métrica | Target | Otimizações |
|---------|--------|-------------|
| FPS | 60 | Todas implementadas |
| Frame Time | <16ms | ✅ |
| AI Time | <3ms | Octree + Batch |
| Memory | <1.5GB | Pooling + Cleanup |
| Bandwidth | <50KB/s | Interest management |

---

## 🔄 Comparações: Com vs Sem Otimizações

### Octree para Busca de Inimigos

**Cenário**: 4 players, 20 inimigos, cada player busca inimigos num raio de 30m.

#### Sem Octree (O(n))
```csharp
// Para cada player, checa TODOS os inimigos
List<Enemy> GetNearby(Vector3 pos, float radius)
{
    List<Enemy> nearby = new List<Enemy>();
    foreach (var enemy in allEnemies) // 20 iterações
    {
        if (Vector3.Distance(pos, enemy.position) < radius)
            nearby.Add(enemy);
    }
    return nearby;
}
```

| Métrica | Valor |
|---------|-------|
| Iterações | 80 (4 players × 20 enemies) |
| Tempo | ~1.2ms |
| Complexidade | O(n) |

#### Com Octree (O(log n))
```csharp
List<Enemy> GetNearby(Vector3 pos, float radius)
{
    return octree.GetNearby(pos, radius); // Usa spatial partitioning
}
```

| Métrica | Valor | Melhoria |
|---------|-------|----------|
| Iterações | ~8 | **90% menos** |
| Tempo | ~0.15ms | **8x mais rápido** |
| Complexidade | O(log n) | ✅ |

**Resultado**: 1ms economizado por frame!

---

### Batch Raycasting

**Cenário**: 10 inimigos fazendo line-of-sight check.

#### Sem Batch
```csharp
// 10 raycasts individuais
foreach (var enemy in enemies)
{
    Physics.Raycast(enemy.pos, direction, out hit);
}
```

| Métrica | Valor |
|---------|-------|
| Raycasts | 10 individuais |
| Tempo | ~2ms |

#### Com Batch (Job System)
```csharp
var commands = new NativeArray<RaycastCommand>(10, Allocator.TempJob);
var results = new NativeArray<RaycastHit>(10, Allocator.TempJob);

// Setup + Schedule + Complete
JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 32);
handle.Complete();
```

| Métrica | Valor | Melhoria |
|---------|-------|----------|
| Raycasts | 10 em batch |  |
| Tempo | ~0.5ms | **4x mais rápido** |

**Resultado**: 1.5ms economizado!

---

### Object Pooling para Projéteis

**Cenário**: Arma que atira 10 bullets/segundo.

#### Sem Pooling
```csharp
void Shoot()
{
    GameObject bullet = Instantiate(bulletPrefab);
    Destroy(bullet, 3f);
}
```

| Métrica | Valor |
|---------|-------|
| Instantiate/s | 10 |
| Destroy/s | 10 |
| GC Allocations | ~500 KB/s |
| Frame hitches | Frequentes |

#### Com Pooling
```csharp
void Shoot()
{
    GameObject bullet = bulletPool.Get();
    // Retorna após 3s
    StartCoroutine(ReturnAfterDelay(bullet, 3f));
}
```

| Métrica | Valor | Melhoria |
|---------|-------|----------|
| Instantiate/s | 0 (após warmup) | ✅ |
| Destroy/s | 0 | ✅ |
| GC Allocations | ~0 KB/s | **100% menos** |
| Frame hitches | Nenhum | ✅ |

---

### Event Bus vs Static Events

**Cenário**: 100 eventos disparados por segundo.

#### Static Events (GameEvents.cs)
```csharp
public static Action<float> OnPlayerDamaged;

OnPlayerDamaged?.Invoke(damage); // Boxing se struct
```

| Métrica | Valor |
|---------|-------|
| GC Alloc (struct boxing) | ~50 KB/s |
| Type safety | ❌ Runtime |

#### Event Bus
```csharp
EventBus<PlayerDamagedEvent>.Raise(new PlayerDamagedEvent
{
    damage = 10f
});
```

| Métrica | Valor | Melhoria |
|---------|-------|----------|
| GC Alloc | ~10 KB/s | **80% menos** |
| Type safety | ✅ Compile time | ✅ |

---

### Improved Timers vs Update Manual

**Cenário**: 20 timers ativos (cooldowns, delays, etc).

#### Update Manual
```csharp
void Update()
{
    cooldownTimer -= Time.deltaTime;
    if (cooldownTimer <= 0)
    {
        // Fazer algo
    }
}
```

| Métrica | Valor |
|---------|-------|
| Update calls | 20 MonoBehaviours |
| Code lines | ~60 |
| Bugs | Esquece de resetar |

#### Improved Timers (Player Loop)
```csharp
private CountdownTimer timer;

void Start()
{
    timer = new CountdownTimer(5f);
    timer.OnTimerStop += OnComplete;
    timer.Start();
}
```

| Métrica | Valor | Melhoria |
|---------|-------|----------|
| Update calls | 1 manager centralizado | **95% menos** |
| Code lines | ~15 | **75% menos** |
| Bugs | Auto-gerenciado | ✅ |

---

## 🐛 Bottlenecks Comuns

### 1. GetComponent no Update

**Problema**:
```csharp
void Update()
{
    var health = GetComponent<EnemyHealth>(); // CARO!
}
```

**Impacto**: ~0.5ms para 10 inimigos

**Solução**:
```csharp
private EnemyHealth health;

void Start()
{
    health = GetComponent<EnemyHealth>(); // Cache
}
```

**Melhoria**: ~0.5ms economizado

---

### 2. Find no Update

**Problema**:
```csharp
void Update()
{
    GameObject player = GameObject.Find("Player"); // MUITO CARO!
}
```

**Impacto**: ~2-5ms

**Solução**:
```csharp
private GameObject player;

void Start()
{
    player = GameObject.FindGameObjectWithTag("Player");
}
```

**Melhoria**: 2-5ms economizado

---

### 3. Camera.main

**Problema**:
```csharp
void Update()
{
    Camera.main.transform.position; // GetComponent interno
}
```

**Solução**:
```csharp
private Camera mainCamera;

void Start()
{
    mainCamera = Camera.main; // Cache
}
```

---

### 4. String Concatenation em Hot Path

**Problema**:
```csharp
void Update()
{
    string text = "Health: " + health; // Aloca string
}
```

**Impacto**: ~20 KB GC allocations/s

**Solução**:
```csharp
// Atualizar apenas quando muda
private int lastHealth;

void Update()
{
    if (health != lastHealth)
    {
        text = $"Health: {health}";
        lastHealth = health;
    }
}
```

---

### 5. Instantiate/Destroy Frequente

**Problema**: Bullets, VFX, projectiles

**Solução**: Object Pooling (veja comparação acima)

---

### 6. Physics.OverlapSphere Todo Frame

**Problema**:
```csharp
void Update()
{
    Collider[] hits = Physics.OverlapSphere(pos, radius); // CARO
}
```

**Solução**:
```csharp
private float checkInterval = 0.5f;
private float lastCheck;

void Update()
{
    if (Time.time - lastCheck > checkInterval)
    {
        Collider[] hits = Physics.OverlapSphere(pos, radius);
        lastCheck = Time.time;
    }
}
```

---

## 🔥 Testes de Stress

### Teste 1: Horda de Inimigos

**Objetivo**: Ver quantos inimigos mantêm 60 FPS.

**Setup**:
- 1 Player
- Inimigos em onda crescente (5, 10, 20, 50)
- IA simples (FSM)

**Resultados Esperados**:

| Inimigos | FPS | Status |
|----------|-----|--------|
| 5 | 60 | ✅ |
| 10 | 60 | ✅ |
| 20 | 55-60 | ⚠️ |
| 50 | 40-50 | ❌ Precisa otimizar |

**Target**: 20+ inimigos a 60 FPS.

---

### Teste 2: Multiplayer Stress

**Objetivo**: Testar latência e bandwidth com carga.

**Setup**:
- 4 Players
- 15 Inimigos
- Ação constante (tiro, movimento)

**Métricas**:

| Métrica | Target | Resultado |
|---------|--------|-----------|
| FPS | 60 | ? |
| Latency | <100ms | ? |
| Bandwidth | <50KB/s | ? |
| Packet Loss | <2% | ? |

---

### Teste 3: Memory Leak

**Objetivo**: Detectar memory leaks.

**Processo**:
1. Jogar por 10 minutos
2. Monitorar memory no Profiler
3. Memory deve estabilizar

**Red flag**: Memory crescendo constantemente.

**Causas comuns**:
- Events não desregistrados
- Coroutines não paradas
- Static references acumulando

---

### Teste 4: Network Resilience

**Objetivo**: Testar com má conexão.

**Setup**:
- Simular latência (100-200ms)
- Simular packet loss (5-10%)

**Ferramentas**:
- Network Simulator (Unity)
- Clumsy (Windows)
- Network Link Conditioner (Mac)

**Target**: Jogo ainda jogável com 150ms latency.

---

## 📋 Checklist de Performance

### Antes de Build Final

**CPU**:
- [ ] FPS >= 60 em cenas principais
- [ ] Frame time < 16ms
- [ ] Nenhum GetComponent/Find no Update
- [ ] AI atualiza de forma staggered

**Memory**:
- [ ] Total memory < 2GB
- [ ] GC allocations < 50KB/frame
- [ ] Sem memory leaks (teste 10min)
- [ ] Object pooling para objetos frequentes

**Rendering**:
- [ ] Draw calls < 300
- [ ] Batching habilitado
- [ ] Occlusion culling configurado
- [ ] LODs para objetos distantes

**Networking** (se multiplayer):
- [ ] Latência < 100ms
- [ ] Bandwidth < 50KB/s por player
- [ ] Packet loss < 2%
- [ ] Testado com 4 players

**AI**:
- [ ] AI update time < 5ms (10 enemies)
- [ ] Pathfinding < 0.5ms por enemy
- [ ] Octree implementado
- [ ] Raycasts em batch

---

## 🎯 Targets Finais

### Requisitos Mínimos (PC)
```
CPU: Intel i5-4460 / AMD FX-6300
GPU: GTX 750 Ti / R7 260X
RAM: 8 GB
FPS: 30 FPS constante
```

### Requisitos Recomendados (PC)
```
CPU: Intel i5-9400 / AMD Ryzen 5 2600
GPU: GTX 1060 6GB / RX 580
RAM: 16 GB
FPS: 60 FPS constante
```

### Performance Multiplayer
```
Players: 2-4
Enemies: 15+
Latency: <100ms
Bandwidth: <50KB/s per player
FPS: 60 constante
```

---

## 📊 Relatório de Benchmark Template

Use este template para documentar seus benchmarks:

```markdown
## Benchmark Report

**Data**: [Data]
**Versão**: [v1.0]
**Build**: [Development/Release]

### Setup
- Players: [1-4]
- Enemies: [número]
- Cena: [nome]
- Duração: [minutos]

### Resultados

| Métrica | Resultado | Target | Status |
|---------|-----------|--------|--------|
| FPS médio | 58 | 60 | ⚠️ |
| Frame time | 17ms | <16ms | ⚠️ |
| Memory | 1.2GB | <1.5GB | ✅ |
| Latency | 45ms | <100ms | ✅ |

### Gargalos Identificados
1. AI updates taking 6ms (target: 3ms)
2. Raycasts individuais (usar batch)

### Ações
- [ ] Implementar staggered AI updates
- [ ] Implementar batch raycasting
- [ ] Re-teste após otimizações
```

---

**📌 Importante**: Sempre profile em build real, não só no Editor!

**🔬 Metodologia**: Teste múltiplas vezes, pegue média, elimine outliers!

**📈 Progresso**: Documente benchmarks regularmente para track melhorias!
