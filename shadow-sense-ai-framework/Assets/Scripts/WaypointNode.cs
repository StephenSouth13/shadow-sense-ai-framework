using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A single node in a 3D pathfinding network. 
/// Used by the AstarPathfinding algorithm to navigate flying entities through open space.
/// </summary>
public class WaypointNode : MonoBehaviour
{
    [Header("Network Configuration")]
    [Tooltip("List of other nodes reachable from this position.")]
    public List<WaypointNode> neighbors = new List<WaypointNode>();

    [Tooltip("Search radius to automatically find nearby nodes.")]
    public float searchRadius = 25f;

    [Header("Pathfinding Data")]
    [HideInInspector] public float gCost; // Cost from start
    [HideInInspector] public float hCost; // Heuristic cost to end
    [HideInInspector] public WaypointNode parent;

    public float fCost => gCost + hCost;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.5f);

        if (neighbors == null) return;

        Gizmos.color = Color.blue;
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }

    /// <summary>
    /// Automatically detects and links nodes within the search radius.
    /// </summary>
    [ContextMenu("Auto-Link Neighbors")]
    public void AutoLink()
    {
        neighbors.Clear();
        WaypointNode[] allNodes = FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);
        foreach (var node in allNodes)
        {
            if (node == this) continue;
            float dist = Vector3.Distance(transform.position, node.transform.position);
            if (dist <= searchRadius)
            {
                // Ensure there is a clear line of sight for flight
                if (!Physics.Linecast(transform.position, node.transform.position))
                {
                    neighbors.Add(node);
                }
            }
        }
    }
}
