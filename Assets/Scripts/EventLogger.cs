using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Writes auditable Audio V0 events as JSON Lines with separate session and route clocks.
/// The logger fails closed: a block must not continue after the output stream becomes unusable.
/// </summary>
public sealed class EventLogger : MonoBehaviour
{
    [Serializable]
    private sealed class EventRecord
    {
        public string schema_version;
        public string session_id;
        public string attempt_id;
        public string event_id;
        public string participant_id;
        public string block_id;
        public string route_id;
        public string route_version;
        public string block_position;
        public string condition_id;
        public string notification_design;
        public string listening_condition;
        public string transition_id;
        public string transition_target;
        public string event_type;
        public string details;
        public int event_sequence;
        public long timestamp_ms;
        public long monotonic_clock_ms;
        public long route_time_ms;
        public double monotonic_time_s;
        public string stimulus_set_version;
        public string noise_version;
        public string control_binding_version;
        public string application_version;
        public string write_status;
    }

    public string sessionId = "";
    public string participantId = "operator_test";
    public string blockId = "block_01";
    public string routeId = "route_v0";
    public string routeVersion = "route_v0_candidate";
    public string blockPosition = "1";
    public string conditionId = "TECH_TEST";
    public string notificationDesign = "technical_only";
    public string listeningCondition = "quiet";
    public string stimulusSetVersion = "provisional_v0";
    public string noiseVersion = "quiet";
    public string controlBindingVersion = "unfrozen";
    public string applicationVersion = "prototype_v0";
    [Tooltip("Keep technical dry-runs separate from pilot and participant data.")]
    public string storageCategory = "technical_demo";

    public string LogPath { get; private set; }
    public string SessionDirectoryPath { get; private set; }
    public string DataRootPath => GetDataRootPath();
    public string AttemptId { get; private set; }
    public string LastEventId { get; private set; }
    public bool IsReady { get; private set; }
    public bool HasFatalError { get; private set; }
    public string LastErrorCode { get; private set; }
    public double SessionTimeSeconds => IsReady ? sessionClock.Elapsed.TotalSeconds : 0d;
    public double RouteTimeSeconds { get; private set; }
    public event Action LoggerReady;

    private StreamWriter writer;
    private int eventSequence;
    private Stopwatch sessionClock;
    private bool sessionEnded;

    private void Awake()
    {
        sessionClock = new Stopwatch();
        InitializeLog();
    }

    private void InitializeLog()
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            sessionId = "session_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff") + "_" + Guid.NewGuid().ToString("N");
        }

        try
        {
            SessionDirectoryPath = Path.Combine(GetDataRootPath(), NormalizeStorageCategory(storageCategory), sessionId);
            Directory.CreateDirectory(SessionDirectoryPath);
            LogPath = Path.Combine(SessionDirectoryPath, "event_log.jsonl");

            FileStream stream = new FileStream(LogPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            writer = new StreamWriter(stream);
            writer.AutoFlush = true;
            IsReady = true;
            sessionClock.Start();

            WriteSessionManifest();
            Log("session_start", details: "logger_ready;storage_category=" + NormalizeStorageCategory(storageCategory) + ";log_path=" + LogPath);
            LoggerReady?.Invoke();
            Debug.Log("Event log: " + LogPath);
        }
        catch (Exception exception)
        {
            FailClosed("LOG_INITIALIZE_FAILED", exception);
        }
    }

    /// <summary>Starts the route clock. It is independent from the session clock and pauses with the route.</summary>
    public void BeginRouteClock()
    {
        RouteTimeSeconds = 0d;
        AttemptId = Guid.NewGuid().ToString("N");
    }

    /// <summary>Pauses the route clock without changing the session clock.</summary>
    public void PauseRouteClock()
    {
        // Route time is advanced explicitly by RoutePlayer, so pausing requires no wall-clock state.
    }

    /// <summary>Resumes the route clock after a valid route pause.</summary>
    public void ResumeRouteClock()
    {
        // RoutePlayer resumes explicit delta-time advancement.
    }

    /// <summary>Advances route time from simulation delta time, never from wall-clock time.</summary>
    public void AdvanceRouteClock(double deltaSeconds)
    {
        if (deltaSeconds > 0d && !double.IsNaN(deltaSeconds) && !double.IsInfinity(deltaSeconds))
        {
            RouteTimeSeconds += deltaSeconds;
        }
    }

    /// <summary>Writes one structured event and returns whether it reached the file.</summary>
    public bool Log(
        string eventType,
        string transitionId = "",
        string details = "",
        string transitionTarget = "")
    {
        if (!IsReady || writer == null || sessionEnded || HasFatalError)
        {
            Debug.LogError("[EventLogger] LOG_WRITE_FAILED: logger is not ready.");
            return false;
        }

        EventRecord record = new EventRecord
        {
            schema_version = "audio-v0-event-v1",
            session_id = sessionId,
            attempt_id = AttemptId,
            event_id = Guid.NewGuid().ToString("N"),
            participant_id = participantId,
            block_id = blockId,
            route_id = routeId,
            route_version = routeVersion,
            block_position = blockPosition,
            condition_id = conditionId,
            notification_design = notificationDesign,
            listening_condition = listeningCondition,
            transition_id = transitionId,
            transition_target = transitionTarget,
            event_type = eventType,
            details = details,
            event_sequence = eventSequence,
            timestamp_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            monotonic_clock_ms = (long)(SessionTimeSeconds * 1000d),
            route_time_ms = (long)(RouteTimeSeconds * 1000d),
            monotonic_time_s = SessionTimeSeconds,
            stimulus_set_version = stimulusSetVersion,
            noise_version = noiseVersion,
            control_binding_version = controlBindingVersion,
            application_version = applicationVersion,
            write_status = "written"
        };

        try
        {
            LastEventId = record.event_id;
            writer.WriteLine(JsonUtility.ToJson(record));
            eventSequence++;
            Debug.Log("[EventLogger] " + record.event_sequence + " " + eventType + " " + transitionId);
            return true;
        }
        catch (Exception exception)
        {
            FailClosed("LOG_WRITE_FAILED", exception);
            return false;
        }
    }

    /// <summary>Closes the log once. Repeated calls are safe and do not duplicate session_end.</summary>
    public void EndSession(string reason = "session_end")
    {
        if (sessionEnded)
        {
            return;
        }

        sessionEnded = true;
        if (writer == null)
        {
            return;
        }

        if (!HasFatalError)
        {
            try
            {
                EventRecord record = new EventRecord
                {
                    schema_version = "audio-v0-event-v1",
                    session_id = sessionId,
                    attempt_id = AttemptId,
                    event_id = Guid.NewGuid().ToString("N"),
                    participant_id = participantId,
                    block_id = blockId,
                    route_id = routeId,
                    route_version = routeVersion,
                    block_position = blockPosition,
                    condition_id = conditionId,
                    notification_design = notificationDesign,
                    listening_condition = listeningCondition,
                    transition_id = "",
                    transition_target = "",
                    event_type = "session_end",
                    details = reason,
                    event_sequence = eventSequence,
                    timestamp_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    monotonic_clock_ms = (long)(SessionTimeSeconds * 1000d),
                    route_time_ms = (long)(RouteTimeSeconds * 1000d),
                    monotonic_time_s = SessionTimeSeconds,
                    stimulus_set_version = stimulusSetVersion,
                    noise_version = noiseVersion,
                    control_binding_version = controlBindingVersion,
                    application_version = applicationVersion,
                    write_status = "written"
                };
                writer.WriteLine(JsonUtility.ToJson(record));
                writer.Flush();
            }
            catch (Exception exception)
            {
                Debug.LogError("[EventLogger] LOG_WRITE_FAILED during session_end: " + exception.Message);
            }
        }

        writer.Close();
        writer = null;
        IsReady = false;
    }

    private void FailClosed(string errorCode, Exception exception)
    {
        HasFatalError = true;
        IsReady = false;
        LastErrorCode = errorCode;
        Debug.LogError("[EventLogger] " + errorCode + ": " + exception.Message);

        if (writer != null)
        {
            writer.Close();
            writer = null;
        }
    }

    private void OnApplicationQuit()
    {
        EndSession("application_quit");
    }

    private void OnDestroy()
    {
        EndSession("object_destroyed");
    }

    /// <summary>Returns the stable top-level folder used by every Audio V0 session.</summary>
    public static string GetDataRootPath()
    {
        return Path.Combine(Application.persistentDataPath, "AudioV0");
    }

    private static string NormalizeStorageCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) return "technical_demo";
        foreach (char character in category)
        {
            if (!char.IsLetterOrDigit(character) && character != '_' && character != '-') return "technical_demo";
        }
        return category;
    }

    private void WriteSessionManifest()
    {
        try
        {
            string manifest = "{\n"
                + "  \"session_id\": \"" + sessionId + "\",\n"
                + "  \"storage_category\": \"" + NormalizeStorageCategory(storageCategory) + "\",\n"
                + "  \"created_utc\": \"" + DateTime.UtcNow.ToString("O") + "\",\n"
                + "  \"participant_id\": \"" + participantId + "\",\n"
                + "  \"schema_version\": \"audio-v0-event-v1\"\n"
                + "}";
            File.WriteAllText(Path.Combine(SessionDirectoryPath, "session_manifest.json"), manifest);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[EventLogger] session manifest was not written: " + exception.Message);
        }
    }
}
