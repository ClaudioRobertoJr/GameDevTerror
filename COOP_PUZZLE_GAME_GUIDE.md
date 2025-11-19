# 🎮👥 Guia Completo: Jogo de Terror Cooperativo com Puzzles

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Por que Co-op + Puzzles + Terror?](#por-que-co-op--puzzles--terror)
- [Arquitetura do Sistema Multiplayer](#arquitetura-do-sistema-multiplayer)
- [Sistema de Puzzles Completo](#sistema-de-puzzles-completo)
- [Integração Co-op + Puzzles](#integração-co-op--puzzles)
- [Mecânicas Cooperativas](#mecânicas-cooperativas)
- [Design de Terror em Co-op](#design-de-terror-em-co-op)
- [Implementação Passo a Passo](#implementação-passo-a-passo)
- [Exemplos de Puzzles](#exemplos-de-puzzles)
- [Balanceamento Solo vs Co-op](#balanceamento-solo-vs-co-op)
- [Performance e Otimizações](#performance-e-otimizações)
- [Troubleshooting Multiplayer](#troubleshooting-multiplayer)

---

## 🎯 Visão Geral

Este guia transforma o framework base de terror FPS em um **jogo cooperativo completo com sistema de puzzles**, mantendo a possibilidade de jogar **solo**.

### Características Principais

✅ **Multiplayer cooperativo** (2-4 jogadores)
✅ **Sistema de puzzles** modular e expansível
✅ **Puzzles cooperativos** que exigem trabalho em equipe
✅ **Puzzles solo** ajustados automaticamente
✅ **Sincronização** de inimigos, items e estado do mundo
✅ **Comunicação** entre jogadores (voz/texto/emotes)
✅ **Progressão compartilhada**

---

## 🤔 Por que Co-op + Puzzles + Terror?

### Exemplos de Sucesso
- **Phasmophobia** - Co-op ghost hunting
- **Lethal Company** - Co-op extraction horror
- **The Forest** - Survival co-op horror
- **Devour** - Puzzle solving co-op horror
- **Escape the Backrooms** - Co-op puzzle horror

### O que funciona:
1. **Terror dinâmico** - Jogadores se separam, criando tensão
2. **Puzzles cooperativos** - Requerem comunicação e coordenação
3. **Resource sharing** - Decisões difíceis sobre quem leva o quê
4. **Revive system** - Drama quando alguém morre
5. **Role assignment** - Cada jogador tem função específica

---

## 🌐 Arquitetura do Sistema Multiplayer

### Opções de Networking

#### 1. **Unity Netcode for GameObjects (RECOMENDADO)**
```csharp
// Gratuito, oficial, moderno
// Suporta: Host-Client, Dedicated Server
// Ideal para: 2-8 jogadores
```

#### 2. **Mirror Networking**
```csharp
// Open-source, estável
// Baseado em UNET
// Ideal para: Indie devs
```

#### 3. **Photon PUN 2**
```csharp
// Cloud-based, fácil setup
// Pago após limite de CCU
// Ideal para: Protótipos rápidos
```

### Arquitetura Recomendada: **Host-Client com Netcode**

```
Host (Player 1)
    ├── Simula o mundo
    ├── Autoridade sobre inimigos
    ├── Sincroniza estado global
    └── Valida ações dos clientes

Clients (Players 2-4)
    ├── Enviam inputs ao host
    ├── Recebem estado do mundo
    └── Renderizam localmente
```

---

## 🔧 Sistema de Puzzles Completo

### Arquitetura Base

```csharp
using UnityEngine;
using System.Collections.Generic;
using HorrorGame.Core;

namespace HorrorGame.Puzzles
{
    /// <summary>
    /// Classe base abstrata para todos os puzzles
    /// </summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        [Header("Puzzle Settings")]
        [SerializeField] protected string puzzleID;
        [SerializeField] protected string puzzleName;
        [TextArea] [SerializeField] protected string puzzleDescription;

        [Header("Configuration")]
        [SerializeField] protected bool requiresMultiplePlayers = false;
        [SerializeField] protected int minPlayersRequired = 1;
        [SerializeField] protected int maxPlayersRequired = 4;

        [Header("Rewards")]
        [SerializeField] protected GameObject[] objectsToActivate;
        [SerializeField] protected GameObject[] objectsToDeactivate;
        [SerializeField] protected AudioClip solvedSound;

        [Header("State")]
        [SerializeField] protected bool isActive = true;
        [SerializeField] protected bool isSolved = false;

        // Eventos
        public System.Action<PuzzleBase> OnPuzzleStarted;
        public System.Action<PuzzleBase> OnPuzzleProgress;
        public System.Action<PuzzleBase> OnPuzzleSolved;
        public System.Action<PuzzleBase> OnPuzzleFailed;

        // Propriedades
        public string PuzzleID => puzzleID;
        public bool IsSolved => isSolved;
        public bool IsActive => isActive;
        public int CurrentPlayersEngaged { get; protected set; }

        protected virtual void Start()
        {
            if (string.IsNullOrEmpty(puzzleID))
            {
                puzzleID = System.Guid.NewGuid().ToString();
            }

            InitializePuzzle();
        }

        /// <summary>
        /// Inicializa o puzzle (override para setup customizado)
        /// </summary>
        protected virtual void InitializePuzzle()
        {
            Debug.Log($"[Puzzle] {puzzleName} initialized");
        }

        /// <summary>
        /// Ativa o puzzle
        /// </summary>
        public virtual void ActivatePuzzle()
        {
            if (isSolved) return;

            isActive = true;
            OnPuzzleStarted?.Invoke(this);
            GameEvents.OnShowMessage?.Invoke($"Puzzle: {puzzleName}", 3f);

            Debug.Log($"[Puzzle] {puzzleName} activated");
        }

        /// <summary>
        /// Player interage com o puzzle
        /// </summary>
        public virtual void PlayerInteract(GameObject player)
        {
            if (!isActive || isSolved) return;

            OnPlayerInteract(player);
        }

        /// <summary>
        /// Lógica de interação (DEVE ser implementado)
        /// </summary>
        protected abstract void OnPlayerInteract(GameObject player);

        /// <summary>
        /// Valida se o puzzle foi resolvido (DEVE ser implementado)
        /// </summary>
        protected abstract bool ValidateSolution();

        /// <summary>
        /// Reseta o puzzle ao estado inicial
        /// </summary>
        public virtual void ResetPuzzle()
        {
            isSolved = false;
            isActive = true;
            CurrentPlayersEngaged = 0;

            OnPuzzleReset();

            Debug.Log($"[Puzzle] {puzzleName} reset");
        }

        /// <summary>
        /// Lógica customizada de reset
        /// </summary>
        protected virtual void OnPuzzleReset() { }

        /// <summary>
        /// Marca puzzle como resolvido
        /// </summary>
        protected virtual void CompletePuzzle()
        {
            if (isSolved) return;

            isSolved = true;
            isActive = false;

            // Ativa/desativa objetos
            foreach (var obj in objectsToActivate)
                if (obj != null) obj.SetActive(true);

            foreach (var obj in objectsToDeactivate)
                if (obj != null) obj.SetActive(false);

            // Toca som
            if (solvedSound != null)
                GameEvents.OnPlaySound2D?.Invoke(solvedSound.name);

            // Dispara eventos
            OnPuzzleSolved?.Invoke(this);
            GameEvents.OnShowMessage?.Invoke($"{puzzleName} Solved!", 3f);

            Debug.Log($"[Puzzle] {puzzleName} completed!");

            OnPuzzleCompleted();
        }

        /// <summary>
        /// Callback quando puzzle é completado
        /// </summary>
        protected virtual void OnPuzzleCompleted() { }

        /// <summary>
        /// Registra player engajado no puzzle
        /// </summary>
        protected void RegisterPlayer(GameObject player)
        {
            CurrentPlayersEngaged++;
            Debug.Log($"[Puzzle] Player engaged. Total: {CurrentPlayersEngaged}");
        }

        /// <summary>
        /// Remove player do puzzle
        /// </summary>
        protected void UnregisterPlayer(GameObject player)
        {
            CurrentPlayersEngaged = Mathf.Max(0, CurrentPlayersEngaged - 1);
            Debug.Log($"[Puzzle] Player left. Total: {CurrentPlayersEngaged}");
        }

        /// <summary>
        /// Verifica se há jogadores suficientes
        /// </summary>
        protected bool HasEnoughPlayers()
        {
            return CurrentPlayersEngaged >= minPlayersRequired;
        }

        /// <summary>
        /// Dica visual no editor
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = isSolved ? Color.green : (isActive ? Color.yellow : Color.gray);
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
```

### Gerenciador de Puzzles

```csharp
using System.Collections.Generic;
using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Puzzles
{
    /// <summary>
    /// Gerencia todos os puzzles do jogo
    /// </summary>
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager Instance { get; private set; }

        [Header("Puzzle Tracking")]
        [SerializeField] private List<PuzzleBase> allPuzzles = new List<PuzzleBase>();

        private Dictionary<string, PuzzleBase> puzzleRegistry = new Dictionary<string, PuzzleBase>();

        // Eventos
        public System.Action<PuzzleBase> OnAnyPuzzleSolved;
        public System.Action OnAllPuzzlesSolved;

        // Stats
        public int TotalPuzzles => allPuzzles.Count;
        public int SolvedPuzzles => GetSolvedPuzzleCount();
        public float CompletionPercentage => TotalPuzzles > 0 ? (float)SolvedPuzzles / TotalPuzzles : 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            DiscoverPuzzles();
            RegisterPuzzleEvents();
        }

        /// <summary>
        /// Encontra todos os puzzles na cena
        /// </summary>
        private void DiscoverPuzzles()
        {
            PuzzleBase[] foundPuzzles = FindObjectsOfType<PuzzleBase>();

            foreach (var puzzle in foundPuzzles)
            {
                RegisterPuzzle(puzzle);
            }

            Debug.Log($"[PuzzleManager] Discovered {allPuzzles.Count} puzzles");
        }

        /// <summary>
        /// Registra um puzzle
        /// </summary>
        public void RegisterPuzzle(PuzzleBase puzzle)
        {
            if (!allPuzzles.Contains(puzzle))
            {
                allPuzzles.Add(puzzle);
                puzzleRegistry[puzzle.PuzzleID] = puzzle;

                // Subscreve eventos
                puzzle.OnPuzzleSolved += HandlePuzzleSolved;
            }
        }

        /// <summary>
        /// Remove puzzle do registro
        /// </summary>
        public void UnregisterPuzzle(PuzzleBase puzzle)
        {
            if (allPuzzles.Contains(puzzle))
            {
                allPuzzles.Remove(puzzle);
                puzzleRegistry.Remove(puzzle.PuzzleID);

                puzzle.OnPuzzleSolved -= HandlePuzzleSolved;
            }
        }

        /// <summary>
        /// Busca puzzle por ID
        /// </summary>
        public PuzzleBase GetPuzzle(string puzzleID)
        {
            return puzzleRegistry.ContainsKey(puzzleID) ? puzzleRegistry[puzzleID] : null;
        }

        /// <summary>
        /// Reseta todos os puzzles
        /// </summary>
        public void ResetAllPuzzles()
        {
            foreach (var puzzle in allPuzzles)
            {
                puzzle.ResetPuzzle();
            }

            Debug.Log("[PuzzleManager] All puzzles reset");
        }

        /// <summary>
        /// Conta puzzles resolvidos
        /// </summary>
        private int GetSolvedPuzzleCount()
        {
            int count = 0;
            foreach (var puzzle in allPuzzles)
            {
                if (puzzle.IsSolved) count++;
            }
            return count;
        }

        /// <summary>
        /// Verifica se todos foram resolvidos
        /// </summary>
        public bool AreAllPuzzlesSolved()
        {
            foreach (var puzzle in allPuzzles)
            {
                if (!puzzle.IsSolved) return false;
            }
            return true;
        }

        /// <summary>
        /// Handler quando puzzle é resolvido
        /// </summary>
        private void HandlePuzzleSolved(PuzzleBase puzzle)
        {
            OnAnyPuzzleSolved?.Invoke(puzzle);

            Debug.Log($"[PuzzleManager] Puzzle solved: {puzzle.PuzzleID} ({SolvedPuzzles}/{TotalPuzzles})");

            if (AreAllPuzzlesSolved())
            {
                OnAllPuzzlesSolved?.Invoke();
                Debug.Log("[PuzzleManager] ALL PUZZLES SOLVED!");
            }
        }

        /// <summary>
        /// Registra eventos globais
        /// </summary>
        private void RegisterPuzzleEvents()
        {
            // Você pode adicionar listeners para GameEvents aqui
        }

        private void OnDestroy()
        {
            // Limpa eventos
            foreach (var puzzle in allPuzzles)
            {
                if (puzzle != null)
                    puzzle.OnPuzzleSolved -= HandlePuzzleSolved;
            }
        }
    }
}
```

---

## 🧩 Exemplos de Puzzles

### 1. Puzzle de Código (4 Dígitos)

```csharp
using UnityEngine;
using TMPro;
using HorrorGame.Core;

namespace HorrorGame.Puzzles
{
    /// <summary>
    /// Puzzle onde jogadores devem inserir um código de 4 dígitos
    /// </summary>
    public class CodePuzzle : PuzzleBase
    {
        [Header("Code Settings")]
        [SerializeField] private string correctCode = "1234";
        [SerializeField] private TextMeshProUGUI displayText;
        [SerializeField] private int maxAttempts = 5;

        private string currentInput = "";
        private int attemptsMade = 0;

        protected override void InitializePuzzle()
        {
            base.InitializePuzzle();
            UpdateDisplay();
        }

        protected override void OnPlayerInteract(GameObject player)
        {
            // Esta função será chamada pelo UI
        }

        /// <summary>
        /// Adiciona dígito ao código
        /// </summary>
        public void AddDigit(int digit)
        {
            if (currentInput.Length < 4)
            {
                currentInput += digit.ToString();
                UpdateDisplay();

                if (currentInput.Length == 4)
                {
                    CheckCode();
                }
            }
        }

        /// <summary>
        /// Remove último dígito
        /// </summary>
        public void RemoveDigit()
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Limpa o código
        /// </summary>
        public void ClearCode()
        {
            currentInput = "";
            UpdateDisplay();
        }

        /// <summary>
        /// Verifica se o código está correto
        /// </summary>
        private void CheckCode()
        {
            attemptsMade++;

            if (ValidateSolution())
            {
                CompletePuzzle();
            }
            else
            {
                // Código errado
                GameEvents.OnPlaySound2D?.Invoke("buzzer");
                GameEvents.OnShowMessage?.Invoke($"Wrong code! ({attemptsMade}/{maxAttempts})", 2f);

                currentInput = "";
                UpdateDisplay();

                if (attemptsMade >= maxAttempts)
                {
                    // Falhou muitas vezes
                    OnPuzzleFailed?.Invoke(this);
                    isActive = false;
                }
            }
        }

        protected override bool ValidateSolution()
        {
            return currentInput == correctCode;
        }

        protected override void OnPuzzleReset()
        {
            currentInput = "";
            attemptsMade = 0;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (displayText != null)
            {
                displayText.text = currentInput.PadRight(4, '_');
            }
        }
    }
}
```

### 2. Puzzle de Alavancas (Cooperativo - 2 Players)

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace HorrorGame.Puzzles
{
    /// <summary>
    /// Puzzle onde 2 jogadores devem puxar alavancas simultaneamente
    /// </summary>
    public class DualLeverPuzzle : PuzzleBase
    {
        [Header("Lever Settings")]
        [SerializeField] private LeverInteractable lever1;
        [SerializeField] private LeverInteractable lever2;
        [SerializeField] private float simultaneousWindow = 2f; // Janela de tempo em segundos

        private float lever1PullTime = -1f;
        private float lever2PullTime = -1f;
        private HashSet<GameObject> playersAtLevers = new HashSet<GameObject>();

        protected override void InitializePuzzle()
        {
            base.InitializePuzzle();

            minPlayersRequired = 2;
            requiresMultiplePlayers = true;

            // Configura callbacks das alavancas
            if (lever1 != null)
                lever1.OnLeverPulled += () => OnLeverPulled(1);

            if (lever2 != null)
                lever2.OnLeverPulled += () => OnLeverPulled(2);
        }

        protected override void OnPlayerInteract(GameObject player)
        {
            // Players interagem com as alavancas individuais
        }

        private void OnLeverPulled(int leverNumber)
        {
            float currentTime = Time.time;

            if (leverNumber == 1)
            {
                lever1PullTime = currentTime;
                Debug.Log("[DualLever] Lever 1 pulled");
            }
            else
            {
                lever2PullTime = currentTime;
                Debug.Log("[DualLever] Lever 2 pulled");
            }

            // Verifica se ambas foram puxadas dentro da janela de tempo
            if (lever1PullTime > 0 && lever2PullTime > 0)
            {
                float timeDifference = Mathf.Abs(lever1PullTime - lever2PullTime);

                if (timeDifference <= simultaneousWindow)
                {
                    // Sucesso!
                    CompletePuzzle();
                }
                else
                {
                    // Muito lento
                    GameEvents.OnShowMessage?.Invoke("Too slow! Try together!", 2f);
                    ResetLevers();
                }
            }
        }

        protected override bool ValidateSolution()
        {
            float timeDiff = Mathf.Abs(lever1PullTime - lever2PullTime);
            return timeDiff <= simultaneousWindow;
        }

        private void ResetLevers()
        {
            lever1PullTime = -1f;
            lever2PullTime = -1f;

            if (lever1 != null) lever1.Reset();
            if (lever2 != null) lever2.Reset();
        }

        protected override void OnPuzzleReset()
        {
            ResetLevers();
            playersAtLevers.Clear();
        }
    }

    /// <summary>
    /// Componente de alavanca interativa
    /// </summary>
    public class LeverInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool isPulled = false;
        [SerializeField] private Transform leverVisual;
        [SerializeField] private float pulledRotation = -45f;

        public System.Action OnLeverPulled;

        public string GetInteractionPrompt()
        {
            return isPulled ? "Already pulled" : "Press E to pull lever";
        }

        public bool CanInteract()
        {
            return !isPulled;
        }

        public void Interact(GameObject player)
        {
            if (!CanInteract()) return;

            isPulled = true;

            // Anima alavanca
            if (leverVisual != null)
            {
                leverVisual.localRotation = Quaternion.Euler(pulledRotation, 0, 0);
            }

            OnLeverPulled?.Invoke();
        }

        public void Reset()
        {
            isPulled = false;

            if (leverVisual != null)
            {
                leverVisual.localRotation = Quaternion.identity;
            }
        }
    }
}
```

### 3. Puzzle de Símbolos/Pattern

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace HorrorGame.Puzzles
{
    /// <summary>
    /// Puzzle onde jogador deve ativar símbolos na ordem correta
    /// </summary>
    public class SymbolPuzzle : PuzzleBase
    {
        [System.Serializable]
        public class Symbol
        {
            public GameObject symbolObject;
            public int symbolID;
            public bool isActive;
            public Renderer symbolRenderer;
        }

        [Header("Symbol Settings")]
        [SerializeField] private List<Symbol> symbols = new List<Symbol>();
        [SerializeField] private List<int> correctSequence = new List<int>(); // Ex: 1, 3, 2, 4

        [Header("Visual Feedback")]
        [SerializeField] private Material activeSymbolMaterial;
        [SerializeField] private Material inactiveSymbolMaterial;

        private List<int> currentSequence = new List<int>();

        protected override void InitializePuzzle()
        {
            base.InitializePuzzle();
            ResetSymbols();
        }

        protected override void OnPlayerInteract(GameObject player)
        {
            // Será chamado pelos objetos de símbolo individuais
        }

        /// <summary>
        /// Ativa um símbolo
        /// </summary>
        public void ActivateSymbol(int symbolID)
        {
            if (!isActive || isSolved) return;

            // Adiciona à sequência atual
            currentSequence.Add(symbolID);

            // Feedback visual
            Symbol symbol = symbols.Find(s => s.symbolID == symbolID);
            if (symbol != null && symbol.symbolRenderer != null)
            {
                symbol.isActive = true;
                symbol.symbolRenderer.material = activeSymbolMaterial;
            }

            // Toca som
            GameEvents.OnPlaySound2D?.Invoke("symbol_activate");

            // Verifica se completou
            if (currentSequence.Count == correctSequence.Count)
            {
                if (ValidateSolution())
                {
                    CompletePuzzle();
                }
                else
                {
                    // Sequência errada
                    GameEvents.OnShowMessage?.Invoke("Wrong sequence!", 2f);
                    GameEvents.OnPlaySound2D?.Invoke("buzzer");
                    Invoke(nameof(ResetSymbols), 1f);
                }
            }
        }

        protected override bool ValidateSolution()
        {
            if (currentSequence.Count != correctSequence.Count)
                return false;

            for (int i = 0; i < correctSequence.Count; i++)
            {
                if (currentSequence[i] != correctSequence[i])
                    return false;
            }

            return true;
        }

        private void ResetSymbols()
        {
            currentSequence.Clear();

            foreach (var symbol in symbols)
            {
                symbol.isActive = false;
                if (symbol.symbolRenderer != null)
                {
                    symbol.symbolRenderer.material = inactiveSymbolMaterial;
                }
            }
        }

        protected override void OnPuzzleReset()
        {
            ResetSymbols();
        }
    }
}
```

### 4. Puzzle de Peso/Pressure Plates (Cooperativo)

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace HorrorGame.Puzzles
{
    /// <summary>
    /// Puzzle onde múltiplos jogadores devem ficar em placas de pressão
    /// </summary>
    public class PressurePlatePuzzle : PuzzleBase
    {
        [System.Serializable]
        public class PressurePlate
        {
            public GameObject plateObject;
            public bool isPressed;
            public Transform visualIndicator;
            [HideInInspector] public List<GameObject> objectsOnPlate = new List<GameObject>();
        }

        [Header("Plate Settings")]
        [SerializeField] private List<PressurePlate> plates = new List<PressurePlate>();
        [SerializeField] private bool requireAllPlates = true;
        [SerializeField] private float activationDelay = 1f; // Tempo que todos devem ficar nas placas

        private float allPlatesActiveTime = 0f;
        private bool allPlatesActive = false;

        protected override void InitializePuzzle()
        {
            base.InitializePuzzle();
            minPlayersRequired = plates.Count;
            requiresMultiplePlayers = plates.Count > 1;

            // Configura triggers nas placas
            foreach (var plate in plates)
            {
                if (plate.plateObject != null)
                {
                    var trigger = plate.plateObject.GetComponent<PressurePlateTrigger>();
                    if (trigger == null)
                    {
                        trigger = plate.plateObject.AddComponent<PressurePlateTrigger>();
                    }
                    trigger.Initialize(this, plate);
                }
            }
        }

        protected override void OnPlayerInteract(GameObject player)
        {
            // Interação é automática via triggers
        }

        private void Update()
        {
            if (!isActive || isSolved) return;

            CheckPlateStatus();
        }

        private void CheckPlateStatus()
        {
            bool allActive = true;
            int activePlates = 0;

            foreach (var plate in plates)
            {
                bool isActive = plate.objectsOnPlate.Count > 0;
                plate.isPressed = isActive;

                if (isActive)
                {
                    activePlates++;
                    UpdatePlateVisual(plate, true);
                }
                else
                {
                    allActive = false;
                    UpdatePlateVisual(plate, false);
                }
            }

            if (allActive && requireAllPlates)
            {
                if (!allPlatesActive)
                {
                    allPlatesActive = true;
                    allPlatesActiveTime = Time.time;
                    GameEvents.OnShowMessage?.Invoke("Hold position...", activationDelay);
                }

                // Verifica se passaram tempo suficiente
                if (Time.time - allPlatesActiveTime >= activationDelay)
                {
                    CompletePuzzle();
                }
            }
            else
            {
                allPlatesActive = false;
            }
        }

        protected override bool ValidateSolution()
        {
            foreach (var plate in plates)
            {
                if (plate.objectsOnPlate.Count == 0)
                    return false;
            }
            return true;
        }

        private void UpdatePlateVisual(PressurePlate plate, bool pressed)
        {
            if (plate.visualIndicator != null)
            {
                // Move placa pra baixo quando pressionada
                float targetY = pressed ? -0.1f : 0f;
                Vector3 pos = plate.visualIndicator.localPosition;
                pos.y = targetY;
                plate.visualIndicator.localPosition = pos;
            }
        }

        public void OnPlayerEnterPlate(PressurePlate plate, GameObject player)
        {
            if (!plate.objectsOnPlate.Contains(player))
            {
                plate.objectsOnPlate.Add(player);
                GameEvents.OnPlaySound2D?.Invoke("plate_press");
            }
        }

        public void OnPlayerExitPlate(PressurePlate plate, GameObject player)
        {
            if (plate.objectsOnPlate.Contains(player))
            {
                plate.objectsOnPlate.Remove(player);
                GameEvents.OnPlaySound2D?.Invoke("plate_release");
            }
        }

        protected override void OnPuzzleReset()
        {
            foreach (var plate in plates)
            {
                plate.isPressed = false;
                plate.objectsOnPlate.Clear();
                UpdatePlateVisual(plate, false);
            }

            allPlatesActive = false;
            allPlatesActiveTime = 0f;
        }
    }

    /// <summary>
    /// Componente trigger para placas de pressão
    /// </summary>
    public class PressurePlateTrigger : MonoBehaviour
    {
        private PressurePlatePuzzle puzzle;
        private PressurePlatePuzzle.PressurePlate plate;

        public void Initialize(PressurePlatePuzzle puzzleRef, PressurePlatePuzzle.PressurePlate plateRef)
        {
            puzzle = puzzleRef;
            plate = plateRef;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                puzzle?.OnPlayerEnterPlate(plate, other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                puzzle?.OnPlayerExitPlate(plate, other.gameObject);
            }
        }
    }
}
```

---

## 🌐 Sistema Multiplayer com Unity Netcode

### Setup Inicial

```bash
# Instale via Package Manager:
# Window > Package Manager > Unity Registry > Netcode for GameObjects
```

### NetworkManager Setup

```csharp
using Unity.Netcode;
using UnityEngine;

namespace HorrorGame.Network
{
    /// <summary>
    /// Gerencia conexão e sessões multiplayer
    /// </summary>
    public class CoopNetworkManager : MonoBehaviour
    {
        public static CoopNetworkManager Instance { get; private set; }

        [Header("Network Settings")]
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private ushort port = 7777;

        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;

        // Estado
        public bool IsHost => NetworkManager.Singleton.IsHost;
        public bool IsClient => NetworkManager.Singleton.IsClient;
        public bool IsConnected => NetworkManager.Singleton.IsConnectedClient;
        public int ConnectedPlayers => NetworkManager.Singleton.ConnectedClientsList.Count;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Inicia como host (servidor + cliente)
        /// </summary>
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("[Network] Started as Host");
        }

        /// <summary>
        /// Inicia como servidor dedicado
        /// </summary>
        public void StartServer()
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log("[Network] Started as Server");
        }

        /// <summary>
        /// Conecta como cliente
        /// </summary>
        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("[Network] Started as Client");
        }

        /// <summary>
        /// Desconecta
        /// </summary>
        public void Disconnect()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
                Debug.Log("[Network] Disconnected");
            }
        }

        /// <summary>
        /// Kicka jogador (apenas host)
        /// </summary>
        public void KickPlayer(ulong clientId)
        {
            if (!IsHost) return;

            NetworkManager.Singleton.DisconnectClient(clientId);
            Debug.Log($"[Network] Kicked player {clientId}");
        }
    }
}
```

### Network Player Controller

```csharp
using Unity.Netcode;
using UnityEngine;

namespace HorrorGame.Network
{
    /// <summary>
    /// Controla player em rede
    /// </summary>
    public class NetworkPlayerController : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private FirstPersonController fpsController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Camera playerCamera;

        [Header("Network Sync")]
        [SerializeField] private float syncInterval = 0.1f;

        // Network Variables (sincronizadas automaticamente)
        private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
        private NetworkVariable<float> networkHealth = new NetworkVariable<float>(100f);

        private float lastSyncTime;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Só ativa controles e câmera para o dono
            if (IsOwner)
            {
                fpsController.enabled = true;
                playerCamera.enabled = true;
                playerCamera.GetComponent<AudioListener>().enabled = true;
            }
            else
            {
                fpsController.enabled = false;
                playerCamera.enabled = false;
                playerCamera.GetComponent<AudioListener>().enabled = false;
            }

            // Subscreve mudanças de variáveis
            networkHealth.OnValueChanged += OnHealthChanged;
        }

        public override void OnNetworkDespawn()
        {
            networkHealth.OnValueChanged -= OnHealthChanged;
            base.OnNetworkDespawn();
        }

        private void Update()
        {
            if (IsOwner)
            {
                // Dono: envia posição/rotação ao servidor
                if (Time.time - lastSyncTime >= syncInterval)
                {
                    lastSyncTime = Time.time;
                    UpdatePositionServerRpc(transform.position, transform.rotation);
                }
            }
            else
            {
                // Outros: interpola para posição sincronizada
                transform.position = Vector3.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation.Value, Time.deltaTime * 10f);
            }
        }

        /// <summary>
        /// ServerRpc: cliente envia ao servidor
        /// </summary>
        [ServerRpc]
        private void UpdatePositionServerRpc(Vector3 position, Quaternion rotation)
        {
            networkPosition.Value = position;
            networkRotation.Value = rotation;
        }

        /// <summary>
        /// Toma dano (sincronizado)
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsOwner) return;

            TakeDamageServerRpc(damage);
        }

        [ServerRpc]
        private void TakeDamageServerRpc(float damage)
        {
            networkHealth.Value = Mathf.Max(0, networkHealth.Value - damage);

            if (networkHealth.Value <= 0)
            {
                OnPlayerDiedClientRpc();
            }
        }

        /// <summary>
        /// ClientRpc: servidor envia a todos os clientes
        /// </summary>
        [ClientRpc]
        private void OnPlayerDiedClientRpc()
        {
            Debug.Log($"[Network] Player {OwnerClientId} died");
            // Lógica de morte
        }

        /// <summary>
        /// Callback quando vida muda
        /// </summary>
        private void OnHealthChanged(float previousValue, float newValue)
        {
            if (playerHealth != null && IsOwner)
            {
                // Atualiza vida local
                // playerHealth.SetHealth(newValue);
            }
        }

        /// <summary>
        /// Interage com puzzle (sincronizado)
        /// </summary>
        public void InteractWithPuzzle(NetworkObject puzzleObject)
        {
            if (!IsOwner) return;

            InteractWithPuzzleServerRpc(puzzleObject.NetworkObjectId);
        }

        [ServerRpc]
        private void InteractWithPuzzleServerRpc(ulong puzzleNetworkId)
        {
            // Servidor valida e processa interação
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(puzzleNetworkId, out NetworkObject puzzleObj))
            {
                var puzzle = puzzleObj.GetComponent<NetworkPuzzle>();
                if (puzzle != null)
                {
                    puzzle.OnPlayerInteract(OwnerClientId);
                }
            }
        }
    }
}
```

### Network Puzzle Base

```csharp
using Unity.Netcode;
using UnityEngine;

namespace HorrorGame.Puzzles
{
    /// <summary>
    /// Puzzle sincronizado em rede
    /// </summary>
    public class NetworkPuzzle : NetworkBehaviour
    {
        [Header("Puzzle Settings")]
        [SerializeField] protected string puzzleID;
        [SerializeField] protected int minPlayersRequired = 1;

        // Network Variables
        protected NetworkVariable<bool> isSolved = new NetworkVariable<bool>(false);
        protected NetworkVariable<int> currentPlayers = new NetworkVariable<int>(0);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscreve mudanças
            isSolved.OnValueChanged += OnSolvedChanged;
        }

        public override void OnNetworkDespawn()
        {
            isSolved.OnValueChanged -= OnSolvedChanged;
            base.OnNetworkDespawn();
        }

        /// <summary>
        /// Jogador interage (chamado pelo servidor)
        /// </summary>
        public virtual void OnPlayerInteract(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkPuzzle] Player {clientId} interacted with {puzzleID}");

            // Implemente lógica aqui
        }

        /// <summary>
        /// Marca como resolvido (apenas servidor)
        /// </summary>
        protected void SetSolved()
        {
            if (!IsServer) return;

            isSolved.Value = true;
            OnPuzzleSolvedClientRpc();
        }

        /// <summary>
        /// Notifica todos os clientes
        /// </summary>
        [ClientRpc]
        protected virtual void OnPuzzleSolvedClientRpc()
        {
            Debug.Log($"[NetworkPuzzle] {puzzleID} solved!");
            // Feedback visual/audio
        }

        /// <summary>
        /// Callback quando estado muda
        /// </summary>
        private void OnSolvedChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                // Puzzle foi resolvido
                HandlePuzzleSolved();
            }
        }

        protected virtual void HandlePuzzleSolved()
        {
            // Override para comportamento específico
        }
    }
}
```

---

## 🎮 Mecânicas Cooperativas Específicas

### 1. Sistema de Revive

```csharp
using UnityEngine;
using Unity.Netcode;

namespace HorrorGame.Player
{
    /// <summary>
    /// Sistema onde jogadores podem reviver aliados caídos
    /// </summary>
    public class PlayerReviveSystem : NetworkBehaviour
    {
        [Header("Revive Settings")]
        [SerializeField] private float reviveTime = 5f;
        [SerializeField] private float reviveRange = 2f;
        [SerializeField] private float healthAfterRevive = 30f;

        [Header("Downed State")]
        [SerializeField] private GameObject downedVisuals;
        [SerializeField] private float bleedoutTime = 30f;

        // Network Variables
        private NetworkVariable<bool> isDowned = new NetworkVariable<bool>(false);
        private NetworkVariable<float> downedTimer = new NetworkVariable<float>(0f);

        private bool isBeingRevived = false;
        private float reviveProgress = 0f;
        private ulong reviverClientId = 0;

        public bool IsDowned => isDowned.Value;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            isDowned.OnValueChanged += OnDownedStateChanged;
        }

        private void Update()
        {
            if (!IsServer) return;

            if (isDowned.Value)
            {
                downedTimer.Value += Time.deltaTime;

                if (downedTimer.Value >= bleedoutTime)
                {
                    // Morreu permanentemente
                    OnPlayerBleedOutClientRpc();
                }
            }
        }

        /// <summary>
        /// Coloca jogador no chão
        /// </summary>
        public void GoDown()
        {
            if (!IsServer) return;

            isDowned.Value = true;
            downedTimer.Value = 0f;

            GoDownClientRpc();
        }

        [ClientRpc]
        private void GoDownClientRpc()
        {
            if (downedVisuals != null)
                downedVisuals.SetActive(true);

            // Desabilita controles
            var fpsController = GetComponent<FirstPersonController>();
            if (fpsController != null && IsOwner)
                fpsController.enabled = false;

            Debug.Log($"[Revive] Player {OwnerClientId} is down!");
        }

        /// <summary>
        /// Inicia revive
        /// </summary>
        public void StartRevive(ulong reviverClient)
        {
            if (!IsServer || !isDowned.Value) return;

            isBeingRevived = true;
            reviverClientId = reviverClient;

            StartReviveClientRpc(reviverClient);
        }

        [ClientRpc]
        private void StartReviveClientRpc(ulong reviverClient)
        {
            GameEvents.OnShowMessage?.Invoke($"Player {reviverClient} is reviving...", reviveTime);
        }

        /// <summary>
        /// Atualiza progresso de revive
        /// </summary>
        public void UpdateRevive(float deltaTime)
        {
            if (!IsServer || !isBeingRevived) return;

            reviveProgress += deltaTime;

            if (reviveProgress >= reviveTime)
            {
                CompleteRevive();
            }
        }

        /// <summary>
        /// Cancela revive
        /// </summary>
        public void CancelRevive()
        {
            if (!IsServer) return;

            isBeingRevived = false;
            reviveProgress = 0f;

            CancelReviveClientRpc();
        }

        [ClientRpc]
        private void CancelReviveClientRpc()
        {
            GameEvents.OnShowMessage?.Invoke("Revive cancelled!", 2f);
        }

        /// <summary>
        /// Completa revive
        /// </summary>
        private void CompleteRevive()
        {
            isDowned.Value = false;
            downedTimer.Value = 0f;
            isBeingRevived = false;
            reviveProgress = 0f;

            RevivePlayerClientRpc();
        }

        [ClientRpc]
        private void RevivePlayerClientRpc()
        {
            if (downedVisuals != null)
                downedVisuals.SetActive(false);

            // Reabilita controles
            var fpsController = GetComponent<FirstPersonController>();
            if (fpsController != null && IsOwner)
                fpsController.enabled = true;

            // Restaura vida
            var health = GetComponent<PlayerHealth>();
            if (health != null)
                health.Heal(healthAfterRevive);

            GameEvents.OnShowMessage?.Invoke("Revived!", 2f);
            Debug.Log($"[Revive] Player {OwnerClientId} revived!");
        }

        [ClientRpc]
        private void OnPlayerBleedOutClientRpc()
        {
            Debug.Log($"[Revive] Player {OwnerClientId} bled out!");
            // Morte permanente
        }

        private void OnDownedStateChanged(bool previousValue, bool newValue)
        {
            // Callback quando estado muda
        }
    }
}
```

### 2. Sistema de Compartilhamento de Items

```csharp
using Unity.Netcode;
using UnityEngine;

namespace HorrorGame.Player
{
    /// <summary>
    /// Permite jogadores darem items uns aos outros
    /// </summary>
    public class ItemSharingSystem : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float sharingRange = 3f;

        private AdvancedInventory inventory;

        private void Start()
        {
            inventory = GetComponent<AdvancedInventory>();
        }

        /// <summary>
        /// Oferece item a outro jogador
        /// </summary>
        public void OfferItem(string itemID, ulong targetClientId, int quantity = 1)
        {
            if (!IsOwner) return;

            // Verifica se tem o item
            if (!inventory.HasItem(itemID, quantity))
            {
                GameEvents.OnShowMessage?.Invoke("You don't have that item!", 2f);
                return;
            }

            OfferItemServerRpc(itemID, targetClientId, quantity);
        }

        [ServerRpc]
        private void OfferItemServerRpc(string itemID, ulong targetClientId, int quantity)
        {
            // Servidor valida e transfere
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var targetClient))
            {
                var targetPlayer = targetClient.PlayerObject.GetComponent<ItemSharingSystem>();

                if (targetPlayer != null)
                {
                    // Remove do doador
                    if (inventory.RemoveItem(itemID, quantity))
                    {
                        // Adiciona ao receptor
                        TransferItemClientRpc(itemID, targetClientId, quantity);
                    }
                }
            }
        }

        [ClientRpc]
        private void TransferItemClientRpc(string itemID, ulong receiverClientId, int quantity)
        {
            if (NetworkManager.Singleton.LocalClientId == receiverClientId)
            {
                // Este cliente recebeu o item
                //inventory.AddItem(itemData, quantity);
                GameEvents.OnShowMessage?.Invoke($"Received {itemID} x{quantity}!", 2f);
            }
        }
    }
}
```

---

## ⚖️ Balanceamento Solo vs Co-op

### Dynamic Difficulty Adjustment

```csharp
using UnityEngine;
using Unity.Netcode;

namespace HorrorGame.Core
{
    /// <summary>
    /// Ajusta dificuldade baseado no número de jogadores
    /// </summary>
    public class DynamicDifficultySystem : MonoBehaviour
    {
        public static DynamicDifficultySystem Instance { get; private set; }

        [Header("Scaling Settings")]
        [SerializeField] private float enemyHealthMultiplier = 0.5f; // +50% vida por jogador extra
        [SerializeField] private float enemyDamageMultiplier = 0.3f; // +30% dano
        [SerializeField] private float enemySpawnMultiplier = 0.4f; // +40% spawns

        [Header("Puzzle Adjustments")]
        [SerializeField] private bool disableCoopPuzzlesInSolo = true;

        private int currentPlayerCount = 1;

        public int PlayerCount => currentPlayerCount;
        public float EnemyHealthScale => 1f + (enemyHealthMultiplier * (currentPlayerCount - 1));
        public float EnemyDamageScale => 1f + (enemyDamageMultiplier * (currentPlayerCount - 1));
        public float SpawnRateScale => 1f + (enemySpawnMultiplier * (currentPlayerCount - 1));

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            UpdatePlayerCount();
            InvokeRepeating(nameof(UpdatePlayerCount), 1f, 1f);
        }

        private void UpdatePlayerCount()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                currentPlayerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
            }
            else
            {
                currentPlayerCount = 1; // Solo
            }

            ApplyDifficultyScaling();
        }

        private void ApplyDifficultyScaling()
        {
            // Ajusta inimigos existentes
            var enemies = FindObjectsOfType<EnemyBase>();
            foreach (var enemy in enemies)
            {
                // Escala vida
                // enemy.MaxHealth *= EnemyHealthScale;

                // Escala dano
                // enemy.DamageMultiplier = EnemyDamageScale;
            }

            // Ajusta puzzles
            if (disableCoopPuzzlesInSolo && currentPlayerCount == 1)
            {
                DisableCoopOnlyPuzzles();
            }

            Debug.Log($"[Difficulty] Scaled for {currentPlayerCount} players");
        }

        private void DisableCoopOnlyPuzzles()
        {
            var allPuzzles = FindObjectsOfType<PuzzleBase>();
            foreach (var puzzle in allPuzzles)
            {
                // Desabilita puzzles que requerem múltiplos jogadores
                if (puzzle.GetType() == typeof(DualLeverPuzzle) ||
                    puzzle.GetType() == typeof(PressurePlatePuzzle))
                {
                    // Substitui por versão solo ou desabilita
                    puzzle.gameObject.SetActive(false);
                    Debug.Log($"[Difficulty] Disabled co-op puzzle: {puzzle.PuzzleID}");
                }
            }
        }

        /// <summary>
        /// Calcula vida de inimigo baseado em jogadores
        /// </summary>
        public float GetScaledEnemyHealth(float baseHealth)
        {
            return baseHealth * EnemyHealthScale;
        }

        /// <summary>
        /// Calcula dano de inimigo baseado em jogadores
        /// </summary>
        public float GetScaledEnemyDamage(float baseDamage)
        {
            return baseDamage * EnemyDamageScale;
        }
    }
}
```

---

## 📊 Performance e Otimizações Multiplayer

### Network Optimization Tips

```csharp
// 1. Reduza frequência de sincronização
[SerializeField] private float syncRate = 10f; // 10 updates/segundo ao invés de 60

// 2. Use NetworkVariable apenas para dados críticos
// ❌ NÃO sincronize posição de objeto cosmético
// ✅ Sincronize posição de player, puzzle state, enemy health

// 3. Comprima dados quando possível
// Ao invés de Vector3 (12 bytes), use short (2 bytes) se precisão não é crítica

// 4. Agrupe RPCs
[ServerRpc]
private void BatchUpdateServerRpc(BatchedData data) { }
// Ao invés de múltiplos RPCs pequenos

// 5. Use Object Pooling para networked objects
// Evite spawnar/destruir objetos em rede constantemente
```

### Bandwidth Management

```csharp
using UnityEngine;
using Unity.Netcode;

namespace HorrorGame.Network
{
    /// <summary>
    /// Monitora e otimiza uso de banda
    /// </summary>
    public class BandwidthMonitor : MonoBehaviour
    {
        [Header("Limits")]
        [SerializeField] private float maxBytesPerSecond = 100000f; // 100 KB/s

        private float currentBytesPerSecond = 0f;
        private float measurementTimer = 0f;

        private void Update()
        {
            measurementTimer += Time.deltaTime;

            if (measurementTimer >= 1f)
            {
                CheckBandwidthUsage();
                measurementTimer = 0f;
            }
        }

        private void CheckBandwidthUsage()
        {
            if (NetworkManager.Singleton != null)
            {
                // Pega estatísticas de rede
                // Implementação depende do sistema específico

                if (currentBytesPerSecond > maxBytesPerSecond)
                {
                    Debug.LogWarning($"[Network] High bandwidth usage: {currentBytesPerSecond / 1000f} KB/s");
                    OptimizeNetworkTraffic();
                }
            }
        }

        private void OptimizeNetworkTraffic()
        {
            // Reduz frequência de sync
            // Desabilita sincronização de objetos distantes
            // etc.
        }
    }
}
```

---

## 🎨 Design de Terror em Co-op

### Desafios Únicos

1. **Terror é menos efetivo em grupo** - Players se sentem mais seguros juntos
2. **Jumpscares não funcionam igual** - Um player pode estragar para outros
3. **Comunicação quebra imersão** - Players conversando casualmente

### Soluções de Design

#### 1. Separação Forçada

```csharp
/// <summary>
/// Puzzle que força jogadores a se separarem
/// </summary>
public class SeparationPuzzle : NetworkPuzzle
{
    [SerializeField] private Transform room1SpawnPoint;
    [SerializeField] private Transform room2SpawnPoint;
    [SerializeField] private GameObject doorBetweenRooms;

    [ServerRpc(RequireOwnership = false)]
    public void ActivatePuzzleServerRpc()
    {
        // Teleporta jogadores para salas separadas
        var players = FindObjectsOfType<NetworkPlayerController>();

        if (players.Length >= 2)
        {
            players[0].transform.position = room1SpawnPoint.position;
            players[1].transform.position = room2SpawnPoint.position;

            // Fecha porta entre eles
            doorBetweenRooms.SetActive(true);

            SeparatePlayersClientRpc();
        }
    }

    [ClientRpc]
    private void SeparatePlayersClientRpc()
    {
        GameEvents.OnShowMessage?.Invoke("You've been separated! Work together!", 5f);
        // Aumenta tensão
        GameEvents.OnTensionLevelChanged?.Invoke(0.8f);
    }
}
```

#### 2. Recursos Escassos Compartilhados

```csharp
/// <summary>
/// Munição/items limitados criam decisões difíceis
/// </summary>
public class SharedResourceSystem : NetworkBehaviour
{
    [Header("Shared Resources")]
    [SerializeField] private int totalBatteries = 3; // Para 4 jogadores
    [SerializeField] private int totalMedkits = 2;

    // Jogadores devem decidir quem leva o quê
}
```

#### 3. Inimigos que Targetam Jogador Isolado

```csharp
/// <summary>
/// Inimigo que persegue o jogador mais isolado
/// </summary>
public class IsolationHunter : EnemyBase
{
    protected override void UpdateAI()
    {
        // Encontra player mais distante dos aliados
        var target = FindMostIsolatedPlayer();

        if (target != null)
        {
            ChasePlayer(target);
        }
    }

    private GameObject FindMostIsolatedPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        GameObject mostIsolated = null;
        float maxIsolation = 0f;

        foreach (var player in players)
        {
            float isolation = CalculateIsolationScore(player, players);
            if (isolation > maxIsolation)
            {
                maxIsolation = isolation;
                mostIsolated = player;
            }
        }

        return mostIsolated;
    }

    private float CalculateIsolationScore(GameObject player, GameObject[] allPlayers)
    {
        float totalDistance = 0f;
        int count = 0;

        foreach (var other in allPlayers)
        {
            if (other == player) continue;

            totalDistance += Vector3.Distance(player.transform.position, other.transform.position);
            count++;
        }

        return count > 0 ? totalDistance / count : 0f;
    }
}
```

#### 4. Eventos Pessoais (Só Um Jogador Vê)

```csharp
/// <summary>
/// Alucinação que só um jogador específico vê
/// </summary>
[ClientRpc]
private void ShowHallucinationToPlayerClientRpc(ulong targetClientId)
{
    if (NetworkManager.Singleton.LocalClientId == targetClientId)
    {
        // Apenas este cliente vê a alucinação
        SpawnHallucination();

        // Outros jogadores não veem nada!
        // Isso cria desconfiança e tensão
    }
}
```

---

## 🚀 Implementação Passo a Passo

### Fase 1: Setup Básico de Networking

1. **Instale Netcode for GameObjects**
   ```
   Window > Package Manager > Unity Registry > Netcode for GameObjects
   ```

2. **Configure NetworkManager**
   - Crie GameObject vazio "NetworkManager"
   - Add Component > NetworkManager
   - Configure Transport (UnityTransport)
   - Defina PlayerPrefab

3. **Adapte Player para Rede**
   - Adicione NetworkObject ao player prefab
   - Adicione NetworkPlayerController
   - Configure ownership

### Fase 2: Sistema de Puzzles Base

1. **Crie PuzzleBase abstrato** (código acima)
2. **Implemente PuzzleManager**
3. **Crie 2-3 puzzles simples** para testar
4. **Teste em single player** primeiro

### Fase 3: Integração Multiplayer + Puzzles

1. **Adapte PuzzleBase para rede** (NetworkPuzzle)
2. **Sincronize estado dos puzzles**
3. **Teste com 2 clientes locais**

### Fase 4: Puzzles Cooperativos

1. **Implemente DualLeverPuzzle**
2. **Implemente PressurePlatePuzzle**
3. **Teste coordenação entre jogadores**

### Fase 5: Sistemas Cooperativos

1. **Sistema de Revive**
2. **Compartilhamento de Items**
3. **Comunicação (voz/texto)**

### Fase 6: Balanceamento

1. **Implemente DynamicDifficultySystem**
2. **Teste com 1, 2, 3, 4 jogadores**
3. **Ajuste multipliers**

### Fase 7: Polish

1. **Feedback visual/audio**
2. **UI de multiplayer** (lista de jogadores, ping, etc)
3. **Lobby system**
4. **Matchmaking** (opcional)

---

## 📝 Checklist de Implementação

### Networking
- [ ] Unity Netcode instalado
- [ ] NetworkManager configurado
- [ ] Player prefab adaptado para rede
- [ ] Sincronização de posição funcionando
- [ ] RPCs básicos funcionando
- [ ] Host-Client model funcionando

### Sistema de Puzzles
- [ ] PuzzleBase implementado
- [ ] PuzzleManager implementado
- [ ] Pelo menos 3 tipos de puzzle
- [ ] Puzzles solo funcionando
- [ ] Puzzles cooperativos funcionando
- [ ] Reset de puzzles funciona

### Integração
- [ ] Puzzles sincronizam em rede
- [ ] Múltiplos jogadores podem interagir
- [ ] Estado persiste para jogadores que conectam depois
- [ ] Feedback visual claro

### Sistemas Cooperativos
- [ ] Revive system
- [ ] Item sharing
- [ ] Comunicação (voz/texto/ping)
- [ ] Compartilhamento de objetivos

### Balanceamento
- [ ] Sistema de dificuldade dinâmica
- [ ] Inimigos escalam com número de jogadores
- [ ] Puzzles ajustam automaticamente
- [ ] Testado com 1-4 jogadores

### Performance
- [ ] FPS estável com 4 jogadores
- [ ] Bandwidth aceitável (<200KB/s)
- [ ] Latência <100ms em lan
- [ ] Sem memory leaks

### Terror & Atmosfera
- [ ] Terror funciona em co-op
- [ ] Mecânicas de separação
- [ ] Recursos escassos
- [ ] Inimigos adaptados para co-op

---

## 🎓 Recursos e Referências

### Documentação
- [Unity Netcode Docs](https://docs-multiplayer.unity3d.com/)
- [Mirror Networking](https://mirror-networking.com/)
- [Photon PUN 2](https://doc.photonengine.com/pun/current/getting-started/pun-intro)

### Tutoriais Recomendados
- **Dapper Dino** - Unity Netcode tutorials (YouTube)
- **Brackeys** - Multiplayer basics
- **Code Monkey** - Advanced networking

### Jogos para Estudar
- **Phasmophobia** - Co-op ghost hunting, puzzle elements
- **Devour** - Co-op horror puzzles
- **Lethal Company** - Co-op extraction horror
- **Content Warning** - Co-op horror with cameras

---

## 🎯 Próximos Passos Recomendados

1. **Comece simples** - Faça um puzzle básico funcionar em single player
2. **Adicione networking** - Adapte para multiplayer
3. **Teste muito** - Com pessoas reais, não sozinho
4. **Itere** - Baseado em feedback
5. **Adicione polish** - Som, VFX, feedback

---

## 💡 Dicas Finais

### Para Terror Cooperativo Eficaz:
1. **Separe jogadores** ocasionalmente
2. **Recursos limitados** criam decisões difíceis
3. **Eventos pessoais** (só um vê) criam paranoia
4. **Comunicação é chave** - mas pode ser interrompida
5. **Revive system** cria drama e tensão

### Para Puzzles em Co-op:
1. **Design para coordenação**, não só para números
2. **Comunicação verbal deve ser necessária**
3. **Timing challenges** são excelentes
4. **Papéis diferentes** para cada jogador
5. **Fallback para solo** sempre que possível

---

**Boa sorte com seu jogo de terror cooperativo com puzzles! 🎮👻**

Se precisar de exemplos mais específicos ou tiver dúvidas, consulte a documentação completa do framework nas páginas:
- [README.md](README.md) - Visão geral
- [QUICK_START.md](QUICK_START.md) - Guia rápido
- [API_REFERENCE.md](API_REFERENCE.md) - Referência da API
- [EXAMPLES.md](EXAMPLES.md) - Mais exemplos de código
