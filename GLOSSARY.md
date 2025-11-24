# 📖 Glossário Técnico - Horror Game Framework

Definições claras de termos técnicos usados no desenvolvimento do jogo de terror cooperativo.

## 📋 Índice

- [Networking](#networking)
- [Inteligência Artificial](#inteligência-artificial)
- [Design Patterns](#design-patterns)
- [Unity Específico](#unity-específico)
- [Performance](#performance)
- [Matemática e Física](#matemática-e-física)
- [Game Design](#game-design)

---

## 🌐 Networking

### Authority (Autoridade)
**O que é**: Define quem tem controle sobre um objeto ou variável na rede.

**Server Authority**: Servidor controla tudo (recomendado para evitar cheats).
**Client Authority**: Cliente controla seu próprio objeto.

**Exemplo**:
```csharp
// Server authority - servidor decide saúde
[ServerRpc]
void TakeDamageServerRpc(int damage)
{
    health -= damage; // Apenas servidor executa
}
```

---

### Bandwidth
**O que é**: Largura de banda, quantidade de dados transmitidos pela rede por segundo.

**Medida**: Bytes/segundo (B/s) ou Bits/segundo (bps).
**Ideal**: <50KB/s por player em jogo cooperativo.

**Otimização**: Sincronizar apenas o necessário, usar compressão.

---

### Client Prediction
**O que é**: Cliente prevê resultado de suas ações localmente sem esperar servidor.

**Por quê**: Reduz lag percebido.
**Problema**: Pode divergir do servidor.
**Solução**: Reconciliation (correção quando servidor responde).

**Exemplo**: Player se move instantaneamente no cliente, servidor valida depois.

---

### ClientRpc
**O que é**: Remote Procedure Call que servidor chama e todos clientes executam.

**Sintaxe Unity**: `[ClientRpc]` attribute.

**Exemplo**:
```csharp
[ClientRpc]
void PlaySoundClientRpc(string soundName)
{
    // Todos os clientes tocam o som
}
```

---

### Desync
**O que é**: Quando estado do cliente difere do servidor.

**Causa**: Latência, packet loss, bugs de sincronização.
**Solução**: Server authority, reconciliation, interpolação.

---

### Host
**O que é**: Jogador que é tanto servidor quanto cliente.

**Vantagem**: Não precisa de servidor dedicado.
**Desvantagem**: Vantagem para o host (zero latência).

---

### Interpolation (Interpolação)
**O que é**: Suavizar movimento entre posições recebidas da rede.

**Exemplo**:
```csharp
transform.position = Vector3.Lerp(
    currentPos,
    targetPos,
    Time.deltaTime * smoothSpeed
);
```

---

### Latency (Latência)
**O que é**: Tempo que dados levam para ir de A até B.

**Medida**: Milissegundos (ms).
**Bom**: <50ms.
**Aceitável**: 50-100ms.
**Ruim**: >150ms.

---

### NetworkBehaviour
**O que é**: Classe base Unity para scripts com networking.

**Herda de**: MonoBehaviour.
**Adiciona**: IsServer, IsClient, IsOwner, NetworkVariables, RPCs.

---

### NetworkObject
**O que é**: Componente que identifica objeto na rede.

**Essencial**: Todo objeto sincronizado precisa deste componente.
**NetworkObjectId**: ID único na rede.

---

### NetworkVariable<T>
**O que é**: Variável auto-sincronizada pela rede.

**Tipos suportados**: int, float, bool, string, Vector3, etc.

**Exemplo**:
```csharp
private NetworkVariable<int> health = new NetworkVariable<int>(100);
```

---

### Packet Loss
**O que é**: Perda de pacotes de dados na transmissão.

**Causa**: Conexão ruim, congestionamento.
**Resultado**: Lag, desync.
**Mitigação**: Redundância, confirmações.

---

### Reconciliation
**O que é**: Correção do cliente quando servidor detecta divergência.

**Processo**:
1. Cliente prediz movimento
2. Servidor valida
3. Se divergente, servidor corrige
4. Cliente ajusta posição

---

### Relay
**O que é**: Serviço intermediário que conecta players sem port forwarding.

**Unity Relay**: Serviço oficial Unity.
**Vantagem**: Players não precisam configurar router.
**Desvantagem**: Latência levemente maior.

---

### ServerRpc
**O que é**: Remote Procedure Call que cliente chama e servidor executa.

**Sintaxe Unity**: `[ServerRpc]` attribute.

**Exemplo**:
```csharp
[ServerRpc]
void RequestSpawnServerRpc()
{
    // Apenas servidor executa
}
```

---

### Snapshot
**O que é**: Estado completo do jogo em um momento específico.

**Uso**: Sincronização, replay, debugging.

---

### Tick
**O que é**: Atualização do servidor (como um frame).

**Tick Rate**: Quantos ticks por segundo (ex: 30 Hz = 30 ticks/s).
**Maior tick rate**: Mais preciso, mais bandwidth.

---

## 🤖 Inteligência Artificial

### Action (Ação)
**O que é**: Comportamento executável da IA (mover, atacar, etc).

**Em GOAP**: Tem precondições e efeitos.
**Em BT**: Leaf node que executa algo.

---

### Blackboard
**O que é**: Memória compartilhada entre componentes de IA.

**Uso**: Armazenar dados que múltiplos nodes precisam (posição do player, estado do mundo).

**Exemplo**:
```csharp
blackboard.SetValue("targetPlayer", playerTransform);
blackboard.SetValue("isAngry", true);
```

---

### Behavior Tree (BT)
**O que é**: Estrutura hierárquica em árvore para IA.

**Nodes**:
- **Composite**: Controle (Sequence, Selector)
- **Decorator**: Modifica filho
- **Leaf**: Ação ou Condição

**Vantagem**: Visual, modular, fácil debug.

---

### Composite Node
**O que é**: Node de Behavior Tree com múltiplos filhos.

**Tipos**:
- **Sequence**: AND lógico (todos devem suceder)
- **Selector**: OR lógico (primeiro a suceder)
- **Parallel**: Múltiplos simultâneos

---

### Condition (Condição)
**O que é**: Teste booleano (verdadeiro/falso).

**Exemplos**:
- CanSeePlayer?
- IsHealthLow?
- IsInRange?

---

### Decorator Node
**O que é**: Node que modifica comportamento de um filho.

**Tipos**:
- **Inverter**: Inverte resultado
- **Repeater**: Repete N vezes
- **Cooldown**: Adiciona delay

---

### Effect (Efeito)
**O que é**: Mudança no World State após executar Action (em GOAP).

**Exemplo**: Action "Attack" tem effect "playerDamaged = true".

---

### FSM (Finite State Machine)
**O que é**: Máquina de estados finitos.

**Estados**: Idle, Patrol, Chase, Attack.
**Transições**: Regras para mudar de estado.

**Simples mas limitado**: Não escala bem para comportamentos complexos.

---

### GOAP (Goal-Oriented Action Planning)
**O que é**: IA que planeja sequência de ações para atingir objetivo.

**Componentes**:
- **Goal**: Objetivo desejado
- **Actions**: Ações disponíveis
- **World State**: Estado atual do mundo
- **Planner**: Algoritmo que cria plano

**Vantagem**: Comportamentos emergentes, muito inteligente.
**Desvantagem**: Complexo, hard to debug.

---

### Heuristic
**O que é**: Estimativa "chute educado" usado em algoritmos de busca.

**Exemplo**: Em A*, heurística é distância em linha reta até objetivo.

---

### NavMesh (Navigation Mesh)
**O que é**: Malha 3D que define áreas walkable (onde IA pode andar).

**Baking**: Processo de gerar NavMesh a partir de geometria.
**NavMeshAgent**: Componente que move objeto pelo NavMesh.

---

### Node State
**O que é**: Resultado da execução de um node de BT.

**Valores**:
- **Running**: Ainda executando
- **Success**: Sucedeu
- **Failure**: Falhou

---

### Pathfinding
**O que é**: Algoritmo para encontrar caminho de A até B.

**Algoritmos**:
- **A***: Ótimo, rápido
- **Dijkstra**: Garante caminho mais curto
- **NavMesh**: Unity implementation

---

### Precondition (Pré-condição)
**O que é**: Requisito para executar Action (em GOAP).

**Exemplo**: Action "Attack" requer precondition "hasWeapon = true".

---

### Selector Node
**O que é**: Composite node que executa filhos até um suceder (OR).

**Uso**: Tentar comportamentos em ordem de prioridade.

---

### Sequence Node
**O que é**: Composite node que executa filhos em ordem até um falhar (AND).

**Uso**: Sequência de passos que todos devem suceder.

---

### Utility AI
**O que é**: IA baseada em scoring onde ação com maior score é escolhida.

**Componentes**:
- **Considerations**: Fatores que influenciam (distância, saúde, etc)
- **Score**: Valor calculado para cada ação
- **Action**: Ação com maior score é executada

**Vantagem**: Decisões contextuais, fácil balancear.

---

### World State
**O que é**: Representação do estado do mundo para IA (em GOAP).

**Exemplo**:
```csharp
worldState.SetState("playerVisible", true);
worldState.SetState("ammoCount", 30);
```

---

## 🏗️ Design Patterns

### Dependency Injection (DI)
**O que é**: Passar dependências via construtor/método ao invés de criar internamente.

**Vantagem**: Testável, desacoplado.

**Exemplo**:
```csharp
// Com DI
public class Enemy
{
    private IWeapon weapon;

    public Enemy(IWeapon weapon) // Injeta
    {
        this.weapon = weapon;
    }
}
```

---

### Event Bus
**O que é**: Sistema centralizado de eventos para comunicação desacoplada.

**Vantagem**: Publishers e Subscribers não se conhecem.

**Exemplo**:
```csharp
// Publicar
EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent());

// Assinar
EventBus<PlayerDiedEvent>.Register(OnPlayerDied);
```

---

### Factory Pattern
**O que é**: Classe responsável por criar objetos.

**Uso**: Centralizar criação complexa.

**Exemplo**:
```csharp
public class EnemyFactory
{
    public Enemy CreateZombie() { }
    public Enemy CreateGhost() { }
}
```

---

### Object Pooling
**O que é**: Reutilizar objetos ao invés de criar/destruir constantemente.

**Vantagem**: Performance (evita GC).

**Exemplo**: Pool de bullets, VFX, enemies.

---

### Observer Pattern
**O que é**: Objetos observam mudanças em outro objeto.

**Implementação**: Events, delegates, Event Bus.

---

### Service Locator
**O que é**: Registro central para acessar serviços globalmente.

**Exemplo**:
```csharp
ServiceLocator.Get<AudioManager>().PlaySound("shot");
```

---

### Singleton Pattern
**O que é**: Classe com apenas uma instância global.

**Uso**: Managers (GameManager, AudioManager).

**Cuidado**: Pode criar acoplamento global.

---

### State Pattern
**O que é**: Encapsular estados como objetos.

**Vantagem**: Mais limpo que FSM com switch.

---

## 🎮 Unity Específico

### Coroutine
**O que é**: Função que pode pausar execução e retornar controle ao Unity.

**Uso**: Delays, animações, operações assíncronas.

**Exemplo**:
```csharp
IEnumerator WaitAndPrint()
{
    yield return new WaitForSeconds(2f);
    Debug.Log("Esperou 2s");
}

StartCoroutine(WaitAndPrint());
```

---

### DontDestroyOnLoad
**O que é**: Marca GameObject para não ser destruído ao carregar cena.

**Uso**: Managers persistentes.

---

### GameObject
**O que é**: Objeto básico na hierarquia Unity.

**Contém**: Components (Transform, scripts, etc).

---

### Gizmos
**O que é**: Visualizações no Editor para debugging.

**Exemplo**:
```csharp
void OnDrawGizmos()
{
    Gizmos.DrawWireSphere(transform.position, radius);
}
```

---

### Inspector
**O que é**: Painel do Unity que mostra propriedades de GameObjects.

**SerializeField**: Expor variável privada no Inspector.

---

### Instantiate
**O que é**: Criar cópia de GameObject ou Prefab.

**Cuidado**: Caro, use Object Pooling se frequente.

---

### Invoke
**O que é**: Chamar método após delay.

**Exemplo**:
```csharp
Invoke("MethodName", 2f); // Chama após 2s
```

---

### Layer
**O que é**: Categoria de GameObject.

**Uso**: Raycasts seletivos, colisões, culling.

---

### Prefab
**O que é**: Template reutilizável de GameObject.

**Vantagem**: Modificações propagam para todas instâncias.

---

### ScriptableObject
**O que é**: Asset que armazena dados.

**Uso**: Configurações, dados compartilhados.

**Vantagem**: Não precisa estar na cena.

---

### Serialization
**O que é**: Converter objeto em formato armazenável.

**Unity**: Usa para salvar estado no Inspector.

---

### Tag
**O que é**: Label de string para identificar GameObject.

**Exemplo**: "Player", "Enemy".

**Uso**: FindGameObjectWithTag("Player").

---

### Transform
**O que é**: Componente com position, rotation, scale.

**Sempre presente**: Todo GameObject tem Transform.

---

## ⚡ Performance

### Batching
**O que é**: Agrupar múltiplas operações em uma.

**Exemplos**: Draw call batching, batch raycasting.

**Vantagem**: Reduz overhead.

---

### Draw Call
**O que é**: Comando para GPU renderizar algo.

**Problema**: Muitos draw calls = performance ruim.
**Solução**: Batching, atlasing.

---

### Garbage Collection (GC)
**O que é**: Sistema que libera memória não usada.

**Problema**: Pode causar lag spikes.
**Solução**: Evitar alocações frequentes, object pooling.

---

### Job System
**O que é**: Sistema Unity para multithreading seguro.

**Uso**: Processar dados pesados em background.

**Vantagem**: Usa múltiplos CPU cores.

---

### LOD (Level of Detail)
**O que é**: Trocar modelo detalhado por simples quando longe.

**Exemplo**: Enemy com 5000 polys de perto, 500 de longe.

---

### Octree
**O que é**: Estrutura de dados que divide espaço 3D em octantes recursivamente.

**Uso**: Busca espacial eficiente (O(log n)).

**Exemplo**: Encontrar inimigos próximos rapidamente.

---

### Profiler
**O que é**: Ferramenta Unity para medir performance.

**Mostra**: CPU usage, memory, rendering, etc.

**Acesso**: Window > Analysis > Profiler.

---

### Spatial Partitioning
**O que é**: Dividir espaço em regiões para otimizar buscas.

**Exemplos**: Octree, Quadtree, Grid.

---

## 🧮 Matemática e Física

### Dot Product (Produto Escalar)
**O que é**: Operação entre dois vetores que retorna scalar.

**Uso**: Calcular ângulo, checar se algo está na frente/atrás.

**Exemplo**:
```csharp
float dot = Vector3.Dot(forward, toTarget);
if (dot > 0) // Target está na frente
```

---

### Lerp (Linear Interpolation)
**O que é**: Interpolação linear entre dois valores.

**Fórmula**: `result = start + (end - start) * t`

**Exemplo**:
```csharp
float newValue = Mathf.Lerp(0, 100, 0.5f); // = 50
```

---

### Normalize
**O que é**: Transformar vetor em comprimento 1 (direção pura).

**Uso**: Quando só importa direção, não magnitude.

---

### Raycast
**O que é**: Lançar raio invisível para detectar colisões.

**Uso**: Line of sight, shooting, ground detection.

---

### Rigidbody
**O que é**: Componente que adiciona física ao GameObject.

**Gravity**: Aplica gravidade.
**Collision**: Detecta colisões físicas.

---

### Slerp (Spherical Lerp)
**O que é**: Interpolação esférica para rotações.

**Uso**: Suavizar rotações.

**Exemplo**:
```csharp
transform.rotation = Quaternion.Slerp(current, target, t);
```

---

## 🎨 Game Design

### Co-op (Cooperative)
**O que é**: Modo de jogo onde players trabalham juntos.

**Desafio**: Balancear para múltiplos players.

---

### Emergent Gameplay
**O que é**: Comportamentos não programados que emergem de sistemas interagindo.

**Exemplo**: GOAP pode criar táticas inesperadas.

---

### FOV (Field of View)
**O que é**: Campo de visão (ângulo que pode ver).

**Exemplo**: Enemy tem FOV de 90° (vê 45° cada lado).

---

### Game Loop
**O que é**: Ciclo Update > Render > Repeat.

---

### Hit Scan
**O que é**: Detecção instantânea (raycast), sem projétil físico.

**Exemplo**: Armas de precisão.

---

### Hitscan vs Projectile
**Hitscan**: Instantâneo (raycast).
**Projectile**: Projétil físico que viaja.

---

### Input Buffer
**O que é**: Armazenar inputs por um curto tempo.

**Uso**: Permitir input antes do momento exato.

---

### Invulnerability Frames (i-frames)
**O que é**: Breve período de invulnerabilidade após tomar dano.

**Uso**: Evitar morte instantânea por múltiplos hits.

---

### Juice
**O que é**: Feedback visual/sonoro exagerado.

**Exemplo**: Tela shake, particles, sound effects.

---

### Lag Compensation
**O que é**: Técnicas para compensar latência.

**Exemplos**: Client prediction, interpolation.

---

### Procedural Generation
**O que é**: Conteúdo gerado algoritmicamente.

**Exemplo**: Níveis aleatórios.

---

### Respawn
**O que é**: Renascer após morte.

---

### Spawn Point
**O que é**: Local onde players/inimigos aparecem.

---

### Tick Rate
**O que é**: Frequência de updates do servidor.

**Exemplo**: 30 Hz = 30 updates/segundo.

---

## 🔤 Siglas Comuns

| Sigla | Significado | Descrição |
|-------|-------------|-----------|
| **AI** | Artificial Intelligence | Inteligência Artificial |
| **BT** | Behavior Tree | Árvore de comportamento |
| **DI** | Dependency Injection | Injeção de dependência |
| **DPS** | Damage Per Second | Dano por segundo |
| **ECS** | Entity Component System | Sistema Unity DOTS |
| **FPS** | First Person Shooter | Jogo em primeira pessoa |
| **FSM** | Finite State Machine | Máquina de estados finitos |
| **GC** | Garbage Collection | Coleta de lixo |
| **GOAP** | Goal-Oriented Action Planning | Planejamento orientado a objetivos |
| **GUI** | Graphical User Interface | Interface gráfica |
| **HUD** | Heads-Up Display | Interface sobreposta |
| **LOD** | Level of Detail | Nível de detalhe |
| **NPC** | Non-Player Character | Personagem não-jogável |
| **PBR** | Physically Based Rendering | Renderização física |
| **RPC** | Remote Procedure Call | Chamada remota de procedimento |
| **SFX** | Sound Effects | Efeitos sonoros |
| **URP** | Universal Render Pipeline | Pipeline de renderização Unity |
| **VFX** | Visual Effects | Efeitos visuais |

---

## 📝 Glossário por Contexto

### Quando alguém diz... eles querem dizer:

**"Server authority"** → Servidor controla tudo, anti-cheat.

**"Client prediction"** → Cliente se move sem esperar servidor.

**"Desync"** → Cliente e servidor discordam sobre estado.

**"Tick rate"** → Frequência de update do servidor.

**"GOAP"** → IA que planeja ações.

**"BT"** → Behavior Tree, IA hierárquica.

**"Octree"** → Estrutura para busca espacial rápida.

**"Object pooling"** → Reutilizar objetos ao invés de criar/destruir.

**"Event Bus"** → Sistema de mensagens desacoplado.

**"Singleton"** → Classe com uma única instância global.

**"Lerp"** → Interpolação suave entre valores.

**"Raycast"** → Lançar raio para detectar colisão.

**"NavMesh"** → Malha de navegação para pathfinding.

**"Prefab"** → Template de GameObject.

**"ScriptableObject"** → Asset que armazena dados.

**"Coroutine"** → Função que pode pausar.

---

## ❓ Perguntas Frequentes Explicadas

### "O que é melhor: GOAP ou BT?"
**Depende**:
- GOAP = Mais inteligente, mais complexo
- BT = Mais organizado, mais fácil debug

### "Por que usar Event Bus?"
**Desacoplamento**: Sistemas não precisam se conhecer.

### "Quando usar Object Pooling?"
**Quando**: Criar/destruir objetos frequentemente (bullets, VFX).

### "Server ou Client authority?"
**Server**: Sempre, para evitar cheats.

### "Como reduzir lag?"
**Client prediction** + **Interpolation** + **Otimizar bandwidth**.

---

## 🔍 Como Usar Este Glossário

1. **Ctrl+F / Cmd+F**: Buscar termo específico
2. **Índice**: Navegar por categoria
3. **Siglas**: Consultar tabela rápida
4. **Contexto**: Ver "Quando alguém diz..."

---

**📌 Dica**: Adicione termos novos conforme aprende!

**🔖 Marcação**: Marque termos que usa frequentemente para consulta rápida!
