using System;
using UnityEngine;

/// <summary>
/// Moves the passenger cabin along a validated route and exposes semantic route evidence.
/// Route time advances only while the route is running; segment transitions consume leftover frame distance.
/// </summary>
public sealed class RoutePlayer : MonoBehaviour
{
    private enum RouteLifecycle
    {
        Ready,
        Running,
        Paused,
        Complete,
        Error
    }

    public Transform movingRoot;
    public Transform[] waypoints;
    public string routeVersion = "route_v0_candidate";
    public string[] segmentLabels = { "climb", "cruise", "descent" };
    public float speedMetersPerSecond = 5f;
    public bool rotateAlongRoute = true;
    [Range(10f, 360f)] public float headingDegreesPerSecond = 75f;
    public bool playOnStart = false;
    public EventLogger logger;

    public bool IsPlaying => lifecycle == RouteLifecycle.Running;
    public bool IsPaused => lifecycle == RouteLifecycle.Paused;
    public bool IsComplete => lifecycle == RouteLifecycle.Complete;
    public bool HasError => lifecycle == RouteLifecycle.Error;
    public bool IsReady => lifecycle != RouteLifecycle.Error && waypoints != null && waypoints.Length >= 2;
    public float Progress { get; private set; }
    public int CurrentSegment => currentSegment;
    public int SegmentCount => waypoints == null ? 0 : Mathf.Max(0, waypoints.Length - 1);
    public Vector3 CurrentVelocity { get; private set; }
    public Vector3 CurrentPosition => movingRoot == null ? Vector3.zero : movingRoot.position;
    public double RouteTimeSeconds { get; private set; }
    public string CurrentSegmentLabel => GetSegmentLabel(currentSegment);
    public float CurrentTurnSignedDegrees { get; private set; }
    public int CurrentTurnSide { get; private set; }

    private RouteLifecycle lifecycle = RouteLifecycle.Ready;
    private int currentSegment;
    private float totalRouteLength;
    private float travelledRouteLength;
    private float[] cumulativeRouteLengths;

    private void Start()
    {
        if (playOnStart)
        {
            StartRoute();
        }
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        if (logger != null && logger.HasFatalError)
        {
            StopWithError("logger_not_ready_route_stopped");
            return;
        }

        if (movingRoot == null || waypoints == null || currentSegment >= waypoints.Length - 1)
        {
            StopWithError("route_runtime_reference_missing");
            return;
        }

        float deltaTime = Time.deltaTime;
        float remainingDistance = speedMetersPerSecond * deltaTime;
        Vector3 frameStart = movingRoot.position;

        while (remainingDistance > 0.0001f && IsPlaying)
        {
            Transform target = waypoints[currentSegment + 1];
            float distanceToTarget = Vector3.Distance(movingRoot.position, target.position);

            if (distanceToTarget <= 0.0001f)
            {
                AdvanceSegment();
                continue;
            }

            float moveDistance = Mathf.Min(remainingDistance, distanceToTarget);
            movingRoot.position = Vector3.MoveTowards(movingRoot.position, target.position, moveDistance);
            travelledRouteLength += moveDistance;
            remainingDistance -= moveDistance;
            Progress = Mathf.Clamp01(travelledRouteLength / totalRouteLength);

            if (distanceToTarget - moveDistance <= 0.0001f)
            {
                AdvanceSegment();
            }
        }

        CurrentVelocity = deltaTime > 0f ? (movingRoot.position - frameStart) / deltaTime : Vector3.zero;
        ApplyHeading(CurrentVelocity, deltaTime);
        RouteTimeSeconds += deltaTime;
        logger?.AdvanceRouteClock(deltaTime);
    }

    /// <summary>Validates and starts a new route attempt from the first waypoint.</summary>
    public void StartRoute()
    {
        if (IsPlaying || IsPaused)
        {
            LogInvalidLifecycle("start", "start_requires_ready_or_complete");
            return;
        }

        if (logger == null || !logger.IsReady || logger.HasFatalError)
        {
            lifecycle = RouteLifecycle.Error;
            Debug.LogError("logger_not_ready_route_blocked");
            return;
        }

        if (!ValidateRoute())
        {
            return;
        }

        currentSegment = 0;
        travelledRouteLength = 0f;
        RouteTimeSeconds = 0d;
        Progress = 0f;
        CurrentVelocity = Vector3.zero;
        lifecycle = RouteLifecycle.Running;
        movingRoot.position = waypoints[0].position;
        CurrentTurnSignedDegrees = 0f;
        CurrentTurnSide = 0;
        ApplyHeading(GetSegmentDirection(0), 999f);
        logger?.BeginRouteClock();
        logger?.Log("route_start", details: "route_version=" + routeVersion);
    }

    /// <summary>Pauses only a running route and preserves the current position.</summary>
    public void PauseRoute()
    {
        if (!IsPlaying)
        {
            LogInvalidLifecycle("pause", "pause_requires_running");
            return;
        }

        lifecycle = RouteLifecycle.Paused;
        logger?.PauseRouteClock();
        logger?.Log("pause", details: "route_paused;route_time_ms=" + (long)(RouteTimeSeconds * 1000d));
    }

    /// <summary>Resumes only a paused route.</summary>
    public void ResumeRoute()
    {
        if (!IsPaused)
        {
            LogInvalidLifecycle("resume", "resume_requires_paused");
            return;
        }

        lifecycle = RouteLifecycle.Running;
        logger?.ResumeRouteClock();
        logger?.Log("resume", details: "route_resumed;route_time_ms=" + (long)(RouteTimeSeconds * 1000d));
    }

    /// <summary>Returns a paused or completed route to a clean ready state for the next block.</summary>
    public void PrepareForNewBlock()
    {
        if (lifecycle == RouteLifecycle.Paused || lifecycle == RouteLifecycle.Complete)
        {
            lifecycle = RouteLifecycle.Ready;
            currentSegment = 0;
            Progress = 0f;
            CurrentVelocity = Vector3.zero;
        }
    }

    /// <summary>Returns the route progress at a waypoint using travelled distance, not a raw percentage guess.</summary>
    public float GetProgressAtWaypoint(int waypointIndex)
    {
        if (cumulativeRouteLengths == null || waypointIndex < 0 || waypointIndex >= cumulativeRouteLengths.Length || totalRouteLength <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(cumulativeRouteLengths[waypointIndex] / totalRouteLength);
    }

    /// <summary>Returns the estimated route clock time for a progress target.</summary>
    public double GetRouteTimeAtProgress(float progress)
    {
        if (totalRouteLength <= 0f || speedMetersPerSecond <= 0f)
        {
            return 0d;
        }

        return Mathf.Clamp01(progress) * totalRouteLength / speedMetersPerSecond;
    }

    /// <summary>Returns the first segment that descends in world Y, or -1 if none exists.</summary>
    public int GetFirstDescendingSegmentIndex()
    {
        if (waypoints == null)
        {
            return -1;
        }

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null && waypoints[i + 1].position.y < waypoints[i].position.y - 0.001f)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Maps a semantic transition to a route boundary; landing preparation uses its configured predicate.</summary>
    public float GetProgressForTransition(string targetState, float fallbackProgress)
    {
        if (string.Equals(targetState, "cruise_entry", StringComparison.OrdinalIgnoreCase))
        {
            return GetProgressAtWaypoint(Mathf.Min(1, SegmentCount));
        }

        if (string.Equals(targetState, "descent_begin", StringComparison.OrdinalIgnoreCase))
        {
            int descendingSegment = GetFirstDescendingSegmentIndex();
            return descendingSegment >= 0 ? GetProgressAtWaypoint(descendingSegment) : Mathf.Clamp01(fallbackProgress);
        }

        return Mathf.Clamp01(fallbackProgress);
    }

    /// <summary>Checks a transition predicate and returns the evidence used for the decision.</summary>
    public bool IsTransitionPredicateSatisfied(string targetState, float fallbackProgress, out string predicate)
    {
        if (string.Equals(targetState, "cruise_entry", StringComparison.OrdinalIgnoreCase))
        {
            bool result = currentSegment >= 1 || (IsComplete && SegmentCount >= 1);
            predicate = "entered_cruise_segment;current_segment=" + currentSegment;
            return result;
        }

        if (string.Equals(targetState, "descent_begin", StringComparison.OrdinalIgnoreCase))
        {
            int descendingSegment = GetFirstDescendingSegmentIndex();
            bool result = descendingSegment >= 0 && currentSegment >= descendingSegment;
            predicate = "entered_descending_segment;descending_segment=" + descendingSegment + ";current_segment=" + currentSegment;
            return result;
        }

        int requiredDescendingSegment = GetFirstDescendingSegmentIndex();
        bool descentEstablished = requiredDescendingSegment >= 0 && currentSegment >= requiredDescendingSegment;
        bool landingPredicate = Progress >= Mathf.Clamp01(fallbackProgress);
        predicate = "landing_region_after_descent;progress=" + Progress.ToString("F4") + ";threshold=" + fallbackProgress.ToString("F4");
        return descentEstablished && landingPredicate;
    }

    private bool ValidateRoute()
    {
        if (movingRoot == null || waypoints == null || waypoints.Length < 2)
        {
            StopWithError("route_not_configured");
            return false;
        }

        if (float.IsNaN(speedMetersPerSecond) || float.IsInfinity(speedMetersPerSecond) || speedMetersPerSecond <= 0f)
        {
            StopWithError("route_speed_invalid");
            return false;
        }

        totalRouteLength = 0f;
        cumulativeRouteLengths = new float[waypoints.Length];

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                StopWithError("route_waypoint_missing");
                return false;
            }

            if (i > 0)
            {
                totalRouteLength += Vector3.Distance(waypoints[i - 1].position, waypoints[i].position);
            }
            cumulativeRouteLengths[i] = totalRouteLength;
        }

        if (totalRouteLength <= 0.001f)
        {
            StopWithError("route_length_zero");
            return false;
        }

        if (segmentLabels == null || segmentLabels.Length != waypoints.Length - 1)
        {
            segmentLabels = new string[waypoints.Length - 1];
        }

        for (int i = 0; i < segmentLabels.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(segmentLabels[i]))
            {
                segmentLabels[i] = i == 0 ? "climb" : i == 1 ? "cruise" : "descent";
            }
        }

        lifecycle = RouteLifecycle.Ready;
        return true;
    }

    private void AdvanceSegment()
    {
        currentSegment++;
        if (currentSegment >= waypoints.Length - 1)
        {
            currentSegment = waypoints.Length - 1;
            Progress = 1f;
            lifecycle = RouteLifecycle.Complete;
            logger?.PauseRouteClock();
            logger?.Log("route_complete", details: "route_version=" + routeVersion + ";segment=" + currentSegment);
            return;
        }

        CurrentTurnSignedDegrees = CalculateSegmentTurn(currentSegment - 1, currentSegment);
        CurrentTurnSide = Mathf.Abs(CurrentTurnSignedDegrees) < 2f ? 0 : CurrentTurnSignedDegrees > 0f ? 1 : -1;
        logger?.Log(
            "route_segment_enter",
            details: "segment=" + currentSegment + ";label=" + CurrentSegmentLabel + ";turn_signed_deg=" + CurrentTurnSignedDegrees.ToString("F1") + ";turn_side=" + CurrentTurnSide + ";position=" + CurrentPosition
        );
    }

    private void ApplyHeading(Vector3 direction, float deltaTime)
    {
        if (!rotateAlongRoute)
        {
            return;
        }

        Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
        if (planarDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion target = Quaternion.LookRotation(planarDirection.normalized, Vector3.up);
        movingRoot.rotation = Quaternion.RotateTowards(movingRoot.rotation, target, headingDegreesPerSecond * Mathf.Max(0f, deltaTime));
    }

    private Vector3 GetSegmentDirection(int segmentIndex)
    {
        if (waypoints == null || segmentIndex < 0 || segmentIndex >= waypoints.Length - 1 || waypoints[segmentIndex] == null || waypoints[segmentIndex + 1] == null)
        {
            return Vector3.forward;
        }
        return waypoints[segmentIndex + 1].position - waypoints[segmentIndex].position;
    }

    private float CalculateSegmentTurn(int previousSegment, int nextSegment)
    {
        Vector3 from = Vector3.ProjectOnPlane(GetSegmentDirection(previousSegment), Vector3.up);
        Vector3 to = Vector3.ProjectOnPlane(GetSegmentDirection(nextSegment), Vector3.up);
        if (from.sqrMagnitude < 0.0001f || to.sqrMagnitude < 0.0001f)
        {
            return 0f;
        }
        return Vector3.SignedAngle(from, to, Vector3.up);
    }

    private string GetSegmentLabel(int segmentIndex)
    {
        if (segmentLabels != null && segmentIndex >= 0 && segmentIndex < segmentLabels.Length)
        {
            return segmentLabels[segmentIndex];
        }

        return "none";
    }

    private void StopWithError(string errorCode)
    {
        lifecycle = RouteLifecycle.Error;
        logger?.PauseRouteClock();
        logger?.Log("error", details: "error_code=" + errorCode + ";route_version=" + routeVersion);
        Debug.LogError(errorCode);
    }

    private void LogInvalidLifecycle(string action, string errorCode)
    {
        logger?.Log("error", details: "error_code=" + errorCode + ";action=" + action + ";route_state=" + lifecycle);
    }
}
