using UnityEngine;

public class BiteAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly BiteAttackHitbox _hitbox;
    private readonly Animator _animator;

    private readonly float _damage;
    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public BiteAction(
        Transform enemy,
        Transform player,
        BiteAttackHitbox hitbox,
        Animator animator,
        float damage
    )
    {
        _enemy = enemy;
        _player = player;
        _hitbox = hitbox;
        _animator = animator;
        _damage = damage;
    }

    public void Enter()
    {
        _isFinished = false;

        Vector3 direction = _player.position - _enemy.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            _enemy.rotation = Quaternion.LookRotation(direction);
        }

        _animator.SetTrigger("AttackTest");
    }

    public void Update()
    {
        // 애니메이션 이벤트로 종료
    }

    public void Exit()
    {
        _hitbox.Disable();
    }

    // 애니메이션 이벤트
    public void OnHitStart() => _hitbox.Enable(_damage);
    public void OnHitEnd() => _hitbox.Disable();
    public void OnAnimEnd() => _isFinished = true;
}
