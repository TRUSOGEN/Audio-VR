using System;
using UnityEngine;

/// <summary>
/// Builds a richer runtime-only AudioV0 demo without modifying scene YAML.
/// It provides four longer routes and wires the code-layer controllers for interactive testing.
/// </summary>
public sealed class AudioV0RuntimeScenario : MonoBehaviour
{
    public EventLogger logger;
    public RoutePlayer routePlayer;
    public FlightStateMachine flightStateMachine;
    public NotificationAudio notificationAudio;
    public SessionController sessionController;
    public IdentificationController identificationController;
    public VisualTaskController visualTaskController;
    public ConditionController conditionController;
    public CabinNoisePlayer cabinNoisePlayer;
    public ExperimentFlowController flowController;
    public AudioV0RuntimeAudioDirector audioDirector;
    public AudioV0BlockRatingController blockRatingController;
    public AudioV0ExperimentSequenceController experimentSequenceController;
    public AudioV0ExperimentProfile experimentProfile;
    public AudioV0DataRepository dataRepository;

    public string selectedConditionId = "NS_Q";
    public int selectedRouteIndex;
    public string[] routeIds = { "route_alpine", "route_coastal", "route_mountain", "route_desert" };
    public bool autoStartDemo = false;
    public float autoStartDelaySeconds = 4f;

    /// <summary>Current passenger-facing phase used by the operator panel and runtime evidence.</summary>
    public string ExperiencePhase { get; private set; } = "boarding_safety_briefing";

    private Transform routeRoot;
    private Transform routeGuideRoot;
    private Transform[][] routeWaypoints;
    private bool configured;
    private float autoStartAt = -1f;
    private bool autoStartConsumed;
    private string lastExperiencePhase;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject systems = GameObject.Find("Systems");
        if (systems == null)
        {
            return;
        }

        AudioV0RuntimeScenario scenario = systems.GetComponent<AudioV0RuntimeScenario>();
        if (scenario == null)
        {
            scenario = systems.AddComponent<AudioV0RuntimeScenario>();
        }

        scenario.ConfigureRuntime();
    }

    /// <summary>Returns the current route label for the operator UI.</summary>
    public string SelectedRouteId => routeIds == null || selectedRouteIndex < 0 || selectedRouteIndex >= routeIds.Length ? "route_unknown" : routeIds[selectedRouteIndex];

    public bool SelectCondition(string conditionId)
    {
        if (sessionController != null && sessionController.State != SessionController.Lifecycle.Ready && sessionController.State != SessionController.Lifecycle.Completed)
        {
            return false;
        }

        selectedConditionId = conditionId;
        return conditionController == null || conditionController.SelectCondition(conditionId);
    }

    public bool SelectRoute(int routeIndex)
    {
        if (routeWaypoints == null || routeIndex < 0 || routeIndex >= routeWaypoints.Length)
        {
            return false;
        }

        bool activeBlock = sessionController != null && (sessionController.State == SessionController.Lifecycle.Running || sessionController.State == SessionController.Lifecycle.Paused);
        if (sessionController != null && !activeBlock && sessionController.State != SessionController.Lifecycle.Ready && sessionController.State != SessionController.Lifecycle.Completed)
        {
            return false;
        }
        if (activeBlock && routeIndex != selectedRouteIndex)
        {
            logger?.Log("route_switch_requested", details: "from=" + SelectedRouteId + ";to=" + routeIds[routeIndex] + ";reason=operator_runtime_selection");
            sessionController.EndBlock("route_switch");
        }

        selectedRouteIndex = routeIndex;
        ApplySelectedRoute();
        if (activeBlock && sessionController.State == SessionController.Lifecycle.Completed)
        {
            return StartSelectedBlock();
        }
        return true;
    }

    public bool StartSelectedBlock()
    {
        if (sessionController == null)
        {
            return false;
        }

        conditionController?.UnlockAfterBlock();
        conditionController?.SelectCondition(selectedConditionId);
        ApplySelectedRoute();
        blockRatingController?.ResetForNewBlock();
        return sessionController.StartBlock();
    }

    public bool IsConfigured => configured;
    public bool RouteGuidesVisible => routeGuideRoot != null && routeGuideRoot.gameObject.activeSelf;

    /// <summary>Toggles only non-task route aids; waypoint evidence and flight geometry stay unchanged.</summary>
    public bool SetRouteGuidesVisible(bool visible)
    {
        if (routeGuideRoot == null) return false;
        routeGuideRoot.gameObject.SetActive(visible);
        logger?.Log("route_guides_visibility", details: "visible=" + visible + ";scope=participant_presentation_only");
        return true;
    }

    /// <summary>Convenience entry point for the runtime UI route-aid button.</summary>
    public bool ToggleRouteGuides()
    {
        return SetRouteGuidesVisible(!RouteGuidesVisible);
    }

    private void ConfigureRuntime()
    {
        if (configured)
        {
            return;
        }

        logger = FindAnyObjectByType<EventLogger>();
        routePlayer = FindAnyObjectByType<RoutePlayer>();
        flightStateMachine = FindAnyObjectByType<FlightStateMachine>();
        notificationAudio = FindAnyObjectByType<NotificationAudio>();
        if (logger == null || routePlayer == null || notificationAudio == null)
        {
            return;
        }

        logger.LoggerReady -= OnLoggerReady;
        logger.LoggerReady += OnLoggerReady;

        GameObject systems = gameObject;
        sessionController = systems.GetComponent<SessionController>() ?? systems.AddComponent<SessionController>();
        identificationController = systems.GetComponent<IdentificationController>() ?? systems.AddComponent<IdentificationController>();
        visualTaskController = systems.GetComponent<VisualTaskController>() ?? systems.AddComponent<VisualTaskController>();
        conditionController = systems.GetComponent<ConditionController>() ?? systems.AddComponent<ConditionController>();
        flowController = systems.GetComponent<ExperimentFlowController>() ?? systems.AddComponent<ExperimentFlowController>();
        blockRatingController = systems.GetComponent<AudioV0BlockRatingController>() ?? systems.AddComponent<AudioV0BlockRatingController>();
        experimentSequenceController = systems.GetComponent<AudioV0ExperimentSequenceController>() ?? systems.AddComponent<AudioV0ExperimentSequenceController>();
        experimentProfile = systems.GetComponent<AudioV0ExperimentProfile>() ?? systems.AddComponent<AudioV0ExperimentProfile>();
        dataRepository = systems.GetComponent<AudioV0DataRepository>() ?? systems.AddComponent<AudioV0DataRepository>();

        GameObject noiseObject = new GameObject("AudioV0_RuntimeCabinNoise");
        noiseObject.transform.SetParent(transform, false);
        AudioSource noiseSource = noiseObject.AddComponent<AudioSource>();
        cabinNoisePlayer = noiseObject.AddComponent<CabinNoisePlayer>();
        cabinNoisePlayer.audioSource = noiseSource;
        cabinNoisePlayer.noiseClip = LoadClip("AudioV0Audio/SciFi/engineCircular_000") ?? notificationAudio.technicalClip;
        cabinNoisePlayer.logger = logger;

        GameObject audioObject = new GameObject("AudioV0_RuntimeExperienceAudio");
        audioObject.transform.SetParent(transform, false);
        AudioSource ambienceSource = audioObject.AddComponent<AudioSource>();
        AudioSource cueSource = audioObject.AddComponent<AudioSource>();
        audioDirector = audioObject.AddComponent<AudioV0RuntimeAudioDirector>();
        audioDirector.logger = logger;
        audioDirector.routePlayer = routePlayer;
        audioDirector.sessionController = sessionController;
        audioDirector.ambienceSource = ambienceSource;
        audioDirector.cueSource = cueSource;
        audioDirector.cabinRoot = routePlayer.movingRoot;
        if (routePlayer.movingRoot != null)
        {
            GameObject spatialObject = new GameObject("AudioV0_RuntimeSpatialTurnCue");
            spatialObject.transform.SetParent(routePlayer.movingRoot, false);
            audioDirector.spatialTurnSource = spatialObject.AddComponent<AudioSource>();
        }
        audioDirector.ambienceClips = LoadClips("AudioV0Audio/SciFi/spaceEngine_000", "AudioV0Audio/SciFi/spaceEngine_001", "AudioV0Audio/SciFi/engineCircular_000", "AudioV0Audio/SciFi/engineCircular_001");
        audioDirector.takeoffCues = LoadClips("AudioV0Audio/SciFi/thrusterFire_000", "AudioV0Audio/SciFi/doorOpen_000");
        audioDirector.cruiseCues = LoadClips("AudioV0Audio/SciFi/computerNoise_000", "AudioV0Audio/SciFi/forceField_000");
        audioDirector.descentCues = LoadClips("AudioV0Audio/SciFi/doorClose_000", "AudioV0Audio/SciFi/engineCircular_001");
        audioDirector.arrivalCues = LoadClips("AudioV0Audio/Music/jingles_HIT00", "AudioV0Audio/Music/jingles_NES00");
        audioDirector.turnCues = LoadClips("AudioV0Audio/SciFi/forceField_000", "AudioV0Audio/SciFi/thrusterFire_000");
        audioDirector.uiClickClips = LoadClips("AudioV0Audio/Interface/click_001", "AudioV0Audio/Interface/select_001", "AudioV0Audio/Interface/switch_001");
        audioDirector.uiConfirmClips = LoadClips("AudioV0Audio/Interface/confirmation_001", "AudioV0Audio/Interface/open_001");
        audioDirector.uiErrorClips = LoadClips("AudioV0Audio/Interface/error_001");
        notificationAudio.variantClips = LoadClips("AudioV0Audio/Interface/confirmation_001", "AudioV0Audio/Interface/open_001", "AudioV0Audio/SciFi/forceField_000");

        sessionController.logger = logger;
        sessionController.routePlayer = routePlayer;
        sessionController.flightStateMachine = flightStateMachine;
        sessionController.notificationAudio = notificationAudio;
        sessionController.identificationController = identificationController;
        sessionController.visualTaskController = visualTaskController;
        sessionController.conditionController = conditionController;
        sessionController.sessionMode = "technical_demo";

        identificationController.logger = logger;
        identificationController.notificationAudio = notificationAudio;
        identificationController.sessionController = sessionController;
        identificationController.Bind(logger, notificationAudio, sessionController);
        visualTaskController.logger = logger;
        dataRepository.logger = logger;
        conditionController.logger = logger;
        conditionController.cabinNoisePlayer = cabinNoisePlayer;
        flowController.logger = logger;
        blockRatingController.logger = logger;
        blockRatingController.sessionController = sessionController;
        experimentSequenceController.logger = logger;
        experimentSequenceController.scenario = this;
        experimentSequenceController.sessionController = sessionController;
        experimentSequenceController.ratingController = blockRatingController;

        notificationAudio.logger = logger;
        notificationAudio.routePlayer = routePlayer;
        routePlayer.logger = logger;
        ApplyExperimentProfile();
        routeWaypoints = BuildRoutes(routePlayer);
        BuildPrototypeRouteMarkers();
        BuildRuntimeCity();
        ApplySelectedRoute();
        conditionController.SelectCondition(selectedConditionId);
        configured = true;
        autoStartAt = Time.unscaledTime + Mathf.Max(0.5f, autoStartDelaySeconds);
        SetExperiencePhase("boarding_safety_briefing", "intro_delay_s=" + autoStartDelaySeconds.ToString("F1"));
        logger.Log("runtime_demo_ready", details: "routes=4;route_duration_target_s=60-80;conditions=NS_Q,NS_N,S_Q,S_N;mode=technical_demo;" + experimentProfile.ToManifestDetails());
        if (autoStartDemo)
        {
            Invoke(nameof(AutoStartIfReady), Mathf.Max(0.5f, autoStartDelaySeconds));
        }
        if (logger.IsReady)
        {
            OnLoggerReady();
        }
    }

    private void Start()
    {
        if (configured && autoStartDemo)
        {
            Invoke(nameof(AutoStartIfReady), Mathf.Max(0.5f, autoStartDelaySeconds));
        }
    }

    /// <summary>Applies one scene-editable experiment profile before routes and tasks are initialized.</summary>
    private void ApplyExperimentProfile()
    {
        if (experimentProfile == null) return;
        routePlayer.routeVersion = experimentProfile.routeVersion;
        routePlayer.playOnStart = false;
        routePlayer.speedMetersPerSecond = experimentProfile.routeSpeedMetersPerSecond;
        identificationController.responseWindowSeconds = experimentProfile.identificationWindowSeconds;
        visualTaskController.taskVersion = experimentProfile.visualTaskVersion;
        visualTaskController.randomSeed = experimentProfile.randomSeed;
        visualTaskController.stimulusDurationSeconds = experimentProfile.visualTargetWindowSeconds;
        visualTaskController.minimumIntervalSeconds = experimentProfile.visualInterStimulusSeconds;
        visualTaskController.targetRate = experimentProfile.visualTargetRate;
        if (flightStateMachine != null) flightStateMachine.notificationLeadTimeSeconds = experimentProfile.notificationLeadSeconds;
    }

    private void Update()
    {
        UpdateExperiencePhase();
        if (!configured || autoStartConsumed || !autoStartDemo || sessionController == null || Time.unscaledTime < autoStartAt)
        {
            return;
        }

        AutoStartIfReady();
    }

    private void AutoStartIfReady()
    {
        if (!configured || autoStartConsumed || !autoStartDemo || sessionController == null)
        {
            return;
        }

        autoStartConsumed = true;
        logger?.Log("runtime_demo_autostart", details: "route_id=" + SelectedRouteId + ";condition_id=" + selectedConditionId);
        if (!StartSelectedBlock())
        {
            logger?.Log("runtime_demo_autostart_failed", details: "state=" + sessionController.State + ";error=" + sessionController.LastErrorCode);
        }
    }

    /// <summary>Called by EventLogger after session_start so the demo never starts before logging is ready.</summary>
    public void OnLoggerReady()
    {
        if (configured && autoStartDemo && !autoStartConsumed)
        {
            AutoStartIfReady();
        }
    }

    private void UpdateExperiencePhase()
    {
        if (!configured || sessionController == null)
        {
            return;
        }

        string phase = ExperiencePhase;
        if (sessionController.State == SessionController.Lifecycle.Ready || sessionController.State == SessionController.Lifecycle.Preflight)
        {
            phase = "boarding_safety_briefing";
        }
        else if (sessionController.State == SessionController.Lifecycle.Paused)
        {
            phase = "paused_cabin_briefing";
        }
        else if (sessionController.State == SessionController.Lifecycle.Completed)
        {
            phase = "arrived_postflight_review";
        }
        else if (sessionController.State == SessionController.Lifecycle.Error)
        {
            phase = "service_recovery";
        }
        else if (routePlayer != null)
        {
            phase = routePlayer.CurrentSegmentLabel switch
            {
                "climb" => "takeoff_climb",
                "cruise" => routePlayer.CurrentSegment >= 3 ? "enroute_turn_cruise" : "cruise_city_view",
                "turn" => "enroute_turn_cruise",
                "descent" => "descent_approach",
                "landing_preparation" => "landing_preparation",
                _ => "in_flight"
            };
        }

        SetExperiencePhase(phase, "segment=" + (routePlayer == null ? -1 : routePlayer.CurrentSegment));
    }

    private void SetExperiencePhase(string phase, string details)
    {
        if (string.Equals(lastExperiencePhase, phase, StringComparison.Ordinal))
        {
            ExperiencePhase = phase;
            return;
        }

        lastExperiencePhase = phase;
        ExperiencePhase = phase;
        logger?.Log("experience_phase", details: "phase=" + phase + ";" + details);
    }

    private void ApplySelectedRoute()
    {
        if (routePlayer == null || routeWaypoints == null || selectedRouteIndex < 0 || selectedRouteIndex >= routeWaypoints.Length)
        {
            return;
        }

        routePlayer.waypoints = routeWaypoints[selectedRouteIndex];
        routePlayer.routeVersion = SelectedRouteId + "_v0";
        routePlayer.segmentLabels = new[] { "climb", "climb", "cruise", "cruise", "turn", "turn", "descent", "descent", "descent", "landing_preparation", "landing_preparation" };
        if (logger != null)
        {
            logger.routeId = SelectedRouteId;
            logger.routeVersion = routePlayer.routeVersion;
        }
    }

    private Transform[][] BuildRoutes(RoutePlayer player)
    {
        Transform parent = player.waypoints != null && player.waypoints.Length > 0 && player.waypoints[0] != null ? player.waypoints[0].parent : null;
        routeRoot = new GameObject("AudioV0_RuntimeRoutes").transform;
        routeRoot.SetParent(parent, false);

        Vector3[][] positions =
        {
            new[] { new Vector3(0f, 0f, 0f), new Vector3(-2f, 4f, 12f), new Vector3(-8f, 9f, 27f), new Vector3(-14f, 12f, 43f), new Vector3(-6f, 11f, 58f), new Vector3(8f, 10f, 72f), new Vector3(14f, 8f, 87f), new Vector3(5f, 6f, 101f), new Vector3(-8f, 4f, 113f), new Vector3(-12f, 2f, 124f), new Vector3(-4f, 0.8f, 135f), new Vector3(0f, 0f, 146f) },
            new[] { new Vector3(0f, 0f, 0f), new Vector3(4f, 5f, 14f), new Vector3(12f, 10f, 30f), new Vector3(18f, 13f, 47f), new Vector3(10f, 12f, 63f), new Vector3(-4f, 11f, 78f), new Vector3(-14f, 9f, 92f), new Vector3(-18f, 6f, 106f), new Vector3(-10f, 4f, 120f), new Vector3(4f, 2f, 133f), new Vector3(10f, 1f, 143f), new Vector3(0f, 0f, 153f) },
            new[] { new Vector3(0f, 0f, 0f), new Vector3(-5f, 6f, 13f), new Vector3(-16f, 13f, 28f), new Vector3(-20f, 16f, 45f), new Vector3(-8f, 14f, 60f), new Vector3(6f, 13f, 76f), new Vector3(18f, 11f, 91f), new Vector3(12f, 8f, 105f), new Vector3(-2f, 5f, 119f), new Vector3(-14f, 3f, 132f), new Vector3(-7f, 1f, 144f), new Vector3(0f, 0f, 156f) },
            new[] { new Vector3(0f, 0f, 0f), new Vector3(5f, 3f, 16f), new Vector3(15f, 8f, 31f), new Vector3(21f, 11f, 48f), new Vector3(12f, 10f, 66f), new Vector3(-2f, 9f, 84f), new Vector3(-17f, 8f, 99f), new Vector3(-22f, 5f, 114f), new Vector3(-13f, 3f, 129f), new Vector3(2f, 1f, 142f), new Vector3(12f, 0.6f, 153f), new Vector3(0f, 0f, 164f) }
        };

        Transform[][] result = new Transform[positions.Length][];
        for (int routeIndex = 0; routeIndex < positions.Length; routeIndex++)
        {
            result[routeIndex] = new Transform[positions[routeIndex].Length];
            for (int pointIndex = 0; pointIndex < positions[routeIndex].Length; pointIndex++)
            {
                GameObject point = new GameObject(routeIds[routeIndex] + "_W" + pointIndex.ToString("D2"));
                point.transform.SetParent(routeRoot, false);
                point.transform.position = positions[routeIndex][pointIndex];
                result[routeIndex][pointIndex] = point.transform;
            }
        }

        return result;
    }

    /// <summary>
    /// Adds lightweight Kenney low-poly gates at route boundaries. They are visual landmarks only;
    /// the route evidence continues to come from the waypoint geometry and logged segment predicates.
    /// </summary>
    private void BuildPrototypeRouteMarkers()
    {
        GameObject markerPrefab = Resources.Load<GameObject>("AudioV0Prototype/indicator-doorway");
        if (markerPrefab == null)
        {
            markerPrefab = Resources.Load<GameObject>("AudioV0Prototype/column-low");
        }

        if (markerPrefab == null || routeWaypoints == null)
        {
            logger?.Log("runtime_art_unavailable", details: "resource=AudioV0Prototype/indicator-doorway_or_column-low");
            return;
        }

        routeGuideRoot = new GameObject("AudioV0_RuntimeRouteGuides").transform;
        routeGuideRoot.SetParent(routeRoot, false);
        Transform markerRoot = new GameObject("AudioV0_RuntimeLowPolyRouteMarkers").transform;
        markerRoot.SetParent(routeGuideRoot, false);
        for (int routeIndex = 0; routeIndex < routeWaypoints.Length; routeIndex++)
        {
            for (int waypointIndex = 1; waypointIndex < routeWaypoints[routeIndex].Length - 1; waypointIndex++)
            {
                GameObject marker = Instantiate(markerPrefab, markerRoot);
                marker.name = routeIds[routeIndex] + "_Gate_" + waypointIndex.ToString("D2");
                marker.transform.position = routeWaypoints[routeIndex][waypointIndex].position;
                Vector3 markerDirection = routeWaypoints[routeIndex][waypointIndex + 1].position - routeWaypoints[routeIndex][waypointIndex - 1].position;
                marker.transform.rotation = markerDirection.sqrMagnitude > 0.001f ? Quaternion.LookRotation(markerDirection, Vector3.up) : Quaternion.identity;
                marker.transform.localScale = Vector3.one * 0.6f;
            }

            CreateRouteTrail(routeIndex);
        }

        int markerCount = routeWaypoints.Length * Mathf.Max(0, routeWaypoints[0].Length - 2);
        logger?.Log("runtime_art_ready", details: "asset=Kenney_Prototype_Kit;markers=" + markerCount.ToString());
        SetRouteGuidesVisible(experimentProfile != null && experimentProfile.showRouteGuidesAtStart);
    }

    private void CreateRouteTrail(int routeIndex)
    {
        if (routeWaypoints == null || routeIndex < 0 || routeIndex >= routeWaypoints.Length)
        {
            return;
        }

        GameObject trailObject = new GameObject(routeIds[routeIndex] + "_RouteTrail");
        trailObject.transform.SetParent(routeGuideRoot == null ? routeRoot : routeGuideRoot, false);
        LineRenderer trail = trailObject.AddComponent<LineRenderer>();
        trail.positionCount = routeWaypoints[routeIndex].Length;
        trail.startWidth = 0.16f;
        trail.endWidth = 0.06f;
        trail.useWorldSpace = true;
        Color[] colors = { new Color(0.12f, 0.85f, 1f, 0.8f), new Color(0.35f, 1f, 0.55f, 0.8f), new Color(0.75f, 0.35f, 1f, 0.8f), new Color(1f, 0.55f, 0.18f, 0.8f) };
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material material = new Material(shader);
            material.color = colors[routeIndex % colors.Length];
            trail.material = material;
        }
        for (int i = 0; i < routeWaypoints[routeIndex].Length; i++)
        {
            Vector3 point = routeWaypoints[routeIndex][i].position;
            trail.SetPosition(i, point + Vector3.up * 0.18f);
        }
    }

    private void BuildRuntimeCity()
    {
        if (routeRoot == null)
        {
            return;
        }

        Transform cityRoot = new GameObject("AudioV0_RuntimeCity").transform;
        cityRoot.SetParent(routeRoot, false);
        System.Random random = new System.Random(1701);
        Color[] palette =
        {
            new Color(0.08f, 0.16f, 0.24f),
            new Color(0.12f, 0.25f, 0.31f),
            new Color(0.22f, 0.27f, 0.32f),
            new Color(0.16f, 0.21f, 0.29f)
        };

        int buildingCount = 0;
        for (int zIndex = 0; zIndex < 8; zIndex++)
        {
            float z = 10f + zIndex * 17f;
            for (int side = -1; side <= 1; side += 2)
            {
                int sideCount = 2 + random.Next(0, 2);
                for (int lane = 0; lane < sideCount; lane++)
                {
                    float x = side * (26f + lane * 8f + (float)random.NextDouble() * 4f);
                    float width = 3.5f + (float)random.NextDouble() * 3.5f;
                    float depth = 4f + (float)random.NextDouble() * 4f;
                    float height = 4f + (float)random.NextDouble() * 12f;
                    GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    building.name = "CityBlock_" + buildingCount.ToString("D2");
                    building.transform.SetParent(cityRoot, false);
                    building.transform.position = new Vector3(x, height * 0.5f, z + (float)random.NextDouble() * 5f);
                    building.transform.localScale = new Vector3(width, height, depth);
                    Renderer renderer = building.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = palette[random.Next(0, palette.Length)];
                    }
                    buildingCount++;
                }
            }
        }

        CreateLandingPad(cityRoot, new Vector3(-9f, 0.15f, 122f), Color.cyan);
        CreateLandingPad(cityRoot, new Vector3(9f, 0.15f, 138f), new Color(1f, 0.55f, 0.18f));
        logger?.Log("runtime_city_ready", details: "buildings=" + buildingCount + ";pads=2;seed=1701;style=low_poly_blocks");
    }

    private void CreateLandingPad(Transform parent, Vector3 position, Color color)
    {
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad.name = "CityBlock_LandingPad";
        pad.transform.SetParent(parent, false);
        pad.transform.position = position;
        pad.transform.localScale = new Vector3(3.5f, 0.15f, 3.5f);
        Renderer renderer = pad.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
    }

    private AudioClip LoadClip(string path)
    {
        return Resources.Load<AudioClip>(path);
    }

    private AudioClip[] LoadClips(params string[] paths)
    {
        System.Collections.Generic.List<AudioClip> clips = new System.Collections.Generic.List<AudioClip>();
        for (int i = 0; i < paths.Length; i++)
        {
            AudioClip clip = LoadClip(paths[i]);
            if (clip != null)
            {
                clips.Add(clip);
            }
        }
        return clips.ToArray();
    }
}
