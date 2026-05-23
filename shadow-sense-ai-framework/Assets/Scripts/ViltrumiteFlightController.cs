using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// High-level AI controller for "Viltrumite Dread-Flies".
/// Combines A* Pathfinding for global navigation and Steering Behaviors for local movement.
/// </summary>
[RequireComponent(typeof(SteeringAgent))]
public class ViltrumiteFlightController : MonoBehaviour, IDamageable
{
    [Header("Combat & Target")]
    public Transform playerTarget;
    public float health = 100f;
    public float detectionRange = 50f;

    [Header("Navigation State")]
    public List<WaypointNode> currentPath = new List<WaypointNode>();
    public int currentWaypointIndex = 0;
    public float waypointThreshold = 2.0f;

    private SteeringAgent steering;
    private bool isDead = false;

    private void Awake()
    {
        steering = GetComponent<SteeringAgent>();
    }

    private void Start()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.Find("Player_Invincible");
            if (player != null) playerTarget = player.transform;
        }

        InvokeRepeating(nameof(RecalculatePath), 0f, 2f);
    }

    private void Update()
    {
        if (isDead) return;

        Vector3 finalForce = Vector3.zero;

        // 1. Environmental Awareness (Highest Priority)
        finalForce += steering.ObstacleAvoidance();
        finalForce += steering.Separation();

        // 2. Goal-Based Movement
        if (currentPath != null && currentWaypointIndex < currentPath.Count)
        {
            Vector3 targetPos = currentPath[currentWaypointIndex].transform.position;
            finalForce += steering.Seek(targetPos);

            if (Vector3.Distance(transform.position, targetPos) < waypointThreshold)
            {
                currentWaypointIndex++;
            }
        }
        else
        {
            // If no path, either seek player directly or wander
            if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) < detectionRange)
            {
                finalForce += steering.Seek(playerTarget.position);
            }
            else
            {
                finalForce += steering.Wander();
            }
        }

        steering.ApplyForce(finalForce);
    }

    private void RecalculatePath()
    {
        if (isDead || playerTarget == null) return;

        WaypointNode start = AstarPathfinding.GetClosestNode(transform.position);
        WaypointNode end = AstarPathfinding.GetClosestNode(playerTarget.position);

        if (start != null && end != null)
        {
            currentPath = AstarPathfinding.FindPath(start, end);
            currentWaypointIndex = 0;
        }
    }

    #region IDamageable Implementation

    public void TakeDamage(float damage, GameObject attacker)
    {
        if (isDead) return;
        health -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage from {attacker.name}!");
        
        if (health <= 0) Die();
    }

    public float GetCurrentHealth() => health;
    public bool IsDead() => isDead;

    private void Die()
    {
        isDead = true;
        // Add physics-based crash behavior
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 5f;
        rb.useGravity = true;
        rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
        
        Destroy(gameObject, 5f);
    }

    #endregion
}
