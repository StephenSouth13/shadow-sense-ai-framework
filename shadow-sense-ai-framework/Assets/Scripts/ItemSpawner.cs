using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages a network of spawn nodes for consumable items across the map.
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject healthPrefab;
    public GameObject energyPrefab;
    public float respawnCooldown = 30f;

    [Header("Spawn Points")]
    [Tooltip("Add empty GameObjects to this list to serve as spawn locations.")]
    public List<Transform> spawnNodes = new List<Transform>();

    private void Start()
    {
        foreach (Transform node in spawnNodes)
        {
            SpawnRandomItem(node);
        }
    }

    private void SpawnRandomItem(Transform node)
    {
        GameObject prefab = Random.value > 0.5f ? healthPrefab : energyPrefab;
        if (prefab == null) return;

        GameObject item = Instantiate(prefab, node.position, Quaternion.identity, node);
        StartCoroutine(MonitorItem(item, node));
    }

    private IEnumerator MonitorItem(GameObject item, Transform node)
    {
        while (true)
        {
            // Wait while item is active
            while (item != null && item.activeSelf)
            {
                yield return new WaitForSeconds(1f);
            }

            // Item was collected (disabled)
            yield return new WaitForSeconds(respawnCooldown);

            // Re-enable item or spawn new one if destroyed
            if (item != null)
            {
                item.SetActive(true);
            }
            else
            {
                // Safety spawn if the item was somehow destroyed
                SpawnRandomItem(node);
                yield break;
            }
        }
    }
}
