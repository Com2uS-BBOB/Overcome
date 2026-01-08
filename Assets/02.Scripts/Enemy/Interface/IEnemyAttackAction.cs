public interface IEnemyAction
{
    void Enter();
    void Update();
    void Exit();
    bool IsFinished { get; }
}