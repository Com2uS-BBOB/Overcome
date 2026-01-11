using UnityEngine;
using UnityEngine.AI;

public class EliteRipAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly EnemyKnockbackHitbox _hitbox;
    private readonly NavMeshAgent _agent;
    private readonly Animator _animator;

    private readonly float _damagePerHit;
    private readonly float _ripMoveSpeed;

    private float _ratio;
    private bool _finished;

    public bool IsFinished => _finished;

    public EliteRipAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        EnemyKnockbackHitbox hitbox,
        NavMeshAgent agent,
        Animator animator,
        float damagePerHit,
        float ripMoveSpeed
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _hitbox = hitbox;
        _agent = agent;
        _animator = animator;
        _damagePerHit = damagePerHit;
        _ripMoveSpeed = ripMoveSpeed;
    }

    public void Enter()
    {
        _finished = false;

        // 난도질 중엔 계속 전진
        _agent.isStopped = false;

        // 필요하면 여기서 속도 조절
        _ratio = _ripMoveSpeed / _agent.speed;
        _movement.SetSpeedMultiplier(_ratio);

        // _animator.SetBool("IsRipping", true); 혹은 트리거
#if UNITY_EDITOR
        Debug.Log("난도질 공격 시도");
#endif
    }

    public void Update()
    {
        if (_finished) return;
        if (_player == null) return;

        // 난도질 중 전진
        _movement.SetRotationToLookAt(_player);
        _movement.MoveTo(_player.position);
    }

    public void Exit()
    {
        // _animator.SetBool("IsRipping", false); 혹은 트리거
        _movement.Stop();
        _hitbox?.Disable();
        _movement.ResetSpeedMultiplier();
    }

    // 애니메이션 이벤트
    public void OnAnimStart()
    {
        // 난도질 시작 시 처리
    }

    public void OnHitStart()
    {
        _hitbox?.Enable(_damagePerHit);
    }

    public void OnHitEnd()
    {
        _hitbox?.Disable();
    }

    public void OnAnimEnd()
    {
        _finished = true;
    }
}
