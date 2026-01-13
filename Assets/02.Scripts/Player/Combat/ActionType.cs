namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 전투 시스템에서 사용되는 액션 타입
    /// 캔슬 규칙 및 입력 버퍼에서 사용
    /// </summary>
    public enum ActionType
    {
        None = 0,
        Move,           // 이동
        Jump,           // 점프
        Attack,         // 기본 공격 (DragonSword)
        Skill,          // 스킬 (Crescent)
        DashAttack,     // 대시 공격
        Any             // 모든 액션 허용 (캔슬 규칙용)
    }
}