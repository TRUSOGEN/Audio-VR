using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Builds the passenger-lab operator surface at runtime. Layout is driven by Unity UI groups,
/// so the panel remains aligned when the Game view aspect ratio or editor scale changes.
/// </summary>
public sealed class AudioV0RuntimeDemoUI : MonoBehaviour
{
    private const string RootName = "AudioV0_RuntimeDemoUI";
    private SessionController sessionController;
    private AudioV0RuntimeScenario scenario;
    private EventLogger logger;
    private RoutePlayer routePlayer;
    private IdentificationController identificationController;
    private VisualTaskController visualTaskController;
    private AudioV0RuntimeAudioDirector audioDirector;
    private AudioV0BlockRatingController blockRatingController;
    private AudioV0ExperimentSequenceController experimentSequenceController;
    private AudioV0ExperimentProfile experimentProfile;
    private AudioV0DataRepository dataRepository;
    private Font uiFont;
    private Image visualOverlay;
    private Image visualFlash;
    private Text visualFlashLabel;
    private Text statusText;
    private Text phaseText;
    private Text routeText;
    private Text metricsText;
    private Text progressText;
    private Slider progressSlider;
    private Text languageLabel;
    private Text titleLabel;
    private Text subtitleLabel;
    private Text routeHeaderLabel;
    private Text conditionHeaderLabel;
    private Text controlHeaderLabel;
    private Text responseHeaderLabel;
    private Text visualHeaderLabel;
    private Text visualHintLabel;
    private Text identificationPromptLabel;
    private GameObject identificationOverlay;
    private Text ratingPromptLabel;
    private GameObject ratingOverlay;
    private Text sequencePromptLabel;
    private Text sequenceActionLabel;
    private GameObject sequenceOverlay;
    private GameObject dataManagementRow;
    private Button visualResponseButton;
    private Text hideUiLabel;
    private Text showUiLabel;
    private Text routeGuideLabel;
    private Text openDataFolderLabel;
    private Text clearTestDataLabel;
    private GameObject operatorPanel;
    private GameObject showUiButtonObject;
    private readonly Dictionary<int, Text> routeLabels = new Dictionary<int, Text>();
    private readonly Dictionary<string, Text> conditionLabels = new Dictionary<string, Text>();
    private readonly List<Text> actionLabels = new List<Text>();
    private readonly List<Text> responseLabels = new List<Text>();
    private readonly List<Button> identificationButtons = new List<Button>();
    private readonly List<Button> ratingButtons = new List<Button>();
    private readonly List<Text> routeMetaLabels = new List<Text>();
    private bool chinese = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (GameObject.Find("Systems") == null || GameObject.Find(RootName) != null)
        {
            return;
        }

        GameObject root = new GameObject(RootName);
        DontDestroyOnLoad(root);
        root.AddComponent<AudioV0RuntimeDemoUI>();
    }

    private void Start()
    {
        BindControllers();
        BuildCanvas();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            ToggleUiVisibility();
        }

        BindControllers();
        if (sessionController == null || statusText == null)
        {
            return;
        }

        string state = sessionController.State.ToString().ToUpperInvariant();
        string phase = scenario == null ? "-" : LocalizePhase(scenario.ExperiencePhase);
        string route = scenario == null ? "-" : scenario.SelectedRouteId.Replace("route_", "").ToUpperInvariant();
        string condition = scenario == null ? "-" : scenario.selectedConditionId;
        string segment = routePlayer == null ? "-" : routePlayer.CurrentSegmentLabel.ToUpperInvariant();
        float progress = routePlayer == null ? 0f : Mathf.Clamp01(routePlayer.Progress);
        statusText.text = T("系统状态  " + state, "SYSTEM  " + state);
        phaseText.text = T("当前阶段  " + phase, "PHASE  " + phase);
        routeText.text = T("路线  " + route + "   条件  " + condition + "   航段  " + segment, "ROUTE  " + route + "   CONDITION  " + condition + "   LEG  " + segment);
        progressText.text = (routePlayer == null ? "00:00.0" : routePlayer.RouteTimeSeconds.ToString("00:00.0")) + "   /   " + (progress * 100f).ToString("F0") + "%";
        progressSlider.value = progress;
        metricsText.text = visualTaskController == null
            ? T("视觉任务等待中", "VISUAL TASK STANDBY")
            : T("视觉命中 " + visualTaskController.HitCount + "   漏报 " + visualTaskController.MissCount + "   误报 " + visualTaskController.FalseAlarmCount + "   正确拒绝 " + visualTaskController.CorrectRejectionCount,
                "HIT " + visualTaskController.HitCount + "   MISS " + visualTaskController.MissCount + "   FA " + visualTaskController.FalseAlarmCount + "   CR " + visualTaskController.CorrectRejectionCount);
        UpdateParticipantGuidance();
        UpdateIdentificationPrompt();
        UpdateVisualStimulus();
        UpdateRatingPrompt();
        UpdateSequencePrompt();
        UpdateSelectionTint();
    }

    private void BindControllers()
    {
        sessionController = sessionController == null ? FindAnyObjectByType<SessionController>() : sessionController;
        scenario = scenario == null ? FindAnyObjectByType<AudioV0RuntimeScenario>() : scenario;
        logger = logger == null ? FindAnyObjectByType<EventLogger>() : logger;
        routePlayer = routePlayer == null ? FindAnyObjectByType<RoutePlayer>() : routePlayer;
        identificationController = identificationController == null ? FindAnyObjectByType<IdentificationController>() : identificationController;
        visualTaskController = visualTaskController == null ? FindAnyObjectByType<VisualTaskController>() : visualTaskController;
        audioDirector = audioDirector == null ? FindAnyObjectByType<AudioV0RuntimeAudioDirector>() : audioDirector;
        blockRatingController = blockRatingController == null ? FindAnyObjectByType<AudioV0BlockRatingController>() : blockRatingController;
        experimentSequenceController = experimentSequenceController == null ? FindAnyObjectByType<AudioV0ExperimentSequenceController>() : experimentSequenceController;
        experimentProfile = experimentProfile == null ? FindAnyObjectByType<AudioV0ExperimentProfile>() : experimentProfile;
        dataRepository = dataRepository == null ? FindAnyObjectByType<AudioV0DataRepository>() : dataRepository;
    }

    private void BuildCanvas()
    {
        uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Segoe UI", "Arial" }, 26);
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        Canvas canvas = gameObject.AddComponent<Canvas>();
        bool useXRPresentation = experimentProfile != null && experimentProfile.enableXRPresentation && XRSettings.isDeviceActive && Camera.main != null;
        canvas.renderMode = useXRPresentation ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvas.pixelPerfect = true;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;
        if (useXRPresentation)
        {
            RectTransform xrRect = gameObject.GetComponent<RectTransform>();
            transform.SetParent(Camera.main.transform, false);
            xrRect.sizeDelta = new Vector2(1600f, 900f);
            xrRect.localScale = Vector3.one * experimentProfile.xrCanvasScale;
            xrRect.localPosition = new Vector3(0f, -0.08f, experimentProfile.xrPanelDistanceMeters);
            xrRect.localRotation = Quaternion.identity;
            canvas.worldCamera = Camera.main;
            logger?.Log("xr_presentation_ready", details: "canvas=world_space;distance_m=" + experimentProfile.xrPanelDistanceMeters.ToString("F2"));
        }
        if (useXRPresentation)
        {
            gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }
        else
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
        EnsureEventSystem();

        BuildStimulusOverlay();
        BuildIdentificationOverlay();
        BuildRatingOverlay();
        BuildSequenceOverlay();
        operatorPanel = CreatePanel(transform, "OperatorPanel", new Color(0.025f, 0.045f, 0.085f, 0.97f));
        GameObject panel = operatorPanel;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0.5f);
        panelRect.anchorMax = new Vector2(0f, 0.5f);
        panelRect.pivot = new Vector2(0f, 0.5f);
        panelRect.anchoredPosition = new Vector2(28f, 0f);
        panelRect.sizeDelta = new Vector2(760f, 844f);
        VerticalLayoutGroup panelLayout = panel.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(24, 24, 20, 18);
        panelLayout.spacing = 10;
        panelLayout.childControlWidth = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandHeight = false;

        BuildHeader(panel.transform);
        BuildStatusCard(panel.transform);
        BuildRouteCard(panel.transform);
        BuildConditionCard(panel.transform);
        BuildControlCard(panel.transform);
        BuildResponseCard(panel.transform);
        BuildVisualCard(panel.transform);
        Text footer = CreateText(panel.transform, "Footer", 12, new Color(0.48f, 0.62f, 0.74f), TextAnchor.MiddleLeft);
        footer.text = T("TECHNICAL DEMO  ·  仅用于交互与声光流程验证，不代表正式实验数据", "TECHNICAL DEMO  ·  For interaction and audiovisual flow validation only");
        AddHeight(footer.gameObject, 22f);
        showUiLabel = CreateButton(transform, "ShowUI", "SHOW UI", new Color(0.08f, 0.45f, 0.68f), ToggleUiVisibility, 38f);
        showUiButtonObject = showUiLabel.transform.parent.gameObject;
        RectTransform showRect = showUiButtonObject.GetComponent<RectTransform>();
        showRect.anchorMin = new Vector2(1f, 1f);
        showRect.anchorMax = new Vector2(1f, 1f);
        showRect.pivot = new Vector2(1f, 1f);
        showRect.anchoredPosition = new Vector2(-26f, -24f);
        showRect.sizeDelta = new Vector2(116f, 38f);
        showUiButtonObject.SetActive(false);
        ApplyLanguage();
    }

    private void BuildHeader(Transform parent)
    {
        GameObject row = CreateRow(parent, 56f);
        titleLabel = CreateText(row.transform, "Title", 22, Color.white, TextAnchor.MiddleLeft);
        titleLabel.fontStyle = FontStyle.Bold;
        titleLabel.text = "AUDIO V0  /  UAM PASSENGER LAB";
        LayoutElement titleLayout = titleLabel.gameObject.AddComponent<LayoutElement>();
        titleLayout.flexibleWidth = 1f;
        languageLabel = CreateButton(row.transform, "Language", "中文 / EN", new Color(0.12f, 0.48f, 0.72f), ToggleLanguage, 38f);
        LayoutElement languageLayout = languageLabel.transform.parent.GetComponent<LayoutElement>();
        languageLayout.preferredWidth = 112f;
        languageLayout.minWidth = 112f;
        languageLayout.preferredHeight = 38f;
        languageLayout.minHeight = 38f;
        hideUiLabel = CreateButton(row.transform, "HideUI", "HIDE UI", new Color(0.22f, 0.27f, 0.38f), ToggleUiVisibility, 38f);
        LayoutElement hideLayout = hideUiLabel.transform.parent.GetComponent<LayoutElement>();
        hideLayout.preferredWidth = 94f;
        hideLayout.minWidth = 94f;
        routeGuideLabel = CreateButton(row.transform, "RouteGuides", "ROUTE", new Color(0.18f, 0.36f, 0.48f), ToggleRouteGuides, 38f);
        LayoutElement guideLayout = routeGuideLabel.transform.parent.GetComponent<LayoutElement>();
        guideLayout.preferredWidth = 112f;
        guideLayout.minWidth = 112f;
        subtitleLabel = CreateText(parent, "Subtitle", 12, new Color(0.52f, 0.72f, 0.83f), TextAnchor.MiddleLeft);
        subtitleLabel.text = "PASSENGER JOURNEY  •  ROUTE / AUDIO / VISUAL TASK CONSOLE";
        AddHeight(subtitleLabel.gameObject, 22f);
    }

    private void BuildStatusCard(Transform parent)
    {
        GameObject card = CreateCard(parent, 128f);
        statusText = CreateText(card.transform, "Status", 19, new Color(0.45f, 0.95f, 0.83f), TextAnchor.MiddleLeft);
        phaseText = CreateText(card.transform, "Phase", 14, Color.white, TextAnchor.MiddleLeft);
        routeText = CreateText(card.transform, "Route", 13, new Color(0.74f, 0.84f, 0.91f), TextAnchor.MiddleLeft);
        GameObject progressRow = CreateRow(card.transform, 22f);
        progressSlider = progressRow.AddComponent<Slider>();
        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.interactable = false;
        progressSlider.fillRect = CreateSliderFill(progressSlider.transform);
        progressSlider.targetGraphic = progressSlider.fillRect.GetComponent<Image>();
        progressSlider.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        progressText = CreateText(progressRow.transform, "ProgressText", 12, new Color(0.63f, 0.8f, 0.9f), TextAnchor.MiddleRight);
        AddWidth(progressText.gameObject, 110f);
        metricsText = CreateText(card.transform, "Metrics", 12, new Color(0.74f, 0.84f, 0.91f), TextAnchor.MiddleLeft);
    }

    private void BuildRouteCard(Transform parent)
    {
        GameObject card = CreateCard(parent, 118f);
        routeHeaderLabel = CreateText(card.transform, "RouteHeader", 12, new Color(0.35f, 0.85f, 1f), TextAnchor.MiddleLeft);
        routeHeaderLabel.fontStyle = FontStyle.Bold;
        AddHeight(routeHeaderLabel.gameObject, 20f);
        GameObject grid = new GameObject("RouteGrid");
        grid.transform.SetParent(card.transform, false);
        GridLayoutGroup layout = grid.AddComponent<GridLayoutGroup>();
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 2;
        layout.cellSize = new Vector2(340f, 38f);
        layout.spacing = new Vector2(8f, 6f);
        layout.childAlignment = TextAnchor.MiddleCenter;
        AddHeight(grid, 82f);
        string[] routes = { "ALPINE", "COASTAL", "MOUNTAIN", "DESERT" };
        string[] routeDetails = { "S-CURVE  ·  60–70 s", "S-CURVE  ·  65–75 s", "S-CURVE  ·  65–75 s", "S-CURVE  ·  70–80 s" };
        for (int i = 0; i < routes.Length; i++)
        {
            int routeIndex = i;
            Text buttonLabel = CreateButton(grid.transform, "Route" + i, routes[i], new Color(0.07f, 0.22f, 0.38f), () => SelectRoute(routeIndex), 38f);
            routeLabels[i] = buttonLabel;
            routeMetaLabels.Add(CreateButtonMeta(buttonLabel.transform.parent, routeDetails[i]));
        }
    }

    private void BuildConditionCard(Transform parent)
    {
        GameObject card = CreateCard(parent, 86f);
        conditionHeaderLabel = CreateText(card.transform, "ConditionHeader", 12, new Color(0.35f, 0.85f, 1f), TextAnchor.MiddleLeft);
        conditionHeaderLabel.fontStyle = FontStyle.Bold;
        AddHeight(conditionHeaderLabel.gameObject, 20f);
        GameObject row = CreateRow(card.transform, 40f);
        string[] ids = { "NS_Q", "NS_N", "S_Q", "S_N" };
        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i];
            Text buttonLabel = CreateButton(row.transform, "Condition" + id, id, new Color(0.18f, 0.14f, 0.35f), () => SelectCondition(id), 0f);
            buttonLabel.transform.parent.GetComponent<LayoutElement>().flexibleWidth = 1f;
            conditionLabels[id] = buttonLabel;
        }
    }

    private void BuildControlCard(Transform parent)
    {
        GameObject card = CreateCard(parent, 78f);
        controlHeaderLabel = CreateText(card.transform, "ControlHeader", 12, new Color(0.35f, 0.85f, 1f), TextAnchor.MiddleLeft);
        controlHeaderLabel.fontStyle = FontStyle.Bold;
        AddHeight(controlHeaderLabel.gameObject, 20f);
        GameObject row = CreateRow(card.transform, 40f);
        actionLabels.Add(CreateButton(row.transform, "Start", "START", new Color(0.10f, 0.56f, 0.70f), StartBlock, 0f));
        actionLabels.Add(CreateButton(row.transform, "Pause", "PAUSE", new Color(0.66f, 0.43f, 0.14f), PauseBlock, 0f));
        actionLabels.Add(CreateButton(row.transform, "Resume", "RESUME", new Color(0.12f, 0.58f, 0.38f), ResumeBlock, 0f));
        actionLabels.Add(CreateButton(row.transform, "End", "END BLOCK", new Color(0.62f, 0.18f, 0.25f), EndBlock, 0f));
        for (int i = 0; i < actionLabels.Count; i++)
        {
            actionLabels[i].transform.parent.GetComponent<LayoutElement>().flexibleWidth = 1f;
        }
    }

    private void BuildResponseCard(Transform parent)
    {
        GameObject card = CreateCard(parent, 78f);
        responseHeaderLabel = CreateText(card.transform, "ResponseHeader", 12, new Color(0.35f, 0.85f, 1f), TextAnchor.MiddleLeft);
        responseHeaderLabel.fontStyle = FontStyle.Bold;
        AddHeight(responseHeaderLabel.gameObject, 20f);
        GameObject row = CreateRow(card.transform, 40f);
        Text cruise = CreateButton(row.transform, "Cruise", "CRUISE", new Color(0.08f, 0.39f, 0.52f), () => SubmitIdentification("cruise_entry", "ui_cruise"), 0f);
        Text descent = CreateButton(row.transform, "Descent", "DESCENT", new Color(0.08f, 0.39f, 0.52f), () => SubmitIdentification("descent_begin", "ui_descent"), 0f);
        Text landing = CreateButton(row.transform, "Landing", "LANDING", new Color(0.08f, 0.39f, 0.52f), () => SubmitIdentification("landing_preparation", "ui_landing"), 0f);
        responseLabels.Add(cruise);
        responseLabels.Add(descent);
        responseLabels.Add(landing);
        RegisterIdentificationButton(cruise);
        RegisterIdentificationButton(descent);
        RegisterIdentificationButton(landing);
        for (int i = 0; i < responseLabels.Count; i++)
        {
            responseLabels[i].transform.parent.GetComponent<LayoutElement>().flexibleWidth = 1f;
        }
    }

    private void BuildVisualCard(Transform parent)
    {
        GameObject card = CreateCard(parent, 72f);
        visualHeaderLabel = CreateText(card.transform, "VisualHeader", 12, new Color(1f, 0.72f, 0.26f), TextAnchor.MiddleLeft);
        visualHeaderLabel.fontStyle = FontStyle.Bold;
        AddHeight(visualHeaderLabel.gameObject, 20f);
        GameObject row = CreateRow(card.transform, 38f);
        visualHintLabel = CreateText(row.transform, "VisualHint", 12, new Color(0.86f, 0.88f, 0.93f), TextAnchor.MiddleLeft);
        visualHintLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        Text visualButton = CreateButton(row.transform, "VisualTarget", "TARGET", new Color(0.74f, 0.40f, 0.12f), SubmitVisualTarget, 0f);
        visualButton.transform.parent.GetComponent<LayoutElement>().preferredWidth = 170f;
        visualResponseButton = visualButton.transform.parent.GetComponent<Button>();
        visualResponseButton.interactable = false;
    }

    private void BuildStimulusOverlay()
    {
        GameObject overlayObject = new GameObject("VisualStimulusOverlay");
        overlayObject.transform.SetParent(transform, false);
        RectTransform overlayRect = overlayObject.AddComponent<RectTransform>();
        overlayRect.anchorMin = new Vector2(0.48f, 0.15f);
        overlayRect.anchorMax = new Vector2(0.98f, 0.92f);
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        visualOverlay = overlayObject.AddComponent<Image>();
        visualOverlay.color = new Color(0.02f, 0.06f, 0.12f, 0.32f);
        visualOverlay.raycastTarget = false;
        visualOverlay.gameObject.SetActive(false);

        GameObject flashObject = new GameObject("StimulusFlash");
        flashObject.transform.SetParent(overlayObject.transform, false);
        RectTransform flashRect = flashObject.AddComponent<RectTransform>();
        flashRect.anchorMin = new Vector2(0.5f, 0.52f);
        flashRect.anchorMax = new Vector2(0.5f, 0.52f);
        flashRect.sizeDelta = new Vector2(190f, 190f);
        visualFlash = flashObject.AddComponent<Image>();
        visualFlash.color = new Color(1f, 0.34f, 0.08f, 0.98f);
        visualFlash.raycastTarget = false;
        Outline outline = flashObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.32f, 0.95f);
        outline.effectDistance = new Vector2(5f, 5f);

        visualFlashLabel = CreateText(flashObject.transform, "StimulusLabel", 18, Color.white, TextAnchor.MiddleCenter);
        visualFlashLabel.text = "VISUAL\nEVENT";
        RectTransform labelRect = visualFlashLabel.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    private void UpdateVisualStimulus()
    {
        bool active = visualTaskController != null && !string.IsNullOrEmpty(visualTaskController.CurrentStimulusId);
        if (visualOverlay != null)
        {
            visualOverlay.gameObject.SetActive(active);
        }
        if (visualResponseButton != null)
        {
            visualResponseButton.interactable = active;
        }
        if (active && visualFlashLabel != null)
        {
            bool target = visualTaskController.CurrentStimulusIsTarget;
            visualFlash.color = target ? new Color(1f, 0.34f, 0.08f, 0.98f) : new Color(0.05f, 0.64f, 0.96f, 0.98f);
            visualFlash.rectTransform.localRotation = target ? Quaternion.Euler(0f, 0f, 45f) : Quaternion.identity;
            visualFlashLabel.rectTransform.localRotation = target ? Quaternion.Euler(0f, 0f, -45f) : Quaternion.identity;
            visualFlashLabel.text = target
                ? T("按下\nTARGET", "PRESS\nTARGET")
                : T("忽略", "IGNORE");
        }
    }

    private void BuildIdentificationOverlay()
    {
        identificationOverlay = CreatePanel(transform, "PassengerIdentificationOverlay", new Color(0.015f, 0.045f, 0.09f, 0.94f));
        RectTransform overlayRect = identificationOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = new Vector2(0.52f, 0.08f);
        overlayRect.anchorMax = new Vector2(0.96f, 0.31f);
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = identificationOverlay.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 14, 14);
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        identificationPromptLabel = CreateText(identificationOverlay.transform, "PassengerPrompt", 19, new Color(0.55f, 0.96f, 0.88f), TextAnchor.MiddleLeft);
        identificationPromptLabel.fontStyle = FontStyle.Bold;
        AddHeight(identificationPromptLabel.gameObject, 56f);
        GameObject row = CreateRow(identificationOverlay.transform, 48f);
        Text cruise = CreateButton(row.transform, "PassengerCruise", "CRUISE", new Color(0.07f, 0.42f, 0.57f), () => SubmitIdentification("cruise_entry", "passenger_overlay_cruise"), 0f);
        Text descent = CreateButton(row.transform, "PassengerDescent", "DESCENT", new Color(0.07f, 0.42f, 0.57f), () => SubmitIdentification("descent_begin", "passenger_overlay_descent"), 0f);
        Text landing = CreateButton(row.transform, "PassengerLanding", "LANDING", new Color(0.07f, 0.42f, 0.57f), () => SubmitIdentification("landing_preparation", "passenger_overlay_landing"), 0f);
        RegisterIdentificationButton(cruise);
        RegisterIdentificationButton(descent);
        RegisterIdentificationButton(landing);
        foreach (Transform child in row.transform)
        {
            LayoutElement element = child.GetComponent<LayoutElement>();
            if (element != null) element.flexibleWidth = 1f;
        }
        identificationOverlay.SetActive(false);
    }

    private void BuildRatingOverlay()
    {
        ratingOverlay = CreatePanel(transform, "PassengerBlockRatingOverlay", new Color(0.015f, 0.045f, 0.09f, 0.95f));
        RectTransform overlayRect = ratingOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = new Vector2(0.45f, 0.30f);
        overlayRect.anchorMax = new Vector2(0.96f, 0.55f);
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = ratingOverlay.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 14, 14);
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        ratingPromptLabel = CreateText(ratingOverlay.transform, "RatingPrompt", 18, new Color(1f, 0.78f, 0.30f), TextAnchor.MiddleLeft);
        ratingPromptLabel.fontStyle = FontStyle.Bold;
        AddHeight(ratingPromptLabel.gameObject, 68f);
        GameObject row = CreateRow(ratingOverlay.transform, 48f);
        for (int value = 1; value <= 7; value++)
        {
            int selectedValue = value;
            Text buttonLabel = CreateButton(row.transform, "Rating" + value, value.ToString(), new Color(0.28f, 0.20f, 0.52f), () => SubmitRating(selectedValue), 0f);
            Button button = buttonLabel.transform.parent.GetComponent<Button>();
            if (button != null) ratingButtons.Add(button);
            LayoutElement element = buttonLabel.transform.parent.GetComponent<LayoutElement>();
            if (element != null) element.flexibleWidth = 1f;
        }
        ratingOverlay.SetActive(false);
    }

    private void BuildSequenceOverlay()
    {
        sequenceOverlay = CreatePanel(transform, "PassengerSequenceOverlay", new Color(0.015f, 0.045f, 0.09f, 0.96f));
        RectTransform overlayRect = sequenceOverlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = new Vector2(0.43f, 0.30f);
        overlayRect.anchorMax = new Vector2(0.96f, 0.57f);
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        VerticalLayoutGroup layout = sequenceOverlay.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 18, 18);
        layout.spacing = 12;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        sequencePromptLabel = CreateText(sequenceOverlay.transform, "SequencePrompt", 20, new Color(0.55f, 0.96f, 0.88f), TextAnchor.MiddleLeft);
        sequencePromptLabel.fontStyle = FontStyle.Bold;
        AddHeight(sequencePromptLabel.gameObject, 82f);
        sequenceActionLabel = CreateButton(sequenceOverlay.transform, "SequenceAction", "NEXT BLOCK", new Color(0.10f, 0.56f, 0.70f), AdvanceSequence, 52f);
        dataManagementRow = CreateRow(sequenceOverlay.transform, 42f);
        openDataFolderLabel = CreateButton(dataManagementRow.transform, "OpenDataFolder", "OPEN DATA FOLDER", new Color(0.16f, 0.38f, 0.55f), OpenDataFolder, 0f);
        clearTestDataLabel = CreateButton(dataManagementRow.transform, "ClearTestData", "CLEAR TEST DATA", new Color(0.58f, 0.22f, 0.18f), ClearTechnicalData, 0f);
        foreach (Transform child in dataManagementRow.transform)
        {
            LayoutElement element = child.GetComponent<LayoutElement>();
            if (element != null) element.flexibleWidth = 1f;
        }
        dataManagementRow.SetActive(false);
        sequenceOverlay.SetActive(false);
    }

    private void RegisterIdentificationButton(Text label)
    {
        Button button = label == null ? null : label.transform.parent.GetComponent<Button>();
        if (button != null)
        {
            identificationButtons.Add(button);
        }
    }

    private void UpdateIdentificationPrompt()
    {
        bool active = identificationController != null && identificationController.HasPendingResponse;
        if (identificationOverlay != null && identificationOverlay.activeSelf != active)
        {
            identificationOverlay.SetActive(active);
        }
        foreach (Button button in identificationButtons)
        {
            if (button != null) button.interactable = active;
        }
        if (identificationPromptLabel != null && active)
        {
            identificationPromptLabel.text = T("听到提示音：下一飞行阶段是什么？请在窗口结束前选择", "AUDIO NOTICE: What flight phase comes next? Choose before the window closes.");
        }
    }

    private void UpdateRatingPrompt()
    {
        bool active = blockRatingController != null && blockRatingController.IsRatingOpen;
        if (ratingOverlay != null && ratingOverlay.activeSelf != active)
        {
            ratingOverlay.SetActive(active);
        }
        foreach (Button button in ratingButtons)
        {
            if (button != null) button.interactable = active;
        }
        if (ratingPromptLabel != null && active)
        {
            ratingPromptLabel.text = T("区块回顾 " + (blockRatingController.CurrentQuestionIndex + 1) + "/" + blockRatingController.QuestionCount + "\n" + blockRatingController.GetPrompt(true),
                "BLOCK REVIEW " + (blockRatingController.CurrentQuestionIndex + 1) + "/" + blockRatingController.QuestionCount + "\n" + blockRatingController.GetPrompt(false));
        }
    }

    private void UpdateSequencePrompt()
    {
        bool nextReady = experimentSequenceController != null && experimentSequenceController.IsAwaitingNextBlock;
        bool finale = experimentSequenceController != null && experimentSequenceController.IsFinaleVisible;
        bool active = nextReady || finale;
        if (sequenceOverlay != null && sequenceOverlay.activeSelf != active)
        {
            sequenceOverlay.SetActive(active);
        }
        if (!active || sequencePromptLabel == null || sequenceActionLabel == null)
        {
            return;
        }

        if (finale)
        {
            sequencePromptLabel.text = T("旅程完成\n感谢参与。原始数据已安全写入本机；点击下方按钮复制 JSONL 日志路径。", "JOURNEY COMPLETE\nThank you. Raw data is saved locally; use the button below to copy the JSONL path.");
            sequenceActionLabel.text = T("复制数据路径", "COPY DATA PATH");
            dataManagementRow.SetActive(true);
            openDataFolderLabel.text = T("打开数据文件夹", "OPEN DATA FOLDER");
            clearTestDataLabel.text = T("删除技术测试数据", "CLEAR TEST DATA");
        }
        else
        {
            string nextRoute = RouteDisplayName(experimentSequenceController.NextRouteIndex);
            sequencePromptLabel.text = T("第 " + (experimentSequenceController.CurrentBlockIndex + 1) + "/" + experimentSequenceController.TotalBlocks + " 段已完成\n稍作休息后，进入下一段：" + experimentSequenceController.NextConditionId + " · " + nextRoute,
                "BLOCK " + (experimentSequenceController.CurrentBlockIndex + 1) + "/" + experimentSequenceController.TotalBlocks + " COMPLETE\nTake a short pause, then continue: " + experimentSequenceController.NextConditionId + " · " + nextRoute);
            sequenceActionLabel.text = T("开始下一段", "START NEXT BLOCK");
            dataManagementRow.SetActive(false);
        }
    }

    private void UpdateParticipantGuidance()
    {
        if (subtitleLabel != null)
        {
            bool running = sessionController != null && sessionController.State == SessionController.Lifecycle.Running;
            if (experimentSequenceController != null && experimentSequenceController.IsFinaleVisible)
            {
                subtitleLabel.text = T("全部 4 段已完成  ·  感谢参与  ·  请复制并保存日志路径", "ALL 4 BLOCKS COMPLETE  ·  THANK YOU  ·  COPY AND SAVE THE LOG PATH");
            }
            else if (experimentSequenceController != null && experimentSequenceController.IsAwaitingNextBlock)
            {
                subtitleLabel.text = T("本段评分已完成  ·  点击右侧卡片开始下一条件", "BLOCK RATING COMPLETE  ·  USE THE RIGHT CARD TO START THE NEXT CONDITION");
            }
            else
            {
                subtitleLabel.text = running
                    ? T("答题卡会保留 8 秒  ·  橙色菱形按 TARGET，蓝色方块忽略", "ANSWER CARD STAYS OPEN FOR 8 S  ·  ORANGE DIAMOND = TARGET, BLUE SQUARE = IGNORE")
                    : T("第 1 步选路线与条件  ·  第 2 步开始 4 段连续实验  ·  F1 隐藏/显示操作面板", "1 SELECT ROUTE + CONDITION  ·  2 START THE 4-BLOCK JOURNEY  ·  F1 HIDES/SHOWS OPERATOR PANEL");
            }
        }
        if (conditionHeaderLabel != null)
        {
            conditionHeaderLabel.text = T("实验条件预览  ·  " + DescribeCondition(true), "CONDITION PREVIEW  ·  " + DescribeCondition(false));
        }
        if (responseHeaderLabel != null)
        {
            responseHeaderLabel.text = identificationController != null && identificationController.HasPendingResponse
                ? T("阶段识别响应  ·  现在作答", "PHASE IDENTIFICATION  ·  ANSWER NOW")
                : T("阶段识别响应  ·  等待音频提示", "PHASE IDENTIFICATION  ·  WAIT FOR AUDIO NOTICE");
        }
    }

    private string DescribeCondition(bool zh)
    {
        string id = scenario == null ? "NS_Q" : scenario.selectedConditionId;
        switch (id)
        {
            case "NS_N": return zh ? "非语音提示 + 模拟客舱噪声" : "NON-SPEECH CUE + SIMULATED CABIN NOISE";
            case "S_Q": return zh ? "语音提示 + 安静客舱" : "SPOKEN CUE + QUIET CABIN";
            case "S_N": return zh ? "语音提示 + 模拟客舱噪声" : "SPOKEN CUE + SIMULATED CABIN NOISE";
            default: return zh ? "非语音提示 + 安静客舱" : "NON-SPEECH CUE + QUIET CABIN";
        }
    }

    private void UpdateSelectionTint()
    {
        for (int i = 0; i < 4; i++)
        {
            if (routeLabels.ContainsKey(i))
            {
                Image image = routeLabels[i].transform.parent.GetComponent<Image>();
                if (image != null)
                {
                    image.color = scenario != null && scenario.selectedRouteIndex == i ? new Color(0.08f, 0.55f, 0.62f) : new Color(0.07f, 0.22f, 0.38f);
                }
            }
        }
        foreach (KeyValuePair<string, Text> pair in conditionLabels)
        {
            Image image = pair.Value.transform.parent.GetComponent<Image>();
            if (image != null)
            {
                image.color = scenario != null && scenario.selectedConditionId == pair.Key ? new Color(0.38f, 0.23f, 0.63f) : new Color(0.18f, 0.14f, 0.35f);
            }
        }
    }

    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        Image image = obj.AddComponent<Image>();
        image.color = color;
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0.16f, 0.42f, 0.58f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);
        return obj;
    }

    private GameObject CreateCard(Transform parent, float height)
    {
        GameObject card = CreatePanel(parent, "Card", new Color(0.05f, 0.095f, 0.16f, 0.96f));
        AddHeight(card, height);
        VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 8, 8);
        layout.spacing = 3;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return card;
    }

    private GameObject CreateRow(Transform parent, float height)
    {
        GameObject row = new GameObject("Row");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>();
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        AddHeight(row, height);
        return row;
    }

    private Text CreateText(Transform parent, string name, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        Text text = obj.AddComponent<Text>();
        text.font = uiFont;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private Text CreateButtonMeta(Transform parent, string value)
    {
        Text text = CreateText(parent, "Meta", 10, new Color(0.67f, 0.82f, 0.9f), TextAnchor.LowerRight);
        text.text = value;
        text.raycastTarget = false;
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.offsetMin = new Vector2(8f, 2f);
        rect.offsetMax = new Vector2(-8f, 0f);
        return text;
    }

    private Text CreateButton(Transform parent, string name, string label, Color tint, UnityEngine.Events.UnityAction action, float preferredHeight)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        Image image = obj.AddComponent<Image>();
        image.color = tint;
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0.25f, 0.7f, 0.9f, 0.55f);
        outline.effectDistance = new Vector2(1f, -1f);
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        colors.pressedColor = new Color(0.75f, 0.85f, 0.95f, 1f);
        colors.selectedColor = Color.white;
        button.colors = colors;
        button.onClick.AddListener(action);
        LayoutElement element = obj.AddComponent<LayoutElement>();
        if (preferredHeight > 0f)
        {
            element.preferredHeight = preferredHeight;
        }
        Text text = CreateText(obj.transform, "Label", 13, Color.white, TextAnchor.MiddleCenter);
        text.text = label;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(6f, 3f);
        textRect.offsetMax = new Vector2(-6f, -3f);
        return text;
    }

    private RectTransform CreateSliderFill(Transform sliderTransform)
    {
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(sliderTransform, false);
        RectTransform rect = fill.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = fill.AddComponent<Image>();
        image.color = new Color(0.14f, 0.78f, 0.86f, 0.9f);
        return rect;
    }

    private void AddHeight(GameObject obj, float height)
    {
        LayoutElement element = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
        element.preferredHeight = height;
        element.minHeight = height;
    }

    private void AddWidth(GameObject obj, float width)
    {
        LayoutElement element = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
        element.preferredWidth = width;
        element.minWidth = width;
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }
        GameObject eventSystemObject = new GameObject("AudioV0_RuntimeEventSystem");
        DontDestroyOnLoad(eventSystemObject);
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private void StartBlock()
    {
        audioDirector?.PlayUiConfirm();
        experimentSequenceController = experimentSequenceController == null ? FindAnyObjectByType<AudioV0ExperimentSequenceController>() : experimentSequenceController;
        bool result = experimentSequenceController != null
            ? experimentSequenceController.BeginSequence()
            : scenario != null && scenario.StartSelectedBlock();
        if (!result) audioDirector?.PlayUiError();
    }

    private void AdvanceSequence()
    {
        audioDirector?.PlayUiConfirm();
        if (experimentSequenceController == null)
        {
            audioDirector?.PlayUiError();
            return;
        }

        bool result = experimentSequenceController.IsFinaleVisible
            ? experimentSequenceController.CopyLogPath()
            : experimentSequenceController.StartNextBlock();
        if (!result) audioDirector?.PlayUiError();
    }

    private void PauseBlock()
    {
        audioDirector?.PlayUiClick();
        bool result = sessionController != null && sessionController.PauseBlock();
        if (!result) audioDirector?.PlayUiError();
    }

    private void ResumeBlock()
    {
        audioDirector?.PlayUiConfirm();
        bool result = sessionController != null && sessionController.ResumeBlock();
        if (!result) audioDirector?.PlayUiError();
    }

    private void EndBlock()
    {
        audioDirector?.PlayUiClick();
        bool result = sessionController != null && sessionController.EndBlock("operator_ui_end");
        if (!result) audioDirector?.PlayUiError();
    }

    private void SelectRoute(int routeIndex)
    {
        audioDirector?.PlayUiClick();
        bool result = scenario != null && scenario.SelectRoute(routeIndex);
        if (!result) audioDirector?.PlayUiError();
    }

    private void SelectCondition(string conditionId)
    {
        audioDirector?.PlayUiClick();
        bool result = scenario != null && scenario.SelectCondition(conditionId);
        if (!result) audioDirector?.PlayUiError();
    }

    private void SubmitIdentification(string targetState, string controlId)
    {
        audioDirector?.PlayUiClick();
        identificationController = identificationController == null ? FindAnyObjectByType<IdentificationController>() : identificationController;
        bool result = identificationController != null && identificationController.SubmitResponse(targetState, controlId);
        if (!result) audioDirector?.PlayUiError();
    }

    private void SubmitVisualTarget()
    {
        audioDirector?.PlayUiClick();
        visualTaskController = visualTaskController == null ? FindAnyObjectByType<VisualTaskController>() : visualTaskController;
        bool result = visualTaskController != null && visualTaskController.RegisterResponse(true, "ui_visual_target");
        if (!result) audioDirector?.PlayUiError();
    }

    private void SubmitRating(int value)
    {
        audioDirector?.PlayUiClick();
        blockRatingController = blockRatingController == null ? FindAnyObjectByType<AudioV0BlockRatingController>() : blockRatingController;
        bool result = blockRatingController != null && blockRatingController.SubmitRating(value, "ui_block_rating_" + value);
        if (!result) audioDirector?.PlayUiError();
    }

    private void ToggleLanguage()
    {
        chinese = !chinese;
        audioDirector?.PlayUiClick();
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        if (languageLabel == null)
        {
            return;
        }
        languageLabel.text = chinese ? "中文 / EN" : "EN / 中文";
        if (hideUiLabel != null) hideUiLabel.text = T("隐藏面板", "HIDE UI");
        if (showUiLabel != null) showUiLabel.text = T("显示面板", "SHOW UI");
        RefreshRouteGuideLabel();
        subtitleLabel.text = T("乘客旅程  ·  路线 / 声音 / 视觉任务控制台", "PASSENGER JOURNEY  ·  ROUTE / AUDIO / VISUAL TASK CONSOLE");
        routeHeaderLabel.text = T("路线选择  ·  运行中可切换", "ROUTE SELECTION  ·  SWITCHABLE IN FLIGHT");
        conditionHeaderLabel.text = T("实验条件预览  ·  " + DescribeCondition(true), "CONDITION PREVIEW  ·  " + DescribeCondition(false));
        controlHeaderLabel.text = T("飞行控制", "FLIGHT CONTROL");
        responseHeaderLabel.text = T("阶段识别响应", "PHASE RESPONSE");
        visualHeaderLabel.text = T("视觉事件  ·  橙色菱形按 TARGET，蓝色方块忽略", "VISUAL EVENT  ·  ORANGE DIAMOND = TARGET, BLUE SQUARE = IGNORE");
        visualHintLabel.text = T("仅在图形出现时可点击 TARGET；答题卡只在音频提示后出现", "TARGET IS ENABLED ONLY WHILE A SHAPE IS VISIBLE; THE ANSWER CARD APPEARS ONLY AFTER AUDIO");
        for (int i = 0; i < actionLabels.Count; i++)
        {
            string[] zh = { "开始 4 段实验", "暂停", "继续", "结束区块" };
            string[] en = { "START 4-BLOCK JOURNEY", "PAUSE", "RESUME", "END BLOCK" };
            actionLabels[i].text = T(zh[i], en[i]);
        }
        string[] responseZh = { "巡航", "下降", "着陆准备" };
        string[] responseEn = { "CRUISE", "DESCENT", "LANDING" };
        for (int i = 0; i < responseLabels.Count; i++) responseLabels[i].text = T(responseZh[i], responseEn[i]);
        footerLanguageRefresh();
    }

    private void ToggleUiVisibility()
    {
        bool shouldShow = operatorPanel != null && !operatorPanel.activeSelf;
        if (operatorPanel != null)
        {
            operatorPanel.SetActive(shouldShow);
        }
        if (showUiButtonObject != null)
        {
            showUiButtonObject.SetActive(!shouldShow);
        }
        audioDirector?.PlayUiClick();
    }

    private void ToggleRouteGuides()
    {
        audioDirector?.PlayUiClick();
        bool result = scenario != null && scenario.ToggleRouteGuides();
        if (!result) audioDirector?.PlayUiError();
        RefreshRouteGuideLabel();
    }

    private void RefreshRouteGuideLabel()
    {
        if (routeGuideLabel == null) return;
        bool visible = scenario != null && scenario.RouteGuidesVisible;
        routeGuideLabel.text = visible ? T("隐藏路线", "HIDE ROUTE") : T("显示路线", "SHOW ROUTE");
    }

    private void OpenDataFolder()
    {
        audioDirector?.PlayUiClick();
        bool result = dataRepository != null && dataRepository.OpenDataFolder();
        if (!result) audioDirector?.PlayUiError();
    }

    private void ClearTechnicalData()
    {
        audioDirector?.PlayUiClick();
        bool result = dataRepository != null && dataRepository.DeleteTechnicalDemoData();
        if (!result) audioDirector?.PlayUiError();
        else if (clearTestDataLabel != null) clearTestDataLabel.text = T("技术测试数据已删除", "TEST DATA CLEARED");
    }

    private void footerLanguageRefresh()
    {
        if (visualFlashLabel != null && visualTaskController != null && !string.IsNullOrEmpty(visualTaskController.CurrentStimulusId))
        {
            visualFlashLabel.text = visualTaskController.CurrentStimulusIsTarget
                ? T("按下\nTARGET", "PRESS\nTARGET")
                : T("忽略", "IGNORE");
        }
    }

    private string T(string zh, string en)
    {
        return chinese ? zh : en;
    }

    private string LocalizePhase(string value)
    {
        if (!chinese) return value.Replace('_', ' ').ToUpperInvariant();
        switch (value)
        {
            case "boarding_safety_briefing": return "登机与安全说明";
            case "takeoff_climb": return "起飞爬升";
            case "cruise_city_view": return "城市巡航";
            case "enroute_turn_cruise": return "航路转弯巡航";
            case "descent_approach": return "下降进近";
            case "landing_preparation": return "着陆准备";
            case "paused_cabin_briefing": return "暂停 · 客舱状态";
            case "arrived_postflight_review": return "抵达 · 旅程回顾";
            default: return value;
        }
    }

    private string RouteDisplayName(int routeIndex)
    {
        string[] names = chinese
            ? new[] { "高山路线", "海岸路线", "山谷路线", "沙漠路线" }
            : new[] { "ALPINE", "COASTAL", "MOUNTAIN", "DESERT" };
        return routeIndex >= 0 && routeIndex < names.Length ? names[routeIndex] : "-";
    }
}
