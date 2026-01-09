using UnityEngine;

public struct NormalAttackPatternConfig
{
    public AttackWaitActionConfig PressureWaitConfig;

    public float BiteCooldownMin;
    public float BiteCooldownMax;
    public float BiteTouchDelay;

    public NormalAttackPatternConfig(
        AttackWaitActionConfig pressureWaitConfig,
        float biteCooldownMin = 3f,
        float biteCooldownMax = 5f,
        float biteTouchDelay = 0.2f
    )
    {
        PressureWaitConfig = pressureWaitConfig;
        BiteCooldownMin = biteCooldownMin;
        BiteCooldownMax = biteCooldownMax;
        BiteTouchDelay = biteTouchDelay;
    }
}
