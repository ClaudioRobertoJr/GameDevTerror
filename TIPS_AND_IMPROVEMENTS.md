# 💡 Dicas e Melhorias - Horror Game Framework

Este documento contém sugestões de melhorias, otimizações e features avançadas para expandir seu jogo de terror.

## 📋 Índice

- [Melhorias Imediatas](#melhorias-imediatas)
- [Otimizações de Performance](#otimizações-de-performance)
- [Features Avançadas](#features-avançadas)
- [Design de Terror](#design-de-terror)
- [Polimento e Juice](#polimento-e-juice)
- [Sistemas Opcionais](#sistemas-opcionais)
- [Acessibilidade](#acessibilidade)
- [Debug e Tools](#debug-e-tools)
- [Considerações Importantes](#considerações-importantes)

---

## 🚀 Melhorias Imediatas

### 1. Input System (Unity's New Input System)

**Por quê?** O Input Manager antigo está obsoleto.

```csharp
// Substituir Input.GetKey por:
using UnityEngine.InputSystem;

public class ModernFirstPersonController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;

    private void Awake()
    {
        moveAction = inputActions.FindAction("Move");
        jumpAction = inputActions.FindAction("Jump");
        runAction = inputActions.FindAction("Run");
    }

    private void Update()
    {
        Vector2 movement = moveAction.ReadValue<Vector2>();
        bool isJumping = jumpAction.triggered;
        bool isRunning = runAction.IsPressed();
    }
}
```

**Benefícios:**
- Suporte nativo a gamepad
- Rebinding de teclas fácil
- Múltiplos esquemas de controle
- Melhor para console ports

---

### 2. Cinemachine para Câmera

**Por quê?** Efeitos de câmera mais profissionais.

```csharp
using Cinemachine;

public class CameraEffects : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin noise;

    private void Start()
    {
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void CameraShake(float intensity, float duration)
    {
        noise.m_AmplitudeGain = intensity;
        StartCoroutine(StopShake(duration));
    }

    private IEnumerator StopShake(float duration)
    {
        yield return new WaitForSeconds(duration);
        noise.m_AmplitudeGain = 0f;
    }
}
```

**Features:**
- Camera shake mais natural
- FOV kicks (ao atirar, correr)
- Screen shake procedural
- Camera trauma system

---

### 3. Object Pooling Genérico

**Por quê?** Evitar instanciação/destruição frequente.

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace HorrorGame.Utilities
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private T prefab;
        private Queue<T> pool = new Queue<T>();
        private Transform parent;

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                T obj = Object.Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj;
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                obj = Object.Instantiate(prefab, parent);
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
```

**Use para:**
- Partículas
- Blood splatter
- Balas/projéteis
- Efeitos visuais
- Sons que se repetem muito

---

### 4. Sistema de Save/Load

**Crítico para jogos longos!**

```csharp
using System;
using System.IO;
using UnityEngine;

namespace HorrorGame.Core
{
    [Serializable]
    public class SaveData
    {
        public Vector3 playerPosition;
        public float playerHealth;
        public float playerStamina;
        public string currentScene;
        public List<string> collectedItems;
        public List<string> unlockedDoors;
        public int enemiesKilled;
        public float playTime;

        // Adicione mais conforme necessário
    }

    public class SaveSystem : MonoBehaviour
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

        public static void SaveGame(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"Game saved to {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        public static SaveData LoadGame()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    SaveData data = JsonUtility.FromJson<SaveData>(json);
                    Debug.Log("Game loaded successfully");
                    return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
            }

            return null;
        }

        public static bool SaveExists()
        {
            return File.Exists(SavePath);
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Save deleted");
            }
        }
    }
}
```

**Considere também:**
- Múltiplos slots de save
- Auto-save em checkpoints
- Cloud saves (Steam, Epic, etc)
- Criptografia básica para evitar cheating

---

## ⚡ Otimizações de Performance

### 1. LOD (Level of Detail) para Inimigos

```csharp
public class EnemyLOD : MonoBehaviour
{
    [SerializeField] private LODGroup lodGroup;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyBase enemy;

    private Transform player;
    private float updateInterval = 0.5f;
    private float timer;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateLOD();
        }
    }

    private void UpdateLOD()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        // LOD 0: Perto (0-10m) - Full detail
        if (distance < 10f)
        {
            animator.updateMode = AnimatorUpdateMode.Normal;
            enemy.enabled = true;
        }
        // LOD 1: Médio (10-20m) - Reduced detail
        else if (distance < 20f)
        {
            animator.updateMode = AnimatorUpdateMode.Normal;
            enemy.enabled = true;
            // Reduzir update rate da IA
        }
        // LOD 2: Longe (20m+) - Minimal
        else
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            enemy.enabled = false; // Desliga IA completamente
        }
    }
}
```

---

### 2. Occlusion Culling

**Setup no Unity:**
1. Window > Rendering > Occlusion Culling
2. Marque objetos estáticos como "Occluder Static"
3. Bake occlusion data
4. Camera automaticamente não renderiza objetos ocultos

**Resultado:** 30-50% melhor performance em ambientes fechados!

---

### 3. Batching e GPU Instancing

```csharp
// Em Materials:
// 1. Use o mesmo material para objetos similares
// 2. Enable GPU Instancing no material
// 3. Use Static Batching para objetos que não se movem

public class MaterialBatcher : MonoBehaviour
{
    [SerializeField] private Material sharedMaterial;

    private void Start()
    {
        // Força todos os renderers filhos a usar o mesmo material
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.sharedMaterial = sharedMaterial;
        }
    }
}
```

---

### 4. Update Manager (Evitar Update() em muitos scripts)

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace HorrorGame.Core
{
    public class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance { get; private set; }

        private List<IUpdatable> updatables = new List<IUpdatable>();
        private List<IFixedUpdatable> fixedUpdatables = new List<IFixedUpdatable>();

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < updatables.Count; i++)
            {
                updatables[i].OnUpdate(deltaTime);
            }
        }

        private void FixedUpdate()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            for (int i = 0; i < fixedUpdatables.Count; i++)
            {
                fixedUpdatables[i].OnFixedUpdate(fixedDeltaTime);
            }
        }

        public void Register(IUpdatable updatable)
        {
            if (!updatables.Contains(updatable))
                updatables.Add(updatable);
        }

        public void Unregister(IUpdatable updatable)
        {
            updatables.Remove(updatable);
        }

        public void Register(IFixedUpdatable fixedUpdatable)
        {
            if (!fixedUpdatables.Contains(fixedUpdatable))
                fixedUpdatables.Add(fixedUpdatable);
        }

        public void Unregister(IFixedUpdatable fixedUpdatable)
        {
            fixedUpdatables.Remove(fixedUpdatable);
        }
    }

    public interface IUpdatable
    {
        void OnUpdate(float deltaTime);
    }

    public interface IFixedUpdatable
    {
        void OnFixedUpdate(float fixedDeltaTime);
    }
}
```

**Uso:**
```csharp
public class MyScript : MonoBehaviour, IUpdatable
{
    private void OnEnable()
    {
        UpdateManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        UpdateManager.Instance.Unregister(this);
    }

    public void OnUpdate(float deltaTime)
    {
        // Seu código de update aqui
    }
}
```

---

### 5. NavMesh Baking em Runtime (Para procedural levels)

```csharp
using UnityEngine;
using UnityEngine.AI;

public class RuntimeNavMeshBaker : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;

    public void RebakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh rebuilt");
        }
    }

    // Chame quando spawnar/destruir geometria
}
```

---

## 🎮 Features Avançadas

### 1. Sistema de Inventário Completo

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace HorrorGame.Player
{
    public class AdvancedInventory : MonoBehaviour
    {
        [SerializeField] private int maxSlots = 20;

        private Dictionary<string, InventoryItemStack> items = new Dictionary<string, InventoryItemStack>();

        public bool AddItem(InventoryItemData itemData, int quantity = 1)
        {
            if (items.ContainsKey(itemData.itemID))
            {
                // Item já existe, empilha se possível
                if (itemData.stackable)
                {
                    items[itemData.itemID].quantity += quantity;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
                else if (GetTotalSlots() < maxSlots)
                {
                    // Cria novo slot
                    items.Add(itemData.itemID + System.Guid.NewGuid(), new InventoryItemStack(itemData, quantity));
                    OnInventoryChanged?.Invoke();
                    return true;
                }
                return false;
            }
            else
            {
                if (GetTotalSlots() >= maxSlots)
                    return false;

                items.Add(itemData.itemID, new InventoryItemStack(itemData, quantity));
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        public bool RemoveItem(string itemID, int quantity = 1)
        {
            if (!items.ContainsKey(itemID))
                return false;

            items[itemID].quantity -= quantity;
            if (items[itemID].quantity <= 0)
            {
                items.Remove(itemID);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasItem(string itemID, int quantity = 1)
        {
            return items.ContainsKey(itemID) && items[itemID].quantity >= quantity;
        }

        public int GetItemCount(string itemID)
        {
            return items.ContainsKey(itemID) ? items[itemID].quantity : 0;
        }

        public int GetTotalSlots()
        {
            return items.Count;
        }

        public Dictionary<string, InventoryItemStack> GetAllItems()
        {
            return new Dictionary<string, InventoryItemStack>(items);
        }

        // Eventos
        public System.Action OnInventoryChanged;
    }

    [System.Serializable]
    public class InventoryItemStack
    {
        public InventoryItemData itemData;
        public int quantity;

        public InventoryItemStack(InventoryItemData data, int qty)
        {
            itemData = data;
            quantity = qty;
        }
    }
}
```

**ScriptableObject para Items:**
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Horror Game/Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public GameObject worldPrefab;

    [Header("Properties")]
    public ItemType type;
    public bool stackable = true;
    public int maxStack = 99;
    public bool consumable = false;

    [Header("Effects")]
    public float healthRestore;
    public float staminaRestore;
    public bool isKey;
    public string keyID;
}

public enum ItemType
{
    Consumable,
    Key,
    Document,
    Quest,
    Weapon,
    Ammo
}
```

---

### 2. Sistema de Sanidade/Insanidade

```csharp
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    public class SanitySystem : MonoBehaviour
    {
        [Header("Sanity Settings")]
        [SerializeField] private float maxSanity = 100f;
        [SerializeField] private float currentSanity = 100f;

        [Header("Drain Rates")]
        [SerializeField] private float darknessDrainRate = 5f;
        [SerializeField] private float enemyProximityDrainRate = 10f;
        [SerializeField] private float recoveryRate = 3f;

        [Header("Effects")]
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private AudioSource heartbeatSource;
        [SerializeField] private AudioSource whispersSource;

        [Header("Hallucinations")]
        [SerializeField] private GameObject[] hallucinationPrefabs;
        [SerializeField] private float hallucinationChance = 0.1f;
        [SerializeField] private float hallucinationInterval = 10f;

        private Vignette vignette;
        private ChromaticAberration chromaticAberration;
        private float hallucinationTimer;
        private bool inDarkness;
        private int nearbyEnemies;

        public float SanityPercent => currentSanity / maxSanity;
        public bool IsInsane => currentSanity <= 0f;

        private void Start()
        {
            if (postProcessVolume != null)
            {
                postProcessVolume.profile.TryGet(out vignette);
                postProcessVolume.profile.TryGet(out chromaticAberration);
            }
        }

        private void Update()
        {
            UpdateSanity();
            UpdateVisualEffects();
            UpdateAudioEffects();
            UpdateHallucinations();
        }

        private void UpdateSanity()
        {
            float drainRate = 0f;

            // Drena na escuridão
            if (inDarkness)
                drainRate += darknessDrainRate;

            // Drena com inimigos próximos
            if (nearbyEnemies > 0)
                drainRate += enemyProximityDrainRate * nearbyEnemies;

            if (drainRate > 0f)
            {
                DrainSanity(drainRate * Time.deltaTime);
            }
            else
            {
                RecoverSanity(recoveryRate * Time.deltaTime);
            }
        }

        private void UpdateVisualEffects()
        {
            float insanityLevel = 1f - SanityPercent;

            // Vignette mais forte com sanidade baixa
            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(0.2f, 0.6f, insanityLevel);
            }

            // Aberração cromática
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(0f, 0.8f, insanityLevel);
            }
        }

        private void UpdateAudioEffects()
        {
            float insanityLevel = 1f - SanityPercent;

            // Batimentos cardíacos
            if (heartbeatSource != null)
            {
                heartbeatSource.volume = Mathf.Lerp(0f, 0.7f, insanityLevel);
                heartbeatSource.pitch = Mathf.Lerp(0.8f, 1.3f, insanityLevel);
            }

            // Sussurros
            if (whispersSource != null && insanityLevel > 0.5f)
            {
                whispersSource.volume = Mathf.Lerp(0f, 0.4f, (insanityLevel - 0.5f) * 2f);
            }
        }

        private void UpdateHallucinations()
        {
            if (SanityPercent > 0.3f) return; // Só acontece com sanidade muito baixa

            hallucinationTimer += Time.deltaTime;

            if (hallucinationTimer >= hallucinationInterval)
            {
                hallucinationTimer = 0f;

                if (Random.value < hallucinationChance)
                {
                    SpawnHallucination();
                }
            }
        }

        private void SpawnHallucination()
        {
            if (hallucinationPrefabs.Length == 0) return;

            GameObject hallucination = hallucinationPrefabs[Random.Range(0, hallucinationPrefabs.Length)];
            Vector3 spawnPos = transform.position + transform.forward * Random.Range(5f, 15f);

            GameObject instance = Instantiate(hallucination, spawnPos, Quaternion.identity);
            Destroy(instance, Random.Range(2f, 5f)); // Desaparece após alguns segundos

            Debug.Log("[Sanity] Hallucination spawned!");
        }

        public void DrainSanity(float amount)
        {
            currentSanity -= amount;
            currentSanity = Mathf.Max(currentSanity, 0f);

            if (IsInsane)
            {
                OnBecomeInsane();
            }
        }

        public void RecoverSanity(float amount)
        {
            currentSanity += amount;
            currentSanity = Mathf.Min(currentSanity, maxSanity);
        }

        public void SetInDarkness(bool isDark)
        {
            inDarkness = isDark;
        }

        public void SetNearbyEnemies(int count)
        {
            nearbyEnemies = count;
        }

        private void OnBecomeInsane()
        {
            // Player totalmente insano
            Debug.Log("[Sanity] Player became insane!");
            // Você pode adicionar efeitos especiais aqui
        }
    }
}
```

---

### 3. Sistema de Diálogo/Notas

```csharp
using UnityEngine;
using TMPro;
using HorrorGame.Core;

namespace HorrorGame.UI
{
    public class DocumentReader : MonoBehaviour
    {
        [SerializeField] private GameObject documentPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private KeyCode closeKey = KeyCode.Escape;

        private bool isReading = false;

        public void ShowDocument(DocumentData data)
        {
            if (isReading) return;

            isReading = true;
            documentPanel.SetActive(true);

            titleText.text = data.title;
            contentText.text = data.content;

            // Pausa o jogo enquanto lê
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (isReading && Input.GetKeyDown(closeKey))
            {
                CloseDocument();
            }
        }

        public void CloseDocument()
        {
            isReading = false;
            documentPanel.SetActive(false);

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

[CreateAssetMenu(fileName = "New Document", menuName = "Horror Game/Document")]
public class DocumentData : ScriptableObject
{
    public string documentID;
    public string title;
    [TextArea(10, 20)]
    public string content;
    public Sprite background;
    public AudioClip ambientSound;
}
```

---

### 4. Sistema de Objetivos/Quests

```csharp
using System.Collections.Generic;
using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Core
{
    public class QuestSystem : MonoBehaviour
    {
        public static QuestSystem Instance { get; private set; }

        private List<Quest> activeQuests = new List<Quest>();
        private List<Quest> completedQuests = new List<Quest>();

        private void Awake()
        {
            Instance = this;
        }

        public void AddQuest(Quest quest)
        {
            if (!activeQuests.Contains(quest))
            {
                activeQuests.Add(quest);
                quest.OnQuestStarted();

                GameEvents.OnObjectiveUpdated?.Invoke(quest.description);
                Debug.Log($"[Quest] New quest: {quest.questName}");
            }
        }

        public void UpdateQuestProgress(string questID, int progress)
        {
            Quest quest = activeQuests.Find(q => q.questID == questID);
            if (quest != null)
            {
                quest.currentProgress = progress;
                quest.OnProgressUpdated();

                if (quest.IsCompleted())
                {
                    CompleteQuest(quest);
                }
            }
        }

        private void CompleteQuest(Quest quest)
        {
            activeQuests.Remove(quest);
            completedQuests.Add(quest);

            quest.OnQuestCompleted();

            GameEvents.OnShowMessage?.Invoke($"Quest Completed: {quest.questName}", 3f);
            Debug.Log($"[Quest] Completed: {quest.questName}");
        }

        public bool IsQuestActive(string questID)
        {
            return activeQuests.Exists(q => q.questID == questID);
        }

        public bool IsQuestCompleted(string questID)
        {
            return completedQuests.Exists(q => q.questID == questID);
        }
    }

    [System.Serializable]
    public class Quest
    {
        public string questID;
        public string questName;
        public string description;
        public QuestType type;
        public int targetProgress;
        public int currentProgress;

        public bool IsCompleted() => currentProgress >= targetProgress;

        public virtual void OnQuestStarted() { }
        public virtual void OnProgressUpdated() { }
        public virtual void OnQuestCompleted() { }
    }

    public enum QuestType
    {
        CollectItems,
        KillEnemies,
        ReachLocation,
        FindDocument,
        Solvepuzzle
    }
}
```

---

## 🎨 Design de Terror - Dicas Essenciais

### 1. Regra dos 3 Tipos de Medo

**Tension (Tensão):**
- Som ambiente constante
- Música sutil
- Iluminação baixa
- Sensação de "algo vai acontecer"

**Surprise (Surpresa):**
- Jumpscares (USE COM MODERAÇÃO!)
- Sons súbitos
- Inimigo aparecendo inesperadamente

**Horror (Horror):**
- Ver inimigo te perseguindo
- Perceber que está sem munição
- Estar preso com inimigo próximo

**Balanceamento ideal:**
- 60% Tension
- 10% Surprise
- 30% Horror

---

### 2. Sound Design É CRUCIAL

```csharp
// Sistema de Som Ambiente Dinâmico
public class DynamicAmbience : MonoBehaviour
{
    [System.Serializable]
    public class AmbienceLayer
    {
        public string name;
        public AudioClip clip;
        public AudioSource source;
        [Range(0f, 1f)] public float baseVolume = 0.5f;
        public float fadeSpeed = 1f;
    }

    [SerializeField] private AmbienceLayer[] layers;

    // Exemplos de layers:
    // - Wind (sempre tocando baixo)
    // - Distant sounds (toca quando longe de inimigos)
    // - Heartbeat (toca quando perto de inimigos)
    // - Music tension (aumenta com tensão)

    public void SetLayerVolume(string layerName, float targetVolume)
    {
        AmbienceLayer layer = System.Array.Find(layers, l => l.name == layerName);
        if (layer != null)
        {
            StartCoroutine(FadeLayer(layer, targetVolume));
        }
    }

    private IEnumerator FadeLayer(AmbienceLayer layer, float targetVolume)
    {
        float startVolume = layer.source.volume;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * layer.fadeSpeed;
            layer.source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed);
            yield return null;
        }
    }
}
```

**Sons Essenciais:**
- Passos do player (diferentes superfícies!)
- Respiração pesada quando correndo/com stamina baixa
- Som ambiente (vento, goteiras, rangidos)
- Batimento cardíaco quando estressado
- Sons direcionais de inimigos (ronco, passos, etc)

---

### 3. Lighting é 50% do Terror

**Regras de Ouro:**
```
✅ Use sombras dinâmicas
✅ Iluminação indireta (bounce light)
✅ Contraste forte (áreas muito claras vs muito escuras)
✅ Fontes de luz limitadas (lanterna, velas)
✅ Flickering lights (tremulação)

❌ NUNCA use iluminação totalmente plana
❌ EVITE luz ambiente alta
❌ NÃO ilumine tudo uniformemente
```

**Sistema de Luzes Tremulantes:**
```csharp
public class FlickeringLight : MonoBehaviour
{
    [SerializeField] private Light lightSource;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float flickerSpeed = 0.1f;
    [SerializeField] private bool randomFlicker = true;

    private float targetIntensity;
    private float flickerTimer;

    private void Start()
    {
        targetIntensity = lightSource.intensity;
    }

    private void Update()
    {
        flickerTimer += Time.deltaTime;

        if (flickerTimer >= flickerSpeed)
        {
            flickerTimer = 0f;

            if (randomFlicker)
            {
                targetIntensity = Random.Range(minIntensity, maxIntensity);
            }
            else
            {
                targetIntensity = targetIntensity == maxIntensity ? minIntensity : maxIntensity;
            }
        }

        lightSource.intensity = Mathf.Lerp(lightSource.intensity, targetIntensity, Time.deltaTime * 10f);
    }
}
```

---

### 4. Psicologia do Jogador

**Técnicas Eficazes:**

1. **False Security** - Dê momentos de calma antes do horror
2. **Anticipation** - Deixe o jogador SABER que algo ruim vai acontecer
3. **Helplessness** - Momentos onde fugir é a única opção
4. **Resource Scarcity** - Munição/bateria limitada cria tensão
5. **Sound Misdirection** - Som vindo de uma direção, inimigo vem de outra

```csharp
// Sistema de "False Jump Scare"
public class FalseJumpscare : MonoBehaviour
{
    [SerializeField] private AudioClip falseScareSound;
    [SerializeField] private GameObject harmlessObject; // Ex: gato pulando

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Toca som assustador
            GameEvents.OnPlaySound3D?.Invoke(falseScareSound.name, transform.position);

            // Mas é só um gato/objeto caindo!
            if (harmlessObject != null)
            {
                harmlessObject.SetActive(true);
            }

            // Isso faz o jogador ficar AINDA MAIS tenso para o próximo
            Destroy(gameObject, 2f);
        }
    }
}
```

---

## ✨ Polimento e "Juice"

### 1. Screen Shake Melhorado

```csharp
public class AdvancedCameraShake : MonoBehaviour
{
    private float trauma = 0f;
    private float traumaDecay = 1.3f;
    private float maxAngle = 10f;
    private float maxOffset = 0.2f;

    private Transform cameraTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        originalPosition = cameraTransform.localPosition;
        originalRotation = cameraTransform.localRotation;
    }

    private void Update()
    {
        if (trauma > 0f)
        {
            trauma = Mathf.Max(0f, trauma - traumaDecay * Time.deltaTime);

            float shake = trauma * trauma; // Quadrático para feel melhor

            // Rotação
            float angleX = maxAngle * shake * Random.Range(-1f, 1f);
            float angleY = maxAngle * shake * Random.Range(-1f, 1f);
            float angleZ = maxAngle * shake * Random.Range(-1f, 1f);

            // Posição
            float offsetX = maxOffset * shake * Random.Range(-1f, 1f);
            float offsetY = maxOffset * shake * Random.Range(-1f, 1f);

            cameraTransform.localRotation = originalRotation * Quaternion.Euler(angleX, angleY, angleZ);
            cameraTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
        }
        else
        {
            // Retorna suavemente
            cameraTransform.localRotation = Quaternion.Lerp(
                cameraTransform.localRotation,
                originalRotation,
                Time.deltaTime * 5f
            );
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                originalPosition,
                Time.deltaTime * 5f
            );
        }
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }
}

// Uso:
// OnPlayerDamaged: AddTrauma(0.5f)
// OnEnemyAttack: AddTrauma(0.3f)
// OnExplosion: AddTrauma(1.0f)
```

---

### 2. Footstep System Avançado

```csharp
using UnityEngine;

public class FootstepSystem : MonoBehaviour
{
    [System.Serializable]
    public class SurfaceSound
    {
        public string surfaceTag;
        public AudioClip[] walkSounds;
        public AudioClip[] runSounds;
    }

    [SerializeField] private SurfaceSound[] surfaceSounds;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;

    private float stepTimer;
    private bool isMoving;
    private bool isRunning;
    private CharacterController controller;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        isMoving = controller.velocity.magnitude > 0.1f;
        isRunning = Input.GetKey(KeyCode.LeftShift);

        if (isMoving && controller.isGrounded)
        {
            float interval = isRunning ? runStepInterval : walkStepInterval;
            stepTimer += Time.deltaTime;

            if (stepTimer >= interval)
            {
                stepTimer = 0f;
                PlayFootstep();
            }
        }
    }

    private void PlayFootstep()
    {
        // Raycast para detectar superfície
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            string surfaceTag = hit.collider.tag;
            SurfaceSound surface = System.Array.Find(surfaceSounds, s => s.surfaceTag == surfaceTag);

            if (surface != null)
            {
                AudioClip[] sounds = isRunning ? surface.runSounds : surface.walkSounds;
                if (sounds.Length > 0)
                {
                    AudioClip clip = sounds[Random.Range(0, sounds.Length)];
                    GameEvents.OnPlaySound3D?.Invoke(clip.name, transform.position);
                }
            }
        }
    }
}
```

---

### 3. Breathing System

```csharp
public class BreathingSystem : MonoBehaviour
{
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private AudioClip normalBreath;
    [SerializeField] private AudioClip heavyBreath;
    [SerializeField] private AudioClip exhaustedBreath;

    [SerializeField] private PlayerHealth health;
    [SerializeField] private FirstPersonController controller;

    private void Update()
    {
        UpdateBreathing();
    }

    private void UpdateBreathing()
    {
        float healthPercent = health.HealthPercent;
        float staminaPercent = controller.CurrentStamina / 100f;

        AudioClip targetClip = normalBreath;
        float targetVolume = 0.3f;
        float targetPitch = 1.0f;

        // Respiração pesada quando stamina baixa
        if (staminaPercent < 0.3f)
        {
            targetClip = heavyBreath;
            targetVolume = 0.6f;
            targetPitch = 1.2f;
        }
        // Respiração exausta quando vida baixa
        else if (healthPercent < 0.3f)
        {
            targetClip = exhaustedBreath;
            targetVolume = 0.7f;
            targetPitch = 0.9f;
        }

        if (breathingSource.clip != targetClip)
        {
            breathingSource.clip = targetClip;
            breathingSource.Play();
        }

        breathingSource.volume = Mathf.Lerp(breathingSource.volume, targetVolume, Time.deltaTime);
        breathingSource.pitch = Mathf.Lerp(breathingSource.pitch, targetPitch, Time.deltaTime);
    }
}
```

---

## 🔧 Debug e Developer Tools

### 1. Console de Debug In-Game

```csharp
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugConsole : MonoBehaviour
{
    [SerializeField] private GameObject consolePanel;
    [SerializeField] private TextMeshProUGUI consoleText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // ~

    private Dictionary<string, System.Action<string[]>> commands = new Dictionary<string, System.Action<string[]>>();
    private List<string> logs = new List<string>();

    private void Start()
    {
        RegisterCommands();
        consolePanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleConsole();
        }
    }

    private void RegisterCommands()
    {
        // God mode
        commands.Add("god", (args) => {
            PlayerHealth health = FindObjectOfType<PlayerHealth>();
            // Implementar invencibilidade
            Log("God mode toggled");
        });

        // Noclip
        commands.Add("noclip", (args) => {
            CharacterController cc = FindObjectOfType<CharacterController>();
            cc.enabled = !cc.enabled;
            Log($"Noclip: {!cc.enabled}");
        });

        // Spawn inimigo
        commands.Add("spawn", (args) => {
            if (args.Length > 0)
            {
                // Spawn inimigo pelo nome
                Log($"Spawning {args[0]}");
            }
        });

        // Teleport
        commands.Add("tp", (args) => {
            if (args.Length >= 3)
            {
                float x = float.Parse(args[0]);
                float y = float.Parse(args[1]);
                float z = float.Parse(args[2]);

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                player.transform.position = new Vector3(x, y, z);
                Log($"Teleported to {x}, {y}, {z}");
            }
        });

        // Kill all enemies
        commands.Add("killall", (args) => {
            var enemies = FindObjectsOfType<EnemyBase>();
            foreach (var enemy in enemies)
            {
                Destroy(enemy.gameObject);
            }
            Log($"Killed {enemies.Length} enemies");
        });

        // Set health
        commands.Add("sethealth", (args) => {
            if (args.Length > 0)
            {
                float health = float.Parse(args[0]);
                FindObjectOfType<PlayerHealth>().Heal(health);
                Log($"Health set to {health}");
            }
        });
    }

    public void ExecuteCommand(string input)
    {
        string[] parts = input.Split(' ');
        string command = parts[0].ToLower();
        string[] args = new string[parts.Length - 1];
        System.Array.Copy(parts, 1, args, 0, args.Length);

        if (commands.ContainsKey(command))
        {
            commands[command](args);
        }
        else
        {
            Log($"Unknown command: {command}");
        }

        inputField.text = "";
    }

    private void Log(string message)
    {
        logs.Add(message);
        if (logs.Count > 10)
            logs.RemoveAt(0);

        consoleText.text = string.Join("\n", logs);
    }

    private void ToggleConsole()
    {
        bool active = !consolePanel.activeSelf;
        consolePanel.SetActive(active);

        if (active)
        {
            inputField.ActivateInputField();
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
```

---

### 2. Enemy Debug Visualizer

```csharp
public class EnemyDebugVisualizer : MonoBehaviour
{
    [SerializeField] private bool showDetectionRange = true;
    [SerializeField] private bool showAttackRange = true;
    [SerializeField] private bool showFOV = true;
    [SerializeField] private bool showPath = true;
    [SerializeField] private bool showState = true;

    private EnemyBase enemy;
    private NavMeshAgent agent;

    private void Start()
    {
        enemy = GetComponent<EnemyBase>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Detection range
        if (showDetectionRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 10f); // Use o valor real
        }

        // Attack range
        if (showAttackRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }

        // FOV
        if (showFOV)
        {
            // Desenhe o cone de visão
        }

        // Path
        if (showPath && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.green;
            var path = agent.path;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }
    }

    private void OnGUI()
    {
        if (!showState) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        if (screenPos.z > 0)
        {
            GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 200, 40),
                $"State: {enemy.CurrentState}\nHealth: {enemy.HealthPercent:P0}");
        }
    }
}
```

---

## ♿ Acessibilidade

### Features Importantes:

1. **Subtitles/Closed Captions**
```csharp
public class SubtitleSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private float displayDuration = 3f;

    public void ShowSubtitle(string text, float duration = -1f)
    {
        subtitleText.text = text;
        StartCoroutine(HideAfterDelay(duration > 0 ? duration : displayDuration));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        subtitleText.text = "";
    }
}
```

2. **Colorblind Modes**
3. **Adjustable UI Scale**
4. **Remappable Controls**
5. **Visual Sound Indicators** (para surdos)
6. **Adjustable Difficulty**

---

## ⚠️ Considerações Importantes

### 1. Performance Budget

Mantenha um "budget" de performance:
- **Draw Calls:** < 1000
- **Batches:** < 500
- **Tris:** < 1M visíveis
- **Audio Sources:** < 32 simultâneos
- **NavMesh Agents:** < 20 ativos

### 2. Memory Management

```csharp
// Limpe recursos não usados periodicamente
public class ResourceCleaner : MonoBehaviour
{
    [SerializeField] private float cleanupInterval = 60f;

    private void Start()
    {
        InvokeRepeating(nameof(Cleanup), cleanupInterval, cleanupInterval);
    }

    private void Cleanup()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.Log("[ResourceCleaner] Cleaned up unused resources");
    }
}
```

### 3. Testing Checklist

- [ ] Teste em hardware mínimo
- [ ] Teste com diferentes resoluções
- [ ] Teste com gamepad
- [ ] Teste sistemas de save/load
- [ ] Playtest com pessoas reais
- [ ] Teste edge cases (0 vida, 0 stamina, etc)
- [ ] Teste transições de cena
- [ ] Teste pause/unpause
- [ ] Profile performance regularmente

### 4. Playtesting É ESSENCIAL

- **Nunca** assuma que algo é assustador
- Observe pessoas jogando (não diga nada!)
- Grave sessões de playtest
- Pergunte sobre momentos específicos
- Itere baseado em feedback

### 5. Evite Estes Erros Comuns

❌ **Jumpscares demais** - Perde o efeito
❌ **Player muito fraco** - Frustrante
❌ **Recursos infinitos** - Sem tensão
❌ **IA muito burra/perfeita** - Quebrando imersão
❌ **Save points ruins** - Player perde muito progresso
❌ **Tutorial muito longo** - Boring
❌ **Controles desconfortáveis** - Perde imersão

---

## 🎓 Recursos de Aprendizado

### Jogos de Terror para Estudar:
1. **Amnesia: The Dark Descent** - Mecânicas de terror psicológico
2. **Outlast** - Helplessness e chase sequences
3. **Alien: Isolation** - IA adaptativa
4. **Resident Evil 7** - Atmosfera e level design
5. **SOMA** - Storytelling e atmosphere
6. **PT (Silent Hills)** - Loop design e jump scares

### YouTube Channels:
- Game Maker's Toolkit (design analysis)
- AI and Games (IA de jogos)
- GDC (talks de desenvolvedores)
- Jonas Tyroller (dev tips)

---

## 📝 Checklist de Lançamento

Antes de publicar:

**Técnico:**
- [ ] Todas as cenas carregam corretamente
- [ ] Sem console errors
- [ ] Performance estável (60 FPS no target hardware)
- [ ] Save/Load funciona
- [ ] Todos os sons têm fallback
- [ ] UI escala corretamente
- [ ] Suporte a múltiplas resoluções

**Gameplay:**
- [ ] Tutorial claro
- [ ] Dificuldade balanceada
- [ ] Checkpoints bem posicionados
- [ ] Sem softlocks possíveis
- [ ] Feedback visual/audio claro

**Polish:**
- [ ] Menus funcionais
- [ ] Créditos completos
- [ ] Settings salvam
- [ ] Pause funciona em todo lugar
- [ ] Música e SFX balanceados

**Legal:**
- [ ] Aviso de conteúdo (horror, violence, etc)
- [ ] Créditos de assets third-party
- [ ] EULA/Terms

---

Essas melhorias vão transformar seu framework básico em um jogo de terror profissional! 🎮👻

Priorize baseado no seu projeto específico. Nem tudo precisa ser implementado de uma vez!
