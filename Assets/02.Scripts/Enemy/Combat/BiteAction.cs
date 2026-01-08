using UnityEngine;
using UnityEngine.AI;

public class BiteAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly BiteAttackHitbox _hitbox;
    private readonly Animator _animator;
    private readonly NavMeshAgent _agent;

    private readonly float _damage;
    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public BiteAction(
        Transform enemy,
        Transform player,
        BiteAttackHitbox hitbox,
        Animator animator,
        NavMeshAgent agent,
        float damage
    )
    {
        _enemy = enemy;
        _player = player;
        _hitbox = hitbox;
        _animator = animator;
        _agent = agent;
        _damage = damage;
    }

    public void Enter()
    {
        _isFinished = false;

        _agent.isStopped = true;
        _agent.ResetPath();

        Vector3 direction = _player.position - _enemy.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            _enemy.rotation = Quaternion.LookRotation(direction);
        }

        _animator.SetTrigger("AttackTest");
        Debug.Log("깨물기 공격 시도");
    }

    public void Update() { }

    public void Exit()
    {
        _hitbox.Disable();
    }

    // Animation Events
    public void OnAnimStart() { }
    public void OnHitStart() => _hitbox.Enable(_damage);
    public void OnHitEnd() => _hitbox.Disable();
    public void OnAnimEnd() => _isFinished = true;
}