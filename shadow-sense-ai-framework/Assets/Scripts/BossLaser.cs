using UnityEngine;

/// <summary>
/// Projectile logic for the Boss Laser attack.
/// Handles high-speed movement and collision with IDamageable entities.
/// </summary>
public class BossLaser : MonoBehaviour
{
    public float speed = 50f;
    public float damage = 10f;
    public float lifeTime = 5f;
    public string poolTag = "BossLaser";
    public string hitFXTag = "HitSpark";

    private float timer;

    private void OnEnable()
    {
        timer = lifeTime;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !other.CompareTag("Boss"))
        {
            damageable.TakeDamage(damage, gameObject);
            
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.SpawnFromPool(hitFXTag, transform.position, Quaternion.identity);
            }

            ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
        }
    }
}
