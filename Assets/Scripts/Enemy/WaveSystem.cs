using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

namespace SurvivalHorror.Enemy
{
    /// <summary>
    /// Sistema de ondas de inimigos para as noites
    /// Spawna inimigos em waves com dificuldade crescente
    /// Sincronizado em multiplayer
    /// </summary>
    public class WaveSystem : NetworkBehaviour
    {
        public static WaveSystem Instance { get; private set; }

        [Header("Configuração de Waves")]
        [SerializeField] private WaveConfiguration[] waveConfigs;
        [SerializeField] private float timeBetweenWaves = 60f;
        [SerializeField] private float spawnInterval = 2f;

        [Header("Spawn")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float spawnRadius = 50f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Balanceamento Multiplayer")]
        [SerializeField] private float enemiesPerPlayerMultiplier = 0.3f;
        [SerializeField] private float healthPerPlayerMultiplier = 0.15f;

        [Header("Referências de Prefabs")]
        [SerializeField] private GameObject stalkerPrefab;
        [SerializeField] private GameObject hunterPrefab;
        [SerializeField] private GameObject brutePrefab;
        [SerializeField] private GameObject screamerPrefab;
        [SerializeField] private GameObject bossPrefab;

        // Estado
        private NetworkVariable<int> currentWave = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<int> enemiesRemaining = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<bool> isWaveActive = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private List<GameObject> activeEnemies = new List<GameObject>();
        private Coroutine waveCoroutine;

        // Properties
        public int CurrentWave => currentWave.Value;
        public int EnemiesRemaining => enemiesRemaining.Value;
        public bool IsWaveActive => isWaveActive.Value;

        // Events
        public event System.Action<int> OnWaveStarted;
        public event System.Action<int> OnWaveCompleted;
        public event System.Action OnAllWavesCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                // Subscribe no ciclo dia/noite
                if (Environment.DayNightCycle.Instance != null)
                {
                    Environment.DayNightCycle.Instance.OnNightStarted += StartNightWaves;
                    Environment.DayNightCycle.Instance.OnDayStarted += StopWaves;
                }
            }

            // Events para todos os clientes
            currentWave.OnValueChanged += (oldVal, newVal) => OnWaveStarted?.Invoke(newVal);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer && Environment.DayNightCycle.Instance != null)
            {
                Environment.DayNightCycle.Instance.OnNightStarted -= StartNightWaves;
                Environment.DayNightCycle.Instance.OnDayStarted -= StopWaves;
            }
        }

        #region Wave Management

        private void StartNightWaves()
        {
            if (!IsServer) return;

            int day = Environment.DayNightCycle.Instance.CurrentDay;
            Debug.Log($"[WaveSystem] Starting waves for Night {day}");

            if (waveCoroutine != null)
                StopCoroutine(waveCoroutine);

            waveCoroutine = StartCoroutine(WaveSequence(day));
        }

        private void StopWaves()
        {
            if (!IsServer) return;

            Debug.Log("[WaveSystem] Day started, stopping waves");

            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
                waveCoroutine = null;
            }

            isWaveActive.Value = false;
            currentWave.Value = 0;

            // Limpar inimigos restantes
            ClearRemainingEnemies();
        }

        private IEnumerator WaveSequence(int nightNumber)
        {
            // Determinar número de waves baseado no dia
            int wavesToSpawn = Mathf.Min(nightNumber, waveConfigs.Length);

            for (int i = 0; i < wavesToSpawn; i++)
            {
                currentWave.Value = i + 1;
                isWaveActive.Value = true;

                WaveConfiguration config = GetWaveConfig(nightNumber, i);
                yield return StartCoroutine(SpawnWave(config));

                // Esperar todos inimigos morrerem
                while (enemiesRemaining.Value > 0)
                {
                    yield return new WaitForSeconds(1f);
                }

                OnWaveCompleted?.Invoke(currentWave.Value);
                Debug.Log($"[WaveSystem] Wave {currentWave.Value} completed!");

                isWaveActive.Value = false;

                // Esperar entre waves (se não for a última)
                if (i < wavesToSpawn - 1)
                {
                    yield return new WaitForSeconds(timeBetweenWaves);
                }
            }

            Debug.Log($"[WaveSystem] All waves completed for Night {nightNumber}!");
            OnAllWavesCompleted?.Invoke();
        }

        private IEnumerator SpawnWave(WaveConfiguration config)
        {
            int playerCount = GetPlayerCount();
            int enemiesToSpawn = CalculateEnemyCount(config.BaseEnemyCount, playerCount);

            enemiesRemaining.Value = enemiesToSpawn;

            Debug.Log($"[WaveSystem] Spawning {enemiesToSpawn} enemies (base: {config.BaseEnemyCount}, players: {playerCount})");

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                EnemyType type = GetRandomEnemyType(config);
                Vector3 spawnPos = GetRandomSpawnPosition();

                SpawnEnemy(type, spawnPos, playerCount);

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        #endregion

        #region Enemy Spawning

        private void SpawnEnemy(EnemyType type, Vector3 position, int playerCount)
        {
            GameObject prefab = GetEnemyPrefab(type);
            if (prefab == null)
            {
                Debug.LogError($"[WaveSystem] No prefab for enemy type: {type}");
                return;
            }

            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

            // Ajustar HP baseado no número de jogadores
            var enemyHealth = enemy.GetComponent<HorrorGame.Enemy.EnemyBase>();
            if (enemyHealth != null)
            {
                float healthMultiplier = 1f + (playerCount - 1) * healthPerPlayerMultiplier;
                // enemyHealth.SetMaxHealth(enemyHealth.MaxHealth * healthMultiplier);
                Debug.Log($"[WaveSystem] Enemy HP multiplier: x{healthMultiplier:F2}");
            }

            // Spawn na rede
            NetworkObject netObj = enemy.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }

            // Registrar inimigo
            activeEnemies.Add(enemy);

            // Subscribe no evento de morte
            if (enemyHealth != null)
            {
                // enemyHealth.OnDeath += () => OnEnemyKilled(enemy);
            }

            Debug.Log($"[WaveSystem] Spawned {type} at {position}");
        }

        private void OnEnemyKilled(GameObject enemy)
        {
            if (!IsServer) return;

            activeEnemies.Remove(enemy);
            enemiesRemaining.Value--;

            Debug.Log($"[WaveSystem] Enemy killed. Remaining: {enemiesRemaining.Value}");
        }

        private void ClearRemainingEnemies()
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            activeEnemies.Clear();
            enemiesRemaining.Value = 0;
        }

        #endregion

        #region Wave Configuration

        private WaveConfiguration GetWaveConfig(int day, int waveIndex)
        {
            // Usar configurações crescentes baseado no dia
            int configIndex = Mathf.Min(day - 1, waveConfigs.Length - 1);
            return waveConfigs[configIndex];
        }

        private EnemyType GetRandomEnemyType(WaveConfiguration config)
        {
            float roll = Random.value;
            float cumulative = 0f;

            if (roll < (cumulative += config.StalkerChance))
                return EnemyType.Stalker;
            if (roll < (cumulative += config.HunterChance))
                return EnemyType.Hunter;
            if (roll < (cumulative += config.BruteChance))
                return EnemyType.Brute;
            if (roll < (cumulative += config.ScreamerChance))
                return EnemyType.Screamer;

            return EnemyType.Stalker; // Default
        }

        private GameObject GetEnemyPrefab(EnemyType type)
        {
            return type switch
            {
                EnemyType.Stalker => stalkerPrefab,
                EnemyType.Hunter => hunterPrefab,
                EnemyType.Brute => brutePrefab,
                EnemyType.Screamer => screamerPrefab,
                EnemyType.Boss => bossPrefab,
                _ => null
            };
        }

        #endregion

        #region Spawn Positions

        private Vector3 GetRandomSpawnPosition()
        {
            // Se tem spawn points definidos, usar um deles
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                return spawnPoint.position + Random.insideUnitSphere * 10f;
            }

            // Caso contrário, spawnar em círculo ao redor do centro
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(randomCircle.x, 0, randomCircle.y);

            // Raycast para chão
            if (Physics.Raycast(spawnPos + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 200f, groundLayer))
            {
                return hit.point;
            }

            return spawnPos;
        }

        #endregion

        #region Balancing

        private int GetPlayerCount()
        {
            // Conta players conectados
            int count = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null)
                    count++;
            }
            return Mathf.Max(1, count);
        }

        private int CalculateEnemyCount(int baseCount, int playerCount)
        {
            // Formula: base + (players - 1) * multiplier
            float multiplier = 1f + (playerCount - 1) * enemiesPerPlayerMultiplier;
            return Mathf.RoundToInt(baseCount * multiplier);
        }

        #endregion

        #region Debug

        [ServerRpc(RequireOwnership = false)]
        public void ForceStartWaveServerRpc()
        {
            if (waveCoroutine != null)
                StopCoroutine(waveCoroutine);

            waveCoroutine = StartCoroutine(WaveSequence(1));
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class WaveConfiguration
    {
        [Header("Wave Info")]
        public string WaveName;
        public int BaseEnemyCount;

        [Header("Enemy Distribution (deve somar 1.0)")]
        [Range(0f, 1f)] public float StalkerChance = 0.4f;
        [Range(0f, 1f)] public float HunterChance = 0.3f;
        [Range(0f, 1f)] public float BruteChance = 0.2f;
        [Range(0f, 1f)] public float ScreamerChance = 0.1f;
    }

    public enum EnemyType
    {
        Stalker,
        Hunter,
        Brute,
        Screamer,
        Boss
    }

    #endregion
}
