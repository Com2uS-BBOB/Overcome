namespace _02.Scripts.Player.StateMachine
{
    /// <summary>
    /// 플레이어 상태 인터페이스
    /// </summary>
    public interface IPlayerState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }
}