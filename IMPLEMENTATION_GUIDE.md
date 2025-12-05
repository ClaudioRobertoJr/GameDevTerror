# 🛠️ Guia de Implementação - Survival Horror Multiplayer

## 📋 Índice

1. [Setup Inicial do Projeto](#setup-inicial-do-projeto)
2. [Sistemas Implementados](#sistemas-implementados)
3. [Ordem de Implementação](#ordem-de-implementação)
4. [Integração dos Sistemas](#integração-dos-sistemas)
5. [Testing e Debug](#testing-e-debug)
6. [Troubleshooting](#troubleshooting)

---

## 🚀 Setup Inicial do Projeto

### 1. Unity e Packages

**Unity Version:** 2022.3 LTS (recomendado) ou Unity 6

**Packages Necessários:**
```
Window > Package Manager > + > Add package by name

1. com.unity.netcode.gameobjects (1.8.0+)
2. com.unity.transport (2.0+)
3. com.unity.services.core
4. com.unity.services.lobby
5. com.unity.services.relay
6. com.unity.inputsystem (1.7.0+)
7. com.unity.cinemachine
8. com.unity.render-pipelines.universal (se usar URP)
```

### 2. Project Settings

```
Edit > Project Settings

【Input System】
Active Input Handling: Input System Package (New)

【Physics】
Fixed Timestep: 0.02 (50Hz)
Solver Iterations: 10

【Quality】
VSync Count: Don't Sync
Anti Aliasing: 4x MSAA

【Player】
API Compatibility Level: .NET Standard 2.1
Allow 'unsafe' Code: ✓
```

### 3. Unity Gaming Services Setup

```
1. Window > General > Services
2. Login ou criar conta Unity
3. Create new Unity Project ID
4. Enable:
   ☑ Authentication
   ☑ Lobby
   ☑ Relay
```

### 4. Estrutura de Pastas

A estrutura já está criada em `Assets/Scripts/`:
```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs (já existe)
│   ├── GameEvents.cs (já existe)
│   └── GameBalancer.cs (✨ NOVO)
├── Survival/
│   └── PlayerSurvivalStats.cs (✨ NOVO)
├── Building/
│   ├── BuildingSystem.cs (✨ NOVO)
│   ├── BuildableStructure.cs (✨ NOVO)
│   └── BuildingDatabase.cs (✨ NOVO)
├── Environment/
│   ├── DayNightCycle.cs (✨ NOVO)
│   └── AtmosphereController.cs (já existe)
├── Enemy/
│   ├── WaveSystem.cs (✨ NOVO)
│   ├── EnemyBase.cs (já existe)
│   └── Network/NetworkEnemyController.cs (já existe)
├── Player/
│   ├── Movement/ (já existem)
│   ├── Health/ (já existe)
│   └── Network/NetworkPlayerController.cs (já existe)
└── Networking/ (já existem sistemas)
```

---

## 🎯 Sistemas Implementados

### ✅ Sistema de Sobrevivência (`PlayerSurvivalStats.cs`)

**O que faz:**
- Gerencia stats: Fome, Sede, Temperatura, Sanidade
- Drenagem automática baseada em ações
- Efeitos de stats baixos
- Sincronizado em multiplayer

**Como usar:**
```csharp
// Anexar ao prefab do Player
1. Add Component > Player Survival Stats
2. Configurar stats máximos
3. Ajustar taxas de drenagem

// No código:
PlayerSurvivalStats stats = GetComponent<PlayerSurvivalStats>();
stats.Eat(30f);        // Restaurar fome
stats.Drink(50f);      // Restaurar sede
stats.SetRunning(true); // Aumenta drenagem de fome
```

**Integração:**
- Conectar com `FirstPersonController` para detectar corrida
- Conectar com `DayNightCycle` para temperatura noturna
- Conectar com sistema de iluminação para sanidade

### ✅ Sistema de Construção (`BuildingSystem.cs`)

**O que faz:**
- Modo de construção com preview
- Grid snapping
- Validação de placement
- Gerenciamento de recursos
- Sincronizado em multiplayer

**Como usar:**
```csharp
// Anexar ao prefab do Player
1. Add Component > Building System
2. Configurar layers de placement
3. Criar materiais de preview (verde/vermelho)

// Criar estruturas (ScriptableObjects):
Right-click > Create > Survival Horror > Building > Structure
- Definir prefab, custo, tamanho
- Criar preview (cópia do prefab com materiais transparentes)

// No código:
BuildingSystem builder = GetComponent<BuildingSystem>();
BuildableStructure wall = ...; // Carregar do database
builder.EnterBuildMode(wall);
```

**Database Setup:**
```csharp
1. Create GameObject: "BuildingDatabase"
2. Add Component > Building Database
3. Arrastar todos ScriptableObjects de estruturas
```

### ✅ Sistema Dia/Noite (`DayNightCycle.cs`)

**O que faz:**
- Ciclo completo de 24 horas (24 min real)
- Rotação do sol
- Iluminação dinâmica
- Névoa adaptativa
- Sincronizado em multiplayer

**Como usar:**
```csharp
// Setup na cena:
1. Create GameObject: "DayNightCycle"
2. Add Component > Day Night Cycle
3. Configurar:
   - Directional Light (sol)
   - Light Color (Gradient)
   - Light Intensity (Animation Curve)
   - Fog settings

// Events:
DayNightCycle.Instance.OnNightStarted += HandleNightStart;
DayNightCycle.Instance.OnDayStarted += HandleDayStart;

// Propriedades:
bool isNight = DayNightCycle.Instance.IsNight;
int day = DayNightCycle.Instance.CurrentDay;
```

### ✅ Sistema de Waves (`WaveSystem.cs`)

**O que faz:**
- Spawna ondas de inimigos durante a noite
- Dificuldade crescente por dia
- Balanceamento automático por nº de jogadores
- Sincronizado em multiplayer

**Como usar:**
```csharp
// Setup na cena:
1. Create GameObject: "WaveSystem"
2. Add Component > Wave System
3. Configurar:
   - Wave Configurations (array)
   - Spawn Points (Transform[])
   - Enemy Prefabs (stalker, hunter, etc)

// Wave Config:
WaveConfiguration {
  BaseEnemyCount: 10
  StalkerChance: 0.4 (40%)
  HunterChance: 0.3 (30%)
  BruteChance: 0.2 (20%)
  ScreamerChance: 0.1 (10%)
}

// Events:
WaveSystem.Instance.OnWaveStarted += (wave) => Debug.Log($"Wave {wave}!");
WaveSystem.Instance.OnWaveCompleted += (wave) => Debug.Log("Survived!");
```

### ✅ Sistema de Balanceamento (`GameBalancer.cs`)

**O que faz:**
- Ajusta dificuldade solo vs multiplayer
- Modifica quantidade/HP de inimigos
- Bônus de recursos para solo
- Redução de custos para solo

**Como usar:**
```csharp
// Setup na cena:
1. Create GameObject: "GameBalancer" (DontDestroyOnLoad)
2. Add Component > Game Balancer
3. Ajustar multiplicadores

// Usar nos sistemas:
int enemyCount = GameBalancer.Instance.GetScaledEnemyCount(10);
float enemyHP = GameBalancer.Instance.GetScaledEnemyHealth(100f);
int resources = GameBalancer.Instance.GetScaledResourceDrop(5);

// Verificar modo:
if (GameBalancer.Instance.IsSolo()) {
  // Lógica específica de solo
}
```

---

## 📝 Ordem de Implementação

### ✅ Fase 1: Core Systems (COMPLETO)

- [x] GameManager (já existe)
- [x] GameEvents (já existe)
- [x] GameBalancer (✨ criado)
- [x] NetworkPlayer (já existe)

### ✅ Fase 2: Survival & Building (COMPLETO)

- [x] PlayerSurvivalStats (✨ criado)
- [x] BuildingSystem (✨ criado)
- [x] BuildableStructure SO (✨ criado)
- [x] BuildingDatabase (✨ criado)

### ✅ Fase 3: Day/Night & Waves (COMPLETO)

- [x] DayNightCycle (✨ criado)
- [x] WaveSystem (✨ criado)
- [x] Wave Configurations

### 🔄 Fase 4: Integração (PRÓXIMO PASSO)

**4.1. Integrar Sobrevivência com Player**
```csharp
// Em FirstPersonController.cs, adicionar:
private PlayerSurvivalStats survivalStats;

void Awake() {
    survivalStats = GetComponent<PlayerSurvivalStats>();
}

void Update() {
    // Detectar corrida
    bool isRunning = Input.GetKey(KeyCode.LeftShift);
    survivalStats.SetRunning(isRunning);

    // Bloquear corrida se muito fraco
    if (!survivalStats.CanRun()) {
        canRun = false;
    }
}
```

**4.2. Integrar Building com Input**
```csharp
// Criar UI para selecionar estruturas
// Quando jogador clica em botão de estrutura:
BuildableStructure selectedStructure = ...;
buildingSystem.EnterBuildMode(selectedStructure);

// Hotkey para sair do build mode (ESC)
if (Input.GetKeyDown(KeyCode.Escape)) {
    buildingSystem.ExitBuildMode();
}
```

**4.3. Conectar DayNight com WaveSystem**
```csharp
// No WaveSystem OnNetworkSpawn():
DayNightCycle.Instance.OnNightStarted += StartNightWaves;
DayNightCycle.Instance.OnDayStarted += StopWaves;

// Já implementado! ✅
```

**4.4. Integrar GameBalancer**
```csharp
// No WaveSystem, usar balancer:
int scaledCount = GameBalancer.Instance.GetScaledEnemyCount(baseCount);

// No EnemySpawner, ajustar HP:
float scaledHP = GameBalancer.Instance.GetScaledEnemyHealth(baseHP);

// No ResourceDrop:
int amount = GameBalancer.Instance.GetScaledResourceDrop(baseAmount);
```

### 🔨 Fase 5: Content Creation

**5.1. Criar Estruturas (ScriptableObjects)**
```
1. Right-click > Create > Survival Horror > Building > Structure

Criar pelo menos:
- Wooden Wall (10 madeira, HP: 100)
- Stone Wall (15 pedra, HP: 250)
- Door (5 madeira)
- Campfire (5 madeira)
- Storage Chest (10 madeira)
- Watchtower (20 madeira + 10 pedra)
```

**5.2. Criar Prefabs de Estruturas**
```
Para cada estrutura:
1. Criar modelo 3D (ou placeholder cubo)
2. Adicionar colliders
3. Adicionar NetworkObject component
4. Criar versão "Preview" (material transparente)
5. Salvar como prefab
```

**5.3. Configurar WaveSystem**
```
Criar 7 WaveConfigurations:

Night 1:
  BaseEnemyCount: 10
  Stalker: 60%, Hunter: 30%, Brute: 10%

Night 2:
  BaseEnemyCount: 15
  Stalker: 50%, Hunter: 35%, Brute: 15%

...

Night 7 (BOSS):
  BaseEnemyCount: 50
  Boss: 100% (spawna 1 boss + horda)
```

**5.4. Configurar Iluminação**
```
1. Criar Gradient para Light Color:
   - 0h (meia-noite): azul escuro
   - 6h (amanhecer): laranja
   - 12h (meio-dia): branco
   - 18h (entardecer): vermelho/laranja
   - 24h: azul escuro

2. Criar Curve para Light Intensity:
   - Noite: 0.1-0.3
   - Dia: 1.0-1.5
```

### 🧪 Fase 6: Testing & Balance

**6.1. Testar Solo**
```
1. Iniciar como Host (1 jogador)
2. Verificar:
   - Stats drenam corretamente?
   - Pode construir estruturas?
   - Noite spawna inimigos?
   - Bônus de solo estão ativos?
```

**6.2. Testar Multiplayer**
```
1. Iniciar como Host
2. Conectar 2-4 clientes
3. Verificar:
   - Stats sincronizam?
   - Estruturas aparecem para todos?
   - Inimigos aumentam?
   - Balanceamento correto?
```

**6.3. Balancear**
```
Ajustar em GameBalancer:
- Multiplicadores de inimigos
- Bônus de recursos
- Custos de crafting

Ajustar em WaveSystem:
- Quantidade base de inimigos
- Intervalo entre waves
- Dificuldade por dia
```

---

## 🔗 Integração dos Sistemas

### Fluxo Completo do Gameplay

```
┌─────────────────────────────────────┐
│ 1. LOBBY                            │
│    - Players entram                 │
│    - Host cria relay                │
│    - Todos ready → Start Game       │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ 2. GAME START (Dia 1, 07:00)        │
│    - Spawn players                  │
│    - Iniciar DayNightCycle          │
│    - Ativar GameBalancer            │
│    - Inicializar stats              │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ 3. DIA (Exploração)                 │
│    - Coletar recursos               │
│    - Explorar mapa                  │
│    - Stats drenam                   │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ 4. TARDE (Construção)               │
│    - Voltar para base               │
│    - Construir defesas              │
│    - Preparar para noite            │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ 5. NOITE (19:00)                    │
│    ✨ DayNightCycle.OnNightStarted  │
│    → WaveSystem.StartNightWaves()   │
│    - Spawnar inimigos               │
│    - Defender base                  │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ 6. AMANHECER (05:00)                │
│    ✨ DayNightCycle.OnDayStarted    │
│    → WaveSystem.StopWaves()         │
│    - Inimigos fogem                 │
│    - Novo dia começa                │
└──────────────┬──────────────────────┘
               ↓
         Repetir até Dia 7
               ↓
┌─────────────────────────────────────┐
│ 7. BOSS FIGHT (Noite 7)             │
│    - Wave especial com boss         │
│    - Vitória ou derrota             │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│ 8. FIM DE JOGO                      │
│    - Estatísticas                   │
│    - Voltar ao lobby                │
└─────────────────────────────────────┘
```

### Comunicação entre Sistemas

```csharp
// GameBalancer → WaveSystem
int enemyCount = GameBalancer.Instance.GetScaledEnemyCount(baseCount);

// DayNightCycle → WaveSystem
DayNightCycle.OnNightStarted → WaveSystem.StartNightWaves()

// DayNightCycle → PlayerSurvivalStats
if (DayNightCycle.Instance.IsNight) {
    temperatureDrain *= coldMultiplier;
}

// PlayerSurvivalStats → FirstPersonController
if (!survivalStats.CanRun()) {
    canRun = false;
}

// BuildingSystem → BuildingDatabase
BuildableStructure structure = BuildingDatabase.Instance.GetStructureByID(id);

// WaveSystem → GameBalancer
float enemyHP = GameBalancer.Instance.GetScaledEnemyHealth(baseHP);
```

---

## 🧪 Testing e Debug

### Debug Tools Incluídos

**1. Survival Stats (OnGUI)**
```
Mostra no canto superior esquerdo:
🍖 Hunger: 75.0 (75%)
💧 Thirst: 60.0 (60%)
🥶 Temperature: 80.0 (80%)
😰 Sanity: 90.0 (90%)
```

**2. Day/Night Cycle (OnGUI)**
```
Mostra no topo:
🌍 Day 3 - 14:35
⏰ Afternoon ☀️

Botões (apenas Server, Debug build):
[Skip to Night] [Skip to Day]
```

**3. Game Balancer (OnGUI)**
```
Mostra stats de balanceamento:
👥 Players: 3
Enemies: x1.60 HP: x1.30
Resources: x1.40
```

### Console Commands

**Força início de wave (Debug):**
```csharp
// No console ou botão de debug:
WaveSystem.Instance.ForceStartWaveServerRpc();
```

**Ajustar tempo:**
```csharp
// Pular para noite:
DayNightCycle.Instance.SetTimeServerRpc(19f);

// Pular para dia:
DayNightCycle.Instance.SetTimeServerRpc(7f);
```

**Testar sobrevivência:**
```csharp
// Dar recursos ao player:
PlayerSurvivalStats stats = player.GetComponent<PlayerSurvivalStats>();
stats.Eat(50f);
stats.Drink(50f);
stats.Warm(30f);
stats.RestoreSanity(20f);
```

### Test Checklist

```
□ Solo Mode
  □ Stats drenam corretamente
  □ Bônus de solo aplicados
  □ Ondas ajustadas (-50% inimigos)
  □ Pode construir com -20% custo

□ Multiplayer (2 players)
  □ Stats sincronizam entre clientes
  □ Ondas aumentam (+30% inimigos)
  □ Estruturas aparecem para todos
  □ Balanceamento correto

□ Multiplayer (4 players)
  □ Ondas aumentam (+90% inimigos)
  □ HP de inimigos aumenta (+45%)
  □ Recursos aumentam (+60%)

□ Day/Night Cycle
  □ Sol rota corretamente
  □ Iluminação muda
  □ Noite inicia waves
  □ Dia para waves

□ Building System
  □ Preview mostra corretamente
  □ Valida placement
  □ Consome recursos
  □ Spawna na rede

□ Wave System
  □ Spawna quantidade correta
  □ Balanceia por jogadores
  □ Para ao amanhecer
  □ Boss spawna no dia 7
```

---

## 🐛 Troubleshooting

### Problema: Stats não sincronizam

**Solução:**
```csharp
// Verificar que PlayerSurvivalStats tem:
public class PlayerSurvivalStats : NetworkBehaviour {
    private NetworkVariable<float> hunger = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone, // ← Importante!
        NetworkVariableWritePermission.Owner    // ← Owner escreve
    );
}

// E que só owner executa Update:
void Update() {
    if (!IsOwner) return; // ← Importante!
    // ...
}
```

### Problema: Estruturas não aparecem para clientes

**Solução:**
```csharp
// Verificar que spawning é feito no servidor:
[ServerRpc(RequireOwnership = false)]
private void SpawnStructureServerRpc(...) {
    GameObject obj = Instantiate(prefab, position, rotation);
    NetworkObject netObj = obj.GetComponent<NetworkObject>();
    netObj.Spawn(); // ← Spawna na rede
}
```

### Problema: Waves não começam à noite

**Solução:**
```csharp
// Verificar subscription no OnNetworkSpawn:
public override void OnNetworkSpawn() {
    if (IsServer) {
        DayNightCycle.Instance.OnNightStarted += StartNightWaves;
    }
}

// Verificar que DayNightCycle está na cena
// Verificar que é NetworkBehaviour e está spawned
```

### Problema: Balanceamento não funciona

**Solução:**
```csharp
// Verificar que GameBalancer está spawned:
// GameObject com NetworkObject component deve estar na cena inicial
// DontDestroyOnLoad para persistir entre cenas

// Verificar que está usando Instance corretamente:
GameBalancer.Instance.GetScaledEnemyCount(10);
```

### Problema: Preview de construção não aparece

**Solução:**
```csharp
// Verificar que BuildableStructure tem PreviewPrefab configurado
// Verificar que materiais de preview estão assignados no BuildingSystem
// Verificar que raycast está acertando o chão (layer correto)
```

---

## 📚 Próximos Passos

### Features Adicionais para Implementar

1. **Sistema de Crafting Expandido**
   - UI de crafting
   - Receitas desbloqueáveis
   - Bancada de crafting avançada

2. **Sistema de Inventário**
   - Hotbar
   - Inventário completo
   - Drag & drop

3. **Sistema de Coleta de Recursos**
   - Árvores, rochas, arbustos
   - Ferramentas (machado, picareta)
   - Durabilidade de ferramentas

4. **Sistema de Revive (Multiplayer)**
   - Estado "down" quando morrer
   - Aliados podem reviver
   - Timer de 60s

5. **Sistema de UI Completo**
   - HUD com stats
   - Building menu
   - Crafting menu
   - Inventory

6. **Sistema de Save/Load**
   - Salvar progresso
   - Checkpoints
   - Carregar save

7. **Boss Fight Final**
   - Boss especial noite 7
   - 3 fases
   - Mecânicas únicas

---

## ✅ Status dos Sistemas

| Sistema | Status | Arquivo |
|---------|--------|---------|
| Sobrevivência | ✅ Implementado | `PlayerSurvivalStats.cs` |
| Construção | ✅ Implementado | `BuildingSystem.cs` |
| Dia/Noite | ✅ Implementado | `DayNightCycle.cs` |
| Waves | ✅ Implementado | `WaveSystem.cs` |
| Balanceamento | ✅ Implementado | `GameBalancer.cs` |
| Player FPS | ✅ Já existe | `FirstPersonController.cs` |
| Networking | ✅ Já existe | `NetworkPlayerController.cs` |
| IA Inimigos | ✅ Já existe | `EnemyBase.cs` |

**Pronto para começar a integrar e testar!** 🚀

---

**Última atualização:** 2025-12-05
**Versão:** 1.0
