using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HorrorGame.Enemy;
using HorrorGame.Core;

namespace HorrorGame.Environment
{
    /// <summary>
    /// Sistema de spawn de inimigos com várias opções de configuração
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int maxEnemies = 5;
        [SerializeField] private float spawnInterval = 10f;

        [Header("Spawn Behavior")]
        [SerializeField] private SpawnMode spawnMode = SpawnMode.Interval;
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool respectMaxEnemies = true;

        [Header("Wave Settings")]
        [SerializeField] private bool useWaves = false;
        [SerializeField] private int enemiesPerWave = 3;
        [SerializeField] private float timeBetweenWaves = 30f;

        [Header("Player Proximity")]
        [SerializeField] private bool avoidPlayerProximity = true;
        [SerializeField] private float minPlayerDistance = 15f;

        private List<GameObject> spawnedEnemies = new List<GameObject>();
        private float spawnTimer;
        private int currentWave = 0;
        private bool isSpawning = false;
        private Transform playerTransform;

        public int ActiveEnemiesCount => spawnedEnemies.Count;
        public int CurrentWave => currentWave;

        private void Start()
        {
            // Encontra o player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            if (spawnOnStart)
            {
                if (useWaves)
                {
                    StartCoroutine(WaveSpawnRoutine());
                }
                else if (spawnMode == SpawnMode.Interval)
                {
                    StartCoroutine(IntervalSpawnRoutine());
                }
                else if (spawnMode == SpawnMode.All)
                {
                    SpawnAll();
                }
            }

            // Inscreve-se em eventos
            GameEvents.OnEnemyDied += OnEnemyDied;
        }

        private void OnDestroy()
        {
            GameEvents.OnEnemyDied -= OnEnemyDied;
        }

        private void OnEnemyDied(GameObject enemy)
        {
            if (spawnedEnemies.Contains(enemy))
            {
                spawnedEnemies.Remove(enemy);
                Debug.Log($"[EnemySpawner] Enemy died. Active enemies: {spawnedEnemies.Count}");
            }
        }

        #region Spawn Methods

        /// <summary>
        /// Spawna um único inimigo
        /// </summary>
        public GameObject SpawnEnemy()
        {
            if (respectMaxEnemies && spawnedEnemies.Count >= maxEnemies)
            {
                Debug.LogWarning("[EnemySpawner] Max enemies reached");
                return null;
            }

            if (enemyPrefabs.Length == 0)
            {
                Debug.LogError("[EnemySpawner] No enemy prefabs assigned");
                return null;
            }

            // Escolhe prefab aleatório
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            // Escolhe ponto de spawn válido
            Transform spawnPoint = GetValidSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogWarning("[EnemySpawner] No valid spawn point found");
                return null;
            }

            // Spawna inimigo
            GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            spawnedEnemies.Add(enemy);

            Debug.Log($"[EnemySpawner] Spawned enemy at {spawnPoint.position}. Active: {spawnedEnemies.Count}/{maxEnemies}");

            return enemy;
        }

        /// <summary>
        /// Spawna todos os inimigos de uma vez
        /// </summary>
        public void SpawnAll()
        {
            int spawnCount = Mathf.Min(spawnPoints.Length, maxEnemies);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnEnemy();
            }
        }

        /// <summary>
        /// Spawna uma wave de inimigos
        /// </summary>
        public void SpawnWave()
        {
            currentWave++;

            int enemiesToSpawn = Mathf.Min(enemiesPerWave, maxEnemies - spawnedEnemies.Count);

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
            }

            Debug.Log($"[EnemySpawner] Wave {currentWave} spawned with {enemiesToSpawn} enemies");
        }

        #endregion

        #region Spawn Routines

        private IEnumerator IntervalSpawnRoutine()
        {
            isSpawning = true;

            while (isSpawning)
            {
                yield return new WaitForSeconds(spawnInterval);

                if (spawnedEnemies.Count < maxEnemies)
                {
                    SpawnEnemy();
                }
            }
        }

        private IEnumerator WaveSpawnRoutine()
        {
            isSpawning = true;

            while (isSpawning)
            {
                SpawnWave();

                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Retorna um ponto de spawn válido (longe do player se configurado)
        /// </summary>
        private Transform GetValidSpawnPoint()
        {
            if (spawnPoints.Length == 0) return transform;

            List<Transform> validPoints = new List<Transform>();

            foreach (Transform point in spawnPoints)
            {
                if (!avoidPlayerProximity || playerTransform == null)
                {
                    validPoints.Add(point);
                }
                else
                {
                    float distance = Vector3.Distance(point.position, playerTransform.position);
                    if (distance >= minPlayerDistance)
                    {
                        validPoints.Add(point);
                    }
                }
            }

            if (validPoints.Count == 0)
            {
                // Se nenhum ponto é válido, usa qualquer um
                return spawnPoints[Random.Range(0, spawnPoints.Length)];
            }

            return validPoints[Random.Range(0, validPoints.Count)];
        }

        /// <summary>
        /// Para o spawn de inimigos
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
            StopAllCoroutines();
        }

        /// <summary>
        /// Reinicia o spawn
        /// </summary>
        public void RestartSpawning()
        {
            StopSpawning();

            if (useWaves)
            {
                StartCoroutine(WaveSpawnRoutine());
            }
            else if (spawnMode == SpawnMode.Interval)
            {
                StartCoroutine(IntervalSpawnRoutine());
            }
        }

        /// <summary>
        /// Destrói todos os inimigos spawnados
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }

            spawnedEnemies.Clear();
            Debug.Log("[EnemySpawner] All enemies cleared");
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (spawnPoints == null) return;

            Gizmos.color = Color.red;
            foreach (Transform point in spawnPoints)
            {
                if (point == null) continue;

                Gizmos.DrawWireSphere(point.position, 1f);
                Gizmos.DrawLine(point.position, point.position + point.forward * 2f);
            }

            // Desenha raio mínimo do player
            if (avoidPlayerProximity)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                foreach (Transform point in spawnPoints)
                {
                    if (point == null) continue;
                    Gizmos.DrawWireSphere(point.position, minPlayerDistance);
                }
            }
        }

        #endregion
    }

    public enum SpawnMode
    {
        Manual,     // Spawn manual via script
        Interval,   // Spawn em intervalos
        All         // Spawn todos de uma vez
    }
}
