using UnityEngine;

/// <summary>
/// Keeps cabin-noise playback independent from notification audio and records its lifecycle.
/// </summary>
public sealed class CabinNoisePlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip noiseClip;
    public EventLogger logger;

    public bool HasConfiguredClip => audioSource != null && noiseClip != null && audioSource.enabled && !audioSource.mute && audioSource.volume > 0f;
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    private void Awake()
    {
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
        }
    }

    /// <summary>Starts noise only when a configured noise clip passes the software preflight.</summary>
    public bool StartNoise()
    {
        if (!HasConfiguredClip)
        {
            logger?.Log("error", details: "error_code=CABIN_NOISE_PRECHECK_FAILED;requires_clip_source_enabled_unmuted_volume");
            return false;
        }

        audioSource.clip = noiseClip;
        audioSource.Play();
        logger?.Log("noise_started", details: "clip=" + noiseClip.name + ";evidence_kind=software_play_call");
        return true;
    }

    /// <summary>Stops noise and records an explicit reason.</summary>
    public void StopNoise(string reason)
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            logger?.Log("noise_stopped", details: "reason=" + reason);
        }
    }
}
