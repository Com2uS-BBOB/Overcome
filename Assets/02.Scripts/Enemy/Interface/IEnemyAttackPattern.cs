public interface IEnemyAttackPattern
{
    void Start();
    void Update();
    void Stop();
    bool IsFinished { get; }
}
