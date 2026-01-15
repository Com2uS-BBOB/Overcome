using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardUnlockConfig", menuName = "Game/RewardUnlockConfig")]
public class RewardUnlockData : ScriptableObject
{
    public List<RewardUnlockConfig> UnlockRequirements;
    
    public bool IsRewardUnlocked(ERewardType rewardType, int playerStar)
    {
        RewardUnlockConfig config = UnlockRequirements.Find(e => e.RewardType == rewardType);
        return config != null && playerStar >= config.RequiredStars;
    }

    public int GetRequiredStars(ERewardType rewardType)
    {
        RewardUnlockConfig config = UnlockRequirements.Find(e => e.RewardType == rewardType);
        return config.RequiredStars;;
    }

    public RewardUnlockConfig GetRewardUnlockInfo(ERewardType rewardType)
    {
        return UnlockRequirements.Find(e => e.RewardType == rewardType);
    }
}

[System.Serializable]
public class RewardUnlockConfig
{
    public ERewardType RewardType;
    public Sprite RewardSprite;
    public int RequiredStars;
}