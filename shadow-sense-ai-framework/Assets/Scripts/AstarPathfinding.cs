using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// High-performance 3D A* Pathfinding implementation for flying entities.
/// Designed for the Viltrumite Dread-Flies to navigate complex vertical environments.
/// </summary>
public static class AstarPathfinding
{
    private static WaypointNode[] cachedNodes;

    /// <summary>
    /// Finds the shortest path between two WaypointNodes using the A* algorithm.
    /// </summary>
    /// <returns>A list of WaypointNodes representing the path, or null if no path exists.</returns>
    public static List<WaypointNode> FindPath(WaypointNode startNode, WaypointNode targetNode)
    {
        if (startNode == null || targetNode == null) return null;

        if (cachedNodes == null || cachedNodes.Length == 0)
        {
            cachedNodes = Object.FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);
        }

        List<WaypointNode> openSet = new List<WaypointNode> { startNode };
        HashSet<WaypointNode> closedSet = new HashSet<WaypointNode>();

        foreach (var node in cachedNodes)
        {
            if (node == null) continue;
            node.gCost = float.MaxValue;
            node.parent = null;
        }
        // ... rest of method

        startNode.gCost = 0;
        startNode.hCost = Vector3.Distance(startNode.transform.position, targetNode.transform.position);

        while (openSet.Count > 0)
        {
            WaypointNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (WaypointNode neighbor in currentNode.neighbors)
            {
                if (neighbor == null || closedSet.Contains(neighbor)) continue;

                float newMovementCostToNeighbor = currentNode.gCost + Vector3.Distance(currentNode.transform.position, neighbor.transform.position);
                if (newMovementCostToNeighbor < neighbor.gCost)
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = Vector3.Distance(neighbor.transform.position, targetNode.transform.position);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null; // No path found
    }

    private static List<WaypointNode> RetracePath(WaypointNode startNode, WaypointNode endNode)
    {
        List<WaypointNode> path = new List<WaypointNode>();
        WaypointNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Finds the closest WaypointNode to a given world position.
    /// </summary>
    public static WaypointNode GetClosestNode(Vector3 position)
    {
        if (cachedNodes == null || cachedNodes.Length == 0)
        {
            cachedNodes = Object.FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);
        }

        WaypointNode closest = null;
        float minDist = float.MaxValue;

        foreach (var node in cachedNodes)
        {
            if (node == null) continue;
            float dist = Vector3.Distance(position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }
        return closest;
    }

    /// <summary>
    /// Forces a refresh of the cached waypoint nodes.
    /// </summary>
    public static void RefreshCache()
    {
        cachedNodes = Object.FindObjectsByType<WaypointNode>(FindObjectsSortMode.None);
    }
}
