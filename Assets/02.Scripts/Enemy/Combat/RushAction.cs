using UnityEngine;
using UnityEngine.AI;

public class RushAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly EnemyKnockbackHitbox _hitbox;
    private readonly NavMeshAgent _agent;

    private readonly float _rushDistance;
    private readonly float _maxDuration;
    private readonly float _damage;

    private float _timer;
    private bool _isFinished;
    private Vector3 _rushTarget;
    private float _originalSpeed;

    private float _sqrMagnitudeThreshold = 0.01f;
    private float _arrived = 0.1f;
    private float _sampleRadius = 3f;

    public bool IsFinished => _isFinished;

    public RushAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        EnemyKnockbackHitbox hitbox,
        NavMeshAgent agent,
        float rushDistance,
        float maxDuration,
        float damage
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _hitbox = hitbox;
        _agent = agent;
        _rushDistance = rushDistance;
        _maxDuration = maxDuration;
        _damage = damage;
    }

    public void Enter()
    {
        _isFinished = false;
        _timer = 0f;

        Vector3 direction = (_player.position - _enemy.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < _sqrMagnitudeThreshold)
        {
            _isFinished = true;
            return;
        }

        direction.Normalize();

        _rushTarget = _enemy.position + direction * _rushDistance;

        // NavMesh 보정
        if (NavMesh.SamplePosition(_rushTarget, out var hit, _sampleRadius, NavMesh.AllAreas))
        {
            _rushTarget = hit.position;
        }
        else
        {
            Debug.LogWarning("돌진 목표 지점을 NavMesh에서 찾지 못했습니다.");
        }

        _originalSpeed = _agent.speed;
        float rushSpeed = _rushDistance / _maxDuration;
        _movement.SetSpeedMultiplier(rushSpeed / _originalSpeed);

        _movement.MoveTo(_rushTarget);
        _movement.SetRotationToMoveDirection();
        
        if (_hitbox != null)
        {
            _hitbox.Enable(_damage);
        }
        else
        {
            Debug.LogWarning("EnemyKnockbackHitbox가 없습니다.");
        }

        Debug.Log("돌진 공격 시도");
    }

    public void Update()
    {
        if (_isFinished) return;

        _timer += Time.deltaTime;

        if (_agent.pathPending || _agent.isPathStale) return;

        if (_timer >= _maxDuration || (!_agent.pathPending && _agent.remainingDistance <= _arrived))
        {
            _isFinished = true;
        }
    }

    public void Exit()
    {
        _movement.Stop();
        
        if (_hitbox != null)
        {
            _hitbox.Disable();
        }

        _movement.ResetSpeedMultiplier();
    }
}