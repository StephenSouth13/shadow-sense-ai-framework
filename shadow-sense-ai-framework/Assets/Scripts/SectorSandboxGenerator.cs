using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Automated environment generator for the Invincible Sandbox.
/// Creates grounds, buildings, navmeshes, waypoints, and spawns entities.
/// </summary>
public class SectorSandboxGenerator : MonoBehaviour
{
    [Header("Arena Settings")]
    public Vector2 arenaSize = new Vector2(100, 100);
    public int obstacleCount = 15;
    public float buildingHeightMin = 10f;
    public float buildingHeightMax = 40f;

    [Header("Prefabs")]
    public GameObject groundPrefab;
    public GameObject buildingPrefab;
    public GameObject waypointPrefab;
    public GameObject groundEnemyPrefab;
    public GameObject flyingEnemyPrefab;
    public GameObject playerPrefab;

    [Header("Spawn Counts")]
    public int groundEnemyCount = 5;
    public int flyingEnemyCount = 3;
    public int waypointDensity = 20;

    private NavMeshSurface navMeshSurface;

    [ContextMenu("Generate Sector")]
    public void Generate()
    {
        ClearPrevious();
        CreateGround();
        CreateBuildings();
        BakeNavigation();
        CreateWaypointNetwork();
        SpawnPlayer();
        SpawnEnemies();
        
        Debug.Log("Sector Generation Complete!");
    }

    private void ClearPrevious()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    private void CreateGround()
    {
        GameObject ground = groundPrefab != null ? Instantiate(groundPrefab, transform) : GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground_Sector";
        ground.transform.localScale = new Vector3(arenaSize.x / 10f, 1, arenaSize.y / 10f);
        ground.transform.position = Vector3.zero;

        navMeshSurface = ground.AddComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.Children;
    }

    private void CreateBuildings()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-arenaSize.x / 2.5f, arenaSize.x / 2.5f),
                0,
                Random.Range(-arenaSize.y / 2.5f, arenaSize.y / 2.5f)
            );

            float height = Random.Range(buildingHeightMin, buildingHeightMax);
            GameObject bldg = buildingPrefab != null ? Instantiate(buildingPrefab, transform) : GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            bldg.name = $"Building_{i}";
            bldg.transform.position = pos + Vector3.up * (height / 2f);
            bldg.transform.localScale = new Vector3(Random.Range(5, 10), height, Random.Range(5, 10));
            
            // Add Navigation Obstacle
            NavMeshObstacle obs = bldg.AddComponent<NavMeshObstacle>();
            obs.carving = true;
            obs.shape = NavMeshObstacleShape.Box;

            // Create Patrol Point nearby
            GameObject patrol = new GameObject($"PatrolPoint_{i}");
            patrol.transform.position = pos + Vector3.forward * 5f;
            patrol.transform.parent = transform;
            patrol.tag = "PatrolPoint";
        }
    }

    private void BakeNavigation()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

    private void CreateWaypointNetwork()
    {
        List<WaypointNode> nodes = new List<WaypointNode>();
        for (int i = 0; i < waypointDensity; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-arenaSize.x / 2f, arenaSize.x / 2f),
                Random.Range(10, 50),
                Random.Range(-arenaSize.y / 2f, arenaSize.y / 2f)
            );

            GameObject wp = waypointPrefab != null ? Instantiate(waypointPrefab, pos, Quaternion.identity, transform) : new GameObject($"Waypoint_{i}");
            wp.transform.position = pos;
            wp.transform.parent = transform;
            
            WaypointNode node = wp.GetComponent<WaypointNode>() ?? wp.AddComponent<WaypointNode>();
            nodes.Add(node);
        }

        // Link nodes
        foreach (var node in nodes) node.AutoLink();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;
        GameObject player = Instantiate(playerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
        player.name = "Player_Invincible";
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < groundEnemyCount; i++)
        {
            if (groundEnemyPrefab == null) break;
            Instantiate(groundEnemyPrefab, new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)), Quaternion.identity, transform);
        }

        for (int i = 0; i < flyingEnemyCount; i++)
        {
            if (flyingEnemyPrefab == null) break;
            Instantiate(flyingEnemyPrefab, new Vector3(Random.Range(-20, 20), 30, Random.Range(-20, 20)), Quaternion.identity, transform);
        }
    }
}
