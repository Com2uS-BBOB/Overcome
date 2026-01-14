using UnityEngine;

public class StageManager : SingletonBehaviour<StageManager>
{
    [SerializeField] private GradeData _gradeData;

    private const int StageCount = 3;
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
        if (_gradeData == null)
        {
            Debug.LogError("[StageManager] GradeData is not assigned!");
            return null;
        }

        return _gradeData.GetGrade(_currentStageId, score);
    }

    public GradeConfig GetGradeConfig(string stageId, int score)
    {
        return _gradeData?.GetGrade(stageId, score);
    }

    public StageGradeConfig GetStageGradeConfig(string stageId)
    {
        return _gradeData?.GetStageGradeConfig(stageId);
    }
    #endregion

    #region Stage Completion
    public void CompleteStage(int finalScore)
    {
        // Previous Data
        GradeConfig grade = _gradeData.GetGrade(_currentStageId, finalScore);
        StageProgress progress = PlayerDataManager.Instance.GetStageProgress(_currentStageIntId);

        // Update Data
        UpdateStageProgress(progress, grade, finalScore);
        PlayerDataManager.Instance.SaveStageProgress(progress);

        // Show Data
        ShowStageResult(grade);
    }

    private void UpdateStageProgress(StageProgress previousProgress, GradeConfig grade, int score)
    {
        if (previousProgress.BestScore > score) return;
        // Update Star Info
        if (grade.RewardStars > 0)
        {
            previousProgress.IsCleared = true;
            previousProgress.StarsEarned = Mathf.Max(previousProgress.StarsEarned, grade.RewardStars);
        }
        previousProgress.PlayCount++;
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
            prevStageId = (chapter - 1) * 10 + StageCount;
        }
        else
        {
            prevStageId = chapter * 10 + (level - 1);
        }

        StageProgress prevProgress = PlayerDataManager.Instance.GetStageProgress(prevStageId);
        return prevProgress != null && prevProgress.IsCleared;
    }

    public int GetPlayerStarCount() => PlayerDataManager.Instance.GetPlayerStarCount();

    public int GetTotalStars() => StageCount * _gradeData.GetStageCount();
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