# 🚀 Guia de Início Rápido - Horror Game Framework

Este guia te leva do zero até ter um protótipo funcional em ~30 minutos.

## 📋 Pré-requisitos

- Unity 6.0 ou superior instalado
- Conhecimento básico de Unity
- TextMesh Pro importado (Window > TextMeshPro > Import TMP Essential Resources)

---

## 🎯 Passo 1: Setup Inicial da Cena (5 min)

### 1.1 Criar Nova Cena

1. File > New Scene
2. Salve como `GameLevel01` em `Assets/Scenes/`
3. Delete a Main Camera (vamos criar uma nova com o player)

### 1.2 Criar Chão

1. GameObject > 3D Object > Plane
2. Scale: (10, 1, 10)
3. Position: (0, 0, 0)
4. Tag: Ground

### 1.3 Adicionar Iluminação Básica

1. GameObject > Light > Directional Light
2. Rotation: (50, -30, 0)
3. Intensity: 0.5 (baixa para criar atmosfera)
4. Color: Levemente azulado (#AACCFF)

### 1.4 Configurar Névoa

1. Window > Rendering > Lighting
2. Aba Environment:
   - ✅ Fog
   - Mode: Exponential Squared
   - Density: 0.015
   - Color: #555555 (cinza escuro)

---

## 🎮 Passo 2: Setup dos Managers (5 min)

### 2.1 GameManager

1. GameObject > Create Empty
2. Rename: "GameManager"
3. Add Component > `GameManager`
4. No Inspector:
   - Current State: Playing
   - Start Paused: ❌

### 2.2 SceneController

1. GameObject > Create Empty
2. Rename: "SceneController"
3. Add Component > `SceneController`
4. No Inspector:
   - Main Menu Scene Name: "MainMenu"
   - Game Scene Name: "GameLevel01"
   - Use Async Loading: ✅

### 2.3 AudioManager

1. GameObject > Create Empty
2. Rename: "AudioManager"
3. Add Component > `AudioManager`
4. O script criará AudioSources automaticamente

### 2.4 AtmosphereController

1. GameObject > Create Empty
2. Rename: "AtmosphereController"
3. Add Component > `AtmosphereController`
4. Arraste a Directional Light para "Main Light"
5. Configure:
   - Use Fog: ✅
   - Normal Light Intensity: 0.5
   - Tense Light Intensity: 0.2
   - React To Enemy Proximity: ✅

---

## 👤 Passo 3: Criar o Player (10 min)

### 3.1 Criar GameObject

1. GameObject > Create Empty
2. Rename: "Player"
3. Tag: "Player"
4. Position: (0, 1, 0)

### 3.2 Adicionar CharacterController

1. Add Component > Character Controller
2. Configure:
   - Height: 2
   - Radius: 0.5
   - Center: (0, 0, 0)

### 3.3 Adicionar Scripts de Movimento

1. Add Component > `FirstPersonController`
   - Walk Speed: 3
   - Run Speed: 6
   - Crouch Speed: 1.5
   - Max Stamina: 100
   - Enable Head Bob: ✅

2. Add Component > `MouseLook`
   - Mouse Sensitivity: 100
   - Smooth Rotation: ✅

### 3.4 Adicionar Sistema de Vida

1. Add Component > `PlayerHealth`
   - Max Health: 100
   - Can Regenerate: ❌

### 3.5 Adicionar Interação

1. Add Component > `PlayerInteraction`
   - Interaction Range: 3
   - Layer Mask: Everything (por enquanto)

### 3.6 Criar Câmera

1. Com Player selecionado: GameObject > Camera
2. Rename child para "MainCamera"
3. Tag: MainCamera
4. Position: (0, 0.6, 0) - altura dos olhos
5. Arraste MainCamera para "Camera Transform" no MouseLook

### 3.7 Adicionar Lanterna

1. Com MainCamera selecionado: GameObject > Light > Spot Light
2. Rename: "Flashlight"
3. Configure a Light:
   - Type: Spot
   - Range: 20
   - Spot Angle: 60
   - Intensity: 2
   - Color: Branco levemente amarelado
   - Shadow Type: Soft Shadows

4. Add Component > `Flashlight` ao MainCamera
5. Arraste o Spot Light para "Flashlight Light"
6. Configure Flashlight:
   - Toggle Key: F
   - Use Battery: ✅
   - Max Battery Life: 100
   - Battery Drain Rate: 5

---

## 🎨 Passo 4: Setup da UI (5 min)

### 4.1 Criar Canvas

1. GameObject > UI > Canvas
2. Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Match: 0.5

### 4.2 Criar PlayerHUD

1. Com Canvas selecionado: GameObject > Create Empty
2. Rename: "PlayerHUD"
3. Add Component > `PlayerHUD`

### 4.3 Criar Barra de Vida

1. Com PlayerHUD selecionado: GameObject > UI > Image
2. Rename: "HealthBarBackground"
3. Position: (-800, -500, 0)
4. Width: 300, Height: 30
5. Color: Preto semi-transparente

6. Crie child Image: "HealthBarFill"
7. Image Type: Filled
8. Fill Method: Horizontal
9. Color: Vermelho
10. Anchor/Pivot: Esquerda

11. Arraste HealthBarFill para "Health Bar" no PlayerHUD

### 4.4 Criar Texto de Vida

1. Com HealthBarBackground: GameObject > UI > Text - TextMeshPro
2. Rename: "HealthText"
3. Text: "100 / 100"
4. Font Size: 24
5. Alignment: Center
6. Color: Branco
7. Arraste para "Health Text" no PlayerHUD

### 4.5 Criar Barra de Stamina

1. Repita processo da barra de vida
2. Position: (-800, -550, 0)
3. Color: Verde
4. Arraste para "Stamina Bar" no PlayerHUD

### 4.6 Criar Interaction Prompt

1. Com Canvas: GameObject > UI > Text - TextMeshPro
2. Rename: "InteractionPrompt"
3. Position: (0, 0, 0) - centro da tela
4. Text: "Press E to interact"
5. Font Size: 28
6. Alignment: Center
7. Color: Branco
8. Add Component > Canvas Group
9. Arraste InteractionPrompt para PlayerHUD

### 4.7 Criar PauseMenu

1. Com Canvas: GameObject > UI > Panel
2. Rename: "PauseMenu"
3. Color: Preto semi-transparente (alpha 0.8)
4. Add Component > `PauseMenu`
5. Desative o GameObject (será ativado ao pausar)

6. Dentro do PauseMenu, crie botões:
   - Resume Button
   - Settings Button
   - Main Menu Button
   - Quit Button

7. Arraste botões para os slots no PauseMenu script
8. Arraste PauseMenu panel para "Pause Menu Panel"

---

## 🤖 Passo 5: Criar um Inimigo Básico (5 min)

### 5.1 Criar GameObject

1. GameObject > 3D Object > Capsule
2. Rename: "Enemy_Patrol"
3. Position: (5, 1, 5)
4. Tag: "Enemy" (criar se não existir)

### 5.2 Adicionar NavMeshAgent

1. Add Component > Nav Mesh Agent
2. Configure:
   - Speed: 3.5
   - Angular Speed: 120
   - Acceleration: 8
   - Stopping Distance: 2
   - Auto Braking: ✅

### 5.3 Adicionar Script

1. Add Component > `PatrolEnemy`
2. Configure:
   - Max Health: 100
   - Damage: 10
   - Attack Range: 2
   - Detection Range: 10
   - Move Speed: 3.5
   - Chase Speed: 5

### 5.4 Criar Waypoints para Patrulha

1. GameObject > Create Empty (3x)
2. Rename: "Waypoint1", "Waypoint2", "Waypoint3"
3. Posicione em triângulo:
   - Waypoint1: (5, 0, 5)
   - Waypoint2: (10, 0, 5)
   - Waypoint3: (7.5, 0, 10)

4. Arraste waypoints para array "Patrol Points" no PatrolEnemy

### 5.5 Bake NavMesh

**IMPORTANTE!**

1. Window > AI > Navigation
2. Aba Bake:
   - Agent Radius: 0.5
   - Agent Height: 2
   - Max Slope: 45
   - Step Height: 0.4
3. Clique "Bake"
4. Você deve ver área azul no chão

Se não funcionar:
- Selecione o Plane (chão)
- Inspector > Static: ✅ Navigation Static
- Tente Bake novamente

---

## 🎵 Passo 6: Adicionar Áudio Básico (OPCIONAL)

### 6.1 Criar Estrutura de Pastas

```
Assets/
└── Resources/
    └── Audio/
        ├── Music/
        ├── SFX/
        └── Ambient/
```

### 6.2 Adicionar Clips

1. Importe seus arquivos de áudio
2. Coloque em pastas apropriadas
3. O AudioManager carregará automaticamente de Resources/Audio/

### 6.3 Teste

```csharp
// Em qualquer script:
GameEvents.OnPlaySound2D?.Invoke("nome_do_arquivo_sem_extensao");
```

---

## ✅ Passo 7: Teste Inicial

### 7.1 Checklist Antes de Testar

- [ ] GameManager, SceneController, AudioManager na cena
- [ ] Player com todos componentes
- [ ] Camera é filha do Player
- [ ] Player tem Tag "Player"
- [ ] Canvas com UI configurada
- [ ] NavMesh está baked (área azul visível)
- [ ] Inimigo tem waypoints configurados

### 7.2 Play!

Pressione Play e teste:

**Movimento:**
- [ ] WASD para mover
- [ ] Shift para correr (stamina diminui)
- [ ] Ctrl para agachar
- [ ] Space para pular
- [ ] Mouse para olhar

**Sistemas:**
- [ ] F para ligar/desligar lanterna
- [ ] ESC para pausar
- [ ] HUD mostra vida e stamina
- [ ] Inimigo patrulha entre waypoints
- [ ] Inimigo te detecta quando próximo
- [ ] Inimigo te persegue
- [ ] Inimigo ataca quando alcança

### 7.3 Problemas Comuns

**Player não se move:**
- Verifique se CharacterController está anexado
- Confira se Input Manager tem os eixos configurados

**Inimigo não se move:**
- NavMesh foi baked?
- NavMeshAgent está enabled?
- Waypoints foram configurados?

**UI não aparece:**
- EventSystem existe na cena?
- Canvas está no modo Screen Space - Overlay?
- Referências estão conectadas no Inspector?

**Lanterna não funciona:**
- Light component está anexado?
- Referência está conectada no Flashlight script?

---

## 🎨 Passo 8: Adicionar Elementos de Terror (OPCIONAL)

### 8.1 Porta Simples

1. GameObject > 3D Object > Cube
2. Scale: (2, 3, 0.2) - formato de porta
3. Add Component > Box Collider
4. Add Component > `Door`
5. Configure sons se tiver

### 8.2 Item de Vida

1. GameObject > 3D Object > Cube
2. Scale: (0.5, 0.5, 0.5)
3. Add Component > `Collectible`
4. Type: HealthKit
5. Value: 50
6. Auto Collect On Trigger: ✅

### 8.3 Checkpoint

1. GameObject > Create Empty
2. Add Component > Sphere Collider
3. Is Trigger: ✅
4. Radius: 2
5. Add Component > `Checkpoint`
6. Configure:
   - Activate On Trigger: ✅
   - Heal Player: ✅
   - Heal Amount: 50

---

## 🚀 Próximos Passos

### Agora você tem um protótipo funcional!

**Para expandir:**

1. **Level Design:**
   - Adicione paredes, corredores
   - Crie áreas de luz e sombra
   - Adicione props (caixas, móveis)

2. **Mais Inimigos:**
   - Duplique Enemy_Patrol
   - Crie variações (mais rápido, mais forte, etc)
   - Use EnemySpawner para waves

3. **Atmosfera:**
   - Adicione mais luzes pontuais
   - Configure névoa mais densa
   - Adicione partículas (poeira, fumaça)

4. **Sons:**
   - Adicione música de fundo
   - Sons ambientes
   - Sons de passos
   - Sons de inimigos

5. **Mecânicas:**
   - Sistema de inventário
   - Puzzles simples
   - Objetivos/quests
   - Save/Load

---

## 📚 Recursos Úteis

**Documentação:**
- README.md - Guia completo
- EXAMPLES.md - Exemplos de código
- STRUCTURE.md - Arquitetura detalhada
- TIPS_AND_IMPROVEMENTS.md - Melhorias avançadas

**Assets Gratuitos:**
- **Unity Asset Store:**
  - Free Horror Props
  - Free Sound Effects
  - Free Character Controller

- **Websites:**
  - freesound.org (sons)
  - opengameart.org (sprites/models)
  - mixamo.com (animações)

**Tutoriais:**
- Unity Learn (learn.unity.com)
- Brackeys (YouTube)
- Sebastian Lague (YouTube)

---

## 🐛 Troubleshooting

### Performance Ruim

1. Reduza sombras (Edit > Project Settings > Quality)
2. Diminua Draw Distance da câmera
3. Use menos luzes em tempo real
4. Bake lighting quando possível

### Erros de Compilação

1. Verifique namespaces (using HorrorGame.Core, etc)
2. Confira se todos os scripts estão na pasta correta
3. Reimporte scripts (Right-click > Reimport)

### Inimigo Atravessa Paredes

1. NavMesh foi baked corretamente?
2. Paredes têm colliders?
3. Paredes estão marcadas como Navigation Static?
4. Rebake NavMesh após adicionar geometria

---

## 🎯 Checklist de Conclusão

Você completou o setup se:

- [ ] Player se move suavemente
- [ ] Câmera responde ao mouse
- [ ] Lanterna liga/desliga
- [ ] UI mostra vida e stamina
- [ ] Inimigo patrulha
- [ ] Inimigo detecta e persegue player
- [ ] Pause menu funciona
- [ ] Atmosfera está escura e tensa

---

## 🎊 Parabéns!

Você agora tem um protótipo funcional de jogo de terror FPS!

**Próximo passo:** Comece a adicionar seu próprio conteúdo - levels, inimigos, história, puzzles, etc.

**Lembre-se:**
- Itere frequentemente
- Teste com outras pessoas
- Não tenha medo de experimentar
- Divirta-se! 👻🎮

---

## 💡 Dica Final

**Comece pequeno!**

Não tente criar tudo de uma vez. Foque em:
1. Um level pequeno mas polido
2. Um tipo de inimigo interessante
3. Uma mecânica única

É melhor ter 5 minutos de gameplay polido do que 1 hora de conteúdo medíocre.

**Boa sorte com seu jogo!** 🚀
