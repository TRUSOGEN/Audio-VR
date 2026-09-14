using UnityEngine;

/// <summary>
/// Stores the scene-level parameters required to turn an Audio V0 scene into a
/// repeatable experiment. Keep this component on the scene's Systems object when
/// duplicating a scene or replacing the passenger-cabin model.
/// </summary>
[DisallowMultipleComponent]
public sealed class AudioV0ExperimentProfile : MonoBehaviour
{
    [Header("Identity and reproducibility")]
    public string profileId = "audio_v0_uam_passenger_v1";
    public string routeVersion = "route_v0_candidate";
    public string visualTaskVersion = "visual_candidate_v1";
    public int randomSeed = 1701;

    [Header("Participant response windows")]
    [Min(1f)] public float identificationWindowSeconds = 8f;
    [Min(0.5f)] public float visualTargetWindowSeconds = 2.2f;
    [Min(0.5f)] public float visualInterStimulusSeconds = 2.8f;
    [Range(0f, 1f)] public float visualTargetRate = 0.25f;

    [Header("Flight and presentation")]
    [Min(0.1f)] public float routeSpeedMetersPerSecond = 2.5f;
    [Min(0f)] public float notificationLeadSeconds = 4f;
    [Tooltip("Route lines and low-poly gate guides are hidden by default for participant runs.")]
    public bool showRouteGuidesAtStart;

    [Header("XR")]
    [Tooltip("When an OpenXR device is active, present the runtime UI as a head-locked world-space panel.")]
    public bool enableXRPresentation = true;
    [Range(1.2f, 3f)] public float xrPanelDistanceMeters = 1.8f;
    [Range(0.0005f, 0.003f)] public float xrCanvasScale = 0.0012f;

    /// <summary>Returns a compact manifest fragment included in the runtime log.</summary>
    public string ToManifestDetails()
    {
        return "profile_id=" + profileId
            + ";route_version=" + routeVersion
            + ";visual_task_version=" + visualTaskVersion
            + ";visual_target_window_s=" + visualTargetWindowSeconds.ToString("F2")
            + ";visual_interval_s=" + visualInterStimulusSeconds.ToString("F2")
            + ";identification_window_s=" + identificationWindowSeconds.ToString("F2")
            + ";route_guides_at_start=" + showRouteGuidesAtStart
            + ";xr_presentation=" + enableXRPresentation;
    }
}
