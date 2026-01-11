using UnityEngine;

public struct EliteAttackPatternConfig
{
    public float OpeningRushDistance;
    public float OpeningRushDuration;

    public float RipRange;
    public float RipMoveSpeed;
    public float RipDamagePerHit;       // -2s 느낌이면 데미지 환산
    public float RipTouchDelay;         // 공격권 실패 시 딜레이

    public float HowlDuration;

    public EliteAttackPatternConfig(
        float openingRushDistance = 20f,
        float openingRushDuration = 1.4f,
        float ripRange = 10f,
        float ripMoveSpeed = 2f,
        float ripDamagePerHit = 2f,
        float ripTouchDelay = 0.25f,
        float howlDuration = 3.2f
    )
    {
        OpeningRushDistance = openingRushDistance;
        OpeningRushDuration = openingRushDuration;
        RipRange = ripRange;
        RipMoveSpeed = ripMoveSpeed;
        RipDamagePerHit = ripDamagePerHit;
        RipTouchDelay = ripTouchDelay;
        HowlDuration = howlDuration;
    }
}
