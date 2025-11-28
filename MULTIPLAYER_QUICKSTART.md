# ⚡ Quick Start: FPS Horror Multiplayer 4-5 Jogadores

## 🎯 Objetivo

Criar jogo de terror FPS multiplayer com 4-5 jogadores usando as tecnologias mais modernas disponíveis.

---

## 📖 Guias Completos

### 🔥 **COMECE AQUI:** Guia Principal
1. **[MULTIPLAYER_FPS_HORROR_GUIDE.md](MULTIPLAYER_FPS_HORROR_GUIDE.md)** - Parte 1
   - Stack tecnológico
   - Configuração do projeto
   - New Input System
   - Sistema de Lobby
   - Relay (sem port forwarding)

2. **[MULTIPLAYER_FPS_HORROR_GUIDE_PART2.md](MULTIPLAYER_FPS_HORROR_GUIDE_PART2.md)** - Parte 2
   - Player Controller Multiplayer
   - Sistema de Inimigos
   - IA Avançada (GOAP)
   - Match System
   - Otimizações
   - Roadmap de 12 semanas

### 📚 Documentação Complementar
- **[MULTIPLAYER_ARCHITECTURE.md](MULTIPLAYER_ARCHITECTURE.md)** - Arquitetura detalhada
- **[ADVANCED_TECHNOLOGIES.md](ADVANCED_TECHNOLOGIES.md)** - IA e tecnologias avançadas
- **[COOP_ROADMAP.md](COOP_ROADMAP.md)** - Roadmap alternativo
- **[QUICK_START.md](QUICK_START.md)** - Single player base

---

## 🛠️ Stack Tecnológico

| Componente | Recomendação | Versão |
|------------|--------------|--------|
| **Unity** | 2022.3 LTS | Latest LTS |
| **Networking** | Netcode for GameObjects | 1.8.0+ |
| **Lobby** | Unity Gaming Services | Latest |
| **Input** | New Input System | 1.7.0+ |
| **IA** | GOAP ou Behavior Trees | - |
| **Arquitetura** | Event Bus | - |

---

## 🚀 Setup em 5 Minutos

### 1. Criar Projeto
```
Unity Hub > New Project
Template: 3D (URP) ou 3D Core
Unity Version: 2022.3 LTS
Nome: HorrorFPSMultiplayer
```

### 2. Instalar Packages
```
Window > Package Manager > + > Add package by name

ESSENCIAIS:
- com.unity.netcode.gameobjects
- com.unity.transport
- com.unity.inputsystem

ÚTEIS:
- com.unity.cinemachine
- com.unity.multiplayer.tools
```

### 3. Unity Gaming Services
```
1. Window > General > Services
2. Criar/Linkar projeto
3. Habilitar:
   - Authentication
   - Lobby
   - Relay
```

### 4. Configurar Input System
```
Edit > Project Settings
Active Input Handling: Input System Package (New)
```

### 5. Criar Input Actions
```
Assets > Create > Input Actions
Nome: PlayerInputActions

Actions:
- Move (Vector2)
- Look (Vector2)
- Jump (Button)
- Sprint (Button)
- Interact (Button)
```

---

## 📋 Checklist de Implementação

### Fase 1: Fundamentos (Semana 1-2)
- [ ] Projeto criado e configurado
- [ ] Packages instalados
- [ ] Unity Gaming Services configurado
- [ ] Input System funcionando
- [ ] Player local com movimento FPS

### Fase 2: Multiplayer (Semana 3-4)
- [ ] Authentication implementado
- [ ] Lobby System funcionando
- [ ] Relay configurado
- [ ] NetworkManager setup
- [ ] 2+ players conectando

### Fase 3: Gameplay (Semana 5-6)
- [ ] Player multiplayer completo
- [ ] Inimigos sincronizados
- [ ] Sistema de combate
- [ ] IA básica (Patrol/Chase/Attack)

### Fase 4: IA Avançada (Semana 7-8)
- [ ] GOAP ou Behavior Trees
- [ ] Inimigos inteligentes
- [ ] Coordenação de grupo
- [ ] Balanceamento

### Fase 5: Match System (Semana 9-10)
- [ ] MatchManager
- [ ] Sistema de objetivos
- [ ] Win/Lose conditions
- [ ] UI completa

### Fase 6: Polish (Semana 11-12)
- [ ] Otimizações de performance
- [ ] Redução de bandwidth
- [ ] Testing com 5 players
- [ ] Bugfixes

---

## 🎯 Fluxo do Jogo

```
┌─────────────────┐
│   Main Menu     │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Authentication │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Lobby Browser  │
│  - Create       │
│  - Join         │
└────────┬────────┘
         ↓
┌─────────────────┐
│   Lobby Room    │
│  - 4-5 players  │
│  - Ready system │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Game (Relay)   │
│  - Explore      │
│  - Fight        │
│  - Objectives   │
└────────┬────────┘
         ↓
┌─────────────────┐
│  Match Results  │
└────────┬────────┘
         ↓
    Return to Lobby
```

---

## 🔗 Recursos Externos

### 📺 YouTube Channels
- **[Adam Myhre (@git-amend)](https://youtube.com/@git-amend)** - IA, GOAP, Event Bus, Netcode
- **[Dapper Dino](https://youtube.com/@DapperDinoCodingTutorials)** - Netcode tutorials
- **[Code Monkey](https://youtube.com/@CodeMonkeyUnity)** - Unity patterns

### 📦 GitHub (Adam Myhre)
```bash
# IA
https://github.com/adammyhre/Unity-GOAP
https://github.com/adammyhre/Unity-Behaviour-Trees

# Arquitetura
https://github.com/adammyhre/Unity-Event-Bus
https://github.com/adammyhre/Unity-Improved-Timers

# Multiplayer
https://github.com/adammyhre/Unity-Multiplayer-Kart

# Performance
https://github.com/adammyhre/Unity-Octrees
```

### 📚 Documentação Oficial
- [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Gaming Services](https://docs.unity.com/ugs/)
- [New Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)

---

## 💡 Dicas Importantes

### ✅ Faça
- Use Unity 2022.3 LTS (mais estável)
- Implemente GOAP para IA inteligente
- Use Relay (evita port forwarding)
- Test com latência simulada
- Profile de performance regularmente

### ❌ Evite
- Atualizar NetworkVariables toda frame
- Rodar IA no cliente (apenas servidor!)
- Sincronizar objetos distantes
- Usar GetComponent em Update
- Ignorar otimizações de bandwidth

---

## 🎓 Ordem de Estudo Recomendada

1. **Leia:** MULTIPLAYER_FPS_HORROR_GUIDE.md (Parte 1 e 2)
2. **Assista:** Tutoriais do Adam Myhre sobre Netcode
3. **Implemente:** Seguindo o roadmap de 12 semanas
4. **Consulte:** MULTIPLAYER_ARCHITECTURE.md para detalhes
5. **Estude:** Repositórios do Adam Myhre (GOAP, Event Bus)

---

## 📞 Suporte

- **Issues:** GitHub Issues do projeto
- **Comunidade:** Unity Forums, Discord
- **Documentação:** Links acima

---

## 🚀 Começar Agora

**Próximo passo:** Abra **[MULTIPLAYER_FPS_HORROR_GUIDE.md](MULTIPLAYER_FPS_HORROR_GUIDE.md)** e comece pelo "Configuração Inicial do Projeto"!

Boa sorte! 🎮👻
