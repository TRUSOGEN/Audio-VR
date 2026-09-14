using System;
using UnityEngine;

/// <summary>
/// Converts route evidence into one-shot semantic transitions and schedules notifications in advance.
/// Transition entry is predicate-based; progress is used only for the configured landing region.
/// </summary>
public sealed class FlightStateMachine : MonoBehaviour
{
    [Serializable]
    public sealed class TransitionRule
    {
        public string transitionId;
        public string targetState;

        [Range(0f, 1f)]
        public float progressThreshold;
    }

    public RoutePlayer routePlayer;
    public EventLogger logger;
    public NotificationAudio notificationAudio;
    public float notificationLeadTimeSeconds = 1.5f;

    public TransitionRule[] rules =
    {
        new TransitionRule { transitionId = "T01", targetState = "cruise_entry", progressThreshold = 0.25f },
        new TransitionRule { transitionId = "T02", targetState = "descent_begin", progressThreshold = 0.55f },
        new TransitionRule { transitionId = "T03", targetState = "landing_preparation", progressThreshold = 0.85f }
    };

    private bool[] scheduled;
    private bool[] fired;
    private bool pauseHandled;

    private void Start()
    {
        EnsureRuleState();
        if (notificationAudio != null && notificationAudio.routePlayer == null)
        {
            notificationAudio.routePlayer = routePlayer;
        }
    }

    private void Update()
    {
        if (routePlayer == null || rules == null || rules.Length == 0)
        {
            return;
        }

        EnsureRuleState();

        if (routePlayer.HasError || routePlayer.IsPaused)
        {
            if (!pauseHandled)
            {
                notificationAudio?.InterruptForRoutePause(routePlayer.HasError ? "route_error" : "route_paused");
                for (int i = 0; i < fired.Length; i++)
                {
                    if (!fired[i])
                    {
                        scheduled[i] = false;
                    }
                }
                pauseHandled = true;
            }
            return;
        }

        if (!routePlayer.IsPlaying)
        {
            return;
        }

        pauseHandled = false;

        for (int i = 0; i < rules.Length; i++)
        {
            TransitionRule rule = rules[i];
            if (rule == null || fired[i])
            {
                continue;
            }

            float transitionProgress = routePlayer.GetProgressForTransition(rule.targetState, rule.progressThreshold);
            double transitionRouteTime = routePlayer.GetRouteTimeAtProgress(transitionProgress);
            double requestedScheduleRouteTime = transitionRouteTime - Math.Max(0f, notificationLeadTimeSeconds);
            double scheduleRouteTime = Math.Max(0.1d, requestedScheduleRouteTime);

            if (transitionRouteTime <= 0.1d)
            {
                logger?.Log("error", rule.transitionId, "error_code=TRANSITION_TOO_EARLY_FOR_NOTIFICATION_LEAD", rule.targetState);
                continue;
            }

            if (!scheduled[i] && routePlayer.RouteTimeSeconds >= scheduleRouteTime && routePlayer.RouteTimeSeconds < transitionRouteTime)
            {
                scheduled[i] = notificationAudio != null && notificationAudio.ScheduleForTransition(
                    rule.transitionId,
                    rule.targetState,
                    scheduleRouteTime,
                    transitionRouteTime,
                    (float)Math.Max(0d, transitionRouteTime - scheduleRouteTime)
                );

                if (notificationAudio == null)
                {
                    logger?.Log("error", rule.transitionId, "error_code=NOTIFICATION_AUDIO_MISSING", rule.targetState);
                }
            }

            if (routePlayer.IsTransitionPredicateSatisfied(rule.targetState, rule.progressThreshold, out string predicate))
            {
                TriggerTransition(i, rule, predicate);
            }
        }
    }

    /// <summary>Resets one block's one-shot transition and notification state.</summary>
    public void ResetBlock()
    {
        EnsureRuleState();
        Array.Clear(scheduled, 0, scheduled.Length);
        Array.Clear(fired, 0, fired.Length);
        notificationAudio?.ResetBlock();
        logger?.Log("block_reset", details: "transition_rules_reset");
    }

    private void TriggerTransition(int index, TransitionRule rule, string predicate)
    {
        if (fired[index])
        {
            return;
        }

        fired[index] = true;
        float transitionProgress = routePlayer.GetProgressForTransition(rule.targetState, rule.progressThreshold);
        double transitionRouteTime = routePlayer.GetRouteTimeAtProgress(transitionProgress);
        logger?.Log(
            "transition_onset",
            rule.transitionId,
            "state_source=RoutePlayer;predicate=" + predicate + ";segment=" + routePlayer.CurrentSegment + ";segment_label=" + routePlayer.CurrentSegmentLabel + ";position=" + routePlayer.CurrentPosition + ";transition_progress=" + transitionProgress.ToString("F3") + ";transition_route_time_s=" + transitionRouteTime.ToString("F3"),
            rule.targetState
        );

        if (!scheduled[index] && notificationAudio != null)
        {
            scheduled[index] = notificationAudio.PlayForTransition(
                rule.transitionId,
                rule.targetState,
                "late_software_play_call",
                transitionRouteTime
            );
        }
    }

    private void EnsureRuleState()
    {
        int count = rules == null ? 0 : rules.Length;
        if (scheduled == null || scheduled.Length != count)
        {
            scheduled = new bool[count];
            fired = new bool[count];
        }
    }
}
