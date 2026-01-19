using UnityEngine;

[System.Serializable]
public struct AttackWaitActionConfig
{
    public float MinWait;
    public float MaxWait;
    public float WaitSpeedMultiplier;

    public bool ReleaseSlotOnExit;
    public int FixedSlotIndex;

    public float ArrivedThreshold;
    public float MinStoppingDistance;

    public AttackWaitActionConfig(
        float minWait,
        float maxWait,
        float waitSpeedMultiplier,
        bool releaseSlotOnExit = true,
        int fixedSlotIndex = -1,
        float arrivedThreshold = 0.2f,
        float minStoppingDistance = 0.1f
    )
    {
        MinWait = minWait;
        MaxWait = maxWait;
        WaitSpeedMultiplier = waitSpeedMultiplier;
        ReleaseSlotOnExit = releaseSlotOnExit;
        FixedSlotIndex = fixedSlotIndex;
        ArrivedThreshold = arrivedThreshold;
        MinStoppingDistance = minStoppingDistance;
    }
}