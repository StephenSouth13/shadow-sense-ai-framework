/*
2026-05-23 AI-Tag
This was created with the help of Assistant, a Unity Artificial Intelligence product.
*/
using UnityEngine;

/// <summary>
/// A modular 3D Steering Behavior engine for high-speed flight physics.
/// </summary>
public class SteeringAgent : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 20f;
    public float maxForce = 25f;
    public float mass = 1.5f;
    public float arrivalDistance = 5f;

    [Header("Behavior Weights")]
    public float seekWeight = 1.0f;
    public float avoidanceWeight = 4.0f; // Higher priority to prevent crashing
    public float separationWeight = 1.5f;
    public float wanderWeight = 0.5f;

    [Header("Detection")]
    public float avoidanceRadius = 8f;
    public LayerMask obstacleLayer;
    public float separationRadius = 4f;
    public LayerMask agentLayer;

    [Header("Target Tracking")]
    public Transform targetTransform; // Assign Player_Invincible here

    private Vector3 velocity;
    private Vector3 currentSteeringForce;

    private void Update()
    {
        // 1. Gather Forces automatically if a target exists
        Vector3 totalForce = Vector3.zero;

        if (targetTransform != null)
        {
            totalForce += Seek(targetTransform.position) * seekWeight;
        }
        else
        {
            totalForce += Wander() * wanderWeight;
        }

        totalForce += ObstacleAvoidance() * avoidanceWeight;
        totalForce += Separation() * separationWeight;

        ApplyForce(totalForce);

        // 2. Apply physics
        Vector3 acceleration = currentSteeringForce / mass;
        velocity = Vector3.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
        transform.position += velocity * Time.deltaTime;

        // 3. Orient character towards movement
        if (velocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * 5f);
        }

        // Reset force for next frame
        currentSteeringForce = Vector3.zero;
    }

    public void ApplyForce(Vector3 force)
    {
        currentSteeringForce += force;
    }

    public Vector3 Seek(Vector3 target)
    {
        Vector3 desired = target - transform.position;
        float distance = desired.magnitude;
        if (distance < 0.1f) return Vector3.zero;

        desired = desired.normalized * maxSpeed;
        if (distance < arrivalDistance)
        {
            desired *= (distance / arrivalDistance);
        }

        Vector3 steer = desired - velocity;
        return Vector3.ClampMagnitude(steer, maxForce);
    }

    public Vector3 ObstacleAvoidance()
    {
        Vector3 avoidanceForce = Vector3.zero;
        RaycastHit hit;

        Vector3[] whiskers = {
            transform.forward,
            Quaternion.AngleAxis(30, transform.up) * transform.forward,
            Quaternion.AngleAxis(-30, transform.up) * transform.forward,
            Quaternion.AngleAxis(30, transform.right) * transform.forward,
            Quaternion.AngleAxis(-30, transform.right) * transform.forward
        };

        foreach (var dir in whiskers)
        {
            if (Physics.Raycast(transform.position, dir, out hit, avoidanceRadius, obstacleLayer))
            {
                Vector3 targetForce = hit.normal * maxForce;
                avoidanceForce += targetForce * (1.0f - (hit.distance / avoidanceRadius));
            }
        }
        return avoidanceForce;
    }

    public Vector3 Separation()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius, agentLayer);
        Vector3 force = Vector3.zero;
        int count = 0;

        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector3 diff = transform.position - neighbor.transform.position;
            float dist = diff.magnitude;
            if (dist > 0)
            {
                force += diff.normalized / dist;
                count++;
            }
        }

        if (count > 0)
        {
            force /= count;
            force = force.normalized * maxSpeed;
            force -= velocity;
            force = Vector3.ClampMagnitude(force, maxForce);
        }
        return force;
    }

    public Vector3 Wander()
    {
        float noise = Mathf.PerlinNoise(Time.time * 0.5f, transform.position.x);
        float angle = noise * Mathf.PI * 2;
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), Mathf.Cos(angle * 0.5f));
        Vector3 target = transform.position + transform.forward * 5f + offset * 3f;
        return Seek(target);
    }

    public Vector3 GetVelocity() => velocity;

    // Draw Whisker Debug Lines in Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3[] whiskers = {
            transform.forward,
            Quaternion.AngleAxis(30, transform.up) * transform.forward,
            Quaternion.AngleAxis(-30, transform.up) * transform.forward,
            Quaternion.AngleAxis(30, transform.right) * transform.forward,
            Quaternion.AngleAxis(-30, transform.right) * transform.forward
        };
        foreach (var dir in whiskers)
        {
            Gizmos.DrawRay(transform.position, dir * avoidanceRadius);
        }
    }
}