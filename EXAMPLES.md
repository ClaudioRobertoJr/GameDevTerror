# 📚 Exemplos de Uso - Horror Game Framework

Este documento contém exemplos práticos de como usar os sistemas do framework.

## Índice
- [Setup Rápido](#setup-rápido)
- [Exemplos de Player](#exemplos-de-player)
- [Exemplos de Inimigos](#exemplos-de-inimigos)
- [Exemplos de Eventos](#exemplos-de-eventos)
- [Exemplos de UI](#exemplos-de-ui)
- [Exemplos de Áudio](#exemplos-de-áudio)
- [Criando Novos Sistemas](#criando-novos-sistemas)

---

## Setup Rápido

### 1. Setup Mínimo da Cena

```csharp
// Hierarquia mínima necessária:
/*
Scene
├── GameManager (GameObject com GameManager.cs)
├── SceneController (GameObject com SceneController.cs)
├── AudioManager (GameObject com AudioManager.cs)
├── Player
│   ├── CharacterController
│   ├── FirstPersonController
│   ├── MouseLook
│   ├── PlayerHealth
│   ├── PlayerInteraction
│   └── Main Camera
│       └── Flashlight (com Light)
├── UI
│   ├── Canvas
│   │   ├── PlayerHUD
│   │   └── PauseMenu
│   └── EventSystem
└── Environment
    ├── Ground (com NavMesh baked)
    ├── Lighting
    └── AtmosphereController
*/
```

### 2. Configuração Inicial do Player

```csharp
// No Inspector do Player:

// FirstPersonController:
Walk Speed: 3
Run Speed: 6
Crouch Speed: 1.5
Jump Height: 1.5
Max Stamina: 100

// MouseLook:
Mouse Sensitivity: 100
Min Vertical Angle: -90
Max Vertical Angle: 90

// PlayerHealth:
Max Health: 100
Can Regenerate: false
```

---

## Exemplos de Player

### Exemplo 1: Criar Zona de Dano

```csharp
using UnityEngine;
using HorrorGame.Player;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 10f;
    [SerializeField] private float damageInterval = 0.5f;

    private float damageTimer = 0f;
    private bool playerInZone = false;
    private PlayerHealth playerHealth;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerHealth>();
            playerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            playerHealth = null;
        }
    }

    private void Update()
    {
        if (playerInZone && playerHealth != null)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                playerHealth.TakeDamage(damagePerSecond * damageInterval);
            }
        }
    }
}
```

### Exemplo 2: Teleporte do Player

```csharp
using UnityEngine;
using HorrorGame.Player;

public class TeleportTrigger : MonoBehaviour
{
    [SerializeField] private Transform teleportDestination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FirstPersonController controller = other.GetComponent<FirstPersonController>();
            if (controller != null)
            {
                controller.Teleport(teleportDestination.position);
            }
        }
    }
}
```

### Exemplo 3: Item de Cura Customizado

```csharp
using UnityEngine;
using HorrorGame.Player;
using HorrorGame.Core;

public class AdvancedHealthKit : MonoBehaviour, IInteractable
{
    [SerializeField] private float healAmount = 50f;
    [SerializeField] private bool fullHeal = false;
    [SerializeField] private GameObject pickupEffect;

    public string GetInteractionPrompt()
    {
        return fullHeal ? "Full Heal [E]" : $"Heal +{healAmount} [E]";
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            if (fullHeal)
                health.FullHeal();
            else
                health.Heal(healAmount);

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
```

---

## Exemplos de Inimigos

### Exemplo 1: Inimigo Perseguidor (Stalker)

```csharp
using UnityEngine;
using HorrorGame.Player;

namespace HorrorGame.Enemy
{
    public class StalkerEnemy : EnemyBase
    {
        [Header("Stalker Settings")]
        [SerializeField] private float stalkDistance = 8f;
        [SerializeField] private float stalkSpeed = 2f;

        private bool isStalking = false;

        protected override void UpdateAI()
        {
            // Detecta player
            if (DetectPlayer())
            {
                float distanceToPlayer = Vector3.Distance(transform.position, target.position);

                // Muito perto? Ataca
                if (IsInAttackRange())
                {
                    navAgent.isStopped = true;
                    Attack();
                }
                // Distância média? Stalking
                else if (distanceToPlayer <= stalkDistance)
                {
                    isStalking = true;
                    navAgent.speed = stalkSpeed;
                    // Mantém distância
                    navAgent.SetDestination(transform.position);
                }
                // Longe? Persegue
                else
                {
                    isStalking = false;
                    navAgent.speed = chaseSpeed;
                    navAgent.SetDestination(target.position);
                }
            }
            else
            {
                isStalking = false;
            }
        }

        protected override void OnAttack()
        {
            if (target == null) return;

            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
        }
    }
}
```

### Exemplo 2: Spawnar Inimigo Manualmente

```csharp
using UnityEngine;
using HorrorGame.Environment;

public class TriggerEnemySpawn : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!onlyOnce || !hasTriggered))
        {
            hasTriggered = true;

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                spawner.SpawnEnemy();
            }

            Debug.Log($"Spawned {enemiesToSpawn} enemies");
        }
    }
}
```

### Exemplo 3: Inimigo com Vida Personalizada

```csharp
using UnityEngine;

namespace HorrorGame.Enemy
{
    public class BossEnemy : EnemyBase
    {
        [Header("Boss Settings")]
        [SerializeField] private float phase2HealthThreshold = 0.5f;
        [SerializeField] private float enragedDamageMultiplier = 1.5f;

        private bool isEnraged = false;

        protected override void OnDamageTaken(float damage, Vector3 damageSource)
        {
            base.OnDamageTaken(damage, damageSource);

            // Entra em fase 2 com 50% de vida
            if (!isEnraged && HealthPercent <= phase2HealthThreshold)
            {
                EnterPhase2();
            }
        }

        private void EnterPhase2()
        {
            isEnraged = true;
            damage *= enragedDamageMultiplier;
            moveSpeed *= 1.3f;
            chaseSpeed *= 1.3f;

            Debug.Log("[Boss] Entered Phase 2!");
        }

        protected override void UpdateAI()
        {
            // Lógica do boss
            if (DetectPlayer())
            {
                navAgent.SetDestination(target.position);

                if (IsInAttackRange())
                {
                    Attack();
                }
            }
        }

        protected override void OnAttack()
        {
            // Ataque do boss
        }
    }
}
```

---

## Exemplos de Eventos

### Exemplo 1: Sistema de Conquistas

```csharp
using UnityEngine;
using HorrorGame.Core;

public class AchievementSystem : MonoBehaviour
{
    private int enemiesKilled = 0;
    private float damageTaken = 0f;

    private void Start()
    {
        GameEvents.OnEnemyDied += OnEnemyKilled;
        GameEvents.OnPlayerDamaged += OnPlayerDamaged;
    }

    private void OnDestroy()
    {
        GameEvents.OnEnemyDied -= OnEnemyKilled;
        GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
    }

    private void OnEnemyKilled(GameObject enemy)
    {
        enemiesKilled++;

        if (enemiesKilled == 10)
        {
            UnlockAchievement("First Blood - Kill 10 enemies");
        }
        else if (enemiesKilled == 50)
        {
            UnlockAchievement("Exterminator - Kill 50 enemies");
        }
    }

    private void OnPlayerDamaged(float damage, float currentHealth)
    {
        damageTaken += damage;
    }

    private void UnlockAchievement(string achievement)
    {
        Debug.Log($"Achievement Unlocked: {achievement}");
        GameEvents.OnShowMessage?.Invoke($"🏆 {achievement}", 5f);
    }
}
```

### Exemplo 2: Sistema de Dificuldade Adaptativa

```csharp
using UnityEngine;
using HorrorGame.Core;

public class AdaptiveDifficulty : MonoBehaviour
{
    [SerializeField] private float difficultyAdjustInterval = 60f;

    private int playerDeaths = 0;
    private float difficultyMultiplier = 1f;
    private float timer = 0f;

    private void Start()
    {
        GameEvents.OnPlayerDied += OnPlayerDied;
    }

    private void OnDestroy()
    {
        GameEvents.OnPlayerDied -= OnPlayerDied;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= difficultyAdjustInterval)
        {
            AdjustDifficulty();
            timer = 0f;
        }
    }

    private void OnPlayerDied()
    {
        playerDeaths++;
    }

    private void AdjustDifficulty()
    {
        // Muitas mortes? Diminui dificuldade
        if (playerDeaths >= 3)
        {
            difficultyMultiplier = 0.8f;
            Debug.Log("Difficulty decreased");
        }
        // Nenhuma morte? Aumenta dificuldade
        else if (playerDeaths == 0)
        {
            difficultyMultiplier = 1.2f;
            Debug.Log("Difficulty increased");
        }

        playerDeaths = 0;
    }

    public float GetDifficultyMultiplier()
    {
        return difficultyMultiplier;
    }
}
```

---

## Exemplos de UI

### Exemplo 1: HUD de Bateria da Lanterna

```csharp
using UnityEngine;
using UnityEngine.UI;
using HorrorGame.Player;

public class FlashlightBatteryUI : MonoBehaviour
{
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private Image batteryBar;
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color emptyColor = Color.red;

    private void Update()
    {
        if (flashlight != null && batteryBar != null)
        {
            float batteryPercent = flashlight.BatteryPercent;
            batteryBar.fillAmount = batteryPercent;
            batteryBar.color = Color.Lerp(emptyColor, fullColor, batteryPercent);
        }
    }
}
```

### Exemplo 2: Contador de Inimigos Vivos

```csharp
using UnityEngine;
using TMPro;
using HorrorGame.Core;

public class EnemyCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    private int activeEnemies = 0;

    private void Start()
    {
        GameEvents.OnEnemySpawned += OnEnemySpawned;
        GameEvents.OnEnemyDied += OnEnemyDied;
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        GameEvents.OnEnemySpawned -= OnEnemySpawned;
        GameEvents.OnEnemyDied -= OnEnemyDied;
    }

    private void OnEnemySpawned(GameObject enemy)
    {
        activeEnemies++;
        UpdateDisplay();
    }

    private void OnEnemyDied(GameObject enemy)
    {
        activeEnemies--;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (counterText != null)
        {
            counterText.text = $"Enemies: {activeEnemies}";
        }
    }
}
```

---

## Exemplos de Áudio

### Exemplo 1: Som de Passos do Player

```csharp
using UnityEngine;
using HorrorGame.Player;
using HorrorGame.Core;

public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private string walkSound = "footstep_walk";
    [SerializeField] private string runSound = "footstep_run";
    [SerializeField] private float stepInterval = 0.5f;

    private float stepTimer = 0f;

    private void Update()
    {
        if (controller != null && controller.IsMoving && controller.IsGrounded)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                stepTimer = 0f;
                PlayFootstep();
            }
        }
    }

    private void PlayFootstep()
    {
        // Determina qual som tocar baseado em se está correndo
        string soundToPlay = Input.GetKey(KeyCode.LeftShift) ? runSound : walkSound;
        GameEvents.OnPlaySound3D?.Invoke(soundToPlay, transform.position);
    }
}
```

### Exemplo 2: Som Ambiente Dinâmico

```csharp
using UnityEngine;
using HorrorGame.Core;

public class DynamicAmbience : MonoBehaviour
{
    [SerializeField] private string calmAmbience = "ambient_calm";
    [SerializeField] private string tenseAmbience = "ambient_tense";

    private float currentTension = 0f;
    private bool isPlayingTenseAmbience = false;

    private void Start()
    {
        GameEvents.OnTensionLevelChanged += OnTensionChanged;

        // Inicia com ambiente calmo
        GameEvents.OnChangeMusicTrack?.Invoke(calmAmbience);
    }

    private void OnDestroy()
    {
        GameEvents.OnTensionLevelChanged -= OnTensionChanged;
    }

    private void OnTensionChanged(float tensionLevel)
    {
        currentTension = tensionLevel;

        // Muda para ambiente tenso se tensão > 0.5
        if (tensionLevel > 0.5f && !isPlayingTenseAmbience)
        {
            isPlayingTenseAmbience = true;
            GameEvents.OnChangeMusicTrack?.Invoke(tenseAmbience);
        }
        // Volta para calmo se tensão < 0.3
        else if (tensionLevel < 0.3f && isPlayingTenseAmbience)
        {
            isPlayingTenseAmbience = false;
            GameEvents.OnChangeMusicTrack?.Invoke(calmAmbience);
        }
    }
}
```

---

## Criando Novos Sistemas

### Exemplo 1: Sistema de Inventário Simples

```csharp
using System.Collections.Generic;
using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private int maxSlots = 10;

        private List<InventoryItem> items = new List<InventoryItem>();

        public bool AddItem(InventoryItem item)
        {
            if (items.Count >= maxSlots)
            {
                Debug.Log("Inventory full!");
                return false;
            }

            items.Add(item);
            Debug.Log($"Added {item.itemName} to inventory");
            GameEvents.OnShowMessage?.Invoke($"Picked up {item.itemName}", 2f);
            return true;
        }

        public bool RemoveItem(InventoryItem item)
        {
            return items.Remove(item);
        }

        public bool HasItem(string itemName)
        {
            return items.Exists(i => i.itemName == itemName);
        }

        public InventoryItem GetItem(string itemName)
        {
            return items.Find(i => i.itemName == itemName);
        }

        public int ItemCount => items.Count;
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public string description;
        public Sprite icon;
    }
}
```

### Exemplo 2: Sistema de Sanidade

```csharp
using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    public class PlayerSanity : MonoBehaviour
    {
        [Header("Sanity Settings")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float sanityDrainRate = 2f;
        [SerializeField] private float sanityRecoveryRate = 5f;

        [Header("Effects")]
        [SerializeField] private float lowSanityThreshold = 30f;
        [SerializeField] private AudioClip heartbeatSound;

        private float currentSanity;
        private bool isInDarkness = false;

        public float CurrentSanity => currentSanity;
        public float SanityPercent => currentSanity / maxSanity;
        public bool IsLowSanity => currentSanity <= lowSanityThreshold;

        private void Start()
        {
            currentSanity = maxSanity;
        }

        private void Update()
        {
            if (isInDarkness)
            {
                DrainSanity(sanityDrainRate * Time.deltaTime);
            }
            else
            {
                RecoverSanity(sanityRecoveryRate * Time.deltaTime);
            }

            // Efeitos de baixa sanidade
            if (IsLowSanity)
            {
                ApplyLowSanityEffects();
            }
        }

        public void DrainSanity(float amount)
        {
            currentSanity -= amount;
            currentSanity = Mathf.Max(currentSanity, 0f);
        }

        public void RecoverSanity(float amount)
        {
            currentSanity += amount;
            currentSanity = Mathf.Min(currentSanity, maxSanity);
        }

        public void SetInDarkness(bool inDarkness)
        {
            isInDarkness = inDarkness;
        }

        private void ApplyLowSanityEffects()
        {
            // Aqui você pode adicionar:
            // - Distorções visuais
            // - Sons alucinatórios
            // - Câmera tremendo
            // - Aparições de inimigos falsos
        }
    }
}
```

### Exemplo 3: Trigger de Susto (Jumpscare)

```csharp
using UnityEngine;
using HorrorGame.Core;

public class JumpscareTrigger : MonoBehaviour
{
    [SerializeField] private GameObject jumpscareObject;
    [SerializeField] private string jumpscareSound = "jumpscare";
    [SerializeField] private float jumpscareDuration = 2f;
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!onlyOnce || !hasTriggered))
        {
            TriggerJumpscare();
        }
    }

    private void TriggerJumpscare()
    {
        hasTriggered = true;

        // Ativa objeto do jumpscare
        if (jumpscareObject != null)
        {
            jumpscareObject.SetActive(true);
        }

        // Toca som
        if (!string.IsNullOrEmpty(jumpscareSound))
        {
            GameEvents.OnPlaySound2D?.Invoke(jumpscareSound);
        }

        // Desativa após duração
        if (jumpscareObject != null)
        {
            Destroy(jumpscareObject, jumpscareDuration);
        }

        Debug.Log("JUMPSCARE!");
    }
}
```

---

## Dicas Finais

1. **Sempre cancele inscrições de eventos** em `OnDestroy()`
2. **Use namespaces** para organizar código
3. **Teste frequentemente** para encontrar bugs cedo
4. **Documente** mudanças importantes
5. **Use Gizmos** para debug visual
6. **Otimize** apenas depois de ter tudo funcionando

---

Boa sorte com seu jogo! 🎮👻
