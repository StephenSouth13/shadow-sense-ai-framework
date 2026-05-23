using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Progressive Wave Spawner for the Invincible Action Game.
/// Spawns enemies at tagged PatrolPoints and transitions waves when all enemies are defeated.
/// </summary>
public class SwarmSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject groundEnemyPrefab;
    public GameObject flyingEnemyPrefab;

    [Header("Wave Settings")]
    public int currentWave = 0;
    public int enemiesPerWaveMultiplier = 5;
    public float spawnDelay = 1.0f;

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

        // Check if all enemies in the current wave are dead
        activeEnemies.RemoveAll(e => e == null);

        if (activeEnemies.Count == 0)
        {
            StartNextWave();
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
            
            // Alternating spawn logic: Mix ground and flying enemies
            GameObject prefab = (i % 2 == 0) ? groundEnemyPrefab : flyingEnemyPrefab;
            
            if (prefab != null)
            {
                Vector3 spawnPos = spawnPoint.position + (prefab == flyingEnemyPrefab ? Vector3.up * 20f : Vector3.zero);
                GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
                activeEnemies.Add(enemy);
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawning = false;
    }
}
