using UnityEngine;

public class StageManager : SingletonBehaviour<StageManager>
{
    [SerializeField] private StageData _stageData;

    [Header("BGM Settings")]
    [Tooltip("SoundManager의 SceneBGMConfig를 사용하면 false로 설정")]
    [SerializeField] private bool _playBgmOnStart = false;
    [SerializeField] private bool _useIntroLoop = false;
    [SerializeField] private string _introBgmName = "BGM_Stage_Intro";
    [SerializeField] private string _loopBgmName = "BGM_Stage_Loop";
    [SerializeField] private string _stageBgmName = "BGM_Stage";

    private const int MaxStarCount = 3;
    private string _currentStageId = "1_1"; // "1_1", "1_2" 형식
    private int _currentStageIntId = 11; // 11, 12, 13... (저장용 ID)
    public int StageID => _currentStageIntId;
    
    protected override void Init()
    {
        GameEventHandler.OnGameEnd += CompleteStage;
    }
    

    private void Start()
    {
        if (!_playBgmOnStart) return;

        if (_useIntroLoop)
        {
            // 인트로 + 루프 방식
            SoundManager.Instance?.PlayBGMWithIntro(_introBgmName, _loopBgmName);
        }
        else if (!string.IsNullOrEmpty(_stageBgmName))
        {
            // 단일 BGM 루프 방식
            SoundManager.Instance?.PlayBGM(_stageBgmName);
        }
    }

    #region Stage Setup
    public void SetCurrentStage(int chapter, int level)
    {
        _currentStageId = $"{chapter}_{level}";
        _currentStageIntId = chapter * 10 + level;
    }
    #endregion

    #region Grade Query
    public GradeConfig GetGradeConfig(int score)
    {
        if (_stageData == null)
        {
            Debug.LogError("[StageManager] StageData is not assigned!");
            return null;
        }

        return _stageData.GetGrade(_currentStageId, score);
    }

    public GradeConfig GetGradeConfig(string stageId, int score)
    {
        return _stageData?.GetGrade(stageId, score);
    }

    public StageGradeConfig GetStageGradeConfig(string stageId)
    {
        return _stageData?.GetStageGradeConfig(stageId);
    }
    #endregion

    #region Stage Completion
    private void CompleteStage()
    {
        int finalScore = ScoreSystem.Instance.CurrentScore;
        string userID = PlayerDataManager.Instance.GetPlayerID();
        
        // Previous Data
        GradeConfig grade = _stageData.GetGrade(_currentStageId, finalScore);
        StageProgress progress = PlayerDataManager.Instance.GetStageProgress(_currentStageIntId);

        // Update Data
        UpdateStageProgress(progress, grade, finalScore);
        PlayerDataManager.Instance.SaveStageProgress(progress);

        RankingDataManager.Instance.UpdateRanking(_currentStageIntId, userID, finalScore);
    }

    private void UpdateStageProgress(StageProgress previousProgress, GradeConfig grade, int score)
    {
        previousProgress.PlayCount++;
        if (previousProgress.BestScore > score) return;
        // Update Star Info
        if (grade.RewardStars > 0)
        {
            previousProgress.IsCleared = true;
            previousProgress.StarsEarned = Mathf.Max(previousProgress.StarsEarned, grade.RewardStars);
        }
        previousProgress.BestScore = score;
    }

    #endregion

    #region Stage Info Query
    public StageProgress GetCurrentStageProgress()
    {
        return PlayerDataManager.Instance.GetStageProgress(_currentStageIntId);
    }

    public StageProgress GetStageProgress(int chapter, int level)
    {
        int stageId = chapter * 10 + level;
        return PlayerDataManager.Instance.GetStageProgress(stageId);
    }

    public bool IsStageUnlocked(int chapter, int level)
    {
        if (chapter == 1 && level == 1) return true;

        int prevStageId;
        if (level == 1)
        {
            int prevChapterStageCount = _stageData.GetStagesInChapter(chapter - 1);
            prevStageId = (chapter - 1) * 10 + prevChapterStageCount;
        }
        else
        {
            prevStageId = chapter * 10 + (level - 1);
        }

        StageProgress prevProgress = PlayerDataManager.Instance.GetStageProgress(prevStageId);
        return prevProgress != null && prevProgress.IsCleared;
    }

    public int GetPlayerStarCount() => PlayerDataManager.Instance.GetPlayerStarCount();

    public int GetTotalStars() => MaxStarCount * _stageData.GetStageCount();
    #endregion
}