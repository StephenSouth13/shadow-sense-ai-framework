using UnityEngine;

/// <summary>
/// Component for bridging entity-local events (animations/states) to the global AudioManager.
/// Designed for high-speed combat SFX integration.
/// </summary>
public class EntityAudio : MonoBehaviour
{
    [Header("Combat SFX")]
    public AudioClip attackClip;
    public AudioClip hitClip;
    public AudioClip deathClip;

    [Header("Movement SFX")]
    public AudioClip thrusterClip;
    public AudioSource thrusterSource;

    private void Start()
    {
        // Setup a local source for looping flight sounds if needed
        if (thrusterClip != null && thrusterSource == null)
        {
            thrusterSource = gameObject.AddComponent<AudioSource>();
            thrusterSource.clip = thrusterClip;
            thrusterSource.loop = true;
            thrusterSource.spatialBlend = 1.0f; // Full 3D
            thrusterSource.playOnAwake = false;
        }
    }

    public void PlayAttack()
    {
        if (AudioManager.Instance != null && attackClip != null)
            AudioManager.Instance.PlaySFX(attackClip, transform.position);
    }

    public void PlayHit()
    {
        if (AudioManager.Instance != null && hitClip != null)
            AudioManager.Instance.PlaySFX(hitClip, transform.position);
    }

    public void PlayDeath()
    {
        if (AudioManager.Instance != null && deathClip != null)
            AudioManager.Instance.PlaySFX(deathClip, transform.position);
    }

    public void SetThrusterVolume(float volume)
    {
        if (thrusterSource == null) return;
        
        if (volume > 0.01f)
        {
            if (!thrusterSource.isPlaying) thrusterSource.Play();
            thrusterSource.volume = volume;
        }
        else
        {
            if (thrusterSource.isPlaying) thrusterSource.Stop();
        }
    }
}
