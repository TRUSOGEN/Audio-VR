using UnityEngine;

/// <summary>
/// Coordinates one experimental block without letting individual systems restart the block.
/// The controller is intentionally UI-agnostic so an OperatorCanvas can bind to these public methods.
/// </summary>
public sealed class SessionController : MonoBehaviour
{
    public enum Lifecycle
    {
        Ready,
        Preflight,
        Running,
        Paused,
        Completed,
        Error,
        Withdrawn
    }

    public EventLogger logger;
    public RoutePlayer routePlayer;
    public FlightStateMachine flightStateMachine;
    public NotificationAudio notificationAudio;
    public ConditionController conditionController;
    public IdentificationController identificationController;
    public VisualTaskController visualTaskController;

    public string sessionMode = "technical";
    public bool allowStartWithoutUi;
    public float completionSettleSeconds = 0.25f;

    public Lifecycle State { get; private set; } = Lifecycle.Ready;
    public string LastErrorCode { get; private set; }

    private float completionTimer;
    private bool endLogged;

    private void Awake()
    {
        if (routePlayer != null)
        {
            routePlayer.playOnStart = false;
        }
    }

    private void Update()
    {
        if (State == Lifecycle.Running && routePlayer != null && routePlayer.IsComplete)
        {
            completionTimer -= Time.unscaledDeltaTime;
            if (completionTimer <= 0f)
            {
                CompleteBlock();
            }
        }

        if (logger != null && logger.HasFatalError && State != Lifecycle.Error && State != Lifecycle.Completed && State != Lifecycle.Withdrawn)
        {
            Fail("LOG_WRITE_FAILED");
        }
    }

    /// <summary>Runs preflight, locks the block metadata, then starts the route exactly once.</summary>
    public bool StartBlock()
    {
        if (State != Lifecycle.Ready && State != Lifecycle.Completed)
        {
            Reject("start", "BLOCK_START_REJECTED_LIFECYCLE");
            return false;
        }

        State = Lifecycle.Preflight;
        if (!Preflight())
        {
            Fail(LastErrorCode);
            return false;
        }

        flightStateMachine?.ResetBlock();
        logger.Log("block_started", details: "mode=" + sessionMode + ";condition_id=" + logger.conditionId + ";route_id=" + logger.routeId);
        routePlayer.StartRoute();
        if (routePlayer.HasError || !routePlayer.IsPlaying)
        {
            Fail("ROUTE_START_REJECTED");
            return false;
        }

        identificationController?.BeginBlock();
        visualTaskController?.BeginBlock();
        if (conditionController != null && conditionController.GetSelectedDefinition() != null && conditionController.GetSelectedDefinition().cabinNoise)
        {
            if (!conditionController.cabinNoisePlayer.StartNoise())
            {
                Fail("CABIN_NOISE_START_FAILED");
                return false;
            }
        }
        completionTimer = completionSettleSeconds;
        State = Lifecycle.Running;
        return true;
    }

    /// <summary>Pauses route, notification, response and visual-task systems together.</summary>
    public bool PauseBlock()
    {
        if (State != Lifecycle.Running)
        {
            Reject("pause", "BLOCK_PAUSE_REJECTED_LIFECYCLE");
            return false;
        }

        routePlayer.PauseRoute();
        notificationAudio?.InterruptForRoutePause("block_paused");
        identificationController?.InterruptForPause("block_paused");
        visualTaskController?.PauseTask();
        logger?.Log("block_paused", details: "operator_pause");
        State = Lifecycle.Paused;
        return true;
    }

    /// <summary>Resumes only a paused block and never starts a fresh attempt implicitly.</summary>
    public bool ResumeBlock()
    {
        if (State != Lifecycle.Paused)
        {
            Reject("resume", "BLOCK_RESUME_REJECTED_LIFECYCLE");
            return false;
        }

        routePlayer.ResumeRoute();
        if (!routePlayer.IsPlaying)
        {
            Fail("ROUTE_RESUME_REJECTED");
            return false;
        }

        visualTaskController?.ResumeTask();
        logger?.Log("block_resumed", details: "operator_resume");
        State = Lifecycle.Running;
        return true;
    }

    /// <summary>Ends the current block and leaves the session log open for later blocks.</summary>
    public bool EndBlock(string reason = "operator_end")
    {
        if (State != Lifecycle.Running && State != Lifecycle.Paused)
        {
            Reject("end_block", "BLOCK_END_REJECTED_LIFECYCLE");
            return false;
        }

        if (routePlayer.IsPlaying)
        {
            routePlayer.PauseRoute();
        }

        routePlayer.PrepareForNewBlock();

        notificationAudio?.InterruptForRoutePause("block_ended");
        conditionController?.cabinNoisePlayer?.StopNoise("block_ended");
        identificationController?.InterruptForPause("block_ended");
        visualTaskController?.EndTask("block_ended");
        logger?.Log("block_end", details: reason + ";completion_status=partial");
        State = Lifecycle.Completed;
        return true;
    }

    /// <summary>Ends the full session exactly once after stopping active block systems.</summary>
    public void EndSession(string reason = "operator_end_session")
    {
        if (endLogged)
        {
            return;
        }

        if (State == Lifecycle.Running || State == Lifecycle.Paused)
        {
            EndBlock(reason);
        }

        endLogged = true;
        logger?.EndSession(reason);
    }

    /// <summary>Marks a participant withdrawal and preserves the partial log.</summary>
    public void Withdraw(string reason = "participant_withdrawal")
    {
        if (State == Lifecycle.Completed || State == Lifecycle.Withdrawn)
        {
            return;
        }

        if (routePlayer != null && routePlayer.IsPlaying)
        {
            routePlayer.PauseRoute();
        }

        notificationAudio?.InterruptForRoutePause("withdrawn");
        conditionController?.cabinNoisePlayer?.StopNoise("withdrawn");
        identificationController?.InterruptForPause("withdrawn");
        visualTaskController?.EndTask("withdrawn");
        logger?.Log("session_withdrawn", details: reason);
        State = Lifecycle.Withdrawn;
    }

    private bool Preflight()
    {
        if (logger == null || !logger.IsReady || logger.HasFatalError)
        {
            LastErrorCode = "LOG_PRECHECK_FAILED";
            return false;
        }

        if (routePlayer == null || routePlayer.movingRoot == null || routePlayer.waypoints == null || routePlayer.waypoints.Length < 2)
        {
            LastErrorCode = "ROUTE_PRECHECK_FAILED";
            return false;
        }

        if (notificationAudio == null || notificationAudio.audioSource == null || notificationAudio.technicalClip == null)
        {
            LastErrorCode = "NOTIFICATION_PRECHECK_FAILED";
            return false;
        }

        if (conditionController != null && !conditionController.LockForBlock(sessionMode))
        {
            LastErrorCode = "CONDITION_PRECHECK_FAILED";
            return false;
        }

        return true;
    }

    private void CompleteBlock()
    {
        if (State != Lifecycle.Running)
        {
            return;
        }

        identificationController?.EndBlock("route_complete");
        visualTaskController?.EndTask("route_complete");
        conditionController?.cabinNoisePlayer?.StopNoise("route_complete");
        logger?.Log("block_end", details: "completion_status=complete;route_complete");
        State = Lifecycle.Completed;
    }

    private void Fail(string errorCode)
    {
        LastErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "SESSION_ERROR" : errorCode;
        notificationAudio?.InterruptForRoutePause("session_error");
        identificationController?.InterruptForPause("session_error");
        visualTaskController?.EndTask("session_error");
        logger?.Log("error", details: "error_code=" + LastErrorCode + ";session_controller");
        State = Lifecycle.Error;
    }

    private void Reject(string action, string errorCode)
    {
        LastErrorCode = errorCode;
        logger?.Log("input_rejected", details: "action=" + action + ";error_code=" + errorCode + ";state=" + State);
    }

    private void OnApplicationQuit()
    {
        if (logger == null || !logger.IsReady || logger.HasFatalError)
        {
            return;
        }

        EndSession("application_quit");
    }
}
