using UnityEngine;

public struct NormalAttackPatternConfig
{
    public AttackWaitActionConfig PressureWaitConfig;

    public float OpeningRushDistance;
    public float OpeningRushDuration;

    public float BiteCooldownMin;
    public float BiteCooldownMax;
    public float BiteTouchDelay;

    public NormalAttackPatternConfig(
        AttackWaitActionConfig pressureWaitConfig,
        float openingRushDistance = 10f,
        float openingRushDuration = 0.4f,
        float biteCooldownMin = 3f,
        float biteCooldownMax = 5f,
        float biteTouchDelay = 0.2f
    )
    {
        PressureWaitConfig = pressureWaitConfig;
        OpeningRushDistance = openingRushDistance;
        OpeningRushDuration = openingRushDuration;
        BiteCooldownMin = biteCooldownMin;
        BiteCooldownMax = biteCooldownMax;
        BiteTouchDelay = biteTouchDelay;
    }
}
