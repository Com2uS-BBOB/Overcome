using UnityEngine;

public struct SmallAttackPatternConfig
{
    public AttackWaitActionConfig PressureWaitConfig;

    public float MeleeCooldownMin;
    public float MeleeCooldownMax;
    public float MeleeAttackDelay;

    public float KnockbackDistance;

    public SmallAttackPatternConfig(
        AttackWaitActionConfig pressureWaitConfig,
        float meleeCooldownMin = 3f,
        float meleeCooldownMax = 5f,
        float meleeAttackDelay = 0.2f,
        float knockbackDistance = 1f
    )
    {
        PressureWaitConfig = pressureWaitConfig;
        MeleeCooldownMin = meleeCooldownMin;
        MeleeCooldownMax = meleeCooldownMax;
        MeleeAttackDelay = meleeAttackDelay;
        KnockbackDistance = knockbackDistance;
    }
}
