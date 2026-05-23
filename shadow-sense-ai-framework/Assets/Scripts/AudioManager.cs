using UnityEngine;
using System.Collections;

/// <summary>
/// Production-ready AudioManager handling BGM crossfading and spatial SFX.
/// Implements Singleton pattern with persistence.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Configuration")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource bgmSourceA;
    private AudioSource bgmSourceB;
    private bool isSourceAActive = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup BGM Sources for crossfading
        bgmSourceA = gameObject.AddComponent<AudioSource>();
        bgmSourceB = gameObject.AddComponent<AudioSource>();

        bgmSourceA.loop = true;
        bgmSourceB.loop = true;
    }

    /// <summary>
    /// Plays BGM with smooth crossfading from the current track.
    /// </summary>
    /// <param name="clip">The new BGM clip to play.</param>
    /// <param name="fadeDuration">Time in seconds to complete the crossfade.</param>
    public void PlayBGM(AudioClip clip, float fadeDuration = 1.5f)
    {
        if (clip == null) return;

        AudioSource activeSource = isSourceAActive ? bgmSourceA : bgmSourceB;
        AudioSource newSource = isSourceAActive ? bgmSourceB : bgmSourceA;

        // If the same clip is already playing, don't restart
        if (activeSource.clip == clip && activeSource.isPlaying) return;

        newSource.clip = clip;
        newSource.Play();

        StartCoroutine(Crossfade(activeSource, newSource, fadeDuration));
        isSourceAActive = !isSourceAActive;
    }

    private IEnumerator Crossfade(AudioSource oldSource, AudioSource newSource, float duration)
    {
        float timer = 0f;
        float targetVolume = bgmVolume * masterVolume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float percent = timer / duration;

            oldSource.volume = Mathf.Lerp(targetVolume, 0f, percent);
            newSource.volume = Mathf.Lerp(0f, targetVolume, percent);

            yield return null;
        }

        oldSource.Stop();
        oldSource.volume = 0f;
        newSource.volume = targetVolume;
    }

    /// <summary>
    /// Plays an SFX at a specific 3D position.
    /// </summary>
    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume);
    }
}
