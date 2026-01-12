public interface IEnemyAttackStep
{
    // 실행 중인 스텝이 없을 때, 이 스텝이 시작을 시도
    bool TryStart();

    // 현재 실행 중인 스텝일 때 매 프레임 호출
    void Tick();

    // 실행 중인 스텝이 멈출 때 호출
    void Stop();

    // 애니메이션 이벤트 포워딩 (필요한 스텝만 사용)
    void OnAnimEvent(EAttackAnimEvent animEvent);

    // 스텝이 끝났는지 확인 (끝났으면 runner가 current를 null로 만들고 다음 시도)
    bool IsFinished { get; }
}
