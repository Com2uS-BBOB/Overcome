using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyConfigData", menuName = "Game/Difficulty Config")]
public class DifficultyConfigData : ScriptableObject
{
    public DifficultyConfig[] Configs;

    public DifficultyConfig GetConfig(EDifficultyType difficulty)
    {
        if (Configs == null || Configs.Length == 0)
        {
            Debug.LogError("[DifficultyConfigData] Configs array is empty");
            return null;
        }
        return Configs.FirstOrDefault(config => config.Difficulty == difficulty);
    }
}

[Serializable]
public class DifficultyConfig
{
    public EDifficultyType Difficulty;
    [Header("시간 기본 세팅")]
    public bool HasTimeLimit;
    public float StartTime;
    public float MaxPlayTime;
    
    [Header("시간 증감량 관련")]
    [Tooltip("난이도에 따른 추가 증감량")]
    public float TimeAdjustment; 
    [Tooltip("시간 증가량 감쇠 시작 시점")]
    public float AdjustmentDecayStart;
    [Tooltip("시간 증가량 감쇠 비율")]
    public float AdjustmentDecayRatio; 
    
    [Header("점수 관련")]
    public float ScoreMultiplier;
    public int ClearBonus;
}