using System;
using UnityEngine;

/// <summary>
/// Records the operator-controlled session flow and prevents invalid automatic jumps between study phases.
/// </summary>
public sealed class ExperimentFlowController : MonoBehaviour
{
    public enum FlowState
    {
        Preparation,
        Acclimatisation,
        Practice,
        SoundLearning,
        Blocks,
        FinalRatings,
        Debrief,
        End,
        Aborted
    }

    public EventLogger logger;
    public FlowState State { get; private set; } = FlowState.Preparation;
    public int CompletedBlocks { get; private set; }

    /// <summary>Moves to a phase only when the current phase permits that transition.</summary>
    public bool MoveTo(FlowState nextState, string reason = "operator_action")
    {
        if (!IsAllowed(State, nextState))
        {
            logger?.Log("flow_transition_rejected", details: "from=" + State + ";to=" + nextState + ";reason=invalid_flow_transition");
            return false;
        }

        FlowState previous = State;
        State = nextState;
        logger?.Log("flow_transition", details: "from=" + previous + ";to=" + nextState + ";reason=" + reason);
        return true;
    }

    /// <summary>Records a practice attempt without mixing it into formal block denominators.</summary>
    public void RecordPracticeAttempt(string practiceRouteId, string response, bool correct, int attemptIndex, string terminationReason = "completed")
    {
        if (State != FlowState.Practice)
        {
            logger?.Log("input_rejected", details: "action=practice_attempt;reason=flow_state_not_practice;state=" + State);
            return;
        }

        logger?.Log(
            "practice_attempt",
            details: "mode=practice;practice_route_id=" + practiceRouteId + ";response=" + response + ";correct=" + correct + ";attempt_index=" + attemptIndex + ";termination_reason=" + terminationReason
        );
    }

    /// <summary>Records a raw questionnaire item; no answer is invented when the participant skips it.</summary>
    public void RecordQuestionnaireItem(string questionnaireVersion, string itemId, string rawAnswer, bool skipped)
    {
        if (State != FlowState.FinalRatings && State != FlowState.Debrief)
        {
            logger?.Log("input_rejected", details: "action=questionnaire;reason=flow_state_not_questionnaire;state=" + State);
            return;
        }

        logger?.Log(
            "questionnaire_response",
            details: "questionnaire_version=" + questionnaireVersion + ";item_id=" + itemId + ";raw_answer=" + rawAnswer + ";skipped=" + skipped
        );
    }

    /// <summary>Marks one block complete only after SessionController has produced its completion status.</summary>
    public void RecordBlockCompleted(string blockId, string conditionId, bool complete)
    {
        if (State != FlowState.Blocks)
        {
            logger?.Log("input_rejected", details: "action=block_complete;reason=flow_state_not_blocks;state=" + State);
            return;
        }

        if (complete)
        {
            CompletedBlocks++;
        }

        logger?.Log("block_flow_result", details: "block_id=" + blockId + ";condition_id=" + conditionId + ";complete=" + complete + ";completed_blocks=" + CompletedBlocks);
    }

    /// <summary>Stops the flow with a partial-session marker and keeps existing raw logs intact.</summary>
    public void Abort(string reason)
    {
        if (State == FlowState.End || State == FlowState.Aborted)
        {
            return;
        }

        State = FlowState.Aborted;
        logger?.Log("flow_aborted", details: "reason=" + reason + ";completed_blocks=" + CompletedBlocks);
    }

    private bool IsAllowed(FlowState from, FlowState to)
    {
        if (from == FlowState.Aborted || from == FlowState.End)
        {
            return false;
        }

        if (to == FlowState.Aborted)
        {
            return true;
        }

        switch (from)
        {
            case FlowState.Preparation:
                return to == FlowState.Acclimatisation;
            case FlowState.Acclimatisation:
                return to == FlowState.Practice;
            case FlowState.Practice:
                return to == FlowState.SoundLearning || to == FlowState.Blocks;
            case FlowState.SoundLearning:
                return to == FlowState.Blocks;
            case FlowState.Blocks:
                return to == FlowState.FinalRatings || to == FlowState.Blocks;
            case FlowState.FinalRatings:
                return to == FlowState.Debrief;
            case FlowState.Debrief:
                return to == FlowState.End;
            default:
                return false;
        }
    }
}
