using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬별 BGM 설정을 중앙에서 관리하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "SceneBGMConfig", menuName = "Sound/Scene BGM Config")]
public class SceneBGMConfig : ScriptableObject
{
    [Header("Stage Common BGM (모든 스테이지 공통)")]
    [SerializeField] private bool _useCommonStageBGM = true;
    [SerializeField] private StageBGMData _commonStageBGM = new StageBGMData
    {
        FirstBGMName = "BGM_Stage_First",
        LoopBGMName = "BGM_Stage_Loop",
        FadeInDuration = 0.5f,
        FadeOutDuration = 0.5f,
        PlayOnSceneLoad = true
    };

    [Header("Scene Specific BGM")]
    [SerializeField] private List<SceneBGMData> _sceneBGMList = new List<SceneBGMData>();

    /// <summary>
    /// 특정 씬의 BGM 설정을 반환
    /// </summary>
    public SceneBGMData GetBGMData(ESceneType sceneType)
    {
        // 스테이지 씬이고 공통 BGM 사용 시
        if (_useCommonStageBGM && IsStageScene(sceneType))
        {
            return _commonStageBGM.ToSceneBGMData();
        }

        return _sceneBGMList.Find(data => data.SceneType == sceneType);
    }

    /// <summary>
    /// 스테이지 씬인지 확인 (Stage로 시작하는 씬)
    /// </summary>
    private bool IsStageScene(ESceneType sceneType)
    {
        string sceneName = sceneType.ToString();
        return sceneName.StartsWith("Stage");
    }

    /// <summary>
    /// 모든 씬 BGM 설정 반환
    /// </summary>
    public IReadOnlyList<SceneBGMData> GetAllBGMData() => _sceneBGMList;

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 기본 설정 초기화
    /// </summary>
    [ContextMenu("Initialize Default Settings")]
    private void InitializeDefaults()
    {
        // 스테이지 공통 BGM 설정
        _useCommonStageBGM = true;
        _commonStageBGM = new StageBGMData
        {
            FirstBGMName = "BGM_Stage_First",
            LoopBGMName = "BGM_Stage_Loop",
            FadeInDuration = 0.5f,
            FadeOutDuration = 0.5f,
            PlayOnSceneLoad = true,
            VolumeMultiplier = 1f
        };

        // 씬별 개별 설정
        _sceneBGMList.Clear();

        // 로그인 씬
        _sceneBGMList.Add(new SceneBGMData
        {
            SceneType = ESceneType.LoginScene,
            BGMName = "BGM_Login",
            UseIntroLoop = false,
            FadeInDuration = 1f,
            FadeOutDuration = 0.5f,
            PlayOnSceneLoad = true
        });

        // 로비 씬
        _sceneBGMList.Add(new SceneBGMData
        {
            SceneType = ESceneType.LobbyScene,
            BGMName = "BGM_Lobby",
            UseIntroLoop = false,
            FadeInDuration = 1f,
            FadeOutDuration = 0.5f,
            PlayOnSceneLoad = true
        });

        // Stage 씬들은 _commonStageBGM 사용 (개별 설정 불필요)

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("[SceneBGMConfig] 기본 설정이 초기화되었습니다. 모든 Stage 씬은 공통 BGM을 사용합니다.");
    }
#endif
}

/// <summary>
/// 스테이지 공통 BGM 설정 (SceneType 없음)
/// </summary>
[Serializable]
public class StageBGMData
{
    [Tooltip("씬 로드 시 자동 재생 여부")]
    public bool PlayOnSceneLoad = true;

    [Header("BGM Settings")]
    [Tooltip("첫번째 BGM 이름 (한 번 재생 후 루프 BGM으로 전환)")]
    public string FirstBGMName;

    [Tooltip("루프 BGM 이름 (첫번째 BGM 끝난 후 무한 반복)")]
    public string LoopBGMName;

    [Header("Fade Settings")]
    [Tooltip("페이드 인 시간 (초)")]
    [Range(0f, 3f)] public float FadeInDuration = 0.5f;

    [Tooltip("페이드 아웃 시간 (초)")]
    [Range(0f, 3f)] public float FadeOutDuration = 0.5f;

    [Header("Volume Settings")]
    [Tooltip("BGM 볼륨 배율 (1 = 기본값)")]
    [Range(0f, 1.5f)] public float VolumeMultiplier = 1f;

    /// <summary>
    /// SceneBGMData로 변환 (SoundManager 호환용)
    /// </summary>
    public SceneBGMData ToSceneBGMData()
    {
        return new SceneBGMData
        {
            PlayOnSceneLoad = PlayOnSceneLoad,
            UseIntroLoop = true,
            IntroBGMName = FirstBGMName,
            LoopBGMName = LoopBGMName,
            FadeInDuration = FadeInDuration,
            FadeOutDuration = FadeOutDuration,
            VolumeMultiplier = VolumeMultiplier
        };
    }
}

/// <summary>
/// 개별 씬의 BGM 설정 데이터
/// </summary>
[Serializable]
public class SceneBGMData
{
    [Header("Scene Settings")]
    [Tooltip("대상 씬 타입")]
    public ESceneType SceneType;

    [Tooltip("씬 로드 시 자동 재생 여부")]
    public bool PlayOnSceneLoad = true;

    [Header("BGM Settings")]
    [Tooltip("인트로 + 루프 방식 사용 여부")]
    public bool UseIntroLoop = false;

    [Tooltip("단일 BGM 이름 (UseIntroLoop가 false일 때 사용)")]
    public string BGMName;

    [Tooltip("인트로 BGM 이름 (UseIntroLoop가 true일 때 사용)")]
    public string IntroBGMName;

    [Tooltip("루프 BGM 이름 (UseIntroLoop가 true일 때 사용)")]
    public string LoopBGMName;

    [Header("Fade Settings")]
    [Tooltip("페이드 인 시간 (초)")]
    [Range(0f, 3f)] public float FadeInDuration = 1f;

    [Tooltip("페이드 아웃 시간 (초)")]
    [Range(0f, 3f)] public float FadeOutDuration = 0.5f;

    [Header("Volume Settings")]
    [Tooltip("이 씬에서의 BGM 볼륨 배율 (1 = 기본값)")]
    [Range(0f, 1.5f)] public float VolumeMultiplier = 1f;
}