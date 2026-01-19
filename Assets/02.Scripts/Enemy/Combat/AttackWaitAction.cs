using UnityEngine;
using UnityEngine.AI;

public class AttackWaitAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly EnemySlotCoordinator _slotCoordinator;
    private readonly NavMeshAgent _agent;
    private readonly EnemyAnimatorController _anim;
    private readonly AttackWaitActionConfig _actionConfig;

    private float _timer;
    private float _duration;
    private bool _isFinished;

    private int _mySlotIndex = -1;

    private EEnemyProbeMode _probeMode;
    private float _modeTimer;
    private float _modeDuration;

    // 대기 모드 관련 설정
    private float _minModeTime = 0.4f;
    private float _maxModeTime = 1.0f;
    private float _shuffleAngle = 40f;     // 원호 이동 각도
    private float _feintDistance = 1.4f;
    private float _navSampleRadius = 1.5f;

    private float _shuffleModeValue = 0.40f;
    private float _feintModeValue = 0.70f;
    private float _repositionModeValue = 0.85f;

    private float _magnitudeThreshold = 0.01f;
    private float _randomValue = 0.5f;

    public bool IsFinished => _isFinished;
    public int SlotIndex => _mySlotIndex;

    public AttackWaitAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        EnemySlotCoordinator slotCoordinator,
        NavMeshAgent agent,
        EnemyAnimatorController anim,
        AttackWaitActionConfig config
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _slotCoordinator = slotCoordinator;
        _agent = agent;
        _anim = anim;
        _actionConfig = config;
    }

    public void Enter()
    {
        _isFinished = false;
        _timer = 0f;
        _duration = Random.Range(_actionConfig.MinWait, _actionConfig.MaxWait);

        // 슬롯 점유 (이미 슬롯이 있으면 그 슬롯 사용)
        _mySlotIndex = (_actionConfig.FixedSlotIndex >= 0)
            ? _actionConfig.FixedSlotIndex
            : _slotCoordinator.ClaimSlot(_enemy);

        if (_mySlotIndex < 0) return;

        _movement.SetSpeedMultiplier(_actionConfig.WaitSpeedMultiplier);

        _anim.SetWait(true);
        _agent.isStopped = false;
        _movement.SetRotationToLookAt(_player);

        PickNewMode();
    }

    public void Update()
    {
        if (_mySlotIndex < 0) return;

        // RushAction이 agent를 제어 중이면 AttackWaitAction은 대기
        if (!_agent.updatePosition)
        {
            return;
        }

        _modeTimer += Time.deltaTime;
        if (_modeTimer >= _modeDuration)
        {
            PickNewMode();
        }

        Vector3 destination = GetModeDestination();
        _agent.SetDestination(destination);

        // 슬롯 근처 또는 모드 목적지 근처면 대기 타이머 진행
        bool arrived = (!_agent.pathPending && _agent.hasPath && _agent.remainingDistance
            <= Mathf.Max(_agent.stoppingDistance, _actionConfig.MinStoppingDistance))
            || FlatDistance(_enemy.position, destination) <= _actionConfig.ArrivedThreshold;

        if (!arrived)
        {
            _movement.ResetSpeedMultiplier();
            _timer = 0f;
            return;
        }
        _timer += Time.deltaTime;

        if (_timer >= _duration)
        {
            _isFinished = true;
        }
    }

    private void PickNewMode()
    {
        _modeTimer = 0f;
        _modeDuration = Random.Range(_minModeTime, _maxModeTime);

        // Hold 빈도는 가중치로 조절 가능
        float r = Random.value;
        if (r < _shuffleModeValue) _probeMode = EEnemyProbeMode.Shuffle;
        else if (r < _feintModeValue) _probeMode = EEnemyProbeMode.Feint;
        else if (r < _repositionModeValue) _probeMode = EEnemyProbeMode.Reposition;
        else _probeMode = EEnemyProbeMode.Hold;
    }

    private Vector3 GetModeDestination()
    {
        Vector3 slotPosition = _slotCoordinator.GetSlotPosition(_mySlotIndex);

        Vector3 baseDirection = slotPosition - _player.position;
        baseDirection.y = 0f;

        if (baseDirection.sqrMagnitude < _magnitudeThreshold)
        {
            return slotPosition;
        }

        float desiredRadius = baseDirection.magnitude;
        Vector3 baseDirN = baseDirection / desiredRadius;

        switch (_probeMode)
        {
            case EEnemyProbeMode.Reposition:
            case EEnemyProbeMode.Hold:
                // 제자리 유지
                return slotPosition;

            case EEnemyProbeMode.Shuffle:
                {
                    // 플레이어 중심 원호 이동
                    float sign = (Random.value < _randomValue) ? -1f : 1f;
                    Quaternion rotation = Quaternion.Euler(0f, sign * _shuffleAngle, 0f);
                    Vector3 direction = rotation * baseDirN;

                    Vector3 raw = _player.position + direction * desiredRadius;

                    if (NavMesh.SamplePosition(raw, out var hit, _navSampleRadius, NavMesh.AllAreas))
                    {
                        return hit.position;
                    }

                    return slotPosition;
                }

            case EEnemyProbeMode.Feint:
                {
                    // slot 반지름 기준으로 살짝 안 혹은 밖으로
                    float sign = (Random.value < _randomValue) ? 1f : -1f;
                    float radius = Mathf.Max(0.5f, desiredRadius + sign * _feintDistance);

                    Vector3 raw = _player.position + baseDirN * radius;

                    if (NavMesh.SamplePosition(raw, out var hit, _navSampleRadius, NavMesh.AllAreas))
                    {
                        return hit.position;
                    }

                    return slotPosition;
                }
        }

        return slotPosition;
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    public void Exit()
    {
        _movement.ResetSpeedMultiplier();

        // NavMeshAgent 경로 정리 - 이게 없으면 이전 목적지로 계속 가려고 함
        if (_agent != null && _agent.enabled)
        {
            _agent.ResetPath();
            _agent.isStopped = true;
        }

        _anim.SetWait(false);

        if (_actionConfig.ReleaseSlotOnExit)
        {
            ReleaseSlotNow();
        }
    }

    public void ReleaseSlotNow()
    {
        if (_mySlotIndex >= 0)
        {
            _slotCoordinator.ReleaseSlot(_mySlotIndex);
            _mySlotIndex = -1;
        }
    }
}