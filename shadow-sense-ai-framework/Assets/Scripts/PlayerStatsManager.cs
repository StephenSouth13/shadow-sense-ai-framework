using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Manages player vitals (Health and Mana) and updates the HUD in real-time.
/// Implements IDamageable for combat integration.
/// </summary>
public class PlayerStatsManager : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxMana = 100f;
    public float currentMana;

    [Header("Rates")]
    public float manaDrainRate = 10f;
    public float manaRegenRate = 15f;

    [Header("UI References")]
    public Slider healthSlider;
    public Slider manaSlider;

    public event Action OnPlayerDeath;

    private CharacterController characterController;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (isDead) return;

        HandleManaLogic();
        UpdateUI();
    }

    private void HandleManaLogic()
    {
        bool isFlying = !characterController.isGrounded;

        if (isFlying)
        {
            currentMana = Mathf.Max(0, currentMana - manaDrainRate * Time.deltaTime);
        }
        else
        {
            currentMana = Mathf.Min(maxMana, currentMana + manaRegenRate * Time.deltaTime);
        }
    }

    private void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth / maxHealth;
        if (manaSlider != null) manaSlider.value = currentMana / maxMana;
    }

    public bool CanFly() => currentMana > 0;

    #region IDamageable Implementation

    public void TakeDamage(float damage, GameObject attacker)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage from {attacker.name}!");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void RestoreMana(float amount)
    {
        if (isDead) return;
        currentMana = Mathf.Min(maxMana, currentMana + amount);
    }

    public float GetCurrentHealth() => currentHealth;
    public bool IsDead() => isDead;

    private void Die()
    {
        isDead = true;
        currentHealth = 0;
        OnPlayerDeath?.Invoke();
        Debug.LogError("PLAYER DEFEATED");
    }

    #endregion
}
