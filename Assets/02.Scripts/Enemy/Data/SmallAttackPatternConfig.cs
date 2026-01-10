using UnityEngine;

public struct SmallAttackPatternConfig
{
    public AttackWaitActionConfig PressureWaitConfig;

    public float MeleeCooldownMin;
    public float MeleeCooldownMax;
    public float MeleeAttackDelay;

    public SmallAttackPatternConfig(
        AttackWaitActionConfig pressureWaitConfig,
        float meleeCooldownMin = 3f,
        float meleeCooldownMax = 5f,
        float meleeAttackDelay = 0.2f
    )
    {
        PressureWaitConfig = pressureWaitConfig;
        MeleeCooldownMin = meleeCooldownMin;
        MeleeCooldownMax = meleeCooldownMax;
        MeleeAttackDelay = meleeAttackDelay;
    }
}
