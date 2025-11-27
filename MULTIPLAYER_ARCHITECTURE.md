# 🌐 Arquitetura Multiplayer para Horror Cooperativo

Guia completo para transformar o Horror Game Framework em um jogo cooperativo usando **Netcode for GameObjects**.

## 📚 Índice

- [Visão Geral](#visão-geral)
- [Escolha da Solução de Networking](#escolha-da-solução-de-networking)
- [Arquitetura Server-Authoritative](#arquitetura-server-authoritative)
- [Sistemas Principais](#sistemas-principais)
- [Sincronização de Estados](#sincronização-de-estados)
- [Otimização de Bandwidth](#otimização-de-bandwidth)
- [Implementação Passo a Passo](#implementação-passo-a-passo)

---

## 🎯 Visão Geral

### Objetivo
Transformar o framework single-player em uma experiência cooperativa 2-4 jogadores onde:
- Jogadores exploram juntos ambientes de terror
- Inimigos são desafios compartilhados
- Atmosfera de tensão é sincronizada
- Performance é mantida mesmo com latência

### Princípios de Design Multiplayer

```
Single Player            Cooperative Horror
┌─────────────┐         ┌──────────────────┐
│   Player    │         │  Player 1 (Host) │
│      ↓      │   →     │  Player 2        │
│   Enemies   │         │  Player 3        │
│      ↓      │         │  Player 4        │
│  Atmosphere │         │        ↓         │
└─────────────┘         │  Shared Enemies  │
                        │        ↓         │
                        │ Synced Atmosphere│
                        └──────────────────┘
```

**Desafios Únicos de Horror Coop:**
1. **Tensão Compartilhada**: Todos devem sentir medo juntos
2. **Coordenação**: Inimigos devem ameaçar o grupo
3. **Recursos Compartilhados**: Itens, munição, saúde
4. **Separação**: Jogadores podem se separar (mais tenso!)
5. **Morte de Aliado**: Impacto na gameplay e atmosfera

---

## 🔌 Escolha da Solução de Networking

### Netcode for GameObjects (NGO) ✅ Recomendado

**Por quê?**
- ✅ Solução oficial da Unity
- ✅ Integração nativa com Unity 6
- ✅ Server-authoritative (anti-cheat)
- ✅ Documentação extensa
- ✅ Transport agnóstico (Unity Transport, Steam, etc)
- ✅ Suporte a Relay (não precisa port forwarding)

**Alternativas:**
- ❌ **Mirror**: Boa mas não oficial
- ❌ **Photon**: Pago para muitos jogadores
- ❌ **Custom Socket**: Muito trabalho

### Instalação

```
Window > Package Manager > Unity Registry
Buscar: "Netcode for GameObjects"
Instalar: com.unity.netcode.gameobjects

Também instalar:
- com.unity.transport (Unity Transport)
- com.unity.services.relay (para hosting fácil)
- com.unity.multiplayer.tools (debugging)
```

---

## 🏗️ Arquitetura Server-Authoritative

### O que é Server Authority?

```
Client-Authoritative (❌ Ruim)         Server-Authoritative (✅ Bom)
┌──────────┐                           ┌──────────┐
│ Cliente  │ "Matei inimigo!"          │ Cliente  │ "Atirei!"
│    ↓     │                           │    ↓     │
│ Inimigo  │ Morre (pode hackear)      │ Servidor │ Valida hit
│   MORTO  │                           │    ↓     │
└──────────┘                           │ Inimigo  │ Se acertou, morre
                                       │  MORTO   │
                                       └──────────┘
```

**Servidor controla:**
- ✅ Saúde de todos (players e inimigos)
- ✅ Posição de inimigos
- ✅ Spawning de inimigos
- ✅ Coletas de itens
- ✅ Estado do jogo
- ✅ Validação de ações

**Cliente controla:**
- ✅ Input do player local
- ✅ Câmera
- ✅ Audio (triggers locais)
- ✅ VFX (efeitos visuais locais)
- ✅ Predição de movimento

---

## 🎮 Sistemas Principais

### 1. Network Manager Setup

```csharp
using Unity.Netcode;

public class HorrorGameNetworkManager : MonoBehaviour
{
    public static HorrorGameNetworkManager Instance { get; private set; }

    [Header("Network Config")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private NetworkManager networkManager;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        networkManager = GetComponent<NetworkManager>();
    }

    private void Start()
    {
        // Callbacks de networking
        networkManager.OnServerStarted += OnServerStarted;
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
    }

    // Host = Server + Client
    public void StartHost()
    {
        networkManager.StartHost();
    }

    // Server puro (dedicated)
    public void StartServer()
    {
        networkManager.StartServer();
    }

    // Cliente conectando
    public void StartClient()
    {
        networkManager.StartClient();
    }

    private void OnServerStarted()
    {
        Debug.Log("[Server] Servidor iniciado!");

        if (networkManager.IsHost)
        {
            SpawnPlayer(networkManager.LocalClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[Server] Cliente {clientId} conectado");

        if (networkManager.IsServer)
        {
            SpawnPlayer(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[Server] Cliente {clientId} desconectado");

        // Cleanup do player
        if (networkManager.IsServer)
        {
            DestroyPlayerObjects(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        // Escolher spawn point
        Transform spawnPoint = GetAvailableSpawnPoint();

        // Instanciar player
        GameObject playerInstance = Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Spawnar na rede
        var networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);

        Debug.Log($"[Server] Player {clientId} spawnado em {spawnPoint.position}");
    }

    private Transform GetAvailableSpawnPoint()
    {
        // Lógica para escolher spawn (round-robin, aleatório, etc)
        int index = (int)networkManager.ConnectedClients.Count % spawnPoints.Length;
        return spawnPoints[index];
    }

    private void DestroyPlayerObjects(ulong clientId)
    {
        if (networkManager.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null)
            {
                Destroy(client.PlayerObject.gameObject);
            }
        }
    }
}
```

---

### 2. Network Player Controller

```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Components")]
    private CharacterController characterController;
    private FirstPersonController fpController;
    private MouseLook mouseLook;

    [Header("Network Sync")]
    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();

    [Header("Player Info")]
    public NetworkVariable<int> playerHealth = new NetworkVariable<int>(100);
    public NetworkVariable<string> playerName = new NetworkVariable<string>("Player");

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        characterController = GetComponent<CharacterController>();
        fpController = GetComponent<FirstPersonController>();
        mouseLook = GetComponentInChildren<MouseLook>();

        if (IsOwner)
        {
            // Este é o player local - habilitar controles
            EnableLocalPlayer();
        }
        else
        {
            // Este é um player remoto - desabilitar controles locais
            DisableRemotePlayerControls();
        }

        // Inscrever em mudanças de saúde
        playerHealth.OnValueChanged += OnHealthChanged;

        // Setup nome
        if (IsOwner)
        {
            SetPlayerNameServerRpc(GeneratePlayerName());
        }
    }

    private void EnableLocalPlayer()
    {
        // Habilitar input
        fpController.enabled = true;
        mouseLook.enabled = true;

        // Habilitar câmera
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
            cam.enabled = true;

        // Habilitar audio listener
        AudioListener listener = GetComponentInChildren<AudioListener>();
        if (listener != null)
            listener.enabled = true;

        Debug.Log("[Network] Local player habilitado");
    }

    private void DisableRemotePlayerControls()
    {
        // Desabilitar input
        fpController.enabled = false;
        mouseLook.enabled = false;

        // Desabilitar câmera
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
            cam.enabled = false;

        // Desabilitar audio listener
        AudioListener listener = GetComponentInChildren<AudioListener>();
        if (listener != null)
            listener.enabled = false;

        Debug.Log("[Network] Remote player - controles desabilitados");
    }

    private void Update()
    {
        if (IsOwner)
        {
            // Player local - enviar posição para servidor
            UpdateServerPosition();
        }
        else
        {
            // Player remoto - interpolar posição
            InterpolateRemotePlayer();
        }
    }

    private void UpdateServerPosition()
    {
        // Enviar posição/rotação atual para servidor
        if (Vector3.Distance(transform.position, networkPosition.Value) > 0.01f ||
            Quaternion.Angle(transform.rotation, networkRotation.Value) > 1f)
        {
            UpdatePositionServerRpc(transform.position, transform.rotation);
        }
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
    {
        networkPosition.Value = position;
        networkRotation.Value = rotation;
    }

    private void InterpolateRemotePlayer()
    {
        // Suavizar movimento de players remotos
        transform.position = Vector3.Lerp(
            transform.position,
            networkPosition.Value,
            Time.deltaTime * 15f
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            networkRotation.Value,
            Time.deltaTime * 15f
        );
    }

    // Sistema de dano
    public void TakeDamage(int damage)
    {
        if (IsOwner)
        {
            // Owner solicita dano ao servidor
            TakeDamageServerRpc(damage);
        }
    }

    [ServerRpc]
    private void TakeDamageServerRpc(int damage)
    {
        // Servidor valida e aplica dano
        playerHealth.Value -= damage;

        if (playerHealth.Value <= 0)
        {
            playerHealth.Value = 0;
            HandlePlayerDeathClientRpc();
        }
    }

    [ClientRpc]
    private void HandlePlayerDeathClientRpc()
    {
        // Todos os clientes reproduzem morte
        Debug.Log($"Player {playerName.Value} morreu!");

        if (IsOwner)
        {
            // Player local morreu - desabilitar controles
            fpController.enabled = false;
            mouseLook.enabled = false;

            // Mostrar tela de morte
            GameEvents.OnPlayerDied?.Invoke();
        }

        // Tocar animação/som de morte
        PlayDeathEffects();
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        Debug.Log($"Player {playerName.Value} saúde: {oldHealth} -> {newHealth}");

        if (IsOwner)
        {
            // Atualizar HUD local
            GameEvents.OnPlayerDamaged?.Invoke(oldHealth - newHealth, newHealth);
        }
    }

    [ServerRpc]
    private void SetPlayerNameServerRpc(string name)
    {
        playerName.Value = name;
    }

    private string GeneratePlayerName()
    {
        return $"Player_{OwnerClientId}";
    }

    private void PlayDeathEffects()
    {
        // Implementar efeitos de morte
    }
}
```

---

### 3. Network Enemy

```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkEnemy : NetworkBehaviour
{
    [Header("Enemy Stats")]
    private NetworkVariable<float> networkHealth = new NetworkVariable<float>(100f);
    private NetworkVariable<EnemyAIState> networkState = new NetworkVariable<EnemyAIState>();

    [Header("Target")]
    private NetworkVariable<ulong> targetPlayerId = new NetworkVariable<ulong>();

    private EnemyBase enemyBase;
    private NavMeshAgent navAgent;

    public override void OnNetworkSpawn()
    {
        enemyBase = GetComponent<EnemyBase>();
        navAgent = GetComponent<NavMeshAgent>();

        if (IsServer)
        {
            // Apenas servidor roda lógica de IA
            networkHealth.Value = enemyBase.maxHealth;
            EnableAI();
        }
        else
        {
            // Clientes apenas visualizam
            DisableAI();
        }

        // Todos escutam mudanças
        networkHealth.OnValueChanged += OnHealthChanged;
        networkState.OnValueChanged += OnStateChanged;
        targetPlayerId.OnValueChanged += OnTargetChanged;
    }

    private void EnableAI()
    {
        enemyBase.enabled = true;
        navAgent.enabled = true;
    }

    private void DisableAI()
    {
        enemyBase.enabled = false;
        navAgent.enabled = false;
    }

    private void Update()
    {
        if (IsServer)
        {
            // Servidor atualiza IA
            UpdateAI();
        }
        else
        {
            // Clientes interpolam posição
            // (posição já é sincronizada via NetworkTransform component)
        }
    }

    private void UpdateAI()
    {
        // Lógica de IA apenas no servidor
        // Encontrar player mais próximo ou focado
        var target = GetTargetPlayer();

        if (target != null)
        {
            var targetNetworkObject = target.GetComponent<NetworkObject>();
            if (targetNetworkObject != null)
            {
                targetPlayerId.Value = targetNetworkObject.OwnerClientId;
            }

            // Atualizar estado da IA
            UpdateEnemyState(target);
        }
    }

    private GameObject GetTargetPlayer()
    {
        // Encontrar player mais próximo vivo
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            if (client.PlayerObject != null)
            {
                var playerHealth = client.PlayerObject.GetComponent<NetworkPlayerController>();
                if (playerHealth != null && playerHealth.playerHealth.Value > 0)
                {
                    float distance = Vector3.Distance(
                        transform.position,
                        client.PlayerObject.transform.position
                    );

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = client.PlayerObject.gameObject;
                    }
                }
            }
        }

        return closestPlayer;
    }

    private void UpdateEnemyState(GameObject target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance < enemyBase.attackRange)
        {
            networkState.Value = EnemyAIState.Attack;
            AttackTarget(target);
        }
        else if (distance < enemyBase.detectionRange)
        {
            networkState.Value = EnemyAIState.Chase;
            navAgent.SetDestination(target.transform.position);
        }
        else
        {
            networkState.Value = EnemyAIState.Patrol;
        }
    }

    private void AttackTarget(GameObject target)
    {
        var playerController = target.GetComponent<NetworkPlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(10);

            // Notificar todos os clientes do ataque (para VFX/SFX)
            PlayAttackEffectsClientRpc();
        }
    }

    [ClientRpc]
    private void PlayAttackEffectsClientRpc()
    {
        // Todos os clientes reproduzem efeitos de ataque
        GameEvents.OnEnemyAttack?.Invoke(gameObject);
        // Tocar som, animação, etc
    }

    // Receber dano
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ulong attackerId)
    {
        if (!IsServer) return;

        networkHealth.Value -= damage;

        if (networkHealth.Value <= 0)
        {
            networkHealth.Value = 0;
            Die();
        }
    }

    private void Die()
    {
        // Notificar morte
        GameEvents.OnEnemyDied?.Invoke(gameObject);

        // Reproduzir morte em todos os clientes
        PlayDeathEffectsClientRpc();

        // Despawnar após delay
        Invoke(nameof(DespawnEnemy), 2f);
    }

    [ClientRpc]
    private void PlayDeathEffectsClientRpc()
    {
        // Efeitos de morte
        Debug.Log("Enemy morreu!");
        // Animação, som, VFX
    }

    private void DespawnEnemy()
    {
        if (IsServer)
        {
            NetworkObject.Despawn(true);
        }
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        Debug.Log($"Enemy saúde: {oldHealth} -> {newHealth}");
    }

    private void OnStateChanged(EnemyAIState oldState, EnemyAIState newState)
    {
        Debug.Log($"Enemy estado: {oldState} -> {newState}");

        // Atualizar animações baseado no estado
        UpdateAnimations(newState);
    }

    private void OnTargetChanged(ulong oldTarget, ulong newTarget)
    {
        Debug.Log($"Enemy alvo mudou: {oldTarget} -> {newTarget}");
    }

    private void UpdateAnimations(EnemyAIState state)
    {
        // Atualizar animator baseado no estado
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("State", (int)state);
        }
    }
}
```

---

### 4. Network Atmosphere Controller

```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkAtmosphereController : NetworkBehaviour
{
    [Header("Network Variables")]
    private NetworkVariable<float> networkTensionLevel = new NetworkVariable<float>(0f);

    private AtmosphereController atmosphereController;

    public override void OnNetworkSpawn()
    {
        atmosphereController = GetComponent<AtmosphereController>();

        if (IsServer)
        {
            // Servidor calcula tensão
            InvokeRepeating(nameof(CalculateTension), 0f, 0.5f);
        }

        // Todos clientes reagem a mudanças
        networkTensionLevel.OnValueChanged += OnTensionChanged;
    }

    private void CalculateTension()
    {
        if (!IsServer) return;

        // Calcular tensão baseado em inimigos próximos a QUALQUER player
        float maxTension = 0f;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            if (client.PlayerObject != null)
            {
                float playerTension = CalculatePlayerTension(client.PlayerObject.transform.position);
                maxTension = Mathf.Max(maxTension, playerTension);
            }
        }

        networkTensionLevel.Value = Mathf.Lerp(
            networkTensionLevel.Value,
            maxTension,
            Time.deltaTime * 2f
        );
    }

    private float CalculatePlayerTension(Vector3 playerPosition)
    {
        // Mesmo cálculo do sistema single-player
        int nearbyEnemies = 0;
        Collider[] enemiesInRange = Physics.OverlapSphere(
            playerPosition,
            atmosphereController.tensionDetectionRadius,
            atmosphereController.enemyLayer
        );

        nearbyEnemies = enemiesInRange.Length;

        return Mathf.Clamp01(nearbyEnemies * 0.2f);
    }

    private void OnTensionChanged(float oldTension, float newTension)
    {
        // Atualizar atmosfera local
        atmosphereController.SetTension(newTension);

        // Disparar evento
        GameEvents.OnTensionLevelChanged?.Invoke(newTension);
    }
}
```

---

## 📡 Sincronização de Estados

### NetworkVariable<T>

Variáveis sincronizadas automaticamente pela rede:

```csharp
// Tipos suportados nativamente
private NetworkVariable<int> intVar = new NetworkVariable<int>();
private NetworkVariable<float> floatVar = new NetworkVariable<float>();
private NetworkVariable<bool> boolVar = new NetworkVariable<bool>();
private NetworkVariable<string> stringVar = new NetworkVariable<string>();
private NetworkVariable<Vector3> vectorVar = new NetworkVariable<Vector3>();

// Apenas servidor pode escrever (padrão)
private NetworkVariable<int> serverOnlyVar = new NetworkVariable<int>(
    writePerm: NetworkVariableWritePermission.Server
);

// Owner pode escrever
private NetworkVariable<string> ownerVar = new NetworkVariable<string>(
    writePerm: NetworkVariableWritePermission.Owner
);

// Escutar mudanças
void OnNetworkSpawn()
{
    intVar.OnValueChanged += OnIntChanged;
}

void OnIntChanged(int oldValue, int newValue)
{
    Debug.Log($"Mudou de {oldValue} para {newValue}");
}
```

### RPCs (Remote Procedure Calls)

Chamar funções pela rede:

```csharp
// ServerRpc - Cliente chama, servidor executa
[ServerRpc]
private void DoSomethingServerRpc(int value)
{
    Debug.Log($"[Server] Recebido: {value}");
}

// ClientRpc - Servidor chama, todos clientes executam
[ClientRpc]
private void DoSomethingClientRpc(int value)
{
    Debug.Log($"[Client] Recebido: {value}");
}

// ServerRpc sem ownership (qualquer um pode chamar)
[ServerRpc(RequireOwnership = false)]
private void AnyoneCanCallServerRpc()
{
    // ...
}

// Uso
void Example()
{
    if (IsOwner)
    {
        DoSomethingServerRpc(42);
    }

    if (IsServer)
    {
        DoSomethingClientRpc(99);
    }
}
```

---

## ⚡ Otimização de Bandwidth

### 1. Network Transform

Componente que sincroniza Transform automaticamente:

```csharp
// Adicione NetworkTransform ao GameObject
// Configure no Inspector:
// - Sync Position X,Y,Z ✓
// - Sync Rotation Y (geralmente só Y para FPS)
// - Interpolate ✓
// - Threshold: 0.01 (só atualiza se mudou 0.01 unidades)
```

### 2. Snapshot Interpolation

```csharp
// Para movement smoothing de objetos remotos
public class SmoothNetworkTransform : NetworkBehaviour
{
    private Vector3 positionVelocity;
    private Quaternion rotationVelocity;

    [SerializeField] private float positionSmoothTime = 0.1f;
    [SerializeField] private float rotationSmoothTime = 0.1f;

    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> netRotation = new NetworkVariable<Quaternion>();

    private void Update()
    {
        if (!IsOwner)
        {
            // Smooth interpolation para objetos remotos
            transform.position = Vector3.SmoothDamp(
                transform.position,
                netPosition.Value,
                ref positionVelocity,
                positionSmoothTime
            );

            transform.rotation = SmoothDampQuaternion(
                transform.rotation,
                netRotation.Value,
                ref rotationVelocity,
                rotationSmoothTime
            );
        }
    }

    private Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Quaternion velocity, float smoothTime)
    {
        // Implementação de smooth quaternion
        return Quaternion.Slerp(current, target, Time.deltaTime / smoothTime);
    }
}
```

### 3. Interesse Management

Reduzir dados enviados usando "áreas de interesse":

```csharp
// Apenas sincronizar objetos próximos ao player
public class NetworkInterestManager : MonoBehaviour
{
    [SerializeField] private float interestRadius = 50f;

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (client.Value.PlayerObject == null) continue;

            Vector3 playerPos = client.Value.PlayerObject.transform.position;

            // Ocultar objetos longe
            HideDistantObjects(client.Key, playerPos);
        }
    }

    private void HideDistantObjects(ulong clientId, Vector3 playerPos)
    {
        var allNetworkObjects = FindObjectsOfType<NetworkObject>();

        foreach (var netObj in allNetworkObjects)
        {
            float distance = Vector3.Distance(playerPos, netObj.transform.position);

            if (distance > interestRadius)
            {
                // Ocultar para este cliente
                netObj.NetworkHide(clientId);
            }
            else
            {
                // Mostrar para este cliente
                netObj.NetworkShow(clientId);
            }
        }
    }
}
```

---

## 🛠️ Implementação Passo a Passo

### Fase 1: Setup Básico (Dia 1)

1. **Instalar Netcode for GameObjects**
   ```
   Window > Package Manager > Add package from git URL
   https://github.com/Unity-Technologies/com.unity.netcode.gameobjects.git
   ```

2. **Criar NetworkManager GameObject**
   - Adicionar componente `NetworkManager`
   - Configurar Unity Transport
   - Adicionar `NetworkPrefabsList`

3. **Converter Player para NetworkObject**
   - Adicionar `NetworkObject` ao prefab do Player
   - Adicionar `NetworkTransform`
   - Criar `NetworkPlayerController` script

4. **Criar Menu de Conexão**
   ```csharp
   // UI com botões Host/Join
   public void OnHostButtonClicked()
   {
       NetworkManager.Singleton.StartHost();
   }

   public void OnJoinButtonClicked()
   {
       NetworkManager.Singleton.StartClient();
   }
   ```

### Fase 2: Player Networking (Dias 2-3)

5. **NetworkPlayerController completo**
   - Sincronizar posição/rotação
   - Sincronizar saúde/stamina
   - Input apenas para owner

6. **Camera e Audio**
   - Desabilitar camera para remote players
   - Um AudioListener por cliente

7. **Animações**
   - NetworkAnimator para sincronizar

### Fase 3: Enemy Networking (Dias 4-5)

8. **NetworkEnemy**
   - IA apenas no servidor
   - Sincronizar estados
   - Dano validado por servidor

9. **Enemy Spawning**
   - Apenas servidor spawna
   - Usar `NetworkObject.Spawn()`

### Fase 4: Interações (Dias 6-7)

10. **Network Interactions**
    - Portas sincronizadas
    - Itens sincronizados
    - Checkpoints sincronizados

11. **Atmosfera e Audio**
    - Tensão sincronizada
    - Música coordenada

### Fase 5: Polish (Dias 8+)

12. **Otimizações**
    - Interest management
    - Bandwidth optimization
    - Lag compensation

13. **Lobby System**
    - Lista de servidores
    - Ready system
    - Chat

---

## 🎯 Checklist de Implementação

### Setup
- [ ] Netcode for GameObjects instalado
- [ ] NetworkManager na cena
- [ ] Unity Transport configurado
- [ ] NetworkPrefabsList criado

### Player
- [ ] Player prefab com NetworkObject
- [ ] NetworkTransform adicionado
- [ ] NetworkPlayerController implementado
- [ ] Camera/Audio funcionando corretamente
- [ ] Input apenas para owner

### Enemies
- [ ] Enemy prefab com NetworkObject
- [ ] NetworkEnemy implementado
- [ ] IA apenas no servidor
- [ ] Estados sincronizados
- [ ] Dano funcionando

### Systems
- [ ] Atmosfera sincronizada
- [ ] Audio coordenado
- [ ] Interações funcionando
- [ ] UI atualizada

### Testing
- [ ] Teste com 2 players
- [ ] Teste com 4 players
- [ ] Teste de latência
- [ ] Teste de disconnect

---

## 📚 Recursos

### Documentação
- [Netcode for GameObjects Docs](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Transport](https://docs-multiplayer.unity3d.com/transport/current/about/)
- [Unity Relay](https://unity.com/products/relay)

### Tutoriais
- [Adam Myhre - Multiplayer Kart](https://github.com/adammyhre/Unity-Multiplayer-Kart)
- [Official Unity Multiplayer Sample](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop)

---

## 🚀 Próximos Passos

1. Leia **ADVANCED_AI_SYSTEMS.md** para implementar IA inteligente
2. Consulte **COOP_ROADMAP.md** para plano completo
3. Veja **INTEGRATION_GUIDE.md** para integração passo a passo

---

**Nota**: Multiplayer é complexo! Comece simples (2 players andando) e vá adicionando features incrementalmente.
