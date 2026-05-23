using UnityEngine;

/// <summary>
/// Production-ready collectible item for restoring player stats.
/// Supports pooling-friendly re-enabling logic.
/// </summary>
public class ConsumableItem : MonoBehaviour
{
    public enum ItemType { HealthFruit, EnergyTube }

    [Header("Settings")]
    public ItemType type;
    public float value = 25f;
    public AudioClip collectionSFX;

    [Header("Visuals")]
    public float rotationSpeed = 50f;

    private void Update()
    {
        // Rotating the item for visual feedback
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatsManager stats = other.GetComponent<PlayerStatsManager>();
            if (stats != null)
            {
                ApplyEffect(stats);
                
                if (AudioManager.Instance != null && collectionSFX != null)
                {
                    AudioManager.Instance.PlaySFX(collectionSFX, transform.position);
                }

                // Disable the object for respawning logic
                gameObject.SetActive(false);
            }
        }
    }

    private void ApplyEffect(PlayerStatsManager stats)
    {
        switch (type)
        {
            case ItemType.HealthFruit:
                stats.Heal(value);
                break;
            case ItemType.EnergyTube:
                stats.RestoreMana(value);
                break;
        }
    }
}
