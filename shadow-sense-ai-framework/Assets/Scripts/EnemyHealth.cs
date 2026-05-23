using UnityEngine;
using System;

/// <summary>
/// Dedicated health component for AI entities. 
/// Integrated with ObjectPoolManager for high-performance recycling.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Vitals")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private string poolTag;
    
    private float currentHealth;
    private bool isDead = false;

    private void OnEnable()
    {
        ResetStats();
    }

    public void ResetStats()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float damage, GameObject attacker)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Visual/Audio Feedback Hooks
        if (CombatFeedbackManager.Instance != null)
        {
            CombatFeedbackManager.Instance.SpawnHitFX(transform.position, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public bool IsDead() => isDead;

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Handle death SFX/VFX via EntityAudio if present
        EntityAudio audio = GetComponent<EntityAudio>();
        if (audio != null) audio.PlayDeath();

        // Instead of destroying, return to pool with its specific tag
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
        }
    }
}
