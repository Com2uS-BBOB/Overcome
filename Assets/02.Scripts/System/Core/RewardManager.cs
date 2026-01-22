using UnityEngine;

public class RewardManager : SingletonBehaviour<RewardManager>
{
    [SerializeField] private RewardUnlockData _rewardUnlockData;
    
    public bool IsRewardUnlocked(ERewardType rewardType)
    {
        if (SceneController.Instance != null)
        {
            ESceneType currentScene = SceneController.Instance.CurrentOrTargetScene;
            if (currentScene == ESceneType.TutorialScene)
            {
                return true;
            }
        }
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

    public bool IsCrescentUnlocked() => IsRewardUnlocked(ERewardType.CrescentSkill);
    public bool IsOverDriveUnlocked() => IsRewardUnlocked(ERewardType.OverDriveSkill);
}