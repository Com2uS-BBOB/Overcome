using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RankingData", menuName = "Game/Rank Config")]
public class RankingData : ScriptableObject
{
    [SerializeField] private RankConfig[] _configs;
    public RankConfig GetRank(int currentScore)
    {
        if (_configs == null)
        {
            Debug.LogError("[RankingData] GetRank: _configs is null");
            return null;
        }
        
        foreach (RankConfig config in _configs)
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
public class RankConfig
{
    public string Grade;
    public int RequiredScore; 
    public int RewardStars; 
}
