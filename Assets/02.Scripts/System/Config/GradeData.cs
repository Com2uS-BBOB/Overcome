using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GradeData", menuName = "Game/Grade Data")]
public class GradeData : ScriptableObject
{
    [SerializeField] private StageGradeConfig[] _stageConfigs;

    public GradeConfig GetGrade(string stageId, int currentScore)
    {
        if (_stageConfigs == null || _stageConfigs.Length == 0)
        {
            Debug.LogError("[GradeData] GetGrade: _stageConfigs is null or empty.");
            return null;
        }

        // 해당 스테이지 찾기
        StageGradeConfig stageConfig = Array.Find(_stageConfigs, config => config.StageId == stageId);
        
        if (stageConfig == null)
        {
            Debug.LogError($"[GradeData] GetGrade: Stage '{stageId}' not found.");
            return null;
        }

        // 점수에 맞는 등급 찾기
        foreach (GradeConfig grade in stageConfig.Grades)
        {
            if (currentScore >= grade.RequiredScore)
            {
                return grade;
            }
        }

        // 모든 조건을 만족하지 못하면 가장 낮은 등급 반환
        return stageConfig.Grades.Length > 0 ? stageConfig.Grades[^1] : null;
    }

    private void OnValidate()
    {
        if (_stageConfigs == null) return;

        foreach (var stageConfig in _stageConfigs)
        {
            if (stageConfig.Grades != null && stageConfig.Grades.Length > 0)
            {
                Array.Sort(stageConfig.Grades, (a, b) => b.RequiredScore.CompareTo(a.RequiredScore));
            }
        }
    }
}

[Serializable]
public class StageGradeConfig
{
    public string StageId;
    public GradeConfig[] Grades;
}

[Serializable]
public class GradeConfig
{
    public string Grade;
    public int RequiredScore;
    public int RewardStars;
}