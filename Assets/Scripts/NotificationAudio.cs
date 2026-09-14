using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Owns the notification AudioSource lifecycle and records planned, observed, estimated and interrupted evidence.
/// A scheduled notification is keyed to route time so pause/resume does not consume the lead interval.
/// </summary>
public sealed class NotificationAudio : MonoBehaviour
{
    private sealed class PendingNotification
    {
        public string transitionId;
        public string targetState;
        public double dueRouteTime;
        public double transitionRouteTime;
        public float plannedLeadTimeSeconds;
    }

    public AudioSource audioSource;
    public AudioClip technicalClip;
    public AudioClip[] variantClips;
    public EventLogger logger;
    public RoutePlayer routePlayer;
    public event Action<string, string, double, string> NotificationOnset;

    private readonly List<PendingNotification> pending = new List<PendingNotification>();
    private readonly HashSet<string> scheduledOrPlayed = new HashSet<string>();
    private readonly HashSet<string> interruptedTransitions = new HashSet<string>();

    private void Awake()
    {
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void Update()
    {
        if (routePlayer == null || !routePlayer.IsPlaying || pending.Count == 0)
        {
            return;
        }

        for (int i = pending.Count - 1; i >= 0; i--)
        {
            PendingNotification item = pending[i];
            if (routePlayer.RouteTimeSeconds < item.dueRouteTime)
            {
                continue;
            }

            pending.RemoveAt(i);
            PlayNow(item.transitionId, item.targetState, "scheduled_route_time", item.plannedLeadTimeSeconds, item.transitionRouteTime);
        }
    }

    /// <summary>Schedules one notification before the semantic transition.</summary>
    public bool ScheduleForTransition(string transitionId, string targetState, double scheduleRouteTime, double transitionRouteTime, float plannedLeadTimeSeconds)
    {
        if (!ValidateAudio(transitionId))
        {
            return false;
        }

        if (scheduledOrPlayed.Contains(transitionId))
        {
            return true;
        }

        scheduledOrPlayed.Add(transitionId);
        pending.Add(new PendingNotification
        {
            transitionId = transitionId,
            targetState = targetState,
            dueRouteTime = scheduleRouteTime,
            transitionRouteTime = transitionRouteTime,
            plannedLeadTimeSeconds = plannedLeadTimeSeconds
        });

        logger?.Log(
            "notification_scheduled",
            transitionId,
            "evidence_kind=software_schedule;scheduled_route_time_s=" + scheduleRouteTime.ToString("F3") + ";planned_lead_time_s=" + plannedLeadTimeSeconds.ToString("F3"),
            targetState
        );

        return true;
    }

    /// <summary>Plays immediately for a late schedule or explicit technical playback test.</summary>
    public bool PlayForTransition(string transitionId, string targetState, string evidenceKind = "software_play_call", double transitionRouteTime = -1d)
    {
        if (!ValidateAudio(transitionId))
        {
            return false;
        }

        if (scheduledOrPlayed.Contains(transitionId))
        {
            return true;
        }

        scheduledOrPlayed.Add(transitionId);
        PlayNow(transitionId, targetState, evidenceKind, 0f, transitionRouteTime);
        return true;
    }

    /// <summary>Cancels unsent notifications when a block is reset.</summary>
    public void ResetBlock()
    {
        for (int i = 0; i < pending.Count; i++)
        {
            logger?.Log("notification_interrupted", pending[i].transitionId, "reason=block_reset;scheduled_but_not_played", pending[i].targetState);
        }

        pending.Clear();
        scheduledOrPlayed.Clear();
        interruptedTransitions.Clear();
    }

    /// <summary>Stops pending or active audio on pause/error; unsent items can be rescheduled after resume.</summary>
    public void InterruptForRoutePause(string reason)
    {
        for (int i = 0; i < pending.Count; i++)
        {
            interruptedTransitions.Add(pending[i].transitionId);
            scheduledOrPlayed.Remove(pending[i].transitionId);
            logger?.Log("notification_interrupted", pending[i].transitionId, "reason=" + reason + ";scheduled_but_not_played", pending[i].targetState);
        }

        pending.Clear();

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            logger?.Log("notification_interrupted", details: "reason=" + reason + ";active_audio_stopped");
        }
    }

    private void PlayNow(string transitionId, string targetState, string evidenceKind, float plannedLeadTimeSeconds, double transitionRouteTime)
    {
        AudioClip chosenClip = SelectNotificationClip(transitionId);
        if (audioSource == null || chosenClip == null)
        {
            logger?.Log("error", transitionId, "error_code=AUDIO_MISSING_ASSET_OR_SOURCE", targetState);
            return;
        }

        interruptedTransitions.Remove(transitionId);
        double observedRouteTime = routePlayer == null ? -1d : routePlayer.RouteTimeSeconds;
        double observedLead = transitionRouteTime < 0d ? -1d : transitionRouteTime - observedRouteTime;

        logger?.Log(
            "notification_requested",
            transitionId,
            "evidence_kind=" + evidenceKind + ";planned_lead_time_s=" + plannedLeadTimeSeconds.ToString("F3") + ";transition_route_time_s=" + transitionRouteTime.ToString("F3") + ";observed_route_time_s=" + observedRouteTime.ToString("F3") + ";observed_software_lead_s=" + observedLead.ToString("F3"),
            targetState
        );

        audioSource.PlayOneShot(chosenClip);

        logger?.Log(
            "playback_observed",
            transitionId,
            "evidence_kind=is_playing_sample;is_playing=" + audioSource.isPlaying + ";clip=" + chosenClip.name,
            targetState
        );

        logger?.Log(
            "notification_onset",
            transitionId,
            "evidence_kind=software_play_call;clip=" + chosenClip.name,
            targetState
        );

        NotificationOnset?.Invoke(transitionId, targetState, observedRouteTime, logger == null ? "" : logger.LastEventId);

        StartCoroutine(LogEstimatedOffset(chosenClip.length, transitionId, targetState));
    }

    private AudioClip SelectNotificationClip(string transitionId)
    {
        if (variantClips == null || variantClips.Length == 0)
        {
            return technicalClip;
        }

        int hash = string.IsNullOrEmpty(transitionId) ? 0 : transitionId.GetHashCode();
        int index = Mathf.Abs(hash == int.MinValue ? 0 : hash) % variantClips.Length;
        return variantClips[index] == null ? technicalClip : variantClips[index];
    }

    private IEnumerator LogEstimatedOffset(float duration, string transitionId, string targetState)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (interruptedTransitions.Contains(transitionId))
        {
            yield break;
        }
        logger?.Log(
            "notification_offset",
            transitionId,
            "evidence_kind=estimated_clip_duration;audio_callback=unavailable;duration_s=" + duration.ToString("F3"),
            targetState
        );
    }

    private bool ValidateAudio(string transitionId)
    {
        if (audioSource != null && technicalClip != null && audioSource.enabled && !audioSource.mute && audioSource.volume > 0f && FindAnyObjectByType<AudioListener>() != null)
        {
            return true;
        }

        logger?.Log("error", transitionId, "error_code=AUDIO_PRECHECK_FAILED;requires_clip_source_enabled_unmuted_volume_listener");
        return false;
    }
}
