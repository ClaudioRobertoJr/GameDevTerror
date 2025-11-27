# 🗺️ Roadmap de Implementação - Horror Game Cooperativo

Plano detalhado passo a passo para transformar o framework single-player em um jogo de terror cooperativo completo com IA avançada.

## 📚 Índice

- [Visão Geral do Roadmap](#visão-geral-do-roadmap)
- [Fase 1: Fundamentos Multiplayer](#fase-1-fundamentos-multiplayer)
- [Fase 2: Sistemas Essenciais](#fase-2-sistemas-essenciais)
- [Fase 3: IA Avançada](#fase-3-ia-avançada)
- [Fase 4: Otimização e Polish](#fase-4-otimização-e-polish)
- [Fase 5: Conteúdo e Balanceamento](#fase-5-conteúdo-e-balanceamento)
- [Métricas de Sucesso](#métricas-de-sucesso)

---

## 🎯 Visão Geral do Roadmap

### Objetivo Final
Criar um jogo de terror cooperativo 2-4 jogadores com:
- ✅ Networking sólido e responsivo
- ✅ Inimigos inteligentes com IA avançada (GOAP ou Behavior Trees)
- ✅ Atmosfera sincronizada e tensão compartilhada
- ✅ Mecânicas cooperativas emergentes
- ✅ Performance otimizada

### Timeline Estimado
**Total: 8-12 semanas (tempo parcial) ou 4-6 semanas (tempo integral)**

```
Semanas 1-2: Fundamentos Multiplayer
Semanas 3-4: Sistemas Essenciais
Semanas 5-6: IA Avançada
Semanas 7-8: Otimização e Polish
Semanas 9-12: Conteúdo e Balanceamento (opcional)
```

### Pré-requisitos
- ✅ Framework base já implementado
- ✅ Unity 6 instalado
- ✅ Conhecimento básico de C#
- ✅ Familiaridade com NavMesh

---

## 📅 Fase 1: Fundamentos Multiplayer (Semanas 1-2)

### Objetivo
Configurar infraestrutura básica de networking e permitir múltiplos jogadores na mesma cena.

### Semana 1: Setup e Player Networking

#### Dia 1-2: Instalação e Configuração
**Tempo estimado: 4-6 horas**

- [ ] **Instalar Netcode for GameObjects**
  ```
  Window > Package Manager > Add from Git URL
  https://github.com/Unity-Technologies/com.unity.netcode.gameobjects.git
  ```

- [ ] **Instalar pacotes complementares**
  - Unity Transport (`com.unity.transport`)
  - Multiplayer Tools (`com.unity.multiplayer.tools`)
  - Unity Relay (`com.unity.services.relay`) - para hosting fácil

- [ ] **Criar NetworkManager GameObject**
  - Adicionar componente `NetworkManager`
  - Configurar `UnityTransport`
  - Criar `NetworkPrefabsList` ScriptableObject

- [ ] **Configurar Build Settings**
  - Garantir que todas as cenas estão em Build Settings
  - Configurar Development Build para debugging

**Checkpoint:** NetworkManager configurado e aparecendo no Inspector

---

#### Dia 3-4: Player Prefab Network
**Tempo estimado: 6-8 horas**

- [ ] **Converter Player para NetworkObject**
  ```csharp
  // Adicionar ao Player prefab:
  - NetworkObject component
  - NetworkTransform component
  ```

- [ ] **Criar NetworkPlayerController.cs**
  - Herdar de `NetworkBehaviour`
  - Implementar `OnNetworkSpawn()`
  - Distinguir player local vs remoto
  - Sincronizar NetworkVariable<Vector3> para posição
  - Sincronizar NetworkVariable<int> para saúde

- [ ] **Desabilitar controles para remote players**
  ```csharp
  if (IsOwner)
  {
      // Habilitar input, camera, audio listener
  }
  else
  {
      // Desabilitar input, camera, audio listener
  }
  ```

- [ ] **Testar com Build & Run**
  - Build do projeto
  - Abrir duas instâncias
  - Um faz Host, outro faz Join
  - Verificar se ambos se veem

**Checkpoint:** Dois jogadores andando na mesma cena

**Código de Referência:**
```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    private CharacterController controller;
    private FirstPersonController fpController;

    public NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<int> netHealth = new NetworkVariable<int>(100);

    public override void OnNetworkSpawn()
    {
        controller = GetComponent<CharacterController>();
        fpController = GetComponent<FirstPersonController>();

        if (IsOwner)
        {
            EnableLocalPlayer();
        }
        else
        {
            DisableRemotePlayer();
        }

        netHealth.OnValueChanged += OnHealthChanged;
    }

    void EnableLocalPlayer()
    {
        fpController.enabled = true;
        GetComponentInChildren<Camera>().enabled = true;
        GetComponentInChildren<AudioListener>().enabled = true;
    }

    void DisableRemotePlayer()
    {
        fpController.enabled = false;
        GetComponentInChildren<Camera>().enabled = false;
        GetComponentInChildren<AudioListener>().enabled = false;
    }

    void Update()
    {
        if (IsOwner)
        {
            UpdatePositionServerRpc(transform.position);
        }
        else
        {
            // Interpolate remote player
            transform.position = Vector3.Lerp(
                transform.position,
                netPosition.Value,
                Time.deltaTime * 10f
            );
        }
    }

    [ServerRpc]
    void UpdatePositionServerRpc(Vector3 pos)
    {
        netPosition.Value = pos;
    }

    void OnHealthChanged(int oldVal, int newVal)
    {
        if (IsOwner)
        {
            GameEvents.OnPlayerDamaged?.Invoke(oldVal - newVal, newVal);
        }
    }
}
```

---

#### Dia 5-6: Menu de Conexão
**Tempo estimado: 4-6 horas**

- [ ] **Criar NetworkUI.cs**
  - Botões: Host, Join, Quit
  - Input field para IP (opcional)

- [ ] **Implementar callbacks**
  ```csharp
  public void OnHostButton()
  {
      NetworkManager.Singleton.StartHost();
  }

  public void OnJoinButton()
  {
      NetworkManager.Singleton.StartClient();
  }
  ```

- [ ] **Criar cena de Menu**
  - Cena separada com NetworkUI
  - Transição para cena de jogo após conexão

- [ ] **Sistema de spawn de players**
  - Criar spawn points
  - Spawnar players em posições diferentes

**Checkpoint:** Menu funcional que inicia host/client

---

#### Dia 7: Testing e Bugfixes
**Tempo estimado: 4-6 horas**

- [ ] **Testes básicos**
  - 2 players conectados
  - 4 players conectados
  - Disconnect e reconnect
  - Player movement sincronizado

- [ ] **Bugfixes comuns**
  - Dual AudioListener warnings
  - Camera conflicts
  - Input não funcionando
  - Posição não sincronizando

- [ ] **Documentar problemas**

**Checkpoint:** 2-4 players jogando estáveis

---

### Semana 2: Enemy e Interações Básicas

#### Dia 1-2: Network Enemy Básico
**Tempo estimado: 6-8 horas**

- [ ] **Converter EnemyBase para NetworkEnemy**
  - Adicionar `NetworkObject`
  - Adicionar `NetworkTransform`
  - Criar `NetworkEnemy.cs`

- [ ] **IA apenas no servidor**
  ```csharp
  if (IsServer)
  {
      // Executar IA
      UpdateAI();
  }
  else
  {
      // Apenas visualizar
  }
  ```

- [ ] **Sincronizar estados**
  - `NetworkVariable<EnemyAIState> netState`
  - `NetworkVariable<float> netHealth`
  - `NetworkVariable<ulong> targetPlayerId`

- [ ] **Sistema de dano validado por servidor**
  ```csharp
  [ServerRpc(RequireOwnership = false)]
  public void TakeDamageServerRpc(float damage)
  {
      if (IsServer)
      {
          netHealth.Value -= damage;
          if (netHealth.Value <= 0)
              DieClientRpc();
      }
  }
  ```

**Checkpoint:** Inimigo spawna e persegue players

---

#### Dia 3-4: Spawning de Enemies
**Tempo estimado: 4-6 horas**

- [ ] **NetworkEnemySpawner.cs**
  - Apenas servidor spawna
  - Usar `NetworkObject.Spawn()`

- [ ] **Pool de inimigos**
  - Evitar instanciar em runtime
  - Despawn ao invés de Destroy

- [ ] **Targeting inteligente**
  - Inimigo escolhe player mais próximo
  - Distribuir inimigos entre players

**Checkpoint:** Múltiplos inimigos spawnando e atacando

---

#### Dia 5-6: Interações Network
**Tempo estimado: 4-6 horas**

- [ ] **Network Door**
  - Sincronizar estado aberto/fechado
  - `NetworkVariable<bool> isOpen`

- [ ] **Network Collectibles**
  - Apenas um player coleta
  - Despawnar para todos

- [ ] **Network Checkpoint**
  - Sincronizar progresso

**Checkpoint:** Portas e itens funcionando multiplayer

---

#### Dia 7: Atmosfera e Audio
**Tempo estimado: 4-6 horas**

- [ ] **NetworkAtmosphereController**
  - Sincronizar tensão
  - Servidor calcula, clientes aplicam

- [ ] **Audio sincronizado**
  - Músicas coordenadas
  - Efeitos sonoros 3D

- [ ] **Testing geral**

**Checkpoint:** Atmosfera sincronizada entre players

---

## 📅 Fase 2: Sistemas Essenciais (Semanas 3-4)

### Objetivo
Implementar sistemas essenciais para gameplay cooperativo sólido.

### Semana 3: Event Bus e Timers

#### Dia 1-3: Event Bus Pattern
**Tempo estimado: 8-12 horas**

- [ ] **Instalar Unity-Event-Bus do adammyhre**
  - Clonar repositório
  - Importar scripts

- [ ] **Migrar GameEvents.cs**
  - Converter eventos estáticos para Event Bus
  - Criar estruturas de eventos

- [ ] **Implementar eventos de rede**
  ```csharp
  public struct NetworkPlayerDamagedEvent : IEvent
  {
      public int playerId;
      public float damage;
      public float currentHealth;
  }
  ```

- [ ] **Atualizar todos sistemas**
  - PlayerHealth usa Event Bus
  - UI usa Event Bus
  - AudioManager usa Event Bus

**Checkpoint:** Event Bus substituindo GameEvents

---

#### Dia 4-6: Improved Timers
**Tempo estimado: 6-8 horas**

- [ ] **Instalar Unity-Improved-Timers**

- [ ] **Substituir timer manual**
  - Flashlight battery usa CountdownTimer
  - Enemy spawner usa FrequencyTimer
  - Cooldowns usam CountdownTimer

- [ ] **Criar utility timers**
  - Sistema de buffs temporários
  - Efeitos de status

**Checkpoint:** Todos timers usando novo sistema

---

#### Dia 7: Testing e Optimização
**Tempo estimado: 4-6 horas**

- [ ] **Performance profiling**
- [ ] **Memory leaks**
- [ ] **Bugfixes**

---

### Semana 4: Polish e UX

#### Dia 1-2: Lobby System
**Tempo estimado: 6-8 horas**

- [ ] **Room browser**
- [ ] **Player ready system**
- [ ] **Host migration (opcional)**

---

#### Dia 3-4: Chat e Comunicação
**Tempo estimado: 4-6 horas**

- [ ] **Text chat**
- [ ] **Quick commands (emotes)**
- [ ] **Ping system**

---

#### Dia 5-7: UI Multiplayer
**Tempo estimado: 6-8 horas**

- [ ] **Mostrar saúde de aliados**
- [ ] **Minimapa com posições**
- [ ] **Indicadores de direção**

**Checkpoint:** Sistema multiplayer completo e polido

---

## 📅 Fase 3: IA Avançada (Semanas 5-6)

### Objetivo
Implementar sistema de IA avançado (GOAP ou Behavior Trees).

### Decisão: Qual Sistema Usar?

**Escolha GOAP se:**
- Quer inimigos muito inteligentes
- Coordenação entre inimigos é importante
- Tem tempo para debugging complexo

**Escolha Behavior Trees se:**
- Quer hierarquia visual clara
- Designers não-programadores vão ajustar
- Precisa de debugging fácil

**Recomendação:** Behavior Trees para primeiro jogo de terror coop.

---

### Semana 5: Implementação BT (ou GOAP)

#### Dia 1-3: Core Framework
**Tempo estimado: 10-12 horas**

- [ ] **Instalar Unity-Behaviour-Trees do adammyhre**

- [ ] **Criar BTNode base**
  - Composite nodes (Sequence, Selector)
  - Decorator nodes (Inverter, Repeater)
  - Leaf nodes (Actions, Conditions)

- [ ] **Implementar Blackboard**

- [ ] **Criar BehaviorTree raiz**

**Checkpoint:** Framework BT funcionando

---

#### Dia 4-6: Inimigo com BT
**Tempo estimado: 8-12 horas**

- [ ] **Criar BTEnemy : NetworkEnemy**

- [ ] **Implementar behaviors**
  - Patrol
  - Chase
  - Attack
  - Investigate
  - Hide

- [ ] **Criar árvore completa**

- [ ] **Testing e tuning**

**Checkpoint:** Inimigo com BT funcional

---

#### Dia 7: Múltiplos Tipos de Inimigos
**Tempo estimado: 6-8 horas**

- [ ] **Stalker** - Segue discretamente
- [ ] **Hunter** - Agressivo e rápido
- [ ] **Ambusher** - Embosca de esconderijos

**Checkpoint:** 3 tipos de inimigos com BT

---

### Semana 6: IA Cooperativa

#### Dia 1-3: Coordenação de Inimigos
**Tempo estimado: 8-10 horas**

- [ ] **Sistema de comunicação**
  - Inimigos compartilham informação
  - Chamam reforços

- [ ] **Flanking behavior**
  - Um distrai, outro ataca por trás

- [ ] **Group tactics**
  - Inimigos cercam players

**Checkpoint:** Inimigos coordenam ataques

---

#### Dia 4-6: Dynamic Difficulty
**Tempo estimado: 6-8 horas**

- [ ] **Adaptar spawn rate**
- [ ] **Ajustar agressividade**
- [ ] **Balancear para múltiplos players**

---

#### Dia 7: Testing e Balanceamento
**Tempo estimado: 6-8 horas**

- [ ] **Playtest com 2 players**
- [ ] **Playtest com 4 players**
- [ ] **Ajustar dificuldade**

**Checkpoint:** IA desafiadora mas justa

---

## 📅 Fase 4: Otimização e Polish (Semanas 7-8)

### Objetivo
Otimizar performance e adicionar polish.

### Semana 7: Performance

#### Dia 1-2: Octrees
**Tempo estimado: 6-8 horas**

- [ ] **Instalar Unity-Octrees**
- [ ] **Implementar busca espacial**
- [ ] **Otimizar detecção de inimigos**

---

#### Dia 3-4: Batch Raycasting
**Tempo estimado: 4-6 horas**

- [ ] **Implementar Job System**
- [ ] **Batch enemy vision checks**

---

#### Dia 5-7: Network Optimization
**Tempo estimado: 8-10 horas**

- [ ] **Interest management**
- [ ] **LOD system**
- [ ] **Bandwidth optimization**

**Checkpoint:** 60 FPS com 4 players e 10+ inimigos

---

### Semana 8: Polish

#### Dia 1-3: Visual Effects
**Tempo estimado: 8-10 horas**

- [ ] **Post-processing**
- [ ] **Partículas**
- [ ] **Animações**

---

#### Dia 4-7: Sound Design
**Tempo estimado: 8-12 horas**

- [ ] **3D audio espacial**
- [ ] **Música dinâmica**
- [ ] **Efeitos sonoros**

**Checkpoint:** Jogo polido e aterrorizante

---

## 📅 Fase 5: Conteúdo (Semanas 9-12, opcional)

### Objetivo
Adicionar conteúdo e mecânicas extras.

### Conteúdo Adicional

- [ ] **Múltiplos níveis**
- [ ] **Sistema de progressão**
- [ ] **Unlockables**
- [ ] **Achievements**
- [ ] **Modos de jogo extras**

---

## 📊 Métricas de Sucesso

### Performance
- ✅ 60 FPS com 4 players
- ✅ Latência < 100ms
- ✅ Memory < 2GB
- ✅ Network bandwidth < 50KB/s por player

### Gameplay
- ✅ Players cooperam naturalmente
- ✅ Inimigos são ameaçadores
- ✅ Atmosfera de terror mantida
- ✅ Replayability alta

### Multiplayer
- ✅ Conexões estáveis
- ✅ Sincronização perfeita
- ✅ Sem exploits
- ✅ Fair gameplay

---

## 🎯 Checklist Final

### Fundamentos
- [ ] Multiplayer funcional (2-4 players)
- [ ] Player movement sincronizado
- [ ] Enemy AI funcional
- [ ] Interações sincronizadas

### Sistemas Avançados
- [ ] Event Bus implementado
- [ ] Improved Timers implementado
- [ ] IA avançada (GOAP ou BT)
- [ ] Atmosfera sincronizada

### Performance
- [ ] 60 FPS mantido
- [ ] Octrees implementado
- [ ] Batch raycasting
- [ ] Network otimizado

### Polish
- [ ] VFX implementados
- [ ] SFX 3D implementado
- [ ] UI completa
- [ ] Tutoriais/tooltips

### Testing
- [ ] Playtest 2 players
- [ ] Playtest 4 players
- [ ] Stress test (muitos inimigos)
- [ ] Network stress test

---

## 📚 Recursos e Referências

### Documentação
- [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/)
- [Unity Transport](https://docs-multiplayer.unity3d.com/transport/)
- [Adam Myhre GitHub](https://github.com/adammyhre)

### Tutoriais
- YouTube: @git-amend
- Unity Learn: Multiplayer Networking
- Brackeys: Horror Game Series

### Assets Úteis
- Horror sounds: freesound.org
- 3D models: Mixamo, Sketchfab
- Textures: Substance Share

---

## 🚀 Começando

1. **Leia todos os documentos**
   - ADVANCED_TECHNOLOGIES.md
   - MULTIPLAYER_ARCHITECTURE.md
   - ADVANCED_AI_SYSTEMS.md
   - INTEGRATION_GUIDE.md

2. **Setup ambiente**
   - Unity 6
   - Instalar pacotes necessários

3. **Comece pela Fase 1, Dia 1**
   - Siga passo a passo
   - Não pule etapas
   - Teste frequentemente

4. **Documente progresso**
   - Mantenha changelog
   - Anote problemas
   - Faça commits regulares

---

## 💡 Dicas Importantes

### Desenvolvimento
- ✅ Commits frequentes (diários)
- ✅ Branches para features
- ✅ Testing contínuo
- ✅ Documentação inline

### Debugging
- ✅ Use logs com prefixos `[Network]`, `[AI]`, etc
- ✅ NetworkManager tem debugging tools
- ✅ Unity Profiler é seu amigo
- ✅ Test builds early and often

### Performance
- ✅ Profile frequentemente
- ✅ Otimize depois de funcionar
- ✅ Use object pooling
- ✅ Evite FindObjectOfType no Update

### Multiplayer
- ✅ Sempre teste com 2+ players
- ✅ Teste com latência simulada
- ✅ Valide tudo no servidor
- ✅ Cliente nunca é fonte da verdade

---

## ⚠️ Armadilhas Comuns

### Networking
- ❌ Não sincronizar tudo (bandwidth!)
- ❌ Lógica crítica no cliente
- ❌ FindObjectByType em Update
- ❌ Não testar com latência

### IA
- ❌ IA muito complexa (lag)
- ❌ Não cachear referências
- ❌ Pathfinding todo frame
- ❌ Raycast sem limites

### Performance
- ❌ Instanciar em Update
- ❌ GetComponent toda hora
- ❌ String concatenation em hot paths
- ❌ Não usar object pooling

---

## 🎉 Próximos Passos

Agora que você tem o roadmap completo:

1. **Comece pela Fase 1** ➡️ Fundamentos são críticos
2. **Siga o ritmo** ➡️ Não tente fazer tudo de uma vez
3. **Teste sempre** ➡️ Cada checkpoint é importante
4. **Ajuste conforme necessário** ➡️ Este roadmap é um guia, não lei

**Boa sorte! Você está pronto para criar um incrível jogo de terror cooperativo! 👻🎮**

---

Consulte **INTEGRATION_GUIDE.md** para começar a implementação passo a passo de cada sistema.
