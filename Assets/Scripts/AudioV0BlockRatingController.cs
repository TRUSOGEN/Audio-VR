using System;
using UnityEngine;

/// <summary>
/// Collects a short, explicitly exploratory post-block passenger rating set.
/// It is kept separate from formal validated questionnaires such as NASA-TLX,
/// so the technical demo never labels these single items as a validated scale.
/// </summary>
public sealed class AudioV0BlockRatingController : MonoBehaviour
{
    private static readonly string[] QuestionIds =
    {
        "cue_clarity",
        "cue_audibility",
        "cue_annoyance",
        "flight_comfort",
        "perceived_workload",
        "transition_confidence"
    };

    public EventLogger logger;
    public SessionController sessionController;

    public bool IsRatingOpen { get; private set; }
    public bool IsComplete { get; private set; }
    public int CurrentQuestionIndex { get; private set; }
    public int QuestionCount => QuestionIds.Length;
    public string CurrentQuestionId => CurrentQuestionIndex < QuestionIds.Length ? QuestionIds[CurrentQuestionIndex] : "";

    private bool completionObserved;

    private void Update()
    {
        if (!completionObserved && sessionController != null && sessionController.State == SessionController.Lifecycle.Completed)
        {
            completionObserved = true;
            IsRatingOpen = true;
            logger?.Log("block_rating_open", details: "scale=1_to_7;question_count=" + QuestionCount + ";instrument=exploratory_single_items");
        }
    }

    /// <summary>Clears the previous block's form before a new selected route is started.</summary>
    public void ResetForNewBlock()
    {
        IsRatingOpen = false;
        IsComplete = false;
        CurrentQuestionIndex = 0;
        completionObserved = false;
    }

    /// <summary>Records one 1–7 rating for the currently presented item.</summary>
    public bool SubmitRating(int value, string controlId)
    {
        if (!IsRatingOpen || IsComplete || value < 1 || value > 7 || string.IsNullOrEmpty(CurrentQuestionId))
        {
            logger?.Log("input_rejected", details: "action=block_rating;control_id=" + controlId + ";reason=no_active_rating_item");
            return false;
        }

        logger?.Log(
            "block_rating_response",
            details: "question_id=" + CurrentQuestionId + ";value=" + value + ";scale_min=1;scale_max=7;control_id=" + controlId + ";instrument=exploratory_single_item"
        );
        CurrentQuestionIndex++;
        if (CurrentQuestionIndex >= QuestionCount)
        {
            IsComplete = true;
            IsRatingOpen = false;
            logger?.Log("block_rating_complete", details: "question_count=" + QuestionCount + ";instrument=exploratory_single_items");
        }
        return true;
    }

    /// <summary>Returns a participant-facing prompt and scale anchors for the active single item.</summary>
    public string GetPrompt(bool chinese)
    {
        switch (CurrentQuestionId)
        {
            case "cue_clarity": return chinese ? "提示音含义有多清楚？  1 非常不清楚  ·  7 非常清楚" : "How clear was the cue meaning?  1 not clear  ·  7 very clear";
            case "cue_audibility": return chinese ? "提示音有多容易听见？  1 很难听见  ·  7 很容易听见" : "How audible was the cue?  1 very hard to hear  ·  7 very easy to hear";
            case "cue_annoyance": return chinese ? "提示音有多令人烦扰？  1 完全不烦扰  ·  7 非常烦扰" : "How annoying was the cue?  1 not annoying  ·  7 very annoying";
            case "flight_comfort": return chinese ? "本段飞行有多舒适？  1 非常不舒适  ·  7 非常舒适" : "How comfortable was this flight segment?  1 very uncomfortable  ·  7 very comfortable";
            case "perceived_workload": return chinese ? "完成任务时主观负荷有多高？  1 很低  ·  7 很高" : "How high was your perceived workload?  1 very low  ·  7 very high";
            case "transition_confidence": return chinese ? "你对即将发生阶段的判断有多有把握？  1 完全没把握  ·  7 非常有把握" : "How confident were you about the next flight phase?  1 not confident  ·  7 very confident";
            default: return chinese ? "区块回顾" : "Block review";
        }
    }
}
