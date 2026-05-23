using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Optimized Wave Spawner utilizing Object Pooling for high-performance memory management.
/// Transitions waves and spawns the boss using pool-based instantiation.
/// </summary>
public class SwarmSpawner : MonoBehaviour
{
    [Header("Pool Tags")]
    public string groundEnemyTag = "XenoStalker";
    public string flyingEnemyTag = "Viltrumite";
    public string bossTag = "Boss";

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int enemiesPerWaveMultiplier = 5;
    public float spawnDelay = 0.5f;

    [Header("Boss Settings")]
    public int bossWave = 5;
    private bool bossSpawned = false;

    [Header("UI")]
    public TextMeshProUGUI waveText;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Transform[] spawnPoints;
    private bool isSpawning = false;

    private void Start()
    {
        // Collect spawn points from tagged objects
        spawnPoints = GameObject.FindGameObjectsWithTag("PatrolPoint")
            .Select(go => go.transform).ToArray();

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No GameObjects tagged 'PatrolPoint' found. Spawner disabled.");
            return;
        }

        StartNextWave();
    }

    private void Update()
    {
        if (isSpawning) return;

        // Efficient cleanup of references: remove objects that are inactive (returned to pool)
        activeEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        if (activeEnemies.Count == 0)
        {
            if (currentWave == bossWave - 1 && !bossSpawned)
            {
                currentWave++;
                SpawnBoss();
                bossSpawned = true;
            }
            else if (currentWave < bossWave || bossSpawned)
            {
                StartNextWave();
            }
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        if (waveText != null) waveText.text = $"WAVE: {currentWave}";
        
        StartCoroutine(SpawnWaveRoutine());
    }

    private System.Collections.IEnumerator SpawnWaveRoutine()
    {
        isSpawning = true;
        int totalToSpawn = currentWave * enemiesPerWaveMultiplier;

        for (int i = 0; i < totalToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Alternating spawn logic using Pool Tags
            string tagToSpawn = (i % 2 == 0) ? groundEnemyTag : flyingEnemyTag;
            
            Vector3 spawnPos = spawnPoint.position + (tagToSpawn == flyingEnemyTag ? Vector3.up * 20f : Vector3.zero);
            GameObject enemy = ObjectPoolManager.Instance.SpawnFromPool(tagToSpawn, spawnPos, Quaternion.identity);
            
            if (enemy != null)
            {
                activeEnemies.Add(enemy);
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawning = false;
    }

    private void SpawnBoss()
    {
        if (waveText != null) waveText.text = "BOSS INCOMING";
        
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPos = spawnPoint.position + Vector3.up * 30f;
        
        GameObject boss = ObjectPoolManager.Instance.SpawnFromPool(bossTag, spawnPos, Quaternion.identity);
        if (boss != null)
        {
            activeEnemies.Add(boss);
        }
    }
}
