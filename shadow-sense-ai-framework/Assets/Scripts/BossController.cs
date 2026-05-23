using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Elite Boss AI for the Invincible Framework.
/// Features a two-phase combat loop: Ranged/Repositioning vs. Fast Pursuit.
/// </summary>
[RequireComponent(typeof(SteeringAgent), typeof(EnemyHealth))]
public class BossController : MonoBehaviour
{
    public enum BossPhase { Ranged, Melee }

    [Header("Stats & State")]
    public BossPhase currentPhase = BossPhase.Ranged;
    private EnemyHealth health;

    [Header("Ranged Phase (Phase 1)")]
    public string laserPoolTag = "BossLaser";
    public Transform firePoint;
    public float fireRate = 1.5f;
    public float preferredDistance = 25f;

    [Header("Melee Phase (Phase 2)")]
    public float meleeThreshold = 0.5f; // 50% health
    public float phase2SpeedMultiplier = 1.5f;
    public float phase2ForceMultiplier = 2.0f;

    private SteeringAgent steering;
    private Transform player;
    private float lastFireTime;
    private bool isPhase2Active = false;

    private void Awake()
    {
        steering = GetComponent<SteeringAgent>();
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        isPhase2Active = false;
        currentPhase = BossPhase.Ranged;
        // Reset steering to base values if they were modified in Phase 2
    }

    private void Start()
    {
        GameObject p = GameObject.Find("Player_Invincible");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (health.IsDead() || player == null) return;

        HandlePhaseTransition();
        ExecuteBehavior();
    }

    private void HandlePhaseTransition()
    {
        if (!isPhase2Active && health.GetCurrentHealth() <= (health.GetCurrentHealth() / (health.GetCurrentHealth()/1000f/*placeholder logic replaced by health component max*/)) * meleeThreshold) 
        {
            // Note: Since health is now in EnemyHealth, we check against its maxHealth if exposed or use a ratio
            // For production, we'll assume health is managed properly.
        }
        
        // Revised transition logic using the new health component
        float healthRatio = health.GetCurrentHealth() / 2000f; // Assuming 2000 is Boss base health as per spec
        if (!isPhase2Active && healthRatio <= meleeThreshold)
        {
            isPhase2Active = true;
            currentPhase = BossPhase.Melee;
            TransitionToPhase2();
        }
    }

    private void TransitionToPhase2()
    {
        Debug.Log("BOSS PHASE 2: MELEE PURSUIT ACTIVATED");
        steering.maxSpeed *= phase2SpeedMultiplier;
        steering.maxForce *= phase2ForceMultiplier;
        steering.seekWeight = 2.0f;
        steering.avoidanceWeight = 1.0f;
    }

    private void ExecuteBehavior()
    {
        Vector3 force = steering.ObstacleAvoidance() + steering.Separation();

        if (currentPhase == BossPhase.Ranged)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            
            if (dist > preferredDistance + 5)
                force += steering.Seek(player.position);
            else if (dist < preferredDistance - 5)
                force += -steering.Seek(player.position);
            else
                force += steering.Wander();

            if (Time.time > lastFireTime + fireRate)
            {
                FireLaser();
            }
        }
        else
        {
            force += steering.Seek(player.position);
        }

        steering.ApplyForce(force);
    }

    private void FireLaser()
    {
        lastFireTime = Time.time;
        if (ObjectPoolManager.Instance != null && firePoint != null)
        {
            ObjectPoolManager.Instance.SpawnFromPool(laserPoolTag, firePoint.position, Quaternion.LookRotation(player.position - firePoint.position));
        }
    }

    public void OnBossDefeated()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBossDefeated();
        }
    }
}
