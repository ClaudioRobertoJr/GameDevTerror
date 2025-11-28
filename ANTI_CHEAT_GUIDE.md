# 🛡️ Guia de Anti-Cheat para Multiplayer

## 🎯 Objetivo

Este guia fornece técnicas essenciais de anti-cheat para jogos multiplayer com Netcode for GameObjects, especialmente para jogos de 4-5 jogadores usando arquitetura Listen Server.

---

## 📋 Índice

1. [Princípios Fundamentais](#princípios-fundamentais)
2. [Server Authority](#server-authority)
3. [Validações Essenciais](#validações-essenciais)
4. [Exemplos Práticos](#exemplos-práticos)
5. [Checklist de Segurança](#checklist-de-segurança)

---

## 🎓 Princípios Fundamentais

### Regra de Ouro: NUNCA CONFIE NO CLIENTE

```csharp
// ❌ ERRADO - Cliente decide
public void TakeDamage(int damage)
{
    health -= damage; // Cliente pode modificar!
}

// ✅ CORRETO - Servidor valida e decide
[ServerRpc(RequireOwnership = false)]
public void TakeDamageServerRpc(int damage, ulong attackerId)
{
    // Servidor valida TUDO antes de aplicar
    if (ValidateDamage(damage, attackerId))
    {
        health -= damage;
    }
}
```

### Arquitetura Segura

```
┌─────────────────────────────────────┐
│ CLIENTE                             │
│  ├── Envia INPUT                    │
│  ├── Envia REQUESTS                 │
│  └── Recebe RESULTADOS              │
└────────────┬────────────────────────┘
             ↓
┌─────────────────────────────────────┐
│ SERVIDOR (AUTORIDADE)               │
│  ├── VALIDA todos os inputs         │
│  ├── DECIDE todas as ações          │
│  ├── CALCULA dano, movimento, etc   │
│  └── SINCRONIZA resultados          │
└─────────────────────────────────────┘
```

---

## 🏛️ Server Authority

### 1. NetworkVariables - Apenas Servidor Modifica

```csharp
public class NetworkPlayerController : NetworkBehaviour
{
    // ✅ NetworkVariable só pode ser modificada pelo servidor
    private NetworkVariable<int> health = new NetworkVariable<int>(100);
    private NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();

    // ❌ ERRADO - Cliente modificando diretamente
    void Update()
    {
        if (IsOwner)
        {
            health.Value = 100; // ❌ ERRO! Apenas servidor pode modificar
        }
    }

    // ✅ CORRETO - Cliente pede, servidor valida e modifica
    public void Heal(int amount)
    {
        HealServerRpc(amount);
    }

    [ServerRpc]
    private void HealServerRpc(int amount)
    {
        // Validar
        if (amount <= 0 || amount > 50) return;

        // Servidor modifica
        health.Value = Mathf.Min(100, health.Value + amount);
    }
}
```

### 2. ServerRpc - Sempre Validar

```csharp
// ✅ Modelo de validação completa
[ServerRpc(RequireOwnership = false)]
private void UseItemServerRpc(int itemId, ulong userId, ServerRpcParams rpcParams = default)
{
    // 1. Validar que request veio do cliente correto
    if (rpcParams.Receive.SenderClientId != userId)
    {
        Debug.LogWarning($"[Anti-Cheat] Client ID mismatch!");
        return;
    }

    // 2. Validar que usuário existe
    if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(userId))
    {
        Debug.LogWarning($"[Anti-Cheat] User {userId} não existe!");
        return;
    }

    // 3. Validar que usuário tem o item
    var player = NetworkManager.Singleton.ConnectedClients[userId].PlayerObject;
    var inventory = player.GetComponent<Inventory>();

    if (!inventory.HasItem(itemId))
    {
        Debug.LogWarning($"[Anti-Cheat] User {userId} não tem item {itemId}!");
        return;
    }

    // 4. Validar limites do item
    if (itemId < 0 || itemId > 1000)
    {
        Debug.LogWarning($"[Anti-Cheat] Item ID inválido: {itemId}");
        return;
    }

    // 5. Processar ação
    inventory.UseItem(itemId);
}
```

---

## ✅ Validações Essenciais

### 1. Validação de Distância (Crítico!)

```csharp
/// <summary>
/// Valida que atacante está próximo o suficiente da vítima
/// Previne ataques à distância impossíveis
/// </summary>
[ServerRpc(RequireOwnership = false)]
private void AttackServerRpc(ulong victimId, ServerRpcParams rpcParams = default)
{
    ulong attackerId = rpcParams.Receive.SenderClientId;

    // Obter posições
    var attacker = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject;
    var victim = NetworkManager.Singleton.ConnectedClients[victimId].PlayerObject;

    if (attacker == null || victim == null) return;

    // ✅ Validar distância usando sqrMagnitude (mais rápido)
    float sqrDistance = (attacker.transform.position - victim.transform.position).sqrMagnitude;
    float maxAttackRangeSqr = 2.5f * 2.5f; // 2.5m * 2.5m = 6.25

    if (sqrDistance > maxAttackRangeSqr)
    {
        float distance = Mathf.Sqrt(sqrDistance);
        Debug.LogWarning($"[Anti-Cheat] Ataque de distância inválida: {distance}m (max: 2.5m)");
        return;
    }

    // ✅ Validação passou, aplicar dano
    var victimHealth = victim.GetComponent<NetworkPlayerController>();
    victimHealth.TakeDamageServerRpc(10, attackerId);
}
```

### 2. Validação de Valores

```csharp
[ServerRpc(RequireOwnership = false)]
private void ApplyDamageServerRpc(int damage, ServerRpcParams rpcParams = default)
{
    // ✅ Validar range de valores
    if (damage < 0)
    {
        Debug.LogWarning($"[Anti-Cheat] Dano negativo: {damage}");
        return;
    }

    if (damage > 100)
    {
        Debug.LogWarning($"[Anti-Cheat] Dano muito alto: {damage} (max: 100)");
        return;
    }

    // ✅ Validar que jogador não está morto
    if (health.Value <= 0)
    {
        Debug.LogWarning($"[Anti-Cheat] Tentativa de atacar jogador morto");
        return;
    }

    // Aplicar dano
    health.Value -= damage;
}
```

### 3. Validação de Tempo (Cooldowns)

```csharp
public class WeaponController : NetworkBehaviour
{
    [SerializeField] private float fireRate = 0.5f; // 2 tiros por segundo
    private float lastFireTime;

    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        // ✅ Validar cooldown (previne rate-of-fire hacks)
        float timeSinceLastShot = Time.time - lastFireTime;

        if (timeSinceLastShot < fireRate)
        {
            Debug.LogWarning($"[Anti-Cheat] Disparo muito rápido: {timeSinceLastShot}s (min: {fireRate}s)");
            return;
        }

        // Atualizar timestamp
        lastFireTime = Time.time;

        // Processar disparo
        ProcessShot();
    }
}
```

### 4. Validação de Recursos (Munição, Stamina)

```csharp
[ServerRpc(RequireOwnership = false)]
private void SprintServerRpc(bool isSprinting, ServerRpcParams rpcParams = default)
{
    ulong clientId = rpcParams.Receive.SenderClientId;
    var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
    var stamina = player.GetComponent<StaminaSystem>();

    // ✅ Validar que tem stamina suficiente
    if (isSprinting && stamina.GetStamina() <= 0)
    {
        Debug.LogWarning($"[Anti-Cheat] Tentativa de correr sem stamina");
        return;
    }

    // Permitir ação
    SetSprintingClientRpc(isSprinting);
}
```

---

## 💡 Exemplos Práticos

### Exemplo 1: Sistema de Dano Completo

```csharp
using Unity.Netcode;
using UnityEngine;

public class NetworkCombatSystem : NetworkBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float attackCooldown = 1.0f;

    private NetworkVariable<int> health = new NetworkVariable<int>(100);
    private float lastAttackTime;

    /// <summary>
    /// Cliente solicita ataque
    /// </summary>
    public void RequestAttack(ulong targetId)
    {
        if (!IsOwner) return;
        AttackServerRpc(targetId);
    }

    /// <summary>
    /// ✅ Servidor valida e executa ataque
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void AttackServerRpc(ulong targetId, ServerRpcParams rpcParams = default)
    {
        ulong attackerId = rpcParams.Receive.SenderClientId;

        // VALIDAÇÃO 1: Cooldown
        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.LogWarning($"[Anti-Cheat] Ataque em cooldown");
            return;
        }

        // VALIDAÇÃO 2: Atacante existe
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(attackerId))
        {
            Debug.LogWarning($"[Anti-Cheat] Atacante {attackerId} não existe");
            return;
        }

        // VALIDAÇÃO 3: Vítima existe
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(targetId))
        {
            Debug.LogWarning($"[Anti-Cheat] Vítima {targetId} não existe");
            return;
        }

        var attacker = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject;
        var target = NetworkManager.Singleton.ConnectedClients[targetId].PlayerObject;

        if (attacker == null || target == null) return;

        // VALIDAÇÃO 4: Distância
        float sqrDistance = (attacker.transform.position - target.transform.position).sqrMagnitude;
        float maxRangeSqr = attackRange * attackRange;

        if (sqrDistance > maxRangeSqr)
        {
            Debug.LogWarning($"[Anti-Cheat] Ataque de distância inválida: {Mathf.Sqrt(sqrDistance)}m");
            return;
        }

        // VALIDAÇÃO 5: Vítima está viva
        var targetHealth = target.GetComponent<NetworkCombatSystem>();
        if (targetHealth == null || targetHealth.health.Value <= 0)
        {
            Debug.LogWarning($"[Anti-Cheat] Tentativa de atacar alvo morto");
            return;
        }

        // ✅ TODAS VALIDAÇÕES PASSARAM

        // Atualizar cooldown
        lastAttackTime = Time.time;

        // Aplicar dano
        targetHealth.TakeDamageInternal(attackDamage, attackerId);

        // Notificar clientes
        PlayAttackEffectsClientRpc(attackerId, targetId);
    }

    /// <summary>
    /// Método interno (servidor) para aplicar dano
    /// </summary>
    private void TakeDamageInternal(int damage, ulong attackerId)
    {
        if (!IsServer) return;

        health.Value -= damage;
        health.Value = Mathf.Max(0, health.Value);

        Debug.Log($"[Server] Dano aplicado: {damage}. Vida restante: {health.Value}");

        if (health.Value <= 0)
        {
            HandleDeathClientRpc();
        }
    }

    [ClientRpc]
    private void PlayAttackEffectsClientRpc(ulong attackerId, ulong targetId)
    {
        // Tocar efeitos visuais e sonoros
        Debug.Log($"Player {attackerId} atacou player {targetId}");
    }

    [ClientRpc]
    private void HandleDeathClientRpc()
    {
        Debug.Log("Player morreu!");
    }
}
```

### Exemplo 2: Sistema de Coleta de Items

```csharp
public class NetworkItemPickup : NetworkBehaviour
{
    [SerializeField] private int itemId;
    [SerializeField] private float pickupRadius = 1.5f;

    private NetworkVariable<bool> isCollected = new NetworkVariable<bool>(false);

    /// <summary>
    /// Cliente solicita coleta de item
    /// </summary>
    public void RequestPickup()
    {
        if (!IsOwner) return;
        if (isCollected.Value) return;

        PickupServerRpc();
    }

    /// <summary>
    /// ✅ Servidor valida e processa coleta
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void PickupServerRpc(ServerRpcParams rpcParams = default)
    {
        // VALIDAÇÃO 1: Item já foi coletado?
        if (isCollected.Value)
        {
            Debug.LogWarning($"[Anti-Cheat] Item {itemId} já coletado");
            return;
        }

        ulong playerId = rpcParams.Receive.SenderClientId;

        // VALIDAÇÃO 2: Player existe?
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(playerId))
        {
            Debug.LogWarning($"[Anti-Cheat] Player {playerId} não existe");
            return;
        }

        var player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject;
        if (player == null) return;

        // VALIDAÇÃO 3: Player está próximo o suficiente?
        float sqrDistance = (player.transform.position - transform.position).sqrMagnitude;
        float radiusSqr = pickupRadius * pickupRadius;

        if (sqrDistance > radiusSqr)
        {
            Debug.LogWarning($"[Anti-Cheat] Player {playerId} muito longe do item: {Mathf.Sqrt(sqrDistance)}m");
            return;
        }

        // VALIDAÇÃO 4: Inventário tem espaço?
        var inventory = player.GetComponent<NetworkInventory>();
        if (inventory == null || inventory.IsFull())
        {
            Debug.LogWarning($"[Anti-Cheat] Inventário cheio");
            return;
        }

        // ✅ VALIDAÇÕES OK

        // Adicionar item ao inventário
        inventory.AddItemServerRpc(itemId);

        // Marcar como coletado
        isCollected.Value = true;

        // Destruir objeto
        Invoke(nameof(DespawnItem), 0.1f);
    }

    private void DespawnItem()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
```

---

## 📋 Checklist de Segurança

### 🔴 CRÍTICO - Implementar SEMPRE

- [ ] **Servidor tem autoridade final** sobre todas as ações
- [ ] **NetworkVariables** modificadas apenas pelo servidor
- [ ] **Validação de distância** em todos os ataques/interações
- [ ] **Validação de valores** (dano, cura, velocidade, etc)
- [ ] **Validação de existência** (player/enemy/item existe?)
- [ ] **Validação de estado** (está vivo? tem munição? cooldown ok?)

### 🟡 IMPORTANTE - Implementar se Relevante

- [ ] **Validação de cooldowns** (previne rate-of-fire hacks)
- [ ] **Validação de recursos** (munição, stamina, mana)
- [ ] **Validação de linha de visão** (para ataques ranged)
- [ ] **Validação de terreno** (está no chão? pode voar?)
- [ ] **Rate limiting** (previne spam de requests)

### 🟢 RECOMENDADO - Nice to Have

- [ ] **Logging de ações suspeitas**
- [ ] **Sistema de ban automático**
- [ ] **Replay/gravação de partidas** para investigação
- [ ] **Métricas de anti-cheat** (quantas validações falharam?)

---

## 🚨 Erros Comuns

### ❌ ERRO 1: Cliente decide dano

```csharp
// ❌ MUITO INSEGURO
public void TakeDamage(int damage)
{
    health -= damage; // Cliente pode chamar com damage = -999999
}
```

### ❌ ERRO 2: Validação apenas no cliente

```csharp
// ❌ INÚTIL - Hacker pode bypassar
void Update()
{
    if (Input.GetKeyDown(KeyCode.F) && CanAttack())
    {
        Attack(); // Hacker pode chamar direto Attack()
    }
}
```

### ❌ ERRO 3: Confiar em valores do cliente

```csharp
[ServerRpc]
private void MoveServerRpc(Vector3 newPosition)
{
    // ❌ PERIGOSO - Cliente pode teleportar
    transform.position = newPosition;
}
```

### ✅ CORREÇÃO: Validar movimento

```csharp
[ServerRpc]
private void MoveServerRpc(Vector3 newPosition, ServerRpcParams rpcParams = default)
{
    // ✅ Validar que movimento é fisicamente possível
    float maxMovement = moveSpeed * Time.deltaTime;
    float distance = Vector3.Distance(transform.position, newPosition);

    if (distance > maxMovement * 1.5f) // 50% de margem para latência
    {
        Debug.LogWarning($"[Anti-Cheat] Movimento impossível: {distance}m");
        return;
    }

    // OK, aplicar movimento
    transform.position = newPosition;
}
```

---

## 🎯 Resumo

### Regras de Ouro

1. **🏛️ Server Authority** - Servidor decide TUDO
2. **✅ Validar SEMPRE** - Não confie em nenhum input do cliente
3. **📏 Validar Distâncias** - Previne exploits mais comuns
4. **⏱️ Validar Tempo** - Cooldowns e rate limiting
5. **🔍 Logar Suspeitas** - Monitorar comportamento anormal

### Para Jogos 4-5 Jogadores

- ✅ Server Authority é **ESSENCIAL**
- ✅ Validação de distância é **CRÍTICA**
- ✅ Validação de valores é **OBRIGATÓRIA**
- 🟡 Anti-cheat avançado é **OPCIONAL** (poucos players)
- 🟡 Sistema de ban pode ser **MANUAL** (host kicka)

---

## 📚 Recursos

### Documentação
- [Netcode Best Practices - Security](https://docs-multiplayer.unity3d.com/netcode/current/learn/bossroom/bossroom-actions/)
- [Boss Room Example](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop)

### Vídeos
- [Dapper Dino - Netcode Security](https://youtube.com/@DapperDinoCodingTutorials)
- [Code Monkey - Anti-Cheat Basics](https://youtube.com/@CodeMonkeyUnity)

---

**Data:** 2025-11-28
**Versão:** 1.0
**Para:** Unity 2022.3 LTS + Netcode for GameObjects 1.8.0+
