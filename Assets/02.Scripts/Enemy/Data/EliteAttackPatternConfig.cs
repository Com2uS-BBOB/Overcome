using UnityEngine;

public struct EliteAttackPatternConfig
{
    public float OpeningRushDuration;

    public float RipRange;
    public float RipMoveSpeed;
    public float RipDamagePerHit;
    public float RipTouchDelay;         // 공격권 실패 시 딜레이
    public float RipKnockbackDistance;

    public float HowlDuration;

    public EliteAttackPatternConfig(
        float openingRushDuration = 0.5f,
        float ripRange = 12f,
        float ripMoveSpeed = 2f,
        float ripDamagePerHit = 2f,
        float ripTouchDelay = 0.25f,
        float ripKnockbackDistance = 1f,
        float howlDuration = 3.4f
    )
    {
        OpeningRushDuration = openingRushDuration;
        RipRange = ripRange;
        RipMoveSpeed = ripMoveSpeed;
        RipDamagePerHit = ripDamagePerHit;
        RipTouchDelay = ripTouchDelay;
        RipKnockbackDistance = ripKnockbackDistance;
        HowlDuration = howlDuration;
    }
}
