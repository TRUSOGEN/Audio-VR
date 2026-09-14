using System;
using UnityEngine;

/// <summary>
/// Locks the four paper-defined conditions before a block and prevents technical/practice assets entering main-study data.
/// </summary>
public sealed class ConditionController : MonoBehaviour
{
    [Serializable]
    public sealed class ConditionDefinition
    {
        public string conditionId;
        public bool speech;
        public bool cabinNoise;
        public string stimulusSetVersion = "provisional_v0";
        public string noiseVersion = "quiet";
    }

    public EventLogger logger;
    public CabinNoisePlayer cabinNoisePlayer;
    public ConditionDefinition[] conditions =
    {
        new ConditionDefinition { conditionId = "NS_Q", speech = false, cabinNoise = false, noiseVersion = "quiet" },
        new ConditionDefinition { conditionId = "NS_N", speech = false, cabinNoise = true, noiseVersion = "simulated_cabin_noise_v0" },
        new ConditionDefinition { conditionId = "S_Q", speech = true, cabinNoise = false, noiseVersion = "quiet" },
        new ConditionDefinition { conditionId = "S_N", speech = true, cabinNoise = true, noiseVersion = "simulated_cabin_noise_v0" }
    };
    public AudioClip[] speechClips;
    public AudioClip[] nonSpeechClips;

    public string SelectedConditionId { get; private set; }
    public bool IsLocked { get; private set; }

    /// <summary>Selects a condition before the block starts; selection becomes immutable after Lock.</summary>
    public bool SelectCondition(string conditionId)
    {
        if (IsLocked)
        {
            logger?.Log("input_rejected", details: "action=condition_select;reason=condition_already_locked;condition_id=" + conditionId);
            return false;
        }

        ConditionDefinition definition = FindCondition(conditionId);
        if (definition == null)
        {
            logger?.Log("input_rejected", details: "action=condition_select;reason=unsupported_condition;condition_id=" + conditionId);
            return false;
        }

        SelectedConditionId = definition.conditionId;
        return true;
    }

    /// <summary>Validates resources and writes the locked condition metadata into the logger.</summary>
    public bool LockForBlock(string mode)
    {
        if (IsLocked)
        {
            return true;
        }

        if (string.Equals(mode, "technical", StringComparison.OrdinalIgnoreCase) || string.Equals(mode, "practice", StringComparison.OrdinalIgnoreCase) || string.Equals(mode, "technical_demo", StringComparison.OrdinalIgnoreCase))
        {
            SelectedConditionId = string.IsNullOrWhiteSpace(SelectedConditionId) ? "TECH_TEST" : SelectedConditionId;
            ConditionDefinition preview = FindCondition(SelectedConditionId);
            if (preview != null && logger != null)
            {
                logger.conditionId = preview.conditionId;
                logger.notificationDesign = preview.speech ? "demo_speech_placeholder" : "demo_non_speech_placeholder";
                logger.listeningCondition = preview.cabinNoise ? "demo_cabin_noise_placeholder" : "quiet";
                logger.stimulusSetVersion = preview.stimulusSetVersion;
                logger.noiseVersion = preview.noiseVersion;
            }
            logger?.Log("condition_locked", details: "mode=" + mode + ";condition_id=" + SelectedConditionId + ";technical_only=true;formal_resources_not_claimed=true");
            IsLocked = true;
            return true;
        }

        ConditionDefinition definition = FindCondition(SelectedConditionId);
        if (definition == null)
        {
            return Fail("CONDITION_NOT_SELECTED");
        }

        AudioClip[] requiredClips = definition.speech ? speechClips : nonSpeechClips;
        if (requiredClips == null || requiredClips.Length < 3 || HasMissingClip(requiredClips))
        {
            return Fail("CONDITION_CUE_SET_INCOMPLETE");
        }

        if (definition.cabinNoise && (cabinNoisePlayer == null || !cabinNoisePlayer.HasConfiguredClip))
        {
            return Fail("CONDITION_NOISE_RESOURCE_MISSING");
        }

        if (logger != null)
        {
            logger.conditionId = definition.conditionId;
            logger.notificationDesign = definition.speech ? "speech" : "non_speech";
            logger.listeningCondition = definition.cabinNoise ? "simulated_cabin_noise" : "quiet";
            logger.stimulusSetVersion = definition.stimulusSetVersion;
            logger.noiseVersion = definition.noiseVersion;
        }

        IsLocked = true;
        logger?.Log("condition_locked", details: "mode=main_study;condition_id=" + definition.conditionId + ";speech=" + definition.speech + ";cabin_noise=" + definition.cabinNoise);
        return true;
    }

    /// <summary>Unlocks only after a block has ended, allowing the next block to select a new condition.</summary>
    public void UnlockAfterBlock()
    {
        IsLocked = false;
        SelectedConditionId = "";
    }

    public ConditionDefinition GetSelectedDefinition()
    {
        return FindCondition(SelectedConditionId);
    }

    private ConditionDefinition FindCondition(string conditionId)
    {
        if (conditions == null)
        {
            return null;
        }

        for (int i = 0; i < conditions.Length; i++)
        {
            if (conditions[i] != null && string.Equals(conditions[i].conditionId, conditionId, StringComparison.OrdinalIgnoreCase))
            {
                return conditions[i];
            }
        }

        return null;
    }

    private bool HasMissingClip(AudioClip[] clips)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] == null)
            {
                return true;
            }
        }

        return false;
    }

    private bool Fail(string errorCode)
    {
        logger?.Log("error", details: "error_code=" + errorCode + ";condition_preflight_failed");
        return false;
    }
}
