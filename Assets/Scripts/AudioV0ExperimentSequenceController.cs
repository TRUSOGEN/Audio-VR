using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the participant-facing four-block technical-demo journey.
/// It preserves a short break after each block rating, then advances to the next
/// condition and route without returning the participant to an unexplained idle screen.
/// </summary>
public sealed class AudioV0ExperimentSequenceController : MonoBehaviour
{
    public EventLogger logger;
    public AudioV0RuntimeScenario scenario;
    public SessionController sessionController;
    public AudioV0BlockRatingController ratingController;
    public int participantSequenceIndex;

    public bool IsSequenceStarted { get; private set; }
    public bool IsAwaitingNextBlock { get; private set; }
    public bool IsFinaleVisible { get; private set; }
    public int CurrentBlockIndex { get; private set; }
    public int TotalBlocks => conditionOrder == null ? 0 : conditionOrder.Length;
    public string CurrentConditionId => GetCondition(CurrentBlockIndex);
    public string NextConditionId => GetCondition(CurrentBlockIndex + 1);
    public int CurrentRouteIndex => GetRoute(CurrentBlockIndex);
    public int NextRouteIndex => GetRoute(CurrentBlockIndex + 1);

    private string[] conditionOrder;
    private int[] routeOrder;
    private bool completionRecorded;

    /// <summary>Starts the participant-selected first block and prepares the remaining unique conditions.</summary>
    public bool BeginSequence()
    {
        if (IsSequenceStarted || scenario == null || sessionController == null || sessionController.State != SessionController.Lifecycle.Ready)
        {
            logger?.Log("input_rejected", details: "action=begin_sequence;reason=sequence_or_session_not_ready");
            return false;
        }

        conditionOrder = BuildConditionOrder(scenario.selectedConditionId);
        routeOrder = BuildRouteOrder(scenario.selectedRouteIndex, conditionOrder.Length);
        IsSequenceStarted = true;
        CurrentBlockIndex = 0;
        logger?.Log("experiment_sequence_started", details: "total_blocks=" + TotalBlocks + ";condition_order=" + string.Join(",", conditionOrder) + ";route_order=" + string.Join(",", routeOrder));
        bool started = StartCurrentBlock();
        if (!started)
        {
            IsSequenceStarted = false;
        }
        return started;
    }

    /// <summary>Starts the explicitly announced next block after its rating card is complete.</summary>
    public bool StartNextBlock()
    {
        if (!IsAwaitingNextBlock || IsFinaleVisible)
        {
            logger?.Log("input_rejected", details: "action=start_next_block;reason=next_block_not_ready");
            return false;
        }

        int previousIndex = CurrentBlockIndex;
        CurrentBlockIndex++;
        bool started = StartCurrentBlock();
        if (!started)
        {
            CurrentBlockIndex = previousIndex;
            return false;
        }

        IsAwaitingNextBlock = false;
        completionRecorded = false;
        return true;
    }

    /// <summary>Copies the raw JSONL location for a participant/operator without altering its contents.</summary>
    public bool CopyLogPath()
    {
        if (logger == null || string.IsNullOrEmpty(logger.LogPath))
        {
            return false;
        }

        GUIUtility.systemCopyBuffer = logger.LogPath;
        if (logger.IsReady)
        {
            logger.Log("log_path_copied", details: "destination=system_clipboard;path=" + logger.LogPath);
        }
        return true;
    }

    private void Update()
    {
        if (!IsSequenceStarted || IsAwaitingNextBlock || IsFinaleVisible || sessionController == null || ratingController == null)
        {
            return;
        }

        if (sessionController.State != SessionController.Lifecycle.Completed || sessionController.routePlayer == null || !sessionController.routePlayer.IsComplete || !ratingController.IsComplete || completionRecorded)
        {
            return;
        }

        completionRecorded = true;
        logger?.Log("experiment_block_complete", details: "block_index=" + (CurrentBlockIndex + 1) + ";total_blocks=" + TotalBlocks + ";condition_id=" + CurrentConditionId + ";route_index=" + CurrentRouteIndex);
        if (CurrentBlockIndex + 1 < TotalBlocks)
        {
            IsAwaitingNextBlock = true;
            logger?.Log("experiment_next_block_ready", details: "next_block_index=" + (CurrentBlockIndex + 2) + ";next_condition_id=" + NextConditionId + ";next_route_index=" + NextRouteIndex);
            return;
        }

        IsFinaleVisible = true;
        logger?.Log("experiment_sequence_complete", details: "completed_blocks=" + TotalBlocks + ";status=technical_demo_complete");
        sessionController.EndSession("technical_demo_sequence_complete");
    }

    private bool StartCurrentBlock()
    {
        string condition = CurrentConditionId;
        int route = CurrentRouteIndex;
        if (string.IsNullOrEmpty(condition) || route < 0 || scenario == null)
        {
            logger?.Log("error", details: "error_code=SEQUENCE_CONFIGURATION_INVALID");
            return false;
        }

        // StartSelectedBlock owns the unlock/select/apply sequence. Setting these public
        // selections first avoids a rejected select against the prior block's lock.
        scenario.selectedConditionId = condition;
        scenario.selectedRouteIndex = route;
        bool started = scenario.StartSelectedBlock();
        logger?.Log("experiment_block_start_requested", details: "block_index=" + (CurrentBlockIndex + 1) + ";total_blocks=" + TotalBlocks + ";condition_id=" + condition + ";route_index=" + route + ";started=" + started);
        return started;
    }

    private string[] BuildConditionOrder(string firstCondition)
    {
        string[] candidate = RouteAllocationPlanner.GetCandidateOrder(participantSequenceIndex);
        List<string> result = new List<string>();
        if (!string.IsNullOrWhiteSpace(firstCondition))
        {
            result.Add(firstCondition);
        }
        for (int i = 0; i < candidate.Length; i++)
        {
            if (!result.Contains(candidate[i]))
            {
                result.Add(candidate[i]);
            }
        }
        return result.ToArray();
    }

    private static int[] BuildRouteOrder(int firstRoute, int count)
    {
        int[] order = new int[count];
        int normalized = Mathf.Clamp(firstRoute, 0, Mathf.Max(0, count - 1));
        for (int i = 0; i < count; i++)
        {
            order[i] = (normalized + i) % count;
        }
        return order;
    }

    private string GetCondition(int index)
    {
        return conditionOrder != null && index >= 0 && index < conditionOrder.Length ? conditionOrder[index] : "";
    }

    private int GetRoute(int index)
    {
        return routeOrder != null && index >= 0 && index < routeOrder.Length ? routeOrder[index] : -1;
    }
}
