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

    private float _keepDistance = 1.15f;  // 유지할 거리
    private float _keepBuffer = 0.15f;   // 경계 떨림 방지 버퍼
    private float _preStoppingDistance;

    private float _ratio;

    private bool _isFinished;

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
        float ripKnockbackDistance
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
    }

    public void Enter()
    {
        _isFinished = false;

        // 난도질 중엔 계속 전진
        _agent.isStopped = false;

        // 원래 stoppingDistance 복구를 위해 저장
        _preStoppingDistance = _agent.stoppingDistance;

        // 난도질 동안은 이 거리까지만 접근
        _agent.stoppingDistance = _keepDistance;

        // 필요하면 여기서 속도 조절
        _ratio = _ripMoveSpeed / _agent.speed;
        _movement.SetSpeedMultiplier(_ratio);

        _anim.SetRip(true);
    }

    public void Update()
    {
        if (_isFinished) return;
        if (_player == null) return;

        // 난도질 중 전진
        _movement.SetRotationToLookAt(_player);
        OnRipGoing();
    }

    private void OnRipGoing()
    {
        Vector3 enemyPosition = _enemy.position; enemyPosition.y = 0f;
        Vector3 playerPosition = _player.position; playerPosition.y = 0f;

        Vector3 toPlayer = playerPosition - enemyPosition;
        float distance = toPlayer.magnitude;

        // 너무 가까우면 잠깐 멈추기 (경계 근처는 떨림 방지로 멈춤 유지)
        if (distance <= _keepDistance + _keepBuffer)
        {
            SoftStopAgentOnly();
            return;
        }

        // 멀면 오프셋 목표점으로 접근 (플레이어 위치에 겹치지 않음)
        Vector3 direction = (distance > 0.0001f) ? (toPlayer / distance) : _enemy.forward;
        Vector3 target = playerPosition - direction * _keepDistance;

        _agent.isStopped = false; // SoftStop에서 멈췄을 수 있으니 풀어줌
        _movement.MoveTo(target);
    }

    private void SoftStopAgentOnly()
    {
        if (_agent == null || !_agent.enabled) return;

        // ResetPath 안 함 (경계에서 재탐색/떨림 감소)
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
    }

    public void Exit()
    {
        _isFinished = true;

        _anim.SetRip(false);

        _agent.stoppingDistance = _preStoppingDistance;
        _movement.Stop();
        _hitbox?.Disable();
        _movement.ResetSpeedMultiplier();
    }

    // 애니메이션 이벤트
    public void OnAnimStart()
    {

    }

    public void OnHitStart()
    {
        _hitbox?.Enable(_damagePerHit, _ripKnockbackDistance);
    }

    public void OnHitEnd()
    {
        _hitbox?.Disable();
    }

    public void OnAnimEnd()
    {

    }
}
