using UnityEngine;

/// <summary>
/// Core interface for all combat-ready entities in the Invincible Action Game package.
/// Enables a standardized way for the Player and AI to interact via damage events.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Inflicts damage on the entity.
    /// </summary>
    /// <param name="damage">Amount of health to reduce.</param>
    /// <param name="attacker">Reference to the GameObject that initiated the attack.</param>
    void TakeDamage(float damage, GameObject attacker);

    /// <summary>
    /// Returns the current health of the entity.
    /// </summary>
    float GetCurrentHealth();

    /// <summary>
    /// Returns true if the entity is dead.
    /// </summary>
    bool IsDead();
}
