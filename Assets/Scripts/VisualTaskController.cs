using System;
using UnityEngine;

/// <summary>
/// Runs the software-side continuous visual-task clock and raw outcome accounting.
/// Rendering can be bound later through a canvas; this controller keeps stimulus IDs and timing auditable.
/// </summary>
public sealed class VisualTaskController : MonoBehaviour
{
    private sealed class ActiveStimulus
    {
        public string stimulusId;
        public bool isTarget;
        public bool responseRecorded;
        public double onsetSessionSeconds;
        public float deadlineUnscaledTime;
    }

    public EventLogger logger;
    public string taskVersion = "visual_candidate_v0";
    public int randomSeed = 1701;
    [Min(0.5f)] public float stimulusDurationSeconds = 2.2f;
    [Min(0.5f)] public float minimumIntervalSeconds = 2.8f;
    [Range(0f, 1f)] public float targetRate = 0.25f;
    public bool retainDuringNotification = true;

    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public string CurrentStimulusId => activeStimulus == null ? "" : activeStimulus.stimulusId;
    public bool CurrentStimulusIsTarget => activeStimulus != null && activeStimulus.isTarget;
    public int HitCount { get; private set; }
    public int MissCount { get; private set; }
    public int FalseAlarmCount { get; private set; }
    public int CorrectRejectionCount { get; private set; }

    private System.Random random;
    private ActiveStimulus activeStimulus;
    private int stimulusSequence;
    private float nextStimulusUnscaledTime;

    private void Update()
    {
        if (!IsRunning || IsPaused)
        {
            return;
        }

        if (activeStimulus != null && Time.unscaledTime >= activeStimulus.deadlineUnscaledTime)
        {
            CloseActiveStimulus(false);
        }

        if (activeStimulus == null && Time.unscaledTime >= nextStimulusUnscaledTime)
        {
            EmitStimulus();
        }
    }

    /// <summary>Starts a fresh visual-task attempt for the current block.</summary>
    public void BeginBlock()
    {
        random = new System.Random(randomSeed);
        stimulusSequence = 0;
        HitCount = 0;
        MissCount = 0;
        FalseAlarmCount = 0;
        CorrectRejectionCount = 0;
        activeStimulus = null;
        IsPaused = false;
        IsRunning = true;
        nextStimulusUnscaledTime = Time.unscaledTime;
        logger?.Log("visual_task_started", details: "task_version=" + taskVersion + ";seed=" + randomSeed + ";retain_during_notification=" + retainDuringNotification);
    }

    /// <summary>Pauses timing without converting an active stimulus into a response outcome.</summary>
    public void PauseTask()
    {
        if (!IsRunning || IsPaused)
        {
            return;
        }

        IsPaused = true;
        logger?.Log("visual_task_paused", details: "active_stimulus_id=" + CurrentStimulusId);
    }

    /// <summary>Resumes stimulus scheduling after a block pause.</summary>
    public void ResumeTask()
    {
        if (!IsRunning || !IsPaused)
        {
            return;
        }

        IsPaused = false;
        nextStimulusUnscaledTime = Time.unscaledTime + minimumIntervalSeconds;
        logger?.Log("visual_task_resumed", details: "next_stimulus_after_s=" + minimumIntervalSeconds.ToString("F3"));
    }

    /// <summary>Stops the task and records raw denominators without inventing responses.</summary>
    public void EndTask(string reason)
    {
        if (!IsRunning)
        {
            return;
        }

        if (activeStimulus != null)
        {
            CloseActiveStimulus(true);
        }

        IsRunning = false;
        IsPaused = false;
        logger?.Log("visual_task_ended", details: "reason=" + reason + ";hits=" + HitCount + ";misses=" + MissCount + ";false_alarms=" + FalseAlarmCount + ";correct_rejections=" + CorrectRejectionCount);
    }

    /// <summary>Registers one response against the active stimulus or as a no-active-stimulus rejection.</summary>
    public bool RegisterResponse(bool targetControl, string controlId)
    {
        if (!IsRunning || IsPaused)
        {
            logger?.Log("input_rejected", details: "action=visual_task;control_id=" + controlId + ";reason=task_not_active");
            return false;
        }

        if (activeStimulus == null)
        {
            logger?.Log("visual_response", details: "stimulus_id=;outcome=no_active_stimulus;control_id=" + controlId);
            return false;
        }

        bool isTarget = activeStimulus.isTarget;
        bool hit = isTarget && targetControl;
        bool falseAlarm = !isTarget && targetControl;
        if (hit)
        {
            HitCount++;
        }
        else if (falseAlarm)
        {
            FalseAlarmCount++;
        }
        else if (isTarget)
        {
            MissCount++;
        }
        else
        {
            CorrectRejectionCount++;
        }

        double responseTimeMilliseconds = logger == null ? -1d : (logger.SessionTimeSeconds - activeStimulus.onsetSessionSeconds) * 1000d;
        logger?.Log(
            "visual_response",
            details: "stimulus_id=" + activeStimulus.stimulusId + ";is_target=" + isTarget + ";response_is_target_control=" + targetControl + ";outcome=" + (hit ? "hit" : falseAlarm ? "false_alarm" : isTarget ? "miss" : "correct_rejection") + ";response_time_ms=" + responseTimeMilliseconds.ToString("F1") + ";control_id=" + controlId + ";task_version=" + taskVersion
        );
        activeStimulus.responseRecorded = true;
        CloseActiveStimulus(false);
        return true;
    }

    private void EmitStimulus()
    {
        stimulusSequence++;
        bool isTarget = random.NextDouble() < Mathf.Clamp01(targetRate);
        string stimulusId = "stim_" + stimulusSequence.ToString("D4");
        activeStimulus = new ActiveStimulus
        {
            stimulusId = stimulusId,
            isTarget = isTarget,
            onsetSessionSeconds = logger == null ? 0d : logger.SessionTimeSeconds,
            deadlineUnscaledTime = Time.unscaledTime + Mathf.Max(0.05f, stimulusDurationSeconds)
        };
        nextStimulusUnscaledTime = activeStimulus.deadlineUnscaledTime + Mathf.Max(0.05f, minimumIntervalSeconds);
        logger?.Log(
            "visual_stimulus_onset",
            details: "stimulus_id=" + stimulusId + ";is_target=" + isTarget + ";task_version=" + taskVersion + ";planned_duration_s=" + stimulusDurationSeconds.ToString("F3") + ";seed=" + randomSeed
        );
    }

    private void CloseActiveStimulus(bool interrupted)
    {
        if (activeStimulus == null)
        {
            return;
        }

        if (interrupted && activeStimulus.isTarget)
        {
            logger?.Log("visual_stimulus_interrupted", details: "stimulus_id=" + activeStimulus.stimulusId + ";reason=task_end");
        }
        else if (activeStimulus.isTarget && !activeStimulus.responseRecorded)
        {
            MissCount++;
            logger?.Log("visual_stimulus_offset", details: "stimulus_id=" + activeStimulus.stimulusId + ";outcome=miss;offset_evidence=software_timer");
        }
        else
        {
            logger?.Log("visual_stimulus_offset", details: "stimulus_id=" + activeStimulus.stimulusId + ";outcome=" + (activeStimulus.isTarget ? "responded_or_pending" : "correct_rejection_window") + ";offset_evidence=software_timer");
        }

        activeStimulus = null;
    }

}
