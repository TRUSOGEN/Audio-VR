using System;
using UnityEngine;

/// <summary>
/// Provides a lightweight, technical-demo soundscape for the passenger journey.
/// It separates flight ambience, phase cues, and UI feedback from the research notification channel.
/// </summary>
public sealed class AudioV0RuntimeAudioDirector : MonoBehaviour
{
    public EventLogger logger;
    public RoutePlayer routePlayer;
    public SessionController sessionController;
    public AudioSource ambienceSource;
    public AudioSource cueSource;
    public AudioSource spatialTurnSource;
    public Transform cabinRoot;
    public AudioClip[] ambienceClips;
    public AudioClip[] takeoffCues;
    public AudioClip[] cruiseCues;
    public AudioClip[] descentCues;
    public AudioClip[] arrivalCues;
    public AudioClip[] turnCues;
    public AudioClip[] uiClickClips;
    public AudioClip[] uiConfirmClips;
    public AudioClip[] uiErrorClips;

    public string CurrentAudioPhase { get; private set; } = "standby";

    private int cueIndex;
    private string lastPhase;
    private bool pausedBySession;
    private int lastSpatialTurnSegment = -1;

    private void Awake()
    {
        ConfigureSource(ambienceSource, true, 0.20f);
        ConfigureSource(cueSource, false, 0.55f);
        ConfigureSpatialSource(spatialTurnSource);
    }

    private void Start()
    {
        ConfigureSource(ambienceSource, true, 0.20f);
        ConfigureSource(cueSource, false, 0.55f);
        ConfigureSpatialSource(spatialTurnSource);
    }

    private void Update()
    {
        if (sessionController == null || routePlayer == null)
        {
            return;
        }

        if (sessionController.State == SessionController.Lifecycle.Paused)
        {
            if (!pausedBySession && ambienceSource != null && ambienceSource.isPlaying)
            {
                ambienceSource.Pause();
            }
            pausedBySession = true;
            return;
        }

        if (pausedBySession && sessionController.State == SessionController.Lifecycle.Running)
        {
            ambienceSource?.UnPause();
            pausedBySession = false;
        }

        UpdateSpatialTurnCue();

        string phase = ResolvePhase();
        if (string.Equals(lastPhase, phase, StringComparison.Ordinal))
        {
            return;
        }

        lastPhase = phase;
        CurrentAudioPhase = phase;
        PlayPhaseAudio(phase);
        logger?.Log("audio_phase_change", details: "phase=" + phase + ";soundscape=technical_demo;route_time_s=" + routePlayer.RouteTimeSeconds.ToString("F3"));
    }

    /// <summary>Plays a short UI click without touching the experiment notification log.</summary>
    public void PlayUiClick()
    {
        PlayOneShot(uiClickClips, 0.32f);
    }

    /// <summary>Plays a positive UI response cue.</summary>
    public void PlayUiConfirm()
    {
        PlayOneShot(uiConfirmClips, 0.42f);
    }

    /// <summary>Plays a rejected-action cue.</summary>
    public void PlayUiError()
    {
        PlayOneShot(uiErrorClips, 0.40f);
    }

    private string ResolvePhase()
    {
        if (sessionController.State == SessionController.Lifecycle.Ready || sessionController.State == SessionController.Lifecycle.Preflight)
        {
            return "boarding";
        }

        if (sessionController.State == SessionController.Lifecycle.Completed)
        {
            return "arrival";
        }

        if (!routePlayer.IsPlaying)
        {
            return "standby";
        }

        switch (routePlayer.CurrentSegmentLabel)
        {
            case "climb":
                return "takeoff";
            case "cruise":
                return routePlayer.CurrentSegment >= 3 ? "turn" : "cruise";
            case "turn":
                return "turn";
            case "descent":
            case "landing_preparation":
                return "descent";
            default:
                return "flight";
        }
    }

    private void PlayPhaseAudio(string phase)
    {
        AudioClip[] phaseClips;
        switch (phase)
        {
            case "takeoff":
                phaseClips = takeoffCues;
                break;
            case "cruise":
            case "turn":
                phaseClips = cruiseCues;
                break;
            case "descent":
                phaseClips = descentCues;
                break;
            case "arrival":
                phaseClips = arrivalCues;
                break;
            default:
                phaseClips = null;
                break;
        }

        PlayOneShot(phaseClips, 0.52f);
        if (ambienceSource != null && ambienceClips != null && ambienceClips.Length > 0 && phase != "arrival" && phase != "standby")
        {
            AudioClip next = ambienceClips[Mathf.Abs(cueIndex++) % ambienceClips.Length];
            if (next != null && ambienceSource.clip != next)
            {
                ambienceSource.clip = next;
                ambienceSource.loop = true;
                ambienceSource.Play();
            }
        }
    }

    private void UpdateSpatialTurnCue()
    {
        if (routePlayer == null || !routePlayer.IsPlaying || routePlayer.CurrentSegmentLabel != "turn")
        {
            return;
        }

        if (routePlayer.CurrentSegment == lastSpatialTurnSegment)
        {
            return;
        }

        lastSpatialTurnSegment = routePlayer.CurrentSegment;
        if (spatialTurnSource == null || turnCues == null || turnCues.Length == 0)
        {
            logger?.Log("spatial_turn_cue_unavailable", details: "segment=" + routePlayer.CurrentSegment + ";reason=source_or_clip_missing");
            return;
        }

        int side = routePlayer.CurrentTurnSide == 0 ? 1 : routePlayer.CurrentTurnSide;
        if (cabinRoot != null)
        {
            spatialTurnSource.transform.SetParent(cabinRoot, false);
            spatialTurnSource.transform.localPosition = new Vector3(2.2f * side, 0.25f, 1.6f);
        }

        AudioClip clip = turnCues[Mathf.Abs(cueIndex++) % turnCues.Length];
        if (clip == null)
        {
            return;
        }

        spatialTurnSource.PlayOneShot(clip, 0.60f);
        logger?.Log(
            "spatial_turn_cue",
            details: "segment=" + routePlayer.CurrentSegment + ";side=" + (side > 0 ? "right" : "left") + ";turn_signed_deg=" + routePlayer.CurrentTurnSignedDegrees.ToString("F1") + ";clip=" + clip.name + ";spatial_blend=1"
        );
    }

    private void PlayOneShot(AudioClip[] clips, float volume)
    {
        if (cueSource == null || clips == null || clips.Length == 0)
        {
            return;
        }

        AudioClip clip = clips[Mathf.Abs(cueIndex++) % clips.Length];
        if (clip == null)
        {
            return;
        }

        cueSource.volume = volume;
        cueSource.PlayOneShot(clip);
    }

    private void ConfigureSource(AudioSource source, bool loop, float volume)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.volume = volume;
    }

    private void ConfigureSpatialSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 0.8f;
        source.maxDistance = 12f;
        source.spread = 0f;
        source.volume = 0.8f;
    }
}
