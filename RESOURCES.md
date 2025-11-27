# 📚 Recursos e Links - Horror Game Framework

Lista completa e organizada de recursos, links, repositórios, tutoriais e assets úteis para desenvolvimento.

## 📋 Índice

- [Repositórios do Adam Myhre](#repositórios-do-adam-myhre)
- [Tutoriais em Vídeo](#tutoriais-em-vídeo)
- [Documentação Oficial](#documentação-oficial)
- [Assets Gratuitos](#assets-gratuitos)
- [Ferramentas e Plugins](#ferramentas-e-plugins)
- [Comunidades](#comunidades)
- [Inspiração e Referências](#inspiração-e-referências)

---

## 🌟 Repositórios do Adam Myhre

### Sistemas de IA

#### Unity-GOAP (135 ⭐)
**Goal-Oriented Action Planning para IA inteligente**
- 📦 Repositório: https://github.com/adammyhre/Unity-GOAP
- 🎥 Tutorial: https://youtu.be/[buscar no canal]
- 📝 Uso: Inimigos que planejam estratégias
- ⚡ Prioridade: Alta para horror coop

#### Unity-Behaviour-Trees (75 ⭐)
**Sistema de Behavior Trees com Blackboard**
- 📦 Repositório: https://github.com/adammyhre/Unity-Behaviour-Trees
- 🎥 Tutorial: https://youtu.be/lusROFJ3_t8
- 🎥 Blackboard: https://youtu.be/HNGJ8KOqdYQ
- 📝 Uso: IA hierárquica organizada
- ⚡ Prioridade: Alta (alternativa ao GOAP)

#### Unity-Utility-AI (72 ⭐)
**Sistema de decisão baseado em scoring**
- 📦 Repositório: https://github.com/adammyhre/Unity-Utility-AI
- 🎥 Tutorial: https://youtu.be/[buscar no canal]
- 📝 Uso: Decisões contextuais
- ⚡ Prioridade: Média

#### Unity-Hierarchical-StateMachine (50 ⭐)
**State Machine hierárquica flexível**
- 📦 Repositório: https://github.com/adammyhre/Unity-Hierarchical-StateMachine
- 📝 Uso: Estados complexos aninhados
- ⚡ Prioridade: Baixa (já tem FSM básica)

---

### Arquitetura e Patterns

#### Unity-Event-Bus (213 ⭐) ⭐ ESSENCIAL
**Sistema de eventos type-safe**
- 📦 Repositório: https://github.com/adammyhre/Unity-Event-Bus
- 🎥 Tutorial: https://youtu.be/[buscar no canal]
- 📝 Uso: Comunicação desacoplada (substitui GameEvents)
- ⚡ Prioridade: CRÍTICA - implementar primeiro

#### Unity-Utils (685 ⭐)
**Coleção de extension methods úteis**
- 📦 Repositório: https://github.com/adammyhre/Unity-Utils
- 📝 Uso: Helpers gerais, qualidade de vida
- ⚡ Prioridade: Média

---

### Performance e Otimização

#### Unity-Improved-Timers (185 ⭐) ⭐ ESSENCIAL
**Sistema de timers integrado ao Player Loop**
- 📦 Repositório: https://github.com/adammyhre/Unity-Improved-Timers
- 🎥 Tutorial: https://youtu.be/[buscar no canal]
- 📝 Uso: Timers auto-gerenciados (substitui Update manual)
- ⚡ Prioridade: CRÍTICA - implementar cedo

#### Unity-Octrees (76 ⭐)
**Estrutura de dados espacial para otimização**
- 📦 Repositório: https://github.com/adammyhre/Unity-Octrees
- 🎥 Tutorial: https://youtu.be/[buscar no canal]
- 📝 Uso: Busca espacial eficiente (inimigos próximos)
- ⚡ Prioridade: Média (implementar na Fase 4)

#### Unity-Batch-Raycasting (71 ⭐)
**Raycasts em batch com Job System**
- 📦 Repositório: https://github.com/adammyhre/Unity-Batch-Raycasting
- 🎥 Tutorial: https://youtu.be/[buscar no canal]
- 📝 Uso: Line of sight para múltiplos inimigos
- ⚡ Prioridade: Média (implementar na Fase 4)

---

### Multiplayer

#### Unity-Multiplayer-Kart
**Exemplo completo de multiplayer com Netcode**
- 📦 Repositório: https://github.com/adammyhre/Unity-Multiplayer-Kart
- 🎥 Tutorial: Verificar canal
- 📝 Uso: Referência para networking server-authoritative
- ⚡ Prioridade: CRÍTICA - estudar para multiplayer
- 🔑 Features:
  - Server authoritative
  - Client prediction
  - Reconciliation
  - Netcode for GameObjects

---

### Gameplay Systems

#### Unity-Inventory-System (238 ⭐)
**Sistema de inventário com UI Toolkit**
- 📦 Repositório: https://github.com/adammyhre/Unity-Inventory-System
- 🎥 Tutorial: https://youtu.be/[buscar no canal]
- 📝 Uso: Inventário multiplayer
- ⚡ Prioridade: Baixa (implementar se necessário)

---

### Projetos Completos de Referência

#### 3D-Platformer
- 📦 Repositório: https://github.com/adammyhre/3D-Platformer
- 📝 Uso: Referência de gameplay e controles

#### Match3
- 📦 Repositório: https://github.com/adammyhre/Match3
- 📝 Uso: Exemplo de game design patterns

---

## 🎥 Tutoriais em Vídeo

### Canal do Adam Myhre: @git-amend
**YouTube**: https://www.youtube.com/@git-amend

### Playlists Essenciais

#### Game Architecture
- Event Bus Pattern
- Dependency Injection
- SOLID Principles
- Design Patterns em Unity

#### AI Systems
- GOAP Implementation
- Behavior Trees Explained
- Utility AI Deep Dive
- State Machines

#### Performance
- Unity Job System
- Burst Compiler
- ECS (Entity Component System)
- Profiling e Optimization

#### Multiplayer
- Netcode for GameObjects Tutorial
- Server Authority Best Practices
- Client Prediction
- Network Optimization

---

## 📖 Documentação Oficial

### Unity

#### Unity Manual
- **URL**: https://docs.unity3d.com/Manual/index.html
- **Uso**: Referência geral Unity

#### Unity Scripting API
- **URL**: https://docs.unity3d.com/ScriptReference/
- **Uso**: API C# completa

#### Unity Learn
- **URL**: https://learn.unity.com/
- **Cursos Recomendados**:
  - FPS Microgame
  - Horror Microgame
  - Intermediate Scripting

---

### Multiplayer

#### Netcode for GameObjects
- **URL**: https://docs-multiplayer.unity3d.com/netcode/current/about/
- **Seções Essenciais**:
  - Getting Started
  - NetworkBehaviour
  - RPC (Remote Procedure Calls)
  - NetworkVariable
  - Scene Management

#### Unity Transport
- **URL**: https://docs-multiplayer.unity3d.com/transport/current/about/
- **Uso**: Transport layer (UDP, etc)

#### Unity Relay
- **URL**: https://docs.unity.com/relay/
- **Uso**: Hosting sem port forwarding

#### Multiplayer Tools
- **URL**: https://docs-multiplayer.unity3d.com/tools/current/about/
- **Uso**: Profiling e debugging de network

---

### NavMesh e IA

#### NavMesh Building Components
- **URL**: https://docs.unity3d.com/Manual/NavMesh-BuildingComponents.html
- **Uso**: Setup de pathfinding

#### NavMeshAgent
- **URL**: https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.html
- **Uso**: Movimentação de inimigos

---

## 🎨 Assets Gratuitos

### Modelos 3D

#### Mixamo
- **URL**: https://www.mixamo.com/
- **Conteúdo**: Personagens humanoides animados
- **Gratuito**: ✅ Sim
- **Uso**: Inimigos, NPCs

#### Sketchfab
- **URL**: https://sketchfab.com/
- **Conteúdo**: Modelos 3D variados
- **Filtro**: Free downloadable
- **Uso**: Props, ambientes

#### Poly Haven
- **URL**: https://polyhaven.com/
- **Conteúdo**: HDRIs, texturas, modelos
- **CC0**: ✅ Public domain
- **Uso**: Iluminação, materiais

#### Quaternius
- **URL**: https://quaternius.com/
- **Conteúdo**: Low-poly assets
- **Gratuito**: ✅ Sim
- **Uso**: Prototyping

---

### Áudio

#### Freesound
- **URL**: https://freesound.org/
- **Conteúdo**: Sound effects
- **Licença**: Variada (checar)
- **Uso**: SFX gerais

#### YouTube Audio Library
- **URL**: https://www.youtube.com/audiolibrary
- **Conteúdo**: Música e SFX
- **Gratuito**: ✅ Sim
- **Uso**: Background music

#### BBC Sound Effects
- **URL**: https://sound-effects.bbcrewind.co.uk/
- **Conteúdo**: 16,000+ sound effects
- **Uso**: Atmosfera, ambience

#### Sonniss GameAudioGDC
- **URL**: https://sonniss.com/gameaudiogdc
- **Conteúdo**: Bundle anual gratuito
- **Tamanho**: 30GB+
- **Uso**: SFX profissionais

---

### Texturas e Materiais

#### Texture Haven (Poly Haven)
- **URL**: https://polyhaven.com/textures
- **Qualidade**: 4K-8K
- **CC0**: ✅ Public domain

#### CC0 Textures
- **URL**: https://cc0textures.com/
- **Conteúdo**: PBR textures
- **Gratuito**: ✅ Sim

#### Substance Share
- **URL**: https://substance3d.adobe.com/community-assets
- **Conteúdo**: Materiais Substance
- **Requer**: Substance Player (gratuito)

---

### Fonts

#### Google Fonts
- **URL**: https://fonts.google.com/
- **Uso**: UI text

#### DaFont
- **URL**: https://www.dafont.com/
- **Uso**: Fontes temáticas

---

## 🛠️ Ferramentas e Plugins

### Unity Asset Store - Essenciais Gratuitos

#### ProBuilder
- **URL**: https://unity.com/features/probuilder
- **Uso**: Modelagem in-engine
- **Preço**: Gratuito (integrado Unity 6)

#### ProGrids
- **Uso**: Grid snapping
- **Preço**: Gratuito

#### Cinemachine
- **URL**: https://unity.com/unity/features/editor/art-and-design/cinemachine
- **Uso**: Câmeras cinematográficas
- **Preço**: Gratuito (integrado)

#### Timeline
- **Uso**: Cutscenes e sequências
- **Preço**: Gratuito (integrado)

#### Post Processing Stack v3
- **Uso**: Efeitos visuais
- **Preço**: Gratuito (integrado Unity 6)

---

### Editores Externos

#### Visual Studio Code
- **URL**: https://code.visualstudio.com/
- **Uso**: Editor de código
- **Preço**: Gratuito
- **Extensões Essenciais**:
  - C# (Microsoft)
  - Unity Code Snippets
  - Unity Tools

#### Visual Studio Community
- **URL**: https://visualstudio.microsoft.com/
- **Uso**: IDE completa
- **Preço**: Gratuito

#### Rider (JetBrains)
- **URL**: https://www.jetbrains.com/rider/
- **Uso**: IDE especializada Unity
- **Preço**: Pago (trial 30 dias)

---

### Áudio

#### Audacity
- **URL**: https://www.audacityteam.org/
- **Uso**: Editor de áudio
- **Preço**: Gratuito

#### FMOD
- **URL**: https://www.fmod.com/
- **Uso**: Sistema de áudio avançado
- **Preço**: Gratuito (indie)

---

### Arte e Modelagem

#### Blender
- **URL**: https://www.blender.org/
- **Uso**: Modelagem 3D, animação
- **Preço**: Gratuito

#### GIMP
- **URL**: https://www.gimp.org/
- **Uso**: Edição de imagens
- **Preço**: Gratuito

#### Krita
- **URL**: https://krita.org/
- **Uso**: Pintura digital
- **Preço**: Gratuito

---

### Versionamento

#### GitHub Desktop
- **URL**: https://desktop.github.com/
- **Uso**: Interface Git visual
- **Preço**: Gratuito

#### GitKraken
- **URL**: https://www.gitkraken.com/
- **Uso**: Cliente Git avançado
- **Preço**: Gratuito (básico)

#### Git LFS
- **URL**: https://git-lfs.github.com/
- **Uso**: Versionamento de arquivos grandes
- **Essencial**: Para assets Unity

---

## 👥 Comunidades

### Forums e Discussão

#### Unity Forum
- **URL**: https://forum.unity.com/
- **Uso**: Perguntas e discussões oficiais

#### Reddit - r/Unity3D
- **URL**: https://www.reddit.com/r/Unity3D/
- **Uso**: Comunidade ativa, showcases

#### Reddit - r/gamedev
- **URL**: https://www.reddit.com/r/gamedev/
- **Uso**: Game development geral

#### Stack Overflow
- **URL**: https://stackoverflow.com/questions/tagged/unity3d
- **Uso**: Perguntas técnicas específicas

---

### Discord Servers

#### Unity
- **Invite**: https://discord.gg/unity
- **Membros**: 100K+

#### Brackeys
- **Invite**: https://discord.gg/brackeys
- **Membros**: 500K+

#### Game Dev League
- **Invite**: https://discord.gg/gamedev
- **Membros**: 50K+

---

### Twitter/X

#### Contas Úteis para Seguir
- @unity3d - Unity oficial
- @adammyhre - Adam Myhre
- @brackeys - Brackeys tutorials
- @unity3dtips - Dicas Unity
- @GameDevField - Game dev resources

---

## 🎮 Inspiração e Referências

### Jogos de Terror Cooperativo

#### Phasmophobia
- **Estudo**: Sistema de investigação cooperativa
- **Mecânicas**: Comunicação, equipamentos especializados

#### Left 4 Dead 2
- **Estudo**: AI Director, coordenação de horda
- **Mecânicas**: Trabalho em equipe forçado

#### Dead Space (series)
- **Estudo**: Atmosfera, tensão, audio design
- **Mecânicas**: Resource management

#### Lethal Company
- **Estudo**: Horror procedural, comunicação
- **Mecânicas**: Risk/reward, teamwork

#### The Forest
- **Estudo**: Survival + horror cooperativo
- **Mecânicas**: Building, exploration

---

### Canais YouTube Educacionais

#### Brackeys
- **URL**: https://www.youtube.com/@Brackeys
- **Foco**: Unity tutorials gerais
- **Status**: Arquivo (não posta mais, mas conteúdo excelente)

#### Code Monkey
- **URL**: https://www.youtube.com/@CodeMonkeyUnity
- **Foco**: Gameplay programming, patterns

#### Sebastian Lague
- **URL**: https://www.youtube.com/@SebastianLague
- **Foco**: Algoritmos, procedural generation

#### Jason Weimann
- **URL**: https://www.youtube.com/@Unity3dCollege
- **Foco**: Architecture, clean code

#### Infallible Code
- **URL**: https://www.youtube.com/@InfallibleCode
- **Foco**: Tutoriais Unity práticos

---

## 📦 Packages Unity Recomendados

### Via Package Manager

#### Instalados por Padrão (Unity 6)
- ✅ Input System
- ✅ Cinemachine
- ✅ Timeline
- ✅ Post Processing
- ✅ TextMesh Pro

#### Para Instalar

**Multiplayer**
```
com.unity.netcode.gameobjects
com.unity.transport
com.unity.services.relay
com.unity.multiplayer.tools
```

**Performance**
```
com.unity.jobs
com.unity.burst
com.unity.collections
```

**AI**
```
com.unity.ai.navigation (NavMesh)
```

---

## 🔗 Links Rápidos Este Projeto

### Documentação Base
- [README.md](README.md)
- [QUICK_START.md](QUICK_START.md)
- [STRUCTURE.md](STRUCTURE.md)
- [API_REFERENCE.md](API_REFERENCE.md)

### Documentação Avançada
- [ADVANCED_TECHNOLOGIES.md](ADVANCED_TECHNOLOGIES.md)
- [MULTIPLAYER_ARCHITECTURE.md](MULTIPLAYER_ARCHITECTURE.md)
- [ADVANCED_AI_SYSTEMS.md](ADVANCED_AI_SYSTEMS.md)
- [COOP_ROADMAP.md](COOP_ROADMAP.md)
- [INTEGRATION_GUIDE.md](INTEGRATION_GUIDE.md)

### Quick Reference
- [QUICKREF.md](QUICKREF.md) - Cheat sheet
- [GLOSSARY.md](GLOSSARY.md) - Glossário (a criar)
- [BENCHMARKS.md](BENCHMARKS.md) - Performance targets (a criar)

---

## 📥 Como Clonar Repositórios

### Adam Myhre - Todos de uma vez

```bash
# Criar pasta para os repos
mkdir ~/UnityResources
cd ~/UnityResources

# Clonar os essenciais
git clone https://github.com/adammyhre/Unity-Event-Bus.git
git clone https://github.com/adammyhre/Unity-Improved-Timers.git
git clone https://github.com/adammyhre/Unity-GOAP.git
git clone https://github.com/adammyhre/Unity-Behaviour-Trees.git
git clone https://github.com/adammyhre/Unity-Utility-AI.git
git clone https://github.com/adammyhre/Unity-Multiplayer-Kart.git
git clone https://github.com/adammyhre/Unity-Octrees.git
git clone https://github.com/adammyhre/Unity-Batch-Raycasting.git
git clone https://github.com/adammyhre/Unity-Utils.git
```

### Copiar para seu Projeto

```bash
# Exemplo: Event Bus
cp -r ~/UnityResources/Unity-Event-Bus/Assets/EventBus ~/GameDevTerror/Assets/Scripts/Core/

# Exemplo: Timers
cp -r ~/UnityResources/Unity-Improved-Timers/Assets/Timers ~/GameDevTerror/Assets/Scripts/Utilities/
```

---

## 🎓 Cursos Recomendados (Pagos)

### Udemy

#### Complete C# Unity Game Developer 2D/3D
- **Instrutor**: GameDev.tv Team
- **Preço**: ~$15 (em promoção)
- **Nível**: Iniciante a Intermediário

#### Unity Multiplayer Course
- **Instrutor**: Vários
- **Foco**: Netcode, Photon, Mirror

---

### Unity Learn Premium
- **URL**: https://learn.unity.com/premium
- **Preço**: $15/mês
- **Conteúdo**: Cursos avançados, projetos completos

---

## 💡 Dicas de Pesquisa

### Google Search Tips

```
site:docs.unity3d.com NetworkVariable
site:github.com/adammyhre GOAP
site:stackoverflow.com unity netcode
```

### YouTube Search

```
"netcode for gameobjects" tutorial
"behavior tree" unity
"horror game" unity tutorial
```

---

## 📱 Apps Móveis Úteis

### Unity Connect
- iOS/Android
- Networking com devs

### GitHub Mobile
- iOS/Android
- Gerenciar repositórios

---

## 🔖 Bookmarks Recomendados

Organize seus favoritos assim:

```
🎮 Unity Dev/
├── 📚 Docs/
│   ├── Unity Manual
│   ├── Scripting API
│   └── Netcode Docs
├── 🌟 Adam Myhre/
│   ├── GitHub Profile
│   └── YouTube Channel
├── 📦 Resources/
│   ├── Freesound
│   ├── Mixamo
│   └── Poly Haven
└── 👥 Communities/
    ├── Unity Forum
    └── Reddit r/Unity3D
```

---

## 🎯 Ordem de Prioridade para Estudar

### Fase 1: Fundamentos
1. ✅ Unity Manual básico
2. ✅ C# básico
3. ✅ Git basics

### Fase 2: Arquitetura
1. ✅ Event Bus (Adam Myhre)
2. ✅ Design Patterns
3. ✅ SOLID principles

### Fase 3: Multiplayer
1. ✅ Netcode for GameObjects docs
2. ✅ Unity-Multiplayer-Kart (Adam)
3. ✅ Network patterns

### Fase 4: IA Avançada
1. ✅ Escolher: GOAP ou BT
2. ✅ Estudar tutoriais do Adam
3. ✅ Implementar em projeto

### Fase 5: Otimização
1. ✅ Unity Profiler
2. ✅ Job System
3. ✅ Octrees e Batch Raycasting

---

## 📧 Newsletters

### Unity Blog
- **URL**: https://blog.unity.com/
- **Frequência**: Semanal

### GameDev.net
- **URL**: https://www.gamedev.net/
- **Conteúdo**: Artigos, tutoriais

---

## 🆘 Suporte

### Unity
- **Forum**: https://forum.unity.com/
- **Answers**: https://answers.unity.com/
- **Docs**: https://docs.unity3d.com/

### Stack Overflow
- **Tag**: [unity3d]
- **URL**: https://stackoverflow.com/questions/tagged/unity3d

---

## ✅ Checklist de Recursos Instalados

Marque conforme instalar:

### Essenciais
- [ ] Visual Studio / VS Code / Rider
- [ ] Git
- [ ] Unity Hub
- [ ] Unity 6

### Packages Unity
- [ ] Netcode for GameObjects
- [ ] Unity Transport
- [ ] Multiplayer Tools

### Repositórios Adam Myhre
- [ ] Event Bus
- [ ] Improved Timers
- [ ] GOAP ou Behavior Trees
- [ ] Multiplayer Kart (referência)

### Assets
- [ ] Sound effects baixados
- [ ] Texturas PBR
- [ ] Modelos 3D temporários

---

**🔖 Dica**: Adicione esta página aos favoritos para acesso rápido a todos os recursos!

**📌 Manutenção**: Atualize esta lista conforme descobrir novos recursos úteis!
