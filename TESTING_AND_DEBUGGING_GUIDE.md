# 🧪 Guia de Testing e Debugging Multiplayer

## 🎯 Objetivo

Guia completo para testar e debugar jogos multiplayer com Netcode for GameObjects, especialmente para 4-5 jogadores.

---

## 📋 Índice

1. [Testing Multiplayer Localmente](#testing-multiplayer-localmente)
2. [Ferramentas de Debugging](#ferramentas-de-debugging)
3. [Simulação de Latência](#simulação-de-latência)
4. [Profiling e Performance](#profiling-e-performance)
5. [Problemas Comuns](#problemas-comuns)

---

## 🧪 Testing Multiplayer Localmente

### Problema: Como testar com 4-5 players sem ter múltiplos computadores?

### Solução 1: ParrelSync (⭐ RECOMENDADO)

**O que é:** Cria clones do projeto Unity que rodam simultaneamente

**Instalação:**
```
1. Window > Package Manager
2. + > Add package from git URL
3. https://github.com/VeriorPies/ParrelSync.git
```

**Uso:**
```
1. ParrelSync > Clones Manager
2. Create New Clone
3. Criar 2-3 clones
4. Open in New Editor (cada clone)
5. Testar multiplayer!
```

**Vantagens:**
- ✅ Rápido para testar
- ✅ Não precisa fazer build
- ✅ Pode debugar vários players ao mesmo tempo
- ✅ Mudanças de código aparecem em todos os clones

**Desvantagens:**
- ❌ Consome muita RAM (recomendado 16GB+)
- ❌ Pode ser lento dependendo do PC

---

### Solução 2: Builds Múltiplos

**Uso:**
```
1. File > Build Settings
2. Build do projeto
3. Executar build 2-3 vezes
4. + 1 Editor instance
5. Testar multiplayer
```

**Vantagens:**
- ✅ Mais leve que ParrelSync
- ✅ Testa build real
- ✅ Mais próximo do produto final

**Desvantagens:**
- ❌ Lento para iterar (rebuild a cada mudança)
- ❌ Mais difícil de debugar

---

### Solução 3: Multiplayer Play Mode (Unity 6+)

**O que é:** Recurso nativo do Unity 6 para testar multiplayer

**Uso:**
```
1. Window > Multiplayer Play Mode
2. Configurar número de players
3. Play
```

**Vantagens:**
- ✅ Nativo do Unity 6
- ✅ Otimizado
- ✅ Fácil de usar

**Desvantagens:**
- ❌ Apenas Unity 6+ (não funciona em 2022.3)

---

## 🔧 Ferramentas de Debugging

### 1. Multiplayer Tools Package

**Instalação:**
```
Window > Package Manager > Add package by name
com.unity.multiplayer.tools
```

**Ferramentas Incluídas:**

#### A) Runtime Net Stats Monitor

```csharp
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Multiplayer.Tools.NetStatsMonitor;

public class NetworkDebugger : MonoBehaviour
{
    private void Start()
    {
        // Mostrar stats em runtime
        var netStatsMonitor = gameObject.AddComponent<RuntimeNetStatsMonitor>();
    }
}
```

**Mostra:**
- Packets enviados/recebidos
- Bandwidth usage
- RTT (Round Trip Time)
- Packet loss

---

#### B) Network Profiler

**Uso:**
```
1. Window > Multiplayer > Netcode Profiler
2. Play
3. Analisar mensagens de rede
```

**Mostra:**
- RPCs enviados/recebidos
- NetworkVariable updates
- Spawn/Despawn events
- Bandwidth por objeto

---

#### C) Network Simulator (Simular Lag)

```csharp
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkDebugger : MonoBehaviour
{
    private void Start()
    {
        var networkManager = NetworkManager.Singleton;
        var transport = networkManager.GetComponent<UnityTransport>();

        // Simular latência
        transport.DebugSimulator.PacketDelayMS = 100;      // 100ms de lag
        transport.DebugSimulator.PacketJitterMS = 20;      // ±20ms variação
        transport.DebugSimulator.PacketDropRate = 2;       // 2% packet loss

        Debug.Log("[NetworkDebug] Simulação de lag ativada: 100ms ±20ms, 2% loss");
    }
}
```

---

### 2. Custom Debug Logger

```csharp
using Unity.Netcode;
using UnityEngine;

public static class NetworkLogger
{
    private static bool enableLogs = true;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogServer(string message)
    {
        if (!enableLogs) return;
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log($"<color=cyan>[SERVER]</color> {message}");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogClient(string message)
    {
        if (!enableLogs) return;
        if (NetworkManager.Singleton.IsServer) return;

        Debug.Log($"<color=yellow>[CLIENT]</color> {message}");
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogRpc(string rpcName, string sender, string receiver)
    {
        if (!enableLogs) return;

        Debug.Log($"<color=magenta>[RPC]</color> {rpcName}: {sender} → {receiver}");
    }

    public static void EnableLogs(bool enable)
    {
        enableLogs = enable;
    }
}

// Uso:
NetworkLogger.LogServer("Player conectado");
NetworkLogger.LogClient("Recebeu spawn");
NetworkLogger.LogRpc("TakeDamageServerRpc", "Client 1", "Server");
```

---

### 3. Network Debug UI

```csharp
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class NetworkDebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private bool showDebug = true;

    private void Update()
    {
        if (!showDebug || debugText == null) return;

        if (NetworkManager.Singleton == null)
        {
            debugText.text = "Network Manager: NULL";
            return;
        }

        var nm = NetworkManager.Singleton;
        var transport = nm.GetComponent<UnityTransport>();

        string text = $"<b>NETWORK DEBUG</b>\n\n";

        // Status
        text += $"Status: {(nm.IsServer ? "<color=cyan>SERVER</color>" : "<color=yellow>CLIENT</color>")}\n";
        text += $"Connected: {nm.IsConnectedClient}\n";
        text += $"Client ID: {nm.LocalClientId}\n\n";

        // Players
        text += $"<b>Players: {nm.ConnectedClients.Count}</b>\n";
        foreach (var client in nm.ConnectedClients)
        {
            text += $"  - Client {client.Key}: {(client.Value.PlayerObject != null ? "✓" : "✗")}\n";
        }
        text += "\n";

        // Stats
        if (transport != null)
        {
            text += $"<b>Network Stats</b>\n";
            text += $"RTT: {transport.GetCurrentRtt()}ms\n";
        }

        debugText.text = text;
    }

    private void OnGUI()
    {
        if (!showDebug) return;

        // Botões de debug
        GUILayout.BeginArea(new Rect(10, 10, 200, 300));

        if (NetworkManager.Singleton != null)
        {
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                if (GUILayout.Button("Start Host"))
                {
                    NetworkManager.Singleton.StartHost();
                }

                if (GUILayout.Button("Start Client"))
                {
                    NetworkManager.Singleton.StartClient();
                }
            }
            else
            {
                if (GUILayout.Button("Disconnect"))
                {
                    NetworkManager.Singleton.Shutdown();
                }
            }

            // Toggle lag simulation
            if (GUILayout.Button("Toggle Lag Sim"))
            {
                ToggleLagSimulation();
            }
        }

        GUILayout.EndArea();
    }

    private void ToggleLagSimulation()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null) return;

        if (transport.DebugSimulator.PacketDelayMS > 0)
        {
            // Desligar
            transport.DebugSimulator.PacketDelayMS = 0;
            transport.DebugSimulator.PacketJitterMS = 0;
            transport.DebugSimulator.PacketDropRate = 0;
            Debug.Log("[Debug] Lag simulation OFF");
        }
        else
        {
            // Ligar
            transport.DebugSimulator.PacketDelayMS = 100;
            transport.DebugSimulator.PacketJitterMS = 20;
            transport.DebugSimulator.PacketDropRate = 2;
            Debug.Log("[Debug] Lag simulation ON");
        }
    }
}
```

---

## 🌐 Simulação de Latência

### Por que simular lag?

- ✅ Testar como jogo se comporta com latência real
- ✅ Identificar bugs de sincronização
- ✅ Testar interpolação
- ✅ Validar que anti-cheat funciona com lag

### Presets de Latência

```csharp
public enum LatencyPreset
{
    Perfect,      // 0ms
    Local,        // 10ms
    Good,         // 50ms
    Average,      // 100ms
    Poor,         // 200ms
    VeryPoor,     // 400ms
    Unplayable    // 800ms
}

public class NetworkLatencySimulator : MonoBehaviour
{
    [SerializeField] private LatencyPreset preset = LatencyPreset.Average;

    private void Start()
    {
        ApplyPreset(preset);
    }

    public void ApplyPreset(LatencyPreset preset)
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null) return;

        switch (preset)
        {
            case LatencyPreset.Perfect:
                transport.DebugSimulator.PacketDelayMS = 0;
                transport.DebugSimulator.PacketJitterMS = 0;
                transport.DebugSimulator.PacketDropRate = 0;
                break;

            case LatencyPreset.Local:
                transport.DebugSimulator.PacketDelayMS = 10;
                transport.DebugSimulator.PacketJitterMS = 2;
                transport.DebugSimulator.PacketDropRate = 0;
                break;

            case LatencyPreset.Good:
                transport.DebugSimulator.PacketDelayMS = 50;
                transport.DebugSimulator.PacketJitterMS = 10;
                transport.DebugSimulator.PacketDropRate = 0.5f;
                break;

            case LatencyPreset.Average:
                transport.DebugSimulator.PacketDelayMS = 100;
                transport.DebugSimulator.PacketJitterMS = 20;
                transport.DebugSimulator.PacketDropRate = 1.0f;
                break;

            case LatencyPreset.Poor:
                transport.DebugSimulator.PacketDelayMS = 200;
                transport.DebugSimulator.PacketJitterMS = 40;
                transport.DebugSimulator.PacketDropRate = 3.0f;
                break;

            case LatencyPreset.VeryPoor:
                transport.DebugSimulator.PacketDelayMS = 400;
                transport.DebugSimulator.PacketJitterMS = 80;
                transport.DebugSimulator.PacketDropRate = 5.0f;
                break;

            case LatencyPreset.Unplayable:
                transport.DebugSimulator.PacketDelayMS = 800;
                transport.DebugSimulator.PacketJitterMS = 160;
                transport.DebugSimulator.PacketDropRate = 10.0f;
                break;
        }

        Debug.Log($"[LatencySim] Preset aplicado: {preset}");
    }
}
```

---

## 📊 Profiling e Performance

### 1. Unity Profiler

```
Window > Analysis > Profiler
```

**Focar em:**
- **Networking** - Bandwidth usage
- **Rendering** - GPU performance
- **Scripts** - CPU hotspots
- **Memory** - Memory leaks

### 2. Bandwidth Monitoring

```csharp
public class BandwidthMonitor : MonoBehaviour
{
    private ulong lastBytesSent = 0;
    private ulong lastBytesReceived = 0;
    private float updateInterval = 1.0f;
    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            LogBandwidth();
            timer = 0f;
        }
    }

    private void LogBandwidth()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;

        var transport = nm.GetComponent<UnityTransport>();
        if (transport == null) return;

        // Obter stats (você precisará implementar isso)
        ulong bytesSent = GetBytesSent();
        ulong bytesReceived = GetBytesReceived();

        ulong sentDelta = bytesSent - lastBytesSent;
        ulong receivedDelta = bytesReceived - lastBytesReceived;

        float sentKBps = (sentDelta / 1024f) / updateInterval;
        float receivedKBps = (receivedDelta / 1024f) / updateInterval;

        Debug.Log($"[Bandwidth] ↑ {sentKBps:F2} KB/s | ↓ {receivedKBps:F2} KB/s");

        lastBytesSent = bytesSent;
        lastBytesReceived = bytesReceived;
    }

    private ulong GetBytesSent()
    {
        // Implementar usando NetworkManager metrics
        return 0;
    }

    private ulong GetBytesReceived()
    {
        // Implementar usando NetworkManager metrics
        return 0;
    }
}
```

---

## 🐛 Problemas Comuns

### Problema 1: Players não spawnam

**Sintomas:**
- NetworkManager conecta
- Mas players não aparecem

**Solução:**
```csharp
// Verificar:
// 1. Player prefab está no NetworkManager
NetworkManager.Singleton.NetworkConfig.PlayerPrefab != null

// 2. Player prefab tem NetworkObject component
GetComponent<NetworkObject>() != null

// 3. Spawn é chamado pelo servidor
if (NetworkManager.Singleton.IsServer)
{
    var player = Instantiate(playerPrefab);
    player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
}
```

---

### Problema 2: Sincronização atrasada

**Sintomas:**
- Players se movem com lag visual
- Posições desincronizadas

**Solução:**
```csharp
// Usar NetworkTransform com configuração correta
[SerializeField] private NetworkTransform networkTransform;

// Configurar no Inspector:
// - Interpolate: true
// - Sync Position: true
// - Sync Rotation: true
// - Position Threshold: 0.001
```

---

### Problema 3: RPCs não chegam

**Sintomas:**
- ServerRpc não executa
- ClientRpc não chega

**Diagnóstico:**
```csharp
[ServerRpc]
private void TestServerRpc()
{
    Debug.Log($"[ServerRpc] Recebido! IsServer: {IsServer}");
}

[ClientRpc]
private void TestClientRpc()
{
    Debug.Log($"[ClientRpc] Recebido! ClientId: {NetworkManager.LocalClientId}");
}

// Chamar no cliente:
if (IsOwner)
{
    Debug.Log("Enviando ServerRpc...");
    TestServerRpc();
}
```

**Soluções comuns:**
- ✅ Verificar que objeto tem NetworkObject
- ✅ Verificar que objeto está spawned
- ✅ Verificar ownership (se RequireOwnership = true)

---

### Problema 4: Memory Leak

**Sintomas:**
- RAM aumenta constantemente
- Garbage Collection frequente

**Diagnóstico:**
```csharp
// Memory Profiler
Window > Analysis > Memory Profiler
Take Snapshot (antes e depois de jogar)
Compare snapshots
```

**Causas comuns:**
- ❌ NetworkVariable events não limpos
- ❌ NetworkManager callbacks não removidos
- ❌ Objetos não despawnados

**Solução:**
```csharp
public override void OnNetworkDespawn()
{
    // SEMPRE limpar eventos
    networkVariable.OnValueChanged -= OnValueChanged;
    NetworkManager.OnClientConnected -= OnClientConnected;

    base.OnNetworkDespawn();
}
```

---

## 📋 Checklist de Testing

### Antes de Cada Build

- [ ] Testar com 2 players
- [ ] Testar com 5 players (máximo)
- [ ] Testar com lag simulado (100ms)
- [ ] Testar disconnect/reconnect
- [ ] Testar morte de host
- [ ] Verificar memory leaks
- [ ] Verificar bandwidth usage < 50 KB/s por player

### Testes de Stress

- [ ] 30 minutos de gameplay contínuo
- [ ] Spawn/Despawn de 100+ objetos
- [ ] Todos players atacando simultaneamente
- [ ] Lag extremo (400ms+)

---

## 🎯 Métricas Alvo (4-5 Jogadores)

### Performance
- **FPS:** 60+ (local), 30+ (remote)
- **Bandwidth:** < 50 KB/s por player
- **Latência:** Jogável até 200ms
- **Memory:** < 500 MB total

### Rede
- **RTT:** < 100ms ideal, < 200ms aceitável
- **Packet Loss:** < 1% ideal, < 5% aceitável
- **Update Rate:** 10-20 Hz (50-100ms)

---

## 🛠️ Ferramentas Externas

### 1. Wireshark (Análise de Pacotes)
- Monitorar tráfego de rede real
- Identificar gargalos

### 2. Clumsy (Windows - Simular Lag)
- Simular condições de rede ruins
- Mais realista que simulador interno

### 3. NetLimiter (Windows)
- Limitar bandwidth
- Simular conexões lentas

---

## 📚 Recursos

### Documentação
- [Netcode Testing Guide](https://docs-multiplayer.unity3d.com/netcode/current/learn/testing/testing_locally/)
- [Multiplayer Tools Package](https://docs-multiplayer.unity3d.com/tools/current/about/)

### Vídeos
- [Dapper Dino - Multiplayer Testing](https://youtube.com/@DapperDinoCodingTutorials)
- [Code Monkey - Netcode Debugging](https://youtube.com/@CodeMonkeyUnity)

---

**Data:** 2025-11-28
**Versão:** 1.0
**Para:** Unity 2022.3 LTS + Netcode for GameObjects 1.8.0+
