public interface IEnemyAttackAlwaysStep

{
    void StartAlways();
    void TickAlways();
    void StopAlways();

    // 현재 스텝에 따라 잠깐 멈춰야 하는지 여부
    bool ShouldTickWhile(IEnemyAttackStep currentStep);
}
