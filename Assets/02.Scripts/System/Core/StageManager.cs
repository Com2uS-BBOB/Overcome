using UnityEngine;

public class StageManager : SingletonBehaviour<StageManager>
{
    [SerializeField] private StageData _stageData;

    private const int MaxStarCount = 3;
    private string _currentStageId = "1_1"; // "1_1", "1_2" 형식
    private int _currentStageIntId; // 11, 12, 13... (저장용 ID)

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
    public void CompleteStage(int finalScore)
    {
        // Previous Data
        GradeConfig grade = _stageData.GetGrade(_currentStageId, finalScore);
        StageProgress progress = PlayerDataManager.Instance.GetStageProgress(_currentStageIntId);

        // Update Data
        UpdateStageProgress(progress, grade, finalScore);
        PlayerDataManager.Instance.SaveStageProgress(progress);

        // Show Data
        ShowStageResult(grade);
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

    private void ShowStageResult(GradeConfig grade)
    {
        // todo. Result UI 표시 로직 위치 수정
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

#if UNITY_EDITOR

    #region Editor Test
    [Header("Test Info")]
    [SerializeField] private int _stage;
    [SerializeField] private int _level;
    [SerializeField] private int _score;

    [ContextMenu("Test/Test Complete Stage")]
    public void TestCompleteStage()
    {
        SetCurrentStage(_stage, _level);
        CompleteStage(_score);
    }
    #endregion

#endif
}