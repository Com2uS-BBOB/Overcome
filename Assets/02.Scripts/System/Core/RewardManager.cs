using UnityEngine;

public class RewardManager : SingletonBehaviour<RewardManager>
{
    [SerializeField] private RewardUnlockData _rewardUnlockData;

    public bool IsRewardUnlocked(ERewardType rewardType)
    {
        int totalStars = PlayerDataManager.Instance.GetPlayerStarCount();
        return _rewardUnlockData.IsRewardUnlocked(rewardType, totalStars);
    }

    public int GetRequiredStars(ERewardType rewardType)
    {
        return _rewardUnlockData.GetRequiredStars(rewardType);
    }

    public RewardUnlockConfig GetRewardUnlockInfo(ERewardType rewardType)
    {
        return _rewardUnlockData.GetRewardUnlockInfo(rewardType);
    }
}