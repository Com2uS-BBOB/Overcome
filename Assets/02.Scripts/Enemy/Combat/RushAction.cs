using _02.Scripts.Player.Data;
using UnityEngine;
using UnityEngine.AI;

public class RushAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly EnemyKnockbackHitbox _hitbox;
    private readonly NavMeshAgent _agent;
    private readonly EnemyAnimatorController _anim;

    private readonly float _maxDuration;  // 속도 계산용
    private readonly float _damage;
    private readonly float _knockbackDistance;
    private readonly KnockbackLevel _knockbackLevel;

    private bool _isFinished;

    private float _targetDistance;
    private Vector3 _rushDirection;  // Enter 순간에 고정되는 방향
    private float _speed;            // distance / duration

    // 실제 이동거리 누적
    private float _traveled;
    private Vector3 _lastPosition;

    // 비상 탈출용 타임아웃
    private float _timeout;
    private float _timer;
    private float _timeoutRatio = 1.4f;
    private float _startTime = 0.2f;
    private float _startTimer;

    private float _sqrMagnitudeThreshold = 0.01f;

    private bool _savedAgentUpdatePosition;
    private bool _savedAgentUpdateRotation;

    private bool _stopOnPlayerHit = true;        // 플레이어 충돌 시 즉시 정지
    private bool _stopShortOnPlayerHit = false;  // 충돌 지점보다 살짝 앞에서 정지
    private float _stopOffset = 0.5f;            // 앞에서 정지 시 플레이어와의 거리

    private float _castRadiusPadding = 0.08f;
    private float _playerHitRadius = 0.3f;

    private int _playerLayerMask;

    public bool IsFinished => _isFinished;

    public RushAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        EnemyKnockbackHitbox hitbox,
        NavMeshAgent agent,
        EnemyAnimatorController anim,
        float maxDuration,
        float damage,
        float knockbackDistance = 2f,
        KnockbackLevel knockbackLevel = KnockbackLevel.Heavy
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _hitbox = hitbox;
        _agent = agent;
        _anim = anim;
        _maxDuration = maxDuration;
        _damage = damage;
        _knockbackDistance = knockbackDistance;
        _knockbackLevel = knockbackLevel;
        _playerLayerMask = ~0;
    }

    public void Enter()
    {
        _isFinished = false;

        _traveled = 0f;
        _timer = 0f;
        _lastPosition = _enemy.position;

        Vector3 snapPlayerPosition = _player != null ? _player.position : (_enemy.position + _enemy.forward);
        Vector3 delta = snapPlayerPosition - _enemy.position;
        delta.y = 0f;

        if (delta.sqrMagnitude < _sqrMagnitudeThreshold)
        {
            _isFinished = true;
            return;
        }

        float planarDistanceToPlayer = delta.magnitude;
        _rushDirection = delta / planarDistanceToPlayer;

        // 여기서 이번 러시 목표거리 결정
        _targetDistance = planarDistanceToPlayer;

        if (_stopShortOnPlayerHit)
        {
            _targetDistance = Mathf.Max(0f, _targetDistance - _stopOffset);
        }

        // 목표 거리가 너무 짧으면 종료
        if (_targetDistance <= _sqrMagnitudeThreshold)
        {
            _isFinished = true;
            return;
        }

        _speed = _targetDistance / _maxDuration;

        // 비상 탈출용 타임아웃 계산
        _timeout = _maxDuration * _timeoutRatio;

        _agent.ResetPath();
        _agent.isStopped = true;

        _savedAgentUpdatePosition = _agent.updatePosition;
        _savedAgentUpdateRotation = _agent.updateRotation;

        _agent.updatePosition = false;
        _agent.updateRotation = false;

        // 시작한 순간 방향 고정
        _enemy.rotation = Quaternion.LookRotation(_rushDirection);

        _hitbox?.Enable(_damage, _knockbackDistance);

        _anim.TryPlayRush();
#if UNITY_EDITOR
        Debug.Log("돌진 시작");
#endif
    }

    public void Update()
    {
        if (_isFinished) return;

        _startTimer+= Time.deltaTime;

        if (_startTimer < _startTime) return;
        _timer += Time.deltaTime;

        float stepDistance = _speed * Time.deltaTime;
        if (stepDistance <= 0f) return;

        float remaining = _targetDistance - _traveled;
        if (remaining <= 0f)
        {
            _isFinished = true;
            return;
        }
        stepDistance = Mathf.Min(stepDistance, remaining);

        // 이번 프레임 이동 경로에 플레이어가 있는지 먼저 검사
        if (_player != null && (_stopOnPlayerHit || _stopShortOnPlayerHit))
        {
            if (TryGetPlayerHit(stepDistance, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (_stopShortOnPlayerHit)
                    {
                        float moveDistance = Mathf.Max(0f, hit.distance - _stopOffset);
                        moveDistance = Mathf.Min(moveDistance, remaining);

                        if (moveDistance > 0f)
                            MoveBy(moveDistance);

#if UNITY_EDITOR
                        Debug.Log("돌진 종료: 플레이어 앞에서 정지");
#endif
                        _isFinished = true;
                        return;
                    }

                    if (_stopOnPlayerHit)
                    {
#if UNITY_EDITOR
                        Debug.Log("돌진 종료: 플레이어 충돌로 정지");
#endif
                        _isFinished = true;
                        return;
                    }
                }
            }
        }

        // 충돌 없으면 기존대로 이동
        MoveBy(stepDistance);

        // 거리 기반 종료
        if (_traveled >= _targetDistance - _sqrMagnitudeThreshold)
        {
#if UNITY_EDITOR
            Debug.Log("돌진 종료: 거리 달성");
#endif
            _isFinished = true;
            return;
        }

        // 타임아웃 종료
        if (_timer >= _timeout)
        {
#if UNITY_EDITOR
            Debug.Log("돌진 종료: 타임아웃");
#endif
            _isFinished = true;
        }
    }

    private void MoveBy(float distance)
    {
        Vector3 step = _rushDirection * distance;

        _agent.Move(step);
        _enemy.position = _agent.nextPosition;

        // 이동거리 누적(평면 기준)
        Vector3 now = _enemy.position;
        Vector3 a = new Vector3(_lastPosition.x, 0, _lastPosition.z);
        Vector3 b = new Vector3(now.x, 0, now.z);

        _traveled += Vector3.Distance(a, b);
        _lastPosition = now;
    }

    private bool TryGetPlayerHit(float stepDistance, out RaycastHit hit)
    {
        hit = default;

        float radius = (_agent != null ? _agent.radius : _playerHitRadius) + _castRadiusPadding;

        Vector3 origin = _enemy.position;

        return Physics.SphereCast(
            origin,
            radius,
            _rushDirection,
            out hit,
            stepDistance,
            _playerLayerMask,
            QueryTriggerInteraction.Ignore
        );
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