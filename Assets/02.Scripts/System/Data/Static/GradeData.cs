using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [SerializeField] private StageGradeConfig[] _stageConfigs;
    private Dictionary<int, int> _chapterStageCountCache;

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

    public StageGradeConfig GetStageGradeConfig(string stageId) => Array.Find(_stageConfigs, config => config.StageId == stageId);

    public int GetStageCount() => _stageConfigs?.Length ?? 0;

    public int GetStagesInChapter(int chapter)
    {
        if (_stageConfigs == null) return 0;

        if (_chapterStageCountCache == null)
        {
            BuildChapterCache();
        }

        return _chapterStageCountCache.TryGetValue(chapter, out int count) ? count : 0;
    }

    private void BuildChapterCache()
    {
        _chapterStageCountCache = new Dictionary<int, int>();

        foreach (var config in _stageConfigs)
        {
            if (string.IsNullOrEmpty(config.StageId)) continue;

            string[] parts = config.StageId.Split('_');
            if (parts.Length >= 2 && int.TryParse(parts[0], out int chapter))
            {
                if (!_chapterStageCountCache.ContainsKey(chapter))
                {
                    _chapterStageCountCache[chapter] = 0;
                }
                _chapterStageCountCache[chapter]++;
            }
        }
    }
    
    private void OnValidate()
    {
        if (_stageConfigs == null) return;

        // 캐시 무효화 (에디터에서 데이터 변경 시)
        _chapterStageCountCache = null;

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