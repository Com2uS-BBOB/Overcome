using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardUnlockConfig", menuName = "Game/RewardUnlockConfig")]
public class RewardUnlockConfig : ScriptableObject
{
    [System.Serializable]
    public class RewardUnlockEntry
    {
        public ERewardType RewardType;
        public int RequiredStars;
    }

    public List<RewardUnlockEntry> UnlockRequirements;
    
    public bool IsRewardUnlocked(ERewardType rewardType, int playerStar)
    {
        RewardUnlockEntry entry = UnlockRequirements.Find(e => e.RewardType == rewardType);
        return entry != null && playerStar >= entry.RequiredStars;
    }
}