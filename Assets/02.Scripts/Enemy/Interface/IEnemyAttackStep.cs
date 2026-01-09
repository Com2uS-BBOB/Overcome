public interface IEnemyAttackStep
{
    /// <summary>
    /// 실행 중인 스텝이 없을 때, 이 스텝이 시작을 시도한다.
    /// 시작했다면 true (runner가 이 스텝을 current로 잡음)
    /// 시작 못하면 false (다음 스텝으로 넘어감)
    /// </summary>
    bool TryStart();

    /// <summary>
    /// 현재 실행 중인 스텝일 때 매 프레임 호출
    /// </summary>
    void Tick();

    /// <summary>
    /// runner.Stop() 또는 스텝 전환 시 정리
    /// </summary>
    void Stop();

    /// <summary>
    /// 애니메이션 이벤트 포워딩 (필요한 스텝만 사용)
    /// </summary>
    void OnAnimEvent(EAttackAnimEvent animEvent);

    /// <summary>
    /// 스텝이 끝났는지 (끝났으면 runner가 current를 null로 만들고 다음 시도)
    /// </summary>
    bool IsFinished { get; }
}
