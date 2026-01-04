namespace _02.Scripts.Player.StateMachine
{
    public interface IPlayerState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }
}