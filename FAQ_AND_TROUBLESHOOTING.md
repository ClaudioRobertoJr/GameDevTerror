# ❓ FAQ e Troubleshooting - Horror Game Framework

Perguntas frequentes e soluções para problemas comuns.

## 📋 Índice

- [Problemas Comuns](#problemas-comuns)
- [Performance](#performance)
- [Gameplay](#gameplay)
- [Audio](#audio)
- [UI](#ui)
- [Inimigos e IA](#inimigos-e-ia)
- [Build e Deploy](#build-e-deploy)
- [FAQ Geral](#faq-geral)

---

## 🔧 Problemas Comuns

### Player não se move

**Sintoma:** Player fica parado, WASD não funciona.

**Soluções:**

1. **Verificar CharacterController:**
   - Player tem componente CharacterController?
   - CharacterController está enabled?

2. **Verificar Input Manager:**
   - Edit > Project Settings > Input Manager
   - Confirmar que existem eixos "Horizontal" e "Vertical"
   - Unity 2022+ já vem com esses eixos configurados

3. **Verificar Scripts:**
   - FirstPersonController está anexado?
   - Algum erro no Console?

4. **Verificar Pause:**
   ```csharp
   // Adicione log temporário
   void Update()
   {
       if (GameManager.Instance != null && GameManager.Instance.IsPaused)
       {
           Debug.Log("Game is paused!");
           return;
       }
       // resto do código
   }
   ```

5. **Verificar Time.timeScale:**
   - Console: Digite `Time.timeScale`
   - Deve ser 1.0 quando não pausado

---

### Inimigo não patrulha/se move

**Sintoma:** Inimigo fica parado, não patrulha.

**Soluções:**

1. **NavMesh não foi baked:**
   ```
   Window > AI > Navigation > Bake
   ```
   - Deve aparecer área azul no chão
   - Se não aparecer:
     - Selecione chão
     - Inspector > Static: ✅ Navigation Static
     - Tente Bake novamente

2. **NavMeshAgent não configurado:**
   - NavMeshAgent está enabled?
   - Speed > 0?
   - Stopping Distance < Distance to waypoint?

3. **Waypoints não configurados:**
   - Array "Patrol Points" tem elementos?
   - Waypoints existem na cena?
   - Waypoints não são null?

4. **Inimigo está fora do NavMesh:**
   - Position do inimigo está sobre área azul?
   - Use Gizmo para verificar

5. **Script incorreto:**
   - Console tem erros?
   - Script está enabled?

**Debug:**
```csharp
// Adicione no PatrolEnemy.cs
void OnDrawGizmos()
{
    // Verde = tem path
    // Vermelho = sem path
    Gizmos.color = navAgent.hasPath ? Color.green : Color.red;
    Gizmos.DrawWireSphere(transform.position, 1f);
}
```

---

### Camera não rotaciona/Look errado

**Sintoma:** Mouse não controla câmera, ou se move estranho.

**Soluções:**

1. **MouseLook não anexado:**
   - Player tem componente MouseLook?
   - Camera Transform está referenciada?

2. **Cursor não está travado:**
   - Cursor deve estar invisível no jogo
   - Se aparecer, algo está chamando `Cursor.visible = true`

3. **Sensibilidade muito baixa/alta:**
   - Tente Mouse Sensitivity = 100
   - Ajuste de acordo

4. **Smooth muito alto:**
   - Se camera está "lerpeada" demais
   - Diminua Smooth Speed ou desative

5. **Múltiplos scripts controlando câmera:**
   - Conflito com outros scripts?
   - Desabilite temporariamente

**Debug:**
```csharp
void Update()
{
    Debug.Log($"Mouse X: {Input.GetAxis("Mouse X")}, Mouse Y: {Input.GetAxis("Mouse Y")}");
}
```

Se retorna 0, problema é no Input.

---

### UI não aparece

**Sintoma:** HUD, menus, etc não aparecem.

**Soluções:**

1. **Canvas não configurado:**
   - Canvas existe na cena?
   - Render Mode: Screen Space - Overlay (mais comum)

2. **EventSystem faltando:**
   - Deve existir GameObject "EventSystem" na cena
   - Se deletou, crie: GameObject > UI > Event System

3. **Referências não conectadas:**
   - Selecione PlayerHUD
   - Inspector: Todos os campos estão preenchidos?
   - Arraste elementos UI corretos

4. **Alpha = 0 ou Inactive:**
   - Canvas Group com alpha = 0?
   - GameObject está active?

5. **Camera renderizando por cima:**
   - Se usando múltiplas câmeras
   - Clear Flags e Depth podem causar problemas

**Debug:**
```csharp
void Start()
{
    Debug.Log($"Health Bar: {healthBar != null}");
    Debug.Log($"Stamina Bar: {staminaBar != null}");
    // etc...
}
```

---

### Audio não toca

**Sintoma:** Sem som no jogo.

**Soluções:**

1. **AudioManager não existe:**
   - GameObject "AudioManager" está na cena?
   - Script AudioManager está anexado?

2. **Arquivos em local errado:**
   - Devem estar em `Resources/Audio/[Music|SFX|Ambient]/`
   - Sem extensão no nome ao chamar:
     ```csharp
     // ✅ CERTO
     "door_open"

     // ❌ ERRADO
     "door_open.wav"
     ```

3. **Nome incorreto:**
   - Case-sensitive!
   - "Door_Open" ≠ "door_open"

4. **Volumes zerados:**
   - Master Volume > 0?
   - Music/SFX Volume > 0?

5. **Audio Listener faltando:**
   - Camera deve ter Audio Listener
   - Só pode haver UM Audio Listener na cena

**Debug:**
```csharp
void Start()
{
    AudioClip clip = Resources.Load<AudioClip>("Audio/SFX/door_open");
    Debug.Log($"Clip loaded: {clip != null}");
}
```

---

## ⚡ Performance

### FPS muito baixo

**Sintoma:** Jogo roda a <30 FPS.

**Diagnóstico:**

1. **Abra Profiler:**
   ```
   Window > Analysis > Profiler
   ```

2. **Identifique gargalo:**
   - CPU? (scripts, física)
   - GPU? (renderização)
   - Memory? (garbage collection)

**Soluções comuns:**

1. **Draw Calls alto (>1000):**
   - Use Static Batching
   - Habilite GPU Instancing em materials
   - Combine meshes similares

2. **Tris/Verts alto:**
   - Use LOD Groups
   - Simplifique geometria
   - Occlusion Culling

3. **Luzes demais:**
   - Limite luzes em tempo real
   - Use Baked Lighting
   - Combine shadow casting

4. **NavMesh Agents demais:**
   - Limite inimigos ativos
   - Use LOD para IA (desliga quando longe)

5. **Audio Sources demais:**
   - Limite 3D Audio Distance
   - Use pooling (já implementado)

6. **GC Spikes:**
   - Evite `new` em Update()
   - Use object pooling
   - Cache referências

**Settings recomendados:**
```csharp
// Em GameManager.Initialize()
Application.targetFrameRate = 60;
QualitySettings.vSyncCount = 0;
QualitySettings.shadows = ShadowQuality.HardOnly;
QualitySettings.shadowDistance = 50f;
```

---

### Memory leaks / Crash após jogar um tempo

**Sintoma:** Jogo usa cada vez mais RAM, eventualmente crasha.

**Causas comuns:**

1. **Eventos não desinscritos:**
   ```csharp
   // ❌ BAD - Causa leak
   void Start()
   {
       GameEvents.OnPlayerDamaged += HandleDamage;
   }
   // Faltando OnDestroy!

   // ✅ GOOD
   void OnDestroy()
   {
       GameEvents.OnPlayerDamaged -= HandleDamage;
   }
   ```

2. **Objetos não destruídos:**
   ```csharp
   // Inimigos mortos, balas, efeitos visuais
   Destroy(gameObject, lifetime);
   ```

3. **Resources nunca liberados:**
   ```csharp
   // Periodicamente
   Resources.UnloadUnusedAssets();
   System.GC.Collect();
   ```

4. **Coroutines infinitas sem cleanup:**
   ```csharp
   Coroutine myCoroutine;

   void OnDisable()
   {
       if (myCoroutine != null)
           StopCoroutine(myCoroutine);
   }
   ```

**Debug:**
```
Window > Analysis > Memory Profiler
```
Tire snapshot antes e depois de jogar.

---

## 🎮 Gameplay

### Player morre muito rápido

**Soluções:**

1. **Aumentar vida:**
   ```csharp
   // PlayerHealth
   maxHealth = 200f;
   ```

2. **Reduzir dano dos inimigos:**
   ```csharp
   // EnemyBase ou EnemyData
   damage = 5f; // Ao invés de 10f
   ```

3. **Aumentar invulnerabilidade:**
   ```csharp
   // PlayerHealth
   damageInvulnerabilityDuration = 1.5f; // Ao invés de 0.5f
   ```

4. **Adicionar regeneração:**
   ```csharp
   // PlayerHealth
   canRegenerate = true;
   regenRate = 5f;
   regenDelay = 5f;
   ```

---

### Stamina acaba muito rápido

**Soluções:**

```csharp
// FirstPersonController

// Aumentar stamina total
maxStamina = 150f;

// Reduzir drain
staminaDrainRate = 10f; // Ao invés de 20f

// Aumentar regeneração
staminaRegenRate = 25f; // Ao invés de 15f

// Reduzir delay
staminaRegenDelay = 1f; // Ao invés de 2f
```

---

### Inimigos muito difíceis/fáceis

**Muito difíceis:**
```csharp
// EnemyBase ou PatrolEnemy
damage = 5f;              // Reduzir dano
chaseSpeed = 4f;          // Mais lentos
detectionRange = 8f;      // Detectam de menos longe
attackCooldown = 2.5f;    // Atacam menos frequente
```

**Muito fáceis:**
```csharp
damage = 20f;             // Mais dano
chaseSpeed = 7f;          // Mais rápidos
detectionRange = 15f;     // Detectam de mais longe
attackCooldown = 1f;      // Atacam mais frequente
```

**Balanceamento ideal:**
- Player deve sobreviver ~3 hits
- Deve conseguir fugir se correr
- Stamina deve durar ~5-10 segundos
- Inimigo não deve ser mais rápido que player correndo

---

## 🎵 Audio

### Som muito baixo/alto

**Volume geral:**
```csharp
AudioManager.Instance.SetMasterVolume(0.7f); // 0-1
```

**Som específico muito alto:**
```csharp
// Ao tocar som
GameEvents.OnPlaySound2D?.Invoke("loud_sound");

// Com multiplier
AudioManager.Instance.PlaySound2D("loud_sound", 0.5f); // 50% volume
```

**Music vs SFX balance:**
```csharp
AudioManager.Instance.SetMusicVolume(0.6f);
AudioManager.Instance.SetSFXVolume(1.0f);
```

---

### Sons cortam/crackle

**Causas:**

1. **Audio Sources insuficientes:**
   ```csharp
   // AudioManager
   maxSFXSources = 20; // Aumente
   ```

2. **Sample rate incorreto:**
   - Selecione audio file
   - Inspector > Import Settings
   - Sample Rate: 44100 Hz
   - Compression Format: Vorbis (música) ou PCM (SFX curtos)

3. **Buffer size:**
   ```
   Edit > Project Settings > Audio
   DSP Buffer Size: Best Performance
   ```

---

## 🎨 UI

### Texto borrado/pixelado

**TextMesh Pro:**
1. Importe TMP:
   ```
   Window > TextMeshPro > Import TMP Essential Resources
   ```

2. Use TextMeshPro ao invés de Text (Legacy)

3. Font Size adequado:
   - UI: 24-36
   - Mensagens: 28-40

4. Font Asset Settings:
   - Atlas Resolution: 2048x2048 ou maior

---

### UI muito pequena/grande em diferentes resoluções

**Canvas Scaler:**
```csharp
// No Canvas
UI Scale Mode: Scale With Screen Size
Reference Resolution: 1920 x 1080
Screen Match Mode: Match Width Or Height
Match: 0.5
```

**Anchors:**
- Use anchors apropriadamente!
- Ctrl + Shift + Click no Anchor preset = Set Position também

---

## 🤖 Inimigos e IA

### Inimigo atravessa paredes

**Soluções:**

1. **NavMesh mal configurado:**
   - Rebake NavMesh
   - Paredes devem ser Navigation Static

2. **Agent Radius muito pequeno:**
   ```
   Navigation > Bake
   Agent Radius: 0.5 (ajuste conforme inimigo)
   ```

3. **Velocidade muito alta:**
   - NavMeshAgent não consegue calcular path rápido o suficiente
   - Reduza speed ou aumente Angular Speed

4. **Obstacle Avoidance:**
   ```csharp
   // NavMeshAgent
   Obstacle Avoidance Type: High Quality
   Avoidance Priority: 50
   ```

---

### Inimigo fica preso em cantos

**Soluções:**

1. **Carve NavMesh Obstacles:**
   - Objetos que devem bloquear path
   - Add Component > NavMesh Obstacle
   - Carve: ✅

2. **Agent Radius vs Level Geometry:**
   - Corredores muito estreitos?
   - Mínimo: Agent Radius * 2 + margem

3. **Off-Mesh Links:**
   - Para gaps, escadas, etc
   - GameObject > AI > Nav Mesh Link

4. **Timeout no Investigate:**
   ```csharp
   // Se inimigo fica preso investigando
   investigateTime = 3f; // Reduzir
   ```

---

### Inimigo não detecta player

**Debug:**
```csharp
// No EnemyBase
void OnDrawGizmos()
{
    // Detection range
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRange);

    // FOV
    Vector3 fovLine1 = Quaternion.AngleAxis(fieldOfView / 2f, transform.up) * transform.forward * detectionRange;
    Gizmos.DrawRay(transform.position, fovLine1);
}
```

**Verificar:**
1. Detection Range suficiente?
2. Field of View correto? (120° é bom)
3. Layer Mask inclui player?
4. Raycast sendo bloqueado por algo?

---

## 📦 Build e Deploy

### Build não funciona

**Erros comuns:**

1. **Cenas não adicionadas:**
   ```
   File > Build Settings > Add Open Scenes
   ```
   Todas as cenas devem estar na lista!

2. **Resources não encontrados:**
   - Tudo em Resources/ foi incluído?
   - Nomes de arquivos corretos?

3. **Platform não instalada:**
   ```
   File > Build Settings > Install Module
   ```

4. **Erros de compilação:**
   - Console deve estar limpo antes de buildar
   - Fix todos os errors

---

### Build roda mas não funciona direito

**Player Prefs:**
```csharp
// Data persiste entre builds de desenvolvimento
// Limpe para testar clean:
PlayerPrefs.DeleteAll();
```

**Paths absolutos:**
```csharp
// ❌ BAD
"/Users/seu_nome/projeto/Assets/file.txt"

// ✅ GOOD
Application.persistentDataPath + "/file.txt"
Application.dataPath // Assets folder
Application.streamingAssetsPath // StreamingAssets
```

**Resources:**
- Só funciona se em pasta "Resources"
- Case-sensitive mesmo em builds

---

## ❓ FAQ Geral

### Q: Preciso do Unity 6 exatamente?

**A:** Recomendado Unity 6.0+, mas deve funcionar em:
- Unity 2022.3 LTS com ajustes mínimos
- Unity 2023.1+

Se usar versão mais antiga:
- Remova `#nullable` directives se houver
- Atualize TextMesh Pro
- Algumas features podem não estar disponíveis

---

### Q: Posso usar com VR?

**A:** Não diretamente. Precisaria:
- Substituir FirstPersonController por VR controller
- Ajustar UI para VR (Canvas World Space)
- Adicionar XR Interaction Toolkit
- Testar intensamente (VR horror é INTENSO!)

---

### Q: Como adicionar gamepad support?

**A:** Use New Input System:
1. Install: Package Manager > Input System
2. Crie Input Actions Asset
3. Configure bindings para gamepad
4. Substitua Input.GetAxis por InputAction.ReadValue

Ou manualmente:
```csharp
// Detecta gamepad
if (Input.GetJoystickNames().Length > 0)
{
    float horizontal = Input.GetAxis("Horizontal");
    float vertical = Input.GetAxis("Vertical");
    // etc
}
```

---

### Q: Como fazer multiplayer?

**A:** Não é suportado nativamente. Precisaria:
- Netcode (Unity Netcode, Photon, Mirror)
- Sincronizar posição de players
- Sincronizar inimigos (host authority)
- Muito trabalho adicional!

Para co-op local (split-screen):
- Mais viável
- Precisa duplicar câmeras e players
- Ajustar UI

---

### Q: Posso vender jogo feito com este framework?

**A:** Sim! É seu projeto. O framework é open source.

**Mas:**
- Dê créditos se usar assets third-party
- Verifique licenças de plugins/assets
- Unity tem suas próprias license terms

---

### Q: Como adicionar mais tipos de inimigos?

**A:** Crie nova classe herdando de `EnemyBase`:

```csharp
public class FastEnemy : EnemyBase
{
    protected override void UpdateAI()
    {
        // Lógica customizada
    }

    protected override void OnAttack()
    {
        // Ataque customizado
    }
}
```

Veja EXAMPLES.md para mais detalhes.

---

### Q: Como salvar progresso?

**A:** Veja SaveSystem em TIPS_AND_IMPROVEMENTS.md.

Básico:
```csharp
// Salvar
SaveData data = new SaveData();
data.playerPosition = player.transform.position;
// ... outros dados
SaveSystem.SaveGame(data);

// Carregar
SaveData data = SaveSystem.LoadGame();
if (data != null)
{
    player.transform.position = data.playerPosition;
    // ... outros dados
}
```

---

### Q: Como otimizar para mobile?

**Difícil!** Jogos de terror 3D não são ideais para mobile.

Se for fazer:
1. Reduza qualidade gráfica drasticamente
2. Limite inimigos ativos (max 3-5)
3. Use lightmaps ao invés de real-time
4. Simplifique geometria
5. Otimize texturas (max 512x512)
6. Reduza draw distance
7. Desabilite sombras em tempo real
8. Use mobile shaders

---

### Q: Onde achar assets gratuitos?

**Models/Props:**
- Unity Asset Store (filter: Free)
- Sketchfab (alguns são free)
- OpenGameArt.org
- Kenney.nl (muito conteúdo grátis!)

**Sons:**
- Freesound.org
- OpenGameArt.org
- BBC Sound Effects
- Zapsplat (free tier)

**Música:**
- Incompetech (Kevin MacLeod)
- Purple Planet Music
- Bensound

**Sempre verifique licenças!**

---

### Q: Como fazer o jogo mais assustador?

Veja TIPS_AND_IMPROVEMENTS.md seção "Design de Terror".

**Quick tips:**
1. Som > Visual (sempre!)
2. Menos é mais (não mostre o monstro muito)
3. Antecipação > Jumpscare
4. Deixe player vulnerável
5. Recursos limitados (bateria, munição)
6. Iluminação é TUDO
7. Teste com pessoas reais
8. Estude jogos como Amnesia, Outlast

---

### Q: Preciso saber programar?

**Para usar framework:** Básico de C# ajuda muito.

**Para customizar:** Sim, programação intermediária.

**Recursos de aprendizado:**
- Unity Learn (learn.unity.com) - FREE
- Brackeys no YouTube
- C# Yellow Book (free PDF)
- Unity Documentation

---

### Q: Quanto tempo leva para fazer um jogo?

**Protótipo jogável:** 1-2 semanas
**Demo de 10 minutos:** 1-3 meses
**Jogo completo (2-3 horas):** 6-12+ meses

Depende de:
- Experiência
- Escopo
- Assets disponíveis
- Solo vs Team
- Tempo dedicado

**Dica:** Comece pequeno!

---

## 🆘 Ainda com Problemas?

1. **Console errors:**
   - Sempre leia os errors completamente
   - Google o erro específico
   - Stack trace mostra onde está o problema

2. **Unity Docs:**
   - docs.unity3d.com
   - Muito completo!

3. **Forums:**
   - Unity Forum
   - Reddit: r/Unity3D, r/gamedev
   - Stack Overflow

4. **Discord:**
   - Unity Discord server
   - Game Dev comunidades

5. **Debug:**
   - Use Debug.Log MUITO
   - Use breakpoints (Visual Studio/Rider)
   - Use Profiler para performance

---

## 📝 Reporting Issues

Se encontrar um bug no framework:

1. **Verifique se não está nesta FAQ**
2. **Teste em cena limpa**
3. **Anote:**
   - Unity version
   - O que você tentou fazer
   - O que aconteceu
   - Console errors (copie completo!)
   - Steps to reproduce

---

**Lembre-se:** Debugging faz parte do desenvolvimento! Não desista! 💪🎮

A maioria dos problemas tem solução simples. Respire, leia o erro com calma, e tente de novo.

Boa sorte! 👻
