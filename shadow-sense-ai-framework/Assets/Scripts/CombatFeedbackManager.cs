using UnityEngine;
using System.Collections;

/// <summary>
/// Production-ready manager for combat visual feedback.
/// Handles high-quality camera shake and procedural hit-sparks.
/// </summary>
public class CombatFeedbackManager : MonoBehaviour
{
    public static CombatFeedbackManager Instance { get; private set; }

    [Header("Camera Shake Settings")]
    public Transform mainCameraTransform;
    private Vector3 originalCameraPos;

    [Header("Particle Settings")]
    public GameObject hitSparkPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (mainCameraTransform == null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    /// <summary>
    /// Triggers a screen shake effect with specified intensity and duration.
    /// </summary>
    public void ShakeCamera(float intensity, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        originalCameraPos = mainCameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            mainCameraTransform.localPosition = originalCameraPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCameraTransform.localPosition = originalCameraPos;
    }

    /// <summary>
    /// Spawns hit-spark particles at a given world position using pooling.
    /// </summary>
    public void SpawnHitFX(Vector3 position, Quaternion rotation)
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.SpawnFromPool("HitSpark", position, rotation);
        }
    }
}
