# 🎮 Horror Game Framework - Unity 6

Framework completo para desenvolvimento de jogos de terror em primeira pessoa usando Unity 6.

## 📚 Documentação

Este projeto inclui documentação completa e profissional:

### 🔥🔥 **NOVÍSSIMO!** Survival Horror Multiplayer/Solo (1-5 Jogadores)
- **🏕️ [SURVIVAL_HORROR_GAME_DESIGN.md](SURVIVAL_HORROR_GAME_DESIGN.md)** - **GAME DESIGN COMPLETO!**
  - Jogo de sobrevivência na floresta (7 dias)
  - Sistema de construção de base
  - Sistema de sobrevivência (fome, sede, temperatura, sanidade)
  - Ciclo dia/noite dinâmico
  - Ondas de inimigos nas noites
  - Balanceamento automático solo vs multiplayer
- **🛠️ [IMPLEMENTATION_GUIDE.md](IMPLEMENTATION_GUIDE.md)** - **GUIA DE IMPLEMENTAÇÃO**
  - Setup passo a passo
  - Integração de todos os sistemas
  - Testing e troubleshooting
  - Código pronto para usar

### 🔥 **NOVO!** Multiplayer FPS Horror (4-5 Jogadores)
- **⚡ [MULTIPLAYER_QUICKSTART.md](MULTIPLAYER_QUICKSTART.md)** - **COMECE AQUI!** Setup rápido em 5 minutos
- **🎮 [MULTIPLAYER_FPS_HORROR_GUIDE.md](MULTIPLAYER_FPS_HORROR_GUIDE.md)** - **GUIA COMPLETO Parte 1**
  - Stack tecnológico moderno (Unity 2022.3 LTS)
  - New Input System
  - Sistema de Lobby (Unity Gaming Services)
  - Relay (sem port forwarding)
- **🎯 [MULTIPLAYER_FPS_HORROR_GUIDE_PART2.md](MULTIPLAYER_FPS_HORROR_GUIDE_PART2.md)** - **GUIA COMPLETO Parte 2**
  - Player Controller Multiplayer
  - Sistema de Inimigos em Rede
  - IA Avançada (GOAP)
  - Match System
  - Roadmap de 12 semanas

### Documentação Base
- **📖 [README.md](README.md)** - Visão geral e documentação principal (você está aqui!)
- **🚀 [QUICK_START.md](QUICK_START.md)** - Guia passo a passo para começar em 30 minutos (single player)
- **🎮👥 [COOP_PUZZLE_GAME_GUIDE.md](COOP_PUZZLE_GAME_GUIDE.md)** - Guia completo para criar jogo cooperativo com puzzles
- **💡 [TIPS_AND_IMPROVEMENTS.md](TIPS_AND_IMPROVEMENTS.md)** - Dicas, otimizações e features avançadas
- **📖 [API_REFERENCE.md](API_REFERENCE.md)** - Referência completa de todas as APIs
- **🏗️ [STRUCTURE.md](STRUCTURE.md)** - Arquitetura detalhada do framework
- **📝 [EXAMPLES.md](EXAMPLES.md)** - Exemplos de código prontos para usar
- **❓ [FAQ_AND_TROUBLESHOOTING.md](FAQ_AND_TROUBLESHOOTING.md)** - Perguntas frequentes e soluções de problemas

### 🚀 Documentação Avançada - Horror Cooperativo
- **🌟 [ADVANCED_TECHNOLOGIES.md](ADVANCED_TECHNOLOGIES.md)** - Tecnologias avançadas (GOAP, BT, Utility AI, Octrees, Event Bus)
- **🌐 [MULTIPLAYER_ARCHITECTURE.md](MULTIPLAYER_ARCHITECTURE.md)** - Arquitetura completa para modo cooperativo com Netcode
- **🤖 [ADVANCED_AI_SYSTEMS.md](ADVANCED_AI_SYSTEMS.md)** - Sistemas de IA avançados com exemplos completos
- **🗺️ [COOP_ROADMAP.md](COOP_ROADMAP.md)** - Roadmap detalhado de 8-12 semanas para implementação
- **🔧 [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md)** - Guia passo a passo de integração de sistemas

### ⚡ Quick Reference
- **📋 [QUICKREF.md](QUICKREF.md)** - Cheat sheet com snippets e comandos rápidos
- **📚 [RESOURCES.md](RESOURCES.md)** - Links organizados (repos, tutoriais, assets, comunidades)
- **📖 [GLOSSARY.md](GLOSSARY.md)** - Glossário técnico completo (networking, IA, patterns)
- **📊 [BENCHMARKS.md](BENCHMARKS.md)** - Performance targets e comparações

> **🔥 Quer fazer um FPS Horror Multiplayer 4-5 jogadores?** Comece pelo **[MULTIPLAYER_QUICKSTART.md](MULTIPLAYER_QUICKSTART.md)**!
> **💡 Novo no Unity ou em jogos de terror?** Comece pelo **[QUICK_START.md](QUICK_START.md)**!
> **🎮 Quer fazer um jogo cooperativo com puzzles?** Veja o **[COOP_PUZZLE_GAME_GUIDE.md](COOP_PUZZLE_GAME_GUIDE.md)**!

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Sistemas Implementados](#sistemas-implementados)
- [Como Usar](#como-usar)
- [Dicas e Melhores Práticas](#dicas-e-melhores-práticas)
- [Próximos Passos](#próximos-passos)

---

## 🎯 Visão Geral

Este framework fornece uma base sólida e modular para criar jogos de terror em primeira pessoa. Todos os sistemas são desacoplados e se comunicam via eventos, facilitando a manutenção e expansão.

### Características Principais

✅ Sistema de movimento FPS completo (andar, correr, agachar, pular)
✅ Sistema de saúde e stamina
✅ Sistema de inimigos com IA baseada em estados
✅ Sistema de atmosfera dinâmica (iluminação, névoa, tensão)
✅ Sistema de áudio com música adaptativa
✅ Sistema de interação com objetos
✅ Sistema de pause e UI
✅ Lanterna com bateria
✅ Checkpoints
✅ Sistema de eventos global

---

## 📁 Estrutura do Projeto

```
Assets/
├── Scripts/
│   ├── Core/                    # Sistemas principais
│   │   ├── GameManager.cs       # Gerenciador principal do jogo
│   │   ├── GameEvents.cs        # Sistema de eventos
│   │   └── SceneController.cs   # Controle de cenas
│   │
│   ├── Player/                  # Tudo relacionado ao player
│   │   ├── Movement/
│   │   │   ├── FirstPersonController.cs  # Movimento FPS
│   │   │   └── MouseLook.cs              # Controle da câmera
│   │   ├── Health/
│   │   │   └── PlayerHealth.cs           # Sistema de vida
│   │   ├── Interaction/
│   │   │   └── PlayerInteraction.cs      # Sistema de interação
│   │   └── Flashlight.cs                 # Lanterna
│   │
│   ├── Enemy/                   # Sistema de inimigos
│   │   ├── EnemyBase.cs         # Classe base abstrata
│   │   ├── AI/
│   │   │   └── EnemyAIState.cs  # Estados da IA
│   │   └── Types/
│   │       └── PatrolEnemy.cs   # Inimigo patrulheiro
│   │
│   ├── Environment/             # Ambiente e interagíveis
│   │   ├── Interactables/
│   │   │   ├── Door.cs          # Portas interagíveis
│   │   │   └── Collectible.cs   # Items coletáveis
│   │   ├── Spawners/
│   │   │   └── EnemySpawner.cs  # Spawner de inimigos
│   │   └── Atmosphere/
│   │       └── AtmosphereController.cs  # Controle de atmosfera
│   │
│   ├── UI/                      # Interface do usuário
│   │   ├── Menus/
│   │   │   └── PauseMenu.cs     # Menu de pausa
│   │   └── HUD/
│   │       └── PlayerHUD.cs     # HUD do player
│   │
│   ├── Audio/
│   │   └── AudioManager.cs      # Gerenciador de áudio
│   │
│   ├── Data/                    # ScriptableObjects
│   │   ├── EnemyData.cs         # Dados de inimigos
│   │   └── GameSettings.cs      # Configurações do jogo
│   │
│   └── Utilities/               # Utilitários
│       ├── Checkpoint.cs
│       └── FPSCounter.cs
│
├── Prefabs/                     # Prefabs organizados
│   ├── Player/
│   ├── Enemy/
│   ├── Environment/
│   └── UI/
│
├── Scenes/                      # Cenas do jogo
├── Materials/                   # Materiais
├── Audio/                       # Arquivos de áudio
│   ├── Music/
│   ├── SFX/
│   └── Ambient/
└── Resources/                   # Recursos carregados dinamicamente
```

---

## 🔧 Sistemas Implementados

### 1. **Core Systems**

#### GameManager
- Singleton persistente entre cenas
- Gerencia estados do jogo (MainMenu, Playing, Paused, GameOver)
- Controla pause/resume
- Gerencia cursor e timeScale

**Uso:**
```csharp
// Pausar jogo
GameManager.Instance.PauseGame();

// Mudar estado
GameManager.Instance.ChangeState(GameState.Playing);

// Iniciar novo jogo
GameManager.Instance.StartNewGame();
```

#### GameEvents
- Sistema de eventos desacoplado
- Permite comunicação entre sistemas sem dependências diretas
- Eventos para player, inimigos, áudio, UI, etc.

**Uso:**
```csharp
// Inscrever em evento
GameEvents.OnPlayerDamaged += HandlePlayerDamage;

// Disparar evento
GameEvents.OnPlayerDamaged?.Invoke(damage, currentHealth);

// Cancelar inscrição (importante!)
void OnDestroy() {
    GameEvents.OnPlayerDamaged -= HandlePlayerDamage;
}
```

### 2. **Player Systems**

#### FirstPersonController
- Movimento suave com aceleração/desaceleração
- Corrida com stamina
- Agachamento dinâmico
- Pulo
- Head bobbing

**Configuração:**
- Ajuste velocidades em `Walk Speed`, `Run Speed`, `Crouch Speed`
- Configure stamina em `Max Stamina`, `Stamina Drain Rate`
- Ative/desative head bobbing

#### PlayerHealth
- Sistema de vida com dano e cura
- Invulnerabilidade temporária após dano
- Regeneração opcional
- Camera shake ao tomar dano
- Dano de queda

#### Flashlight
- Lanterna com bateria
- Recarga automática quando desligada
- Efeito de tremulação com bateria baixa
- Intensidade diminui com bateria baixa

### 3. **Enemy Systems**

#### EnemyBase (Classe Abstrata)
Base para todos os inimigos com:
- Sistema de vida
- Detecção do player (FOV + raycast)
- Sistema de ataque
- Pathfinding com NavMesh

#### PatrolEnemy
Implementação concreta com:
- Patrulha entre waypoints
- Perseguição ao detectar player
- Investigação da última posição conhecida
- Estados: Idle, Patrol, Investigate, Chase, Attack

**Setup:**
1. Adicione `PatrolEnemy` script ao objeto
2. Configure waypoints no array `Patrol Points`
3. Ajuste `Detection Range`, `Attack Range`, etc.
4. Certifique-se que há NavMesh na cena

### 4. **Atmosphere System**

#### AtmosphereController
Sistema dinâmico que:
- Ajusta iluminação baseado na tensão
- Controla densidade e cor da névoa
- Reage à proximidade de inimigos
- Comunica com AudioManager para música adaptativa

**Níveis de Tensão:**
- 0.0 = Calmo, iluminação normal
- 1.0 = Máxima tensão, iluminação escura, névoa densa

### 5. **Audio System**

#### AudioManager
- Música de fundo com crossfade
- Efeitos sonoros 2D e 3D
- Sons ambientes
- Sistema de tensão (aumenta volume quando inimigos próximos)
- Pool de AudioSources para performance

**Uso:**
```csharp
// Tocar música
GameEvents.OnChangeMusicTrack?.Invoke("main_theme");

// Tocar som 2D
GameEvents.OnPlaySound2D?.Invoke("door_open");

// Tocar som 3D em posição
GameEvents.OnPlaySound3D?.Invoke("enemy_growl", enemyPosition);

// Ajustar tensão (0-1)
GameEvents.OnTensionLevelChanged?.Invoke(0.8f);
```

### 6. **Interaction System**

#### IInteractable Interface
Interface para objetos interagíveis:
```csharp
public interface IInteractable
{
    string GetInteractionPrompt();  // Texto mostrado ao player
    bool CanInteract();             // Se pode interagir agora
    void Interact(GameObject player); // Executar interação
}
```

**Implementações:**
- `Door` - Portas que abrem/fecham
- `Collectible` - Items coletáveis (vida, munição, chaves)

### 7. **UI System**

#### PlayerHUD
- Barra de vida
- Barra de stamina (aparece só quando em uso)
- Prompt de interação
- Mensagens temporárias
- Vignette de dano

#### PauseMenu
- Menu de pausa com ESC
- Botões: Resume, Settings, Main Menu, Quit
- Pausa o tempo (timeScale = 0)

---

## 🚀 Como Usar

### Setup Inicial

1. **Criar Cena de Jogo:**
   - Crie uma cena nova
   - Adicione um plano como chão
   - Bake NavMesh (Window > AI > Navigation)

2. **Setup do Player:**
   ```
   Player (GameObject)
   ├── CharacterController
   ├── FirstPersonController
   ├── MouseLook
   ├── PlayerHealth
   ├── PlayerInteraction
   └── Camera
       └── Flashlight (com Light component)
   ```
   - Tag: "Player"
   - Layer: Configure conforme necessário

3. **Setup dos Managers:**
   - Crie GameObject vazio "GameManager" com script `GameManager`
   - Crie GameObject vazio "SceneController" com script `SceneController`
   - Crie GameObject vazio "AudioManager" com script `AudioManager`
   - Estes objetos persistem entre cenas (DontDestroyOnLoad)

4. **Setup da UI:**
   - Crie Canvas
   - Adicione `PlayerHUD` ao Canvas
   - Configure referências para os elementos UI
   - Adicione `PauseMenu` com painel de pause

5. **Setup de Inimigos:**
   - Crie GameObject com NavMeshAgent
   - Adicione script `PatrolEnemy`
   - Crie waypoints vazios para patrulha
   - Configure parâmetros

6. **Setup de Atmosfera:**
   - Adicione `AtmosphereController` à cena
   - Configure luz principal
   - Ajuste configurações de névoa

### Criando Novo Inimigo

1. Crie classe herdando de `EnemyBase`:
```csharp
public class MyEnemy : EnemyBase
{
    protected override void UpdateAI()
    {
        // Sua lógica de IA
    }

    protected override void OnAttack()
    {
        // Lógica de ataque
    }
}
```

2. Configure no Inspector
3. Certifique-se de ter NavMeshAgent

### Criando Objeto Interagível

```csharp
public class MyInteractable : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt()
    {
        return "Press E to interact";
    }

    public bool CanInteract()
    {
        return true; // Sua lógica
    }

    public void Interact(GameObject player)
    {
        // Faça algo
    }
}
```

### Usando ScriptableObjects

1. **Criar Enemy Data:**
   - Right-click em Project > Create > Horror Game > Enemy Data
   - Configure stats
   - Referencie no script do inimigo

2. **Criar Game Settings:**
   - Right-click > Create > Horror Game > Game Settings
   - Configure valores padrão
   - Carregue no GameManager

---

## 💡 Dicas e Melhores Práticas

### Performance

1. **NavMesh:**
   - Bake NavMesh apenas em superfícies necessárias
   - Use NavMesh Obstacles para objetos dinâmicos
   - Configure Agent Radius apropriadamente

2. **Audio:**
   - Coloque arquivos de áudio em `Resources/Audio/` para loading dinâmico
   - Use AudioSource pooling (já implementado no AudioManager)
   - Limite 3D Audio Distance para sons que não precisam ser ouvidos longe

3. **Enemy Spawning:**
   - Use `respectMaxEnemies` para limitar inimigos ativos
   - Configure `minPlayerDistance` para evitar spawn perto do player
   - Use waves para controle de dificuldade

### Design de Terror

1. **Iluminação:**
   - Use iluminação escura (intensity 0.3-0.5)
   - Adicione luzes pontuais para áreas importantes
   - Use sombras em tempo real

2. **Som:**
   - Sons ambientes são cruciais (vento, rangidos, etc)
   - Use sons 3D para criar espacialidade
   - Música adaptativa baseada na tensão

3. **Atmosfera:**
   - Configure névoa para limitar visibilidade
   - Use partículas (poeira, fumaça)
   - Varie a intensidade da tensão

4. **Enemy Design:**
   - Inimigos devem ser ouvidos antes de serem vistos
   - Use sons de detecção para avisar o player
   - Balance velocidade vs dano

### Organização

1. **Prefabs:**
   - Crie prefabs de tudo que será reutilizado
   - Use Prefab Variants para variações

2. **Nomenclatura:**
   - Use nomes descritivos
   - Prefixe por tipo: `enemy_zombie`, `sfx_door_open`

3. **Layers:**
   - Configure layers: Player, Enemy, Interactable, Ground
   - Use para raycast filtering

### Debugging

1. **Gizmos:**
   - Todos os scripts importantes têm `OnDrawGizmosSelected`
   - Use para visualizar ranges, FOV, patrulhas

2. **Logs:**
   - Logs já implementados em pontos importantes
   - Use `Debug.Log` com prefixo: `[ClassName]`

3. **FPS Counter:**
   - Adicione `FPSCounter` para monitorar performance

---

## 🎯 Próximos Passos Sugeridos

### 🌟 Multiplayer Cooperativo + Puzzles (RECOMENDADO)

**Quer transformar isso em um jogo cooperativo com puzzles?**
Veja o guia completo: **[COOP_PUZZLE_GAME_GUIDE.md](COOP_PUZZLE_GAME_GUIDE.md)**

Inclui:
- Sistema multiplayer completo com Unity Netcode
- Framework de puzzles modular
- Puzzles cooperativos (2-4 jogadores)
- Sistema de revive
- Balanceamento solo vs co-op
- Exemplos práticos de código
- Design de terror em co-op

### Sistemas Adicionais

1. **Inventário:**
   - Sistema de items
   - UI de inventário
   - Sistema de chaves para portas

2. **Save/Load:**
   - Serialização de dados
   - Checkpoints automáticos
   - Save states

3. **Sanidade:**
   - Barra de sanidade
   - Efeitos visuais quando baixa
   - Mecânicas relacionadas

4. **Objetivos/Quests:**
   - Sistema de objetivos
   - Tracking de progresso
   - UI de quest log

5. **Cinemática:**
   - Sistema de cutscenes
   - Diálogos
   - Triggers de eventos

### Melhorias de Gameplay

1. **Esconderijos:**
   - Armários/caixas para esconder
   - Detecção de visibilidade

2. **Puzzles:**
   - Sistema de puzzles
   - Pistas e notas

3. **Múltiplos Níveis:**
   - Sistema de progressão
   - Transição entre níveis

4. **Dificuldade Adaptativa:**
   - Ajuste baseado em performance do player

### Polish

1. **Post-Processing:**
   - Aberração cromática
   - Vignette
   - Grain/noise

2. **Partículas:**
   - Poeira
   - Fumaça
   - Efeitos de morte

3. **Animações:**
   - Animar inimigos
   - Animar portas
   - Animar items coletáveis

---

## 📚 Recursos Úteis

### Unity 6 Features para Usar

- **Global Illumination:** Para iluminação mais realista
- **Post-Processing v3:** Efeitos visuais
- **Cinemachine:** Câmeras cinematográficas
- **Timeline:** Cutscenes
- **Shader Graph:** Shaders customizados
- **Visual Effect Graph:** Partículas avançadas

### Assets Recomendados

- **TextMesh Pro:** Já integrado, melhor que UI Text
- **ProBuilder:** Para prototipar geometria
- **Polybrush:** Para pintar textures e mesh details

### Patterns Usados

- **Singleton:** GameManager, AudioManager, SceneController
- **Observer Pattern:** Sistema de eventos
- **State Machine:** IA dos inimigos
- **Object Pooling:** AudioSources no AudioManager
- **ScriptableObjects:** Dados configuráveis

---

## 🐛 Troubleshooting Comum

**Player não se move:**
- Verifique se CharacterController está anexado
- Confira se não está pausado
- Verifique Input Settings (Edit > Project Settings > Input Manager)

**Inimigo não patrulha:**
- Certifique-se que há NavMesh na cena
- Verifique se waypoints estão configurados
- Confira se NavMeshAgent está habilitado

**Áudio não toca:**
- Verifique se arquivos estão em `Resources/Audio/[SFX|Music|Ambient]`
- Confirme que AudioManager existe na cena
- Verifique nome do arquivo (case-sensitive)

**UI não aparece:**
- Verifique referências no Inspector
- Confira se Canvas está configurado corretamente
- Event System deve existir na cena

---

## 📝 Notas Importantes

### Namespace
Todos os scripts usam namespace `HorrorGame.*`:
- `HorrorGame.Core`
- `HorrorGame.Player`
- `HorrorGame.Enemy`
- `HorrorGame.Environment`
- `HorrorGame.UI`
- `HorrorGame.Audio`
- `HorrorGame.Data`
- `HorrorGame.Utilities`

### Dependências
- Unity 6.0 ou superior
- TextMesh Pro (package)
- NavMesh Components (integrado)

### Tags Necessárias
- Player
- Enemy (opcional, use layers)

### Layers Sugeridos
- Player (layer 6)
- Enemy (layer 7)
- Interactable (layer 8)
- Ground (layer 9)

---

## 🤝 Contribuindo

Este é um framework base. Sinta-se livre para:
- Adicionar novos sistemas
- Modificar comportamentos existentes
- Otimizar performance
- Adicionar features

---

## 📄 Licença

Este framework foi criado como base para desenvolvimento de jogos de terror em primeira pessoa.
Use livremente em seus projetos!

---

**Desenvolvido para Unity 6**
**Framework versão 1.0**

Boa sorte com seu jogo de terror! 👻🎮
