using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RankingData", menuName = "Game/Rank Config")]
public class GradeData : ScriptableObject
{
    [SerializeField] private GradeConfig[] _configs;
    public GradeConfig GetGrade(int currentScore)
    {
        if (_configs == null || _configs.Length == 0)
        {
            Debug.LogError("[RankingData] GetRank: _configs is null or empty.");
            return null;
        }

        foreach (GradeConfig config in _configs)
        {
            if (currentScore >= config.RequiredScore)
            {
                return config;
            }
        }
        return _configs[^1];
    }
    
    private void OnValidate()
    {
        if (_configs == null || _configs.Length == 0) return;
        Array.Sort(_configs, (a, b) => b.RequiredScore.CompareTo(a.RequiredScore));
    }
}

[Serializable]
public class GradeConfig
{
    public string Grade;
    public int RequiredScore; 
    public int RewardStars; 
}
