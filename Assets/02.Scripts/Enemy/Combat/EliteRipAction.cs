using _02.Scripts.Player.Data;
using UnityEngine;
using UnityEngine.AI;

public class EliteRipAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly EnemyKnockbackHitbox _hitbox;
    private readonly NavMeshAgent _agent;
    private readonly EnemyAnimatorController _anim;

    private readonly float _damagePerHit;
    private readonly float _ripMoveSpeed;
    private readonly float _ripKnockbackDistance;
    private readonly KnockbackLevel _knockbackLevel;

    private float _ratio;
    private bool _isFinished;

    private bool _cycleEnded;

    public bool IsFinished => _isFinished;

    public EliteRipAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        EnemyKnockbackHitbox hitbox,
        NavMeshAgent agent,
        EnemyAnimatorController anim,
        float damagePerHit,
        float ripMoveSpeed,
        float ripKnockbackDistance,
        KnockbackLevel knockbackLevel = KnockbackLevel.Light
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _hitbox = hitbox;
        _agent = agent;
        _anim = anim;
        _damagePerHit = damagePerHit;
        _ripMoveSpeed = ripMoveSpeed;
        _ripKnockbackDistance = ripKnockbackDistance;
        _knockbackLevel = knockbackLevel;
    }

    public void Enter()
    {
        _isFinished = false;

        // 난도질 중엔 계속 전진
        _agent.isStopped = false;

        // 필요하면 여기서 속도 조절
        _ratio = _ripMoveSpeed / _agent.speed;
        _movement.SetSpeedMultiplier(_ratio);

        _anim.SetRip(true);
#if UNITY_EDITOR
        Debug.Log("난도질 공격 시도");
#endif
    }

    public void Update()
    {
        if (_isFinished) return;
        if (_player == null) return;

        // 난도질 중 전진
        _movement.SetRotationToLookAt(_player);
        _movement.MoveTo(_player.position);
    }

    public void Exit()
    {
        _isFinished = true;

        _anim.SetRip(false);

        _movement.Stop();
        _hitbox?.Disable();
        _movement.ResetSpeedMultiplier();
    }

    // 애니메이션 이벤트
    public void OnAnimStart()
    {
        _cycleEnded = false;
    }

    public void OnHitStart()
    {
        _hitbox?.Enable(_damagePerHit, _ripKnockbackDistance, _knockbackLevel);
    }

    public void OnHitEnd()
    {
        _hitbox?.Disable();
    }

    public void OnAnimEnd()
    {
        _hitbox?.Disable();
        _cycleEnded = true;
    }
}
