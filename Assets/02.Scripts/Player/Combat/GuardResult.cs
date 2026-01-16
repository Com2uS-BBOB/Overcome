namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 가드 결과
    /// </summary>
    public enum GuardResult
    {
        None,           // 가드하지 않음
        NormalBlock,    // 일반 가드 성공
        JustGuard,      // 저스트 가드 성공
        GuardBreak,     // 가드 브레이크 (게이지 부족)
        RearAttack      // 후방 공격 (가드 실패)
    }
}