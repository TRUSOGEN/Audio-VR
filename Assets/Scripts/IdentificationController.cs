using UnityEngine;

/// <summary>
/// Implements the software-side three-alternative response window and input arbitration.
/// UI buttons or XR bindings call SubmitResponse with a stable control identifier.
/// </summary>
public sealed class IdentificationController : MonoBehaviour
{
    private sealed class PendingResponse
    {
        public string transitionId;
        public string communicatedTarget;
        public string parentEventId;
        public double openedAtSessionSeconds;
        public float deadlineSeconds;
    }

    public EventLogger logger;
    public NotificationAudio notificationAudio;
    public SessionController sessionController;
    [Min(1f)] public float responseWindowSeconds = 8f;
    public bool practiceMode;

    public bool HasPendingResponse => pending != null;
    public string PendingTransitionId => pending == null ? "" : pending.transitionId;

    private PendingResponse pending;
    private bool blockActive;

    /// <summary>Rebinds the notification source when a runtime demo creates controllers after scene Awake.</summary>
    public void Bind(EventLogger eventLogger, NotificationAudio audio, SessionController controller)
    {
        if (notificationAudio != null)
        {
            notificationAudio.NotificationOnset -= HandleNotificationOnset;
        }

        logger = eventLogger;
        notificationAudio = audio;
        sessionController = controller;
        if (notificationAudio != null && isActiveAndEnabled)
        {
            notificationAudio.NotificationOnset += HandleNotificationOnset;
        }
    }

    private void OnEnable()
    {
        if (notificationAudio != null)
        {
            notificationAudio.NotificationOnset += HandleNotificationOnset;
        }
    }

    private void OnDisable()
    {
        if (notificationAudio != null)
        {
            notificationAudio.NotificationOnset -= HandleNotificationOnset;
        }
    }

    private void Update()
    {
        if (pending == null || logger == null || logger.SessionTimeSeconds < pending.deadlineSeconds)
        {
            return;
        }

        string transitionId = pending.transitionId;
        string parentEventId = pending.parentEventId;
        pending = null;
        logger.Log(
            "identification_timeout",
            transitionId,
            "parent_event_id=" + parentEventId + ";response_status=missing;response_window_s=" + responseWindowSeconds.ToString("F3"),
            "");
    }

    /// <summary>Enables response capture for a new block.</summary>
    public void BeginBlock()
    {
        blockActive = true;
        pending = null;
    }

    /// <summary>Closes the response window without converting an interruption into a miss.</summary>
    public void InterruptForPause(string reason)
    {
        if (pending == null)
        {
            return;
        }

        logger?.Log(
            "identification_interrupted",
            pending.transitionId,
            "parent_event_id=" + pending.parentEventId + ";reason=" + reason,
            pending.communicatedTarget
        );
        pending = null;
    }

    /// <summary>Accepts at most one valid response for the current notification.</summary>
    public bool SubmitResponse(string selectedTarget, string controlId)
    {
        if (!blockActive || pending == null)
        {
            logger?.Log("input_rejected", details: "action=identification;control_id=" + controlId + ";reason=no_active_response_window");
            return false;
        }

        if (string.IsNullOrWhiteSpace(selectedTarget))
        {
            logger?.Log("input_rejected", pending.transitionId, "parent_event_id=" + pending.parentEventId + ";control_id=" + controlId + ";reason=empty_target");
            return false;
        }

        double responseTime = logger.SessionTimeSeconds - pending.openedAtSessionSeconds;
        bool correct = string.Equals(selectedTarget, pending.communicatedTarget, System.StringComparison.OrdinalIgnoreCase);
        string transitionId = pending.transitionId;
        string parentEventId = pending.parentEventId;
        string communicatedTarget = pending.communicatedTarget;
        pending = null;

        logger.Log(
            "identification_response",
            transitionId,
            "parent_event_id=" + parentEventId + ";selected_target=" + selectedTarget + ";communicated_target=" + communicatedTarget + ";correct=" + correct + ";response_time_ms=" + (long)(responseTime * 1000d) + ";control_id=" + controlId + ";accepted=true;mode=" + (practiceMode ? "practice" : "main_study"),
            selectedTarget
        );
        return true;
    }

    /// <summary>Ends capture and records an interrupted pending response if needed.</summary>
    public void EndBlock(string reason)
    {
        InterruptForPause(reason);
        blockActive = false;
    }

    private void HandleNotificationOnset(string transitionId, string targetState, double routeTime, string parentEventId)
    {
        if (!blockActive || logger == null)
        {
            return;
        }

        if (pending != null)
        {
            logger.Log("identification_interrupted", pending.transitionId, "parent_event_id=" + pending.parentEventId + ";reason=replaced_by_new_notification");
        }

        pending = new PendingResponse
        {
            transitionId = transitionId,
            communicatedTarget = targetState,
            parentEventId = parentEventId,
            openedAtSessionSeconds = logger.SessionTimeSeconds,
            deadlineSeconds = (float)(logger.SessionTimeSeconds + Mathf.Max(0.1f, responseWindowSeconds))
        };

        logger.Log(
            "identification_window_open",
            transitionId,
            "parent_event_id=" + parentEventId + ";route_time_s=" + routeTime.ToString("F3") + ";response_window_s=" + responseWindowSeconds.ToString("F3") + ";mode=" + (practiceMode ? "practice" : "main_study"),
            targetState
        );
    }

}
