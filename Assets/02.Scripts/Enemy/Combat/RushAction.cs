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
    private readonly float _maxDuration;  // 속도 계산용
    private readonly float _damage;

    private bool _isFinished;

    private Vector3 _rushDirection;  // Enter 순간에 고정되는 방향
    private float _speed;            // distance / duration

    // 실제 이동거리 누적
    private float _traveled;
    private Vector3 _lastPosition;

    private float _sqrMagnitudeThreshold = 0.01f;

    private bool _savedAgentUpdatePosition;
    private bool _savedAgentUpdateRotation;

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

        Vector3 snapPlayerPosition = _player != null ? _player.position : (_enemy.position + _enemy.forward);
        Vector3 direction = snapPlayerPosition - _enemy.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < _sqrMagnitudeThreshold || _rushDistance <= _sqrMagnitudeThreshold)
        {
            _isFinished = true;
            return;
        }

        _rushDirection = direction.normalized;
        _speed = _rushDistance / _maxDuration;

        _traveled = 0f;

        _agent.ResetPath();
        _agent.isStopped = true;

        _savedAgentUpdatePosition = _agent.updatePosition;
        _savedAgentUpdateRotation = _agent.updateRotation;

        _agent.updatePosition = false;
        _agent.updateRotation = false;

        // 시작한 순간 방향 고정
        _enemy.rotation = Quaternion.LookRotation(_rushDirection);

        _lastPosition = _enemy.position;

        _hitbox?.Enable(_damage);

#if UNITY_EDITOR
        Debug.Log("돌진 공격 시도");
#endif
    }

    public void Update()
    {
        if (_isFinished) return;

        Vector3 step = _rushDirection * (_speed * Time.deltaTime);
        _agent.Move(step);

        _enemy.position = _agent.nextPosition;

        // 실제 이동 거리 누적(평면 기준)
        Vector3 now = _enemy.position;
        Vector3 a = new Vector3(_lastPosition.x, 0, _lastPosition.z);
        Vector3 b = new Vector3(now.x, 0, now.z);

        _traveled += Vector3.Distance(a, b);
        _lastPosition = now;

        // 거리 기반으로 종료
        if (_traveled >= _rushDistance)
        {
            _isFinished = true;
        }
    }

    public void Exit()
    {
        if (_hitbox != null)
        {
            _hitbox.Disable();
        }

        // agent 원상복구
        _agent.updatePosition = _savedAgentUpdatePosition;
        _agent.updateRotation = _savedAgentUpdateRotation;

        _agent.isStopped = false;

        // Transform과 agent 위치 싱크 확정 및 이전 경로 제거
        _agent.Warp(_enemy.position);
        _agent.ResetPath();

        _movement.ResetSpeedMultiplier();
    }
}