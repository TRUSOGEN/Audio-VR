# Audio Experiment Unity/VR V0 完整搭建手册

日期：2026-09-09  
项目：UAM  
当前 Editor：Unity 6000.6.0f1  
目标：从空白 Unity 项目搭建到可执行、可记录、可 dry run 的 passenger VR prototype

## 0. 先明确最终要搭什么

你要搭建的是一个 fixed-base seated VR passenger simulator：

- 参与者坐在普通椅子上，不驾驶 eVTOL。
- XR Origin 提供乘客视角。
- PassengerCabin 携带座椅、舱壁和 camera 沿固定路线自动移动。
- RoutePlayer 只播放预定义路线，不实现空气动力学或飞行控制。
- FlightStateMachine 根据路线进度触发三个可重复的 flight-phase transitions。
- NotificationAudio 播放 speech 或 non-speech notification。
- EventLogger 把路线、transition、音频和输入写成可追踪日志。
- 后续再加入 visual task、四种 condition 和 pilot。

完成标准不是画面像真实飞机，而是系统可以重复运行同一条路线，并让另一名研究者从日志中核对三个 transition。

本手册中的路线速度、位置、音频、response window、condition 顺序和样本量都是 prototype 或 pilot-dependent，不是正式研究参数。

## 1. 当前已完成状态

根据你目前的操作：

- 项目名为 UAM。
- Unity Editor 为 6000.6.0f1。
- 项目使用 3D (URP)。
- XR Plug-in Management、OpenXR Plugin 和 XR Interaction Toolkit 已进入项目。
- XR Plug-in Management 页面可以看到 OpenXR 和 Mock HMD。
- 场景中已经有 XR Origin (VR)。
- 顶层重复 Main Camera 已删除。
- Directional Light 和 Global Volume 保留。
- PassengerCabin、CabinFloor、SeatBase 和 SeatBack 正在建立或已建立。

如果某一项和当前 Unity 界面不一致，以实际 Console、Hierarchy 和 Inspector 为准。

## 2. 建议的项目文件夹

在 Project 窗口的 Assets 下创建：

~~~text
Assets/
  Scenes/
  Scripts/
  Prefabs/
  Materials/
  Audio/
    Technical/
    Speech/
    NonSpeech/
    Noise/
  UI/
  Data/
  Logs/
~~~

把当前场景保存为：

~~~text
Assets/Scenes/AudioV0.unity
~~~

不要继续使用 SampleScene 作为项目主场景。

## 3. 最终 Hierarchy

先达到以下结构，后续再加组件：

~~~text
AudioV0
  Directional Light
  Global Volume
  PassengerCabin
    CabinFloor
    SeatBase
    SeatBack
    WindowLeft
    WindowRight
    XR Origin (VR)
      Camera Offset
        Main Camera
  RouteEnvironment
    RouteWaypoints
      W0_Start
      W1_Cruise
      W2_Descent
      W3_Landing
  Systems
    EventLogger
    RoutePlayer
    FlightStateMachine
    NotificationAudio
  UI
    VisualTaskCanvas
~~~

PassengerCabin 是移动的根对象。XR Origin、座椅和舱体几何都放在 PassengerCabin 下面。

RouteEnvironment 不要放在 PassengerCabin 下面，因为它应该保持静止，作为外部参考环境。

Systems 也不要放在 PassengerCabin 下面，因为日志和控制器不应该随着乘客舱移动。

## 4. 搭建最小 passenger cabin

### 4.1 地板

在 PassengerCabin 下创建 Cube：

~~~text
Name: CabinFloor
Local Position: (0, 0, 0)
Local Rotation: (0, 0, 0)
Local Scale: (4, 0.1, 4)
~~~

### 4.2 座椅底座

在 PassengerCabin 下创建 Cube：

~~~text
Name: SeatBase
Local Position: (0, 0.3, -0.3)
Local Rotation: (0, 0, 0)
Local Scale: (1.2, 0.5, 1.2)
~~~

### 4.3 座椅靠背

在 PassengerCabin 下创建 Cube：

~~~text
Name: SeatBack
Local Position: (0, 0.9, -0.8)
Local Rotation: (0, 0, 0)
Local Scale: (1.2, 1.2, 0.2)
~~~

### 4.4 舱壁和窗口

先创建两个低复杂度侧壁：

~~~text
WindowLeft
Local Position: (-2, 1.3, 1)
Local Scale: (0.1, 2, 3)

WindowRight
Local Position: (2, 1.3, 1)
Local Scale: (0.1, 2, 3)
~~~

如果侧壁完全遮挡视野，把它们删除，改成只保留窗口边框。V0 的重点是 passenger view、路线和声音，不是客舱外观。

### 4.5 XR Origin 的 seated position

选中 XR Origin (VR)，保持 Rotation 为零。先把 Camera Offset 的 local Position 设置为：

~~~text
X: 0
Y: 1.5
Z: 0
~~~

这个高度只是 seated prototype 候选值。正式高度要根据目标 headset、座椅和 pilot 决定。

点击 Play，确认 camera 不在地板下面，座椅不会完全挡住前方视野。

## 5. 建立最小外部环境

现在不要导入 Cesium，也不要下载 eVTOL 模型。先用 primitive 验证移动路线。

### 5.1 创建静止环境

创建 Empty，命名为 RouteEnvironment。

在 RouteEnvironment 下创建 Plane：

~~~text
Name: Ground
Position: (0, -0.05, 20)
Scale: (10, 1, 10)
~~~

如果 Plane 太大或遮挡视野，先删除。它只是帮助判断移动，不是最终场景。

### 5.2 创建路线点

创建 Empty，命名为 RouteWaypoints。把它放到 RouteEnvironment 下。

在 RouteWaypoints 下创建四个 Empty：

~~~text
W0_Start    Position: (0, 0, 0)
W1_Cruise   Position: (0, 2, 10)
W2_Descent  Position: (0, 5, 25)
W3_Landing  Position: (0, 1, 40)
~~~

这些点的数值是 prototype candidate。路线方向先固定为正 Z，避免同时加入转弯、摇摆和旋转。

## 6. 创建 Systems 对象

创建 Empty，命名为 Systems。

先把下面四个 C# 文件放进 Assets/Scripts：

- EventLogger.cs
- RoutePlayer.cs
- FlightStateMachine.cs
- NotificationAudio.cs

然后把四个脚本分别拖到 Systems 上。一个 GameObject 可以同时挂载多个 MonoBehaviour。

## 7. EventLogger.cs

这个脚本先写 JSON Lines。每行是一条事件，便于后续检查事件顺序。

~~~csharp
using System;
using System.IO;
using UnityEngine;

public sealed class EventLogger : MonoBehaviour
{
    [Serializable]
    private sealed class EventRecord
    {
        public string session_id;
        public string participant_id;
        public string block_id;
        public string route_id;
        public string condition_id;
        public string transition_id;
        public string event_type;
        public string details;
        public int event_sequence;
        public long timestamp_ms;
        public double monotonic_time_s;
        public string application_version;
        public string write_status;
    }

    public string sessionId = "";
    public string participantId = "operator_test";
    public string blockId = "block_01";
    public string routeId = "route_v0";
    public string conditionId = "TECH_TEST";
    public string applicationVersion = "prototype_v0";
    public string stimulusSetVersion = "stimuli_provisional_v0";

    public string LogPath { get; private set; }

    private StreamWriter writer;
    private int eventSequence;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            sessionId = "session_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        }

        string directory = Path.Combine(
            Application.persistentDataPath,
            "AudioV0",
            sessionId
        );

        Directory.CreateDirectory(directory);
        LogPath = Path.Combine(directory, "event_log.jsonl");
        writer = new StreamWriter(LogPath, append: false);
        writer.AutoFlush = true;

        Log("session_start", details: "logger_ready");
    }

    public void Log(
        string eventType,
        string transitionId = "",
        string details = ""
    )
    {
        if (writer == null)
        {
            Debug.LogError("EventLogger is not ready.");
            return;
        }

        EventRecord record = new EventRecord
        {
            session_id = sessionId,
            participant_id = participantId,
            block_id = blockId,
            route_id = routeId,
            condition_id = conditionId,
            transition_id = transitionId,
            event_type = eventType,
            details = details,
            event_sequence = eventSequence,
            timestamp_ms = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond,
            monotonic_time_s = Time.realtimeSinceStartupAsDouble,
            application_version = applicationVersion,
            write_status = "written"
        };

        writer.WriteLine(JsonUtility.ToJson(record));
        eventSequence++;
    }

    private void OnDestroy()
    {
        if (writer == null)
        {
            return;
        }

        Log("session_end", details: "logger_destroyed");
        writer.Flush();
        writer.Close();
        writer = null;
    }
}
~~~

把 EventLogger 的 LogPath 记下来。上述代码为历史搭建示例；2026-09-21 当前工程在 macOS 使用 `~/AudioV0_Data/`，其他平台使用 `Application.persistentDataPath/AudioV0/`，以 `EventLogger.GetDataRootPath()` 为准。

先不要把真实参与者姓名或可识别信息写进日志。participant_id 只使用伪名。

## 8. RoutePlayer.cs

这个脚本让 PassengerCabin 沿 W0 到 W3 移动。它使用路线进度，不使用独立的未记录 timer。

~~~csharp
using UnityEngine;

public sealed class RoutePlayer : MonoBehaviour
{
    public Transform movingRoot;
    public Transform[] waypoints;
    public float speedMetersPerSecond = 1.5f;
    public bool playOnStart = false;
    public EventLogger logger;

    public bool IsPlaying { get; private set; }
    public bool IsComplete { get; private set; }
    public float Progress { get; private set; }

    private int currentSegment;

    private void Start()
    {
        if (playOnStart)
        {
            StartRoute();
        }
    }

    private void Update()
    {
        if (!IsPlaying || movingRoot == null || waypoints == null)
        {
            return;
        }

        if (waypoints.Length < 2)
        {
            StopWithError("route_requires_two_waypoints");
            return;
        }

        Transform target = waypoints[currentSegment + 1];
        float step = speedMetersPerSecond * Time.deltaTime;

        movingRoot.position = Vector3.MoveTowards(
            movingRoot.position,
            target.position,
            step
        );

        float totalSegments = waypoints.Length - 1;
        Progress = (currentSegment + 1f) / totalSegments;

        if (Vector3.Distance(movingRoot.position, target.position) < 0.01f)
        {
            currentSegment++;

            if (currentSegment >= waypoints.Length - 1)
            {
                IsPlaying = false;
                IsComplete = true;
                Progress = 1f;
                logger?.Log("route_complete", details: "route_v0_complete");
            }
        }
    }

    public void StartRoute()
    {
        if (movingRoot == null || waypoints == null || waypoints.Length < 2)
        {
            StopWithError("route_not_configured");
            return;
        }

        currentSegment = 0;
        Progress = 0f;
        IsComplete = false;
        IsPlaying = true;
        movingRoot.position = waypoints[0].position;

        logger?.Log("route_start", details: "route_v0_started");
    }

    public void PauseRoute()
    {
        if (!IsPlaying)
        {
            return;
        }

        IsPlaying = false;
        logger?.Log("pause", details: "route_paused");
    }

    public void ResumeRoute()
    {
        if (IsComplete)
        {
            return;
        }

        IsPlaying = true;
        logger?.Log("resume", details: "route_resumed");
    }

    private void StopWithError(string errorCode)
    {
        IsPlaying = false;
        logger?.Log("error", details: errorCode);
        Debug.LogError(errorCode);
    }
}
~~~

## 9. FlightStateMachine.cs

三个状态用 route progress 触发。每个 transition 在一个 block 内只能触发一次。

~~~csharp
using System;
using UnityEngine;

public sealed class FlightStateMachine : MonoBehaviour
{
    [Serializable]
    public sealed class TransitionRule
    {
        public string transitionId;
        public string targetState;
        [Range(0f, 1f)]
        public float progressThreshold;
    }

    public RoutePlayer routePlayer;
    public EventLogger logger;
    public NotificationAudio notificationAudio;
    public TransitionRule[] rules =
    {
        new TransitionRule
        {
            transitionId = "T01",
            targetState = "cruise_entry",
            progressThreshold = 0.25f
        },
        new TransitionRule
        {
            transitionId = "T02",
            targetState = "descent_begin",
            progressThreshold = 0.55f
        },
        new TransitionRule
        {
            transitionId = "T03",
            targetState = "landing_preparation",
            progressThreshold = 0.85f
        }
    };

    private int nextRule;

    private void Update()
    {
        if (routePlayer == null || rules == null)
        {
            return;
        }

        while (
            nextRule < rules.Length &&
            routePlayer.Progress >= rules[nextRule].progressThreshold
        )
        {
            TriggerTransition(rules[nextRule]);
            nextRule++;
        }
    }

    public void ResetBlock()
    {
        nextRule = 0;
        logger?.Log("block_reset", details: "transition_rules_reset");
    }

    private void TriggerTransition(TransitionRule rule)
    {
        logger?.Log(
            "transition_onset",
            transitionId: rule.transitionId,
            details: "target_state=" + rule.targetState
        );

        notificationAudio?.PlayForTransition(
            rule.transitionId,
            rule.targetState
        );
    }
}
~~~

## 10. NotificationAudio.cs

第一轮只测试音频请求、播放和日志。不要把技术测试 clip 当作正式 participant-facing stimulus。

~~~csharp
using System.Collections;
using UnityEngine;

public sealed class NotificationAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip technicalClip;
    public EventLogger logger;

    public void PlayForTransition(
        string transitionId,
        string targetState
    )
    {
        if (audioSource == null || technicalClip == null)
        {
            logger?.Log(
                "error",
                transitionId,
                "audio_missing_asset_or_source"
            );
            return;
        }

        logger?.Log(
            "notification_requested",
            transitionId,
            "target_state=" + targetState
        );

        audioSource.PlayOneShot(technicalClip);

        logger?.Log(
            "notification_onset",
            transitionId,
            "clip=" + technicalClip.name
        );

        StartCoroutine(LogOffset(
            technicalClip.length,
            transitionId
        ));
    }

    private IEnumerator LogOffset(
        float duration,
        string transitionId
    )
    {
        yield return new WaitForSeconds(duration);

        logger?.Log(
            "notification_offset",
            transitionId,
            "technical_clip_finished"
        );
    }
}
~~~

AudioSource 的 Spatial Blend 先设为 0，作为技术播放测试。是否使用空间化、哪种耳机、音量和校准方式要等后续 protocol gate 和 pilot 决定。

## 11. Unity Inspector 连线

在 Systems 对象上设置：

### EventLogger

- sessionId：留空，让脚本自动生成。
- participantId：operator_test。
- blockId：block_01。
- routeId：route_v0。
- conditionId：TECH_TEST。
- applicationVersion：prototype_v0。

### RoutePlayer

- movingRoot：拖入 PassengerCabin。
- waypoints：Size 设为 4，依次拖入 W0_Start、W1_Cruise、W2_Descent、W3_Landing。
- speedMetersPerSecond：先用 1.5。
- playOnStart：第一轮设为 true。
- logger：拖入 Systems 上的 EventLogger。

### FlightStateMachine

- routePlayer：拖入 RoutePlayer。
- logger：拖入 EventLogger。
- notificationAudio：拖入 NotificationAudio。
- 检查三个 rules 的 transitionId、targetState 和 threshold。

### NotificationAudio

- audioSource：创建或拖入 AudioSource。
- technicalClip：先放一个短的测试音。
- logger：拖入 EventLogger。

## 12. 第一轮 silent route

在加入 AudioSource 之前，先完成无声音的路线测试。

1. 暂时把 FlightStateMachine 的 notificationAudio 留空。
2. 点击 Play。
3. 观察 PassengerCabin 是否沿 W0 到 W3 移动。
4. 检查 Console 是否出现 route_start、三个 transition_onset 和 route_complete。
5. Stop 后找到 event_log.jsonl。
6. 确认事件顺序和 event_sequence 连续。
7. 重新 Play 一次，确认三个 transition 不会重复触发。

silent route 的通过条件：

- route_start 只出现一次。
- transition_onset 恰好出现三次。
- transition_id 为 T01、T02、T03。
- target state 顺序为 cruise_entry、descent_begin、landing_preparation。
- route_complete 出现一次。
- 没有 error。
- 第二次运行生成新的 session_id，而不是覆盖旧日志。

如果路线运行时 camera 旋转或身体不适，先固定 PassengerCabin 的 rotation，不加入摇摆、俯仰和滚转。

## 13. 第二轮 technical audio

silent route 通过后再加入 NotificationAudio。

先只播放一个 technical clip，检查：

- requested start 是否写入日志。
- notification_onset 是否写入日志。
- notification_offset 是否写入日志。
- 缺少 AudioClip 时是否写入 error。
- 路线完成前音频是否会阻塞状态机。

完成后再准备 V0 candidate assets：

~~~text
Audio/
  Technical/
    technical_test_v0.wav
  Speech/
    speech_T01_candidate.wav
    speech_T02_candidate.wav
    speech_T03_candidate.wav
  NonSpeech/
    nonspeech_T01_candidate.wav
    nonspeech_T02_candidate.wav
    nonspeech_T03_candidate.wav
  Noise/
    quiet_v0.wav
    cabin_noise_candidate.wav
~~~

speech wording、non-speech cue、noise mix、SPL、SNR 和 duration 都保持 candidate 或 pilot-dependent。

## 14. 四种 condition

当前 V0 条件表先使用：

| condition_id | notification_design | listening_condition |
|---|---|---|
| NS_Q | non-speech | quiet |
| NS_N | non-speech | simulated_cabin_noise |
| S_Q | speech | quiet |
| S_N | speech | simulated_cabin_noise |

每个 block 开始前记录：

- condition_id
- route_id
- block_position
- stimulus_set_version
- noise_version
- control_binding_version
- application_version

正式条件顺序不要在 prototype 里随意固定。先用 operator dry run 检查四个 condition 各出现一次，再根据正式 counterbalancing 方案生成 allocation table。

## 15. 加入 visual task

静默路线和技术音频都通过后，才加入 visual task。

推荐第一版使用不遮挡前方视野的简单 UI：

- Canvas：World Space 或 Camera Space。
- 一个小的 peripheral target。
- target / non-target 两类刺激。
- 一个可配置的 response action。
- UI 不放在视野中心，不覆盖整个窗口。

每个 visual task 事件至少记录：

~~~text
visual_task_stimulus
visual_task_response
input_accepted
input_rejected
pause
error
~~~

visual task 的难度、target duration、response window 和位置都是 pilot-dependent。

## 16. 输入和暂停

至少设置三类不同的输入：

- pause
- identification response
- visual task response

同一时刻发生多个输入时按以下优先级处理：

1. system safety 或 menu
2. pause
3. identification response
4. visual-task response

每个输入都记录 control ID、focus state、accepted 或 rejected、debounce result 和 rejection reason。

不要让一个键同时承担 pause 和 participant response。

## 17. 运行完整 operator dry run

只有 silent route、technical audio、visual task 和 input 都各自通过后，才运行四个条件的完整 dry run。

每个 dry run 保留：

- application version
- route-state manifest
- condition allocation table
- session manifest
- raw event log
- exported event log
- error-path log
- pause/resume log
- asset and licence register
- operator checklist

四条件 dry run 的最低检查：

- NS_Q、NS_N、S_Q、S_N 各出现一次。
- 每个 block 有三个 transition_onset。
- 每个 transition 都能关联 notification 和 response。
- pause/resume 不会重复触发 transition。
- missing audio、missing route 或 missing log path 会停止 block 并写 error。
- 每个 block 的日志都有 condition_id 和 version fields。

## 18. 当前不要做的事

- 不实现真实飞行动力学。
- 不实现 pilot controls。
- 不购买或拍摄 Insta360 Pro 2 8K 360°视频。
- 不为了外观寻找高面数 eVTOL 模型。
- 不把 Cesium streaming 当作 route logic。
- 不把 technical clip 当成正式实验刺激。
- 不把 prototype timing、速度、音量、N 或 response window 写进论文 Methods。
- 不在 Gate 7 和 Gate 8 之前招募正式参与者。

## 19. 故障排查

### Camera 在地板下面

检查 Camera Offset 的 Y 是否约为 1.5，检查 XR Origin 是否被错误地缩放或旋转。

### RoutePlayer 不移动

检查 movingRoot 是否拖入 PassengerCabin，waypoints 是否至少有两个，W0 是否和 PassengerCabin 起点一致。

### transition_onset 没有出现

检查 FlightStateMachine 的 routePlayer 引用、rules threshold 顺序和 RoutePlayer.Progress 是否达到 0.25、0.55、0.85。

### Audio 没声音

检查 AudioSource、AudioClip、Mute、Audio Listener 和系统输出设备，并检查日志中是否出现 error。

### 运行一次后第二次不触发

需要在重新开始 block 时调用 FlightStateMachine.ResetBlock，并重新调用 RoutePlayer.StartRoute。

### 日志找不到

查看 Console 中 EventLogger.LogPath，或在运行系统上搜索 AudioV0/session_* /event_log.jsonl。

## 20. 立即执行顺序

你现在从第 1 项开始，不要跳步：

1. 保存 AudioV0 场景。
2. 确认 PassengerCabin、地板和座椅层级。
3. 创建 RouteEnvironment、RouteWaypoints 和四个路线点。
4. 创建 Systems。
5. 创建四个 C# 脚本文件并逐个确认 Console 无编译错误。
6. 按 Inspector 连线。
7. 运行 silent route。
8. 检查 event_log.jsonl。
9. silent route 通过后加入 technical audio。
10. technical audio 通过后再加入四种 condition、visual task 和输入。
11. 最后运行完整 operator dry run。

相关基线材料：

- Audio Experiment V0 Protocol and Prototype Checklist
- Audio Experiment Full Audit Handoff 2026-09-07
- Audio Experiment V0 执行指南

