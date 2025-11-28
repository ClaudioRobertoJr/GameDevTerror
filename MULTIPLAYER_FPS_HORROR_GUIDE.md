# 🎮 Guia Completo: FPS Horror Multiplayer 4-5 Jogadores

## 🎯 Visão Geral

Este é o guia DEFINITIVO para criar um jogo de terror em primeira pessoa com **4-5 jogadores simultâneos**, usando as **tecnologias mais modernas** disponíveis em 2024/2025.

### O que vamos construir?

```
🎮 Gameplay Loop
┌─────────────────────────────────────┐
│ 1. Lobby (4-5 jogadores)            │
│    ↓                                │
│ 2. Entrar na Partida                │
│    ↓                                │
│ 3. Explorar Mapa Juntos             │
│    ↓                                │
│ 4. Enfrentar Inimigos               │
│    ↓                                │
│ 5. Completar Objetivos              │
│    ↓                                │
│ 6. Vitória/Derrota                  │
│    ↓                                │
│ 7. Retornar ao Lobby                │
└─────────────────────────────────────┘
```

### Características Principais

✅ **4-5 jogadores simultâneos** em tempo real
✅ **Sistema de Lobby/Matchmaking** moderno
✅ **Unity 6** (ou Unity 2022.3 LTS)
✅ **New Input System** para controles modernos
✅ **Netcode for GameObjects** (solução oficial)
✅ **Unity Gaming Services** (Lobby + Relay)
✅ **IA Avançada** (GOAP ou Behavior Trees)
✅ **Event Bus** moderno (desacoplado)
✅ **Otimizado** para performance

---

## 📋 Índice

1. [Stack Tecnológico Recomendado](#stack-tecnológico-recomendado)
2. [Configuração Inicial do Projeto](#configuração-inicial-do-projeto)
3. [Sistema de Input Moderno](#sistema-de-input-moderno)
4. [Arquitetura Multiplayer](#arquitetura-multiplayer)
5. [Sistema de Lobby e Matchmaking](#sistema-de-lobby-e-matchmaking)
6. [Player Controller Multiplayer](#player-controller-multiplayer)
7. [Sistema de Inimigos em Rede](#sistema-de-inimigos-em-rede)
8. [IA Avançada para Multiplayer](#ia-avançada-para-multiplayer)
9. [Sincronização de Atmosfera](#sincronização-de-atmosfera)
10. [Sistema de Partidas](#sistema-de-partidas)
11. [Otimizações e Performance](#otimizações-e-performance)
12. [Roadmap de Implementação](#roadmap-de-implementação)

---

## 🛠️ Stack Tecnológico Recomendado

### Unity Version
```
🔥 RECOMENDADO: Unity 2022.3 LTS (mais estável)
⚡ ALTERNATIVA: Unity 6 (mais recente, features novas)

Por quê LTS?
- Mais estável para produção
- Melhor suporte da comunidade
- Menos bugs críticos
- Ideal para multiplayer (confiabilidade)
```

### Packages Essenciais

| Package | Versão | Prioridade | Uso |
|---------|--------|------------|-----|
| **Netcode for GameObjects** | 1.8.0+ | 🔴 Crítico | Networking oficial Unity |
| **Unity Transport** | 2.0+ | 🔴 Crítico | Transporte de rede |
| **Unity Gaming Services** | Latest | 🔴 Crítico | Lobby + Relay + Auth |
| **Input System** | 1.7.0+ | 🔴 Crítico | Controles modernos |
| **Cinemachine** | 2.9+ | 🟡 Importante | Câmeras dinâmicas |
| **Post Processing** | 3.2+ | 🟡 Importante | Efeitos visuais |
| **TextMeshPro** | 3.0+ | 🟡 Importante | UI texto |
| **Multiplayer Tools** | 1.0+ | 🟢 Útil | Debug multiplayer |

### Recursos Externos Recomendados

#### 📺 YouTube Channels
- **[Adam Myhre (@git-amend)](https://youtube.com/@git-amend)**
  - Tutoriais sobre GOAP AI
  - Behavior Trees
  - Event Bus
  - Netcode for GameObjects

- **[Dapper Dino](https://youtube.com/@DapperDinoCodingTutorials)**
  - Netcode tutorials completos
  - Unity Gaming Services

- **[Code Monkey](https://youtube.com/@CodeMonkeyUnity)**
  - Multiplayer patterns
  - Unity best practices

#### 📦 GitHub Repositories (Adam Myhre)
```bash
# IA Avançada
https://github.com/adammyhre/Unity-GOAP
https://github.com/adammyhre/Unity-Behaviour-Trees
https://github.com/adammyhre/Unity-Utility-AI

# Arquitetura
https://github.com/adammyhre/Unity-Event-Bus
https://github.com/adammyhre/Unity-Improved-Timers

# Multiplayer
https://github.com/adammyhre/Unity-Multiplayer-Kart

# Performance
https://github.com/adammyhre/Unity-Octrees
https://github.com/adammyhre/Unity-Batch-Raycasting

# Utils
https://github.com/adammyhre/Unity-Utils
```

---

## 🚀 Configuração Inicial do Projeto

### Passo 1: Criar Novo Projeto

1. **Abrir Unity Hub**
2. **New Project**
3. **Template:** 3D (URP) ou 3D Core
4. **Nome:** HorrorFPSMultiplayer
5. **Unity Version:** 2022.3 LTS

### Passo 2: Instalar Packages

#### Via Package Manager

```
Window > Package Manager > + > Add package by name

1. com.unity.netcode.gameobjects
2. com.unity.transport
3. com.unity.inputsystem
4. com.unity.cinemachine
5. com.unity.render-pipelines.universal (se URP)
```

#### Via Package Manager (Git URL)

```
Window > Package Manager > + > Add package from git URL

1. https://github.com/Unity-Technologies/com.unity.netcode.gameobjects.git
2. https://github.com/Unity-Technologies/multiplayer-community-contributions.git
```

### Passo 3: Configurar Unity Gaming Services

```csharp
// 1. Criar conta Unity Cloud
// 2. Window > General > Services
// 3. Criar novo projeto ou linkar existente
// 4. Habilitar:
//    - Authentication
//    - Lobby
//    - Relay
```

### Passo 4: Project Settings

```
Edit > Project Settings

【Input System Package】
Active Input Handling: Input System Package (New)
ou Both (para compatibilidade)

【Physics】
Fixed Timestep: 0.02 (50Hz)
Default Max Angular Speed: 50

【Quality】
VSync Count: Don't Sync (importante para FPS)
Anti Aliasing: 4x Multi Sampling

【Player】
Allow 'unsafe' Code: ✓ (para performance avançada)
```

### Passo 5: Estrutura de Pastas

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── Managers/
│   │   │   ├── Events/
│   │   │   └── Data/
│   │   ├── Player/
│   │   │   ├── Movement/
│   │   │   ├── Combat/
│   │   │   └── Network/
│   │   ├── Enemy/
│   │   │   ├── AI/
│   │   │   ├── Behaviors/
│   │   │   └── Network/
│   │   ├── Networking/
│   │   │   ├── Lobby/
│   │   │   ├── Relay/
│   │   │   └── Sync/
│   │   ├── Environment/
│   │   ├── UI/
│   │   └── Utilities/
│   ├── Prefabs/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Environment/
│   │   ├── Network/
│   │   └── UI/
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Lobby.unity
│   │   ├── GameLevel01.unity
│   │   └── TestScene.unity
│   ├── Materials/
│   ├── Audio/
│   ├── Input/
│   └── Resources/
└── Settings/
```

---

## 🎮 Sistema de Input Moderno

### Por que New Input System?

✅ **Multiplayer-ready** - Suporta múltiplos devices
✅ **Rebind fácil** - Players podem customizar controles
✅ **Cross-platform** - PC, Console, Mobile
✅ **Actions-based** - Mais flexível que Input.GetKey

### Configuração do Input System

#### 1. Criar Input Actions Asset

```
Assets/_Project/Input/ > Create > Input Actions
Nome: PlayerInputActions
```

#### 2. Configurar Action Maps

```
【Action Maps】
Player
  ├── Move (Value, Vector2)
  ├── Look (Value, Vector2)
  ├── Jump (Button)
  ├── Sprint (Button)
  ├── Crouch (Button)
  ├── Interact (Button)
  ├── Flashlight (Button)
  └── Pause (Button)

UI
  ├── Navigate
  ├── Submit
  └── Cancel
```

#### 3. Script de Input Handler

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorGame.Input
{
    /// <summary>
    /// Handler moderno de input usando New Input System
    /// Multiplayer-ready - cada player tem seu próprio input
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Actions Asset")]
        [SerializeField] private InputActionAsset playerInputActions;

        // Action Maps
        private InputActionMap playerActionMap;
        private InputActionMap uiActionMap;

        // Player Actions
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private InputAction interactAction;
        private InputAction flashlightAction;
        private InputAction pauseAction;

        // Valores atuais
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool CrouchPressed { get; private set; }
        public bool InteractPressed { get; private set; }

        private void Awake()
        {
            SetupInputActions();
        }

        private void SetupInputActions()
        {
            // Get action maps
            playerActionMap = playerInputActions.FindActionMap("Player");
            uiActionMap = playerInputActions.FindActionMap("UI");

            // Get individual actions
            moveAction = playerActionMap.FindAction("Move");
            lookAction = playerActionMap.FindAction("Look");
            jumpAction = playerActionMap.FindAction("Jump");
            sprintAction = playerActionMap.FindAction("Sprint");
            crouchAction = playerActionMap.FindAction("Crouch");
            interactAction = playerActionMap.FindAction("Interact");
            flashlightAction = playerActionMap.FindAction("Flashlight");
            pauseAction = playerActionMap.FindAction("Pause");

            // Subscribe to button events
            jumpAction.performed += ctx => JumpPressed = true;
            crouchAction.performed += ctx => CrouchPressed = true;
            interactAction.performed += ctx => InteractPressed = true;
            flashlightAction.performed += OnFlashlightToggled;
            pauseAction.performed += OnPausePressed;
        }

        private void OnEnable()
        {
            EnablePlayerInput();
        }

        private void OnDisable()
        {
            DisablePlayerInput();
        }

        private void Update()
        {
            // Read continuous input
            MoveInput = moveAction.ReadValue<Vector2>();
            LookInput = lookAction.ReadValue<Vector2>();
            SprintHeld = sprintAction.IsPressed();
        }

        private void LateUpdate()
        {
            // Reset one-frame inputs
            JumpPressed = false;
            CrouchPressed = false;
            InteractPressed = false;
        }

        public void EnablePlayerInput()
        {
            playerActionMap?.Enable();
            uiActionMap?.Disable();
        }

        public void EnableUIInput()
        {
            playerActionMap?.Disable();
            uiActionMap?.Enable();
        }

        public void DisablePlayerInput()
        {
            playerActionMap?.Disable();
        }

        private void OnFlashlightToggled(InputAction.CallbackContext context)
        {
            // Disparar evento
            GameEvents.OnFlashlightToggled?.Invoke();
        }

        private void OnPausePressed(InputAction.CallbackContext context)
        {
            // Disparar evento
            GameEvents.OnPauseRequested?.Invoke();
        }

        /// <summary>
        /// Permite rebinding de teclas
        /// </summary>
        public void RebindAction(string actionName)
        {
            var action = playerActionMap.FindAction(actionName);
            if (action != null)
            {
                // Implementar rebinding UI
                Debug.Log($"Rebinding {actionName}");
            }
        }
    }
}
```

#### 4. Player Input Component (Alternativa Automática)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

namespace HorrorGame.Input
{
    /// <summary>
    /// Alternativa usando PlayerInput component (mais automático)
    /// Melhor para multiplayer local (split-screen)
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputReader : MonoBehaviour
    {
        private PlayerInput playerInput;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Jump { get; private set; }
        public bool Sprint { get; private set; }

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        // Callbacks chamados pelo PlayerInput component
        public void OnMove(InputValue value)
        {
            Move = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            Look = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            Jump = value.isPressed;
        }

        public void OnSprint(InputValue value)
        {
            Sprint = value.isPressed;
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                GameEvents.OnInteractPressed?.Invoke(gameObject);
            }
        }

        public void OnFlashlight(InputValue value)
        {
            if (value.isPressed)
            {
                GameEvents.OnFlashlightToggled?.Invoke();
            }
        }

        public void OnPause(InputValue value)
        {
            if (value.isPressed)
            {
                GameEvents.OnPauseRequested?.Invoke();
            }
        }
    }
}
```

---

## 🌐 Arquitetura Multiplayer

### Modelo: Listen Server (Host-Client)

```
┌─────────────────────────────────────────┐
│ Host (Player 1)                         │
│  ├── Simula o jogo (servidor)           │
│  ├── Joga como cliente                  │
│  ├── Autoridade sobre:                  │
│  │   ├── Inimigos (IA)                  │
│  │   ├── Validação de ações             │
│  │   ├── Spawning de items              │
│  │   └── Estado da partida              │
│  └── Sincroniza para clientes           │
└─────────────────────────────────────────┘
           ↓ ↓ ↓
┌──────────┐ ┌──────────┐ ┌──────────┐
│ Client 1 │ │ Client 2 │ │ Client 3 │
│ Player 2 │ │ Player 3 │ │ Player 4 │
└──────────┘ └──────────┘ └──────────┘
```

### Vantagens
- ✅ Mais simples de implementar
- ✅ Sem custo de servidor dedicado
- ✅ Ideal para 4-5 jogadores
- ✅ Funciona com Relay (não precisa port forwarding)

### Desvantagens
- ❌ Host tem vantagem (0ms latência)
- ❌ Se host sai, jogo acaba
- ❌ Host precisa de boa conexão

### Alternativa: Dedicated Server
```
Usar quando:
- Mais de 8 jogadores
- Competitivo (precisa ser justo)
- Jogo comercial grande
```

---

## 🎯 Sistema de Lobby e Matchmaking

### Arquitetura do Sistema

```
Flow do Lobby
┌────────────┐
│ Main Menu  │
└─────┬──────┘
      ↓
┌────────────┐
│ Auth Login │ ← Unity Authentication
└─────┬──────┘
      ↓
┌────────────────────┐
│ Lobby Browser      │
│ ├── Criar Lobby    │
│ ├── Listar Lobbies │
│ └── Entrar Lobby   │
└─────┬──────────────┘
      ↓
┌────────────────────┐
│ Lobby Room         │
│ ├── Lista Players  │
│ ├── Chat           │
│ ├── Ready System   │
│ └── Start Game     │
└─────┬──────────────┘
      ↓
┌────────────────────┐
│ Game (via Relay)   │
└────────────────────┘
```

### Implementação Completa

#### 1. Authentication Service

```csharp
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;

namespace HorrorGame.Networking
{
    /// <summary>
    /// Gerencia autenticação com Unity Gaming Services
    /// </summary>
    public class AuthenticationManager : MonoBehaviour
    {
        public static AuthenticationManager Instance { get; private set; }

        public bool IsAuthenticated => AuthenticationService.Instance.IsSignedIn;
        public string PlayerId => AuthenticationService.Instance.PlayerId;
        public string PlayerName { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            await InitializeUnityServices();
        }

        private async Task InitializeUnityServices()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("[Auth] Unity Services inicializados");

                await SignInAnonymously();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Auth] Erro ao inicializar: {e.Message}");
            }
        }

        public async Task SignInAnonymously()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                PlayerName = $"Player_{Random.Range(1000, 9999)}";

                Debug.Log($"[Auth] Logado como: {PlayerName} (ID: {PlayerId})");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Auth] Erro no login: {e.Message}");
            }
        }

        public void SetPlayerName(string name)
        {
            PlayerName = name;
        }
    }
}
```

#### 2. Lobby Manager

```csharp
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HorrorGame.Networking
{
    /// <summary>
    /// Gerencia lobbies usando Unity Lobby Service
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        [Header("Configuração")]
        [SerializeField] private int maxPlayers = 5;
        [SerializeField] private bool isPrivate = false;

        private Lobby currentLobby;
        private float heartbeatTimer;
        private float lobbyUpdateTimer;

        // Events
        public event System.Action<Lobby> OnLobbyCreated;
        public event System.Action<Lobby> OnLobbyJoined;
        public event System.Action<List<Lobby>> OnLobbyListUpdated;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            HandleLobbyHeartbeat();
            HandleLobbyPolling();
        }

        /// <summary>
        /// Cria um novo lobby
        /// </summary>
        public async Task<Lobby> CreateLobby(string lobbyName)
        {
            try
            {
                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = GetPlayer(),
                    Data = new Dictionary<string, DataObject>
                    {
                        { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Horror") },
                        { "Map", new DataObject(DataObject.VisibilityOptions.Public, "Level01") }
                    }
                };

                currentLobby = await LobbyService.Instance.CreateLobbyAsync(
                    lobbyName,
                    maxPlayers,
                    options
                );

                Debug.Log($"[Lobby] Criado: {currentLobby.Name} (ID: {currentLobby.Id})");
                Debug.Log($"[Lobby] Código: {currentLobby.LobbyCode}");

                OnLobbyCreated?.Invoke(currentLobby);
                return currentLobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[Lobby] Erro ao criar: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lista lobbies disponíveis
        /// </summary>
        public async Task<List<Lobby>> ListLobbies()
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions
                {
                    Count = 25,
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.AvailableSlots,
                            op: QueryFilter.OpOptions.GT,
                            value: "0"
                        )
                    },
                    Order = new List<QueryOrder>
                    {
                        new QueryOrder(
                            asc: false,
                            field: QueryOrder.FieldOptions.Created
                        )
                    }
                };

                QueryResponse response = await Lobbies.Instance.QueryLobbiesAsync(options);

                Debug.Log($"[Lobby] Encontrados {response.Results.Count} lobbies");

                OnLobbyListUpdated?.Invoke(response.Results);
                return response.Results;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[Lobby] Erro ao listar: {e.Message}");
                return new List<Lobby>();
            }
        }

        /// <summary>
        /// Entra em um lobby por código
        /// </summary>
        public async Task<Lobby> JoinLobbyByCode(string lobbyCode)
        {
            try
            {
                JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
                {
                    Player = GetPlayer()
                };

                currentLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, options);

                Debug.Log($"[Lobby] Entrou: {currentLobby.Name}");

                OnLobbyJoined?.Invoke(currentLobby);
                return currentLobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[Lobby] Erro ao entrar: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Entra em um lobby por ID
        /// </summary>
        public async Task<Lobby> JoinLobbyById(string lobbyId)
        {
            try
            {
                JoinLobbyByIdOptions options = new JoinLobbyByIdOptions
                {
                    Player = GetPlayer()
                };

                currentLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId, options);

                Debug.Log($"[Lobby] Entrou: {currentLobby.Name}");

                OnLobbyJoined?.Invoke(currentLobby);
                return currentLobby;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[Lobby] Erro ao entrar: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sai do lobby atual
        /// </summary>
        public async Task LeaveLobby()
        {
            if (currentLobby == null) return;

            try
            {
                await LobbyService.Instance.RemovePlayerAsync(
                    currentLobby.Id,
                    AuthenticationManager.Instance.PlayerId
                );

                Debug.Log("[Lobby] Saiu do lobby");
                currentLobby = null;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[Lobby] Erro ao sair: {e.Message}");
            }
        }

        /// <summary>
        /// Atualiza dados do player no lobby
        /// </summary>
        public async Task UpdatePlayerData(string key, string value)
        {
            if (currentLobby == null) return;

            try
            {
                UpdatePlayerOptions options = new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { key, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, value) }
                    }
                };

                currentLobby = await LobbyService.Instance.UpdatePlayerAsync(
                    currentLobby.Id,
                    AuthenticationManager.Instance.PlayerId,
                    options
                );

                Debug.Log($"[Lobby] Player data atualizado: {key} = {value}");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[Lobby] Erro ao atualizar player: {e.Message}");
            }
        }

        /// <summary>
        /// Sistema de "ready" para players
        /// </summary>
        public async Task SetPlayerReady(bool ready)
        {
            await UpdatePlayerData("Ready", ready.ToString());
        }

        /// <summary>
        /// Verifica se todos players estão prontos
        /// </summary>
        public bool AllPlayersReady()
        {
            if (currentLobby == null) return false;

            foreach (var player in currentLobby.Players)
            {
                if (player.Data.TryGetValue("Ready", out var readyData))
                {
                    if (!bool.Parse(readyData.Value))
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Heartbeat para manter lobby ativo
        /// </summary>
        private async void HandleLobbyHeartbeat()
        {
            if (currentLobby == null) return;
            if (!IsLobbyHost()) return;

            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = 15f; // Enviar heartbeat a cada 15s

                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"[Lobby] Erro no heartbeat: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Poll de atualização do lobby
        /// </summary>
        private async void HandleLobbyPolling()
        {
            if (currentLobby == null) return;

            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                lobbyUpdateTimer = 1.1f; // Atualizar a cada 1.1s

                try
                {
                    currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"[Lobby] Erro ao atualizar: {e.Message}");
                }
            }
        }

        private Player GetPlayer()
        {
            return new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationManager.Instance.PlayerName) },
                    { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "false") }
                }
            };
        }

        private bool IsLobbyHost()
        {
            return currentLobby != null && currentLobby.HostId == AuthenticationManager.Instance.PlayerId;
        }

        public Lobby GetCurrentLobby() => currentLobby;
        public bool IsInLobby() => currentLobby != null;
        public int GetPlayerCount() => currentLobby?.Players?.Count ?? 0;
    }
}
```

#### 3. Relay Manager (para não precisar port forwarding)

```csharp
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using System.Threading.Tasks;

namespace HorrorGame.Networking
{
    /// <summary>
    /// Gerencia Unity Relay para não precisar abrir portas
    /// </summary>
    public class RelayManager : MonoBehaviour
    {
        public static RelayManager Instance { get; private set; }

        [SerializeField] private int maxConnections = 5;

        private string joinCode;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Cria um relay allocation (Host)
        /// </summary>
        public async Task<string> CreateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                Debug.Log($"[Relay] Criado com código: {joinCode}");

                // Configurar transport
                var relayServerData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"[Relay] Erro ao criar: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Conecta a um relay existente (Client)
        /// </summary>
        public async Task<bool> JoinRelay(string joinCode)
        {
            try
            {
                Debug.Log($"[Relay] Tentando entrar com código: {joinCode}");

                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                // Configurar transport
                var relayServerData = new RelayServerData(allocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                Debug.Log("[Relay] Conectado com sucesso");
                return true;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError($"[Relay] Erro ao entrar: {e.Message}");
                return false;
            }
        }

        public string GetJoinCode() => joinCode;
    }
}
```

---

## 🎮 (Continua na Parte 2...)

**Este guia continua com:**
- Player Controller Multiplayer
- Sistema de Inimigos em Rede
- IA Avançada (GOAP/Behavior Trees)
- Sistema de Partidas
- Otimizações
- Roadmap Completo

---

## 📚 Recursos e Referências

### Documentação Oficial
- [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [Unity Gaming Services](https://docs.unity.com/ugs/en-us/manual/overview/manual/unity-gaming-services-home)
- [New Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)

### Canais Recomendados
- **Adam Myhre** - IA Avançada e Arquitetura
- **Dapper Dino** - Netcode Tutorials
- **Code Monkey** - Unity Best Practices

### GitHub Essenciais
- Unity-GOAP (IA inteligente)
- Unity-Event-Bus (arquitetura)
- Unity-Multiplayer-Kart (referência networking)

---

**Próximo passo:** Continue lendo a Parte 2 deste guia ou consulte os documentos específicos:
- `MULTIPLAYER_ARCHITECTURE.md` - Arquitetura detalhada
- `ADVANCED_TECHNOLOGIES.md` - IA e otimizações
- `COOP_ROADMAP.md` - Roadmap de implementação
