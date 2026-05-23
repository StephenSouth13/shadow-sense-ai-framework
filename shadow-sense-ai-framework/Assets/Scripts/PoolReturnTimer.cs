using UnityEngine;

/// <summary>
/// Utility component for returning objects to the ObjectPoolManager after a duration.
/// Ideal for VFX and transient entities.
/// </summary>
public class PoolReturnTimer : MonoBehaviour
{
    public string poolTag;
    public float duration = 2.0f;

    private void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(Return), duration);
    }

    private void Return()
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
        }
    }
}
