using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("스탯")]
    private EnemyStatData _enemyStatData;
    private EnemyBase _enemy;

    [Header("회전 옵션")]
    [SerializeField] private ERotationMode _rotationMode = ERotationMode.MoveDirection;
    private Transform _lookTarget;

    [Header("스피드 보간")]
    [SerializeField] private float _speedLerpSpeed = 6f;

    [Header("회전 보간")]
    [SerializeField] private float _rotationLerpSpeed = 14f;

    [Header("적 피격 넉백 옵션")]
    [SerializeField] private bool _allowKnockbackWhileLocked = true;
    [SerializeField] private float _knockbackDamping = 16f;    // 클수록 빨리 멈춤
    [SerializeField] private float _maxKnockbackSpeed = 12f;  // 넉백 최대치
    private Vector3 _knockbackVelocity;

    private int _moveLockCount = 0;     // 여러 군데에서 락 걸 때 대비 안전 장치
    private float _lockUntilTime = 0f;  // 최소 락 유지 시간 (경로 복귀 지연용)
    private bool _movementLocked;

    private NavMeshAgent _agent;
    private EnemyAnimatorController _anim;

    private float _currentSpeedMultiplier = 1f;
    private float _targetSpeedMultiplier = 1f;

    public bool IsMoving => _agent != null && _agent.enabled && _agent.velocity.sqrMagnitude > 0.01f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemy = GetComponent<EnemyBase>();
        _anim = GetComponent<EnemyAnimatorController>();

        if (_agent != null)
        {
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }
    }

    private void OnEnable()
    {
        _knockbackVelocity = Vector3.zero;

        // 풀링 대비 락이 남아있을 가능성 제거
        ForceUnlockAll();
    }

    private void OnDisable()
    {
        _knockbackVelocity = Vector3.zero;
    }

    public void Initialize()
    {
        _enemyStatData = _enemy.EnemyStatData;
        if (_agent != null) _agent.speed = _enemyStatData.MoveSpeed;
    }

    private void Update()
    {
        // 경직 중에도 밀리게 넉백 먼저 처리
        ApplyKnockback();

        // 락 시간 지나면 자동으로 해제
        if (_movementLocked && _moveLockCount == 0 && Time.time >= _lockUntilTime)
        {
            InternalUnlock();
        }

        // 이동 Lock 상태면 목적지 이동 / 회전 / 애니 멈추기
        if (_movementLocked)
        {
            _anim?.SetMove(false);
            return;
        }

        UpdateSpeed();
        UpdateRotation();
        _anim?.SetMove(IsMoving);
    }

    #region Knockback

    public void AddKnockback(Vector3 direction, float strength)
    {
        if (_agent == null || !_agent.enabled) return;

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        // 누적 (연속 히트 시 경직되듯 조금씩 밀림)
        _knockbackVelocity += direction.normalized * strength;

        // 과도한 누적 방지
        float magnitude = _knockbackVelocity.magnitude;
        if (magnitude > _maxKnockbackSpeed)
        {
            _knockbackVelocity = _knockbackVelocity / magnitude * _maxKnockbackSpeed;
        }
    }

    private void ApplyKnockback()
    {
        if (_agent == null || !_agent.enabled) return;
        if (_knockbackVelocity.sqrMagnitude < 0.000001f) return;

        if (_movementLocked && !_allowKnockbackWhileLocked) return;

        // 경직 느낌 살리기
        Vector3 delta = _knockbackVelocity * Time.deltaTime;
        _agent.Move(delta);

        // 점점 느려짐
        _knockbackVelocity = Vector3.Lerp(
            _knockbackVelocity,
            Vector3.zero,
            Time.deltaTime * _knockbackDamping
        );
    }

    #endregion

    #region Movement Lock

    public void LockMovement(bool locked, float minLockTime = 0f)
    {
        if (locked)
        {
            _moveLockCount++;
            _movementLocked = true;

            if (minLockTime > 0f)
            {
                _lockUntilTime = Mathf.Max(_lockUntilTime, Time.time + minLockTime);
            }

            if (_agent != null && _agent.enabled)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }

            _anim?.SetMove(false);
            return;
        }

        // unlock
        _moveLockCount = Mathf.Max(0, _moveLockCount - 1);

        // 다른 곳에 락 걸려있으면 유지
        if (_moveLockCount > 0) return;

        // 최소 락 시간 남아있으면 Update에서 자동 해제되도록 유지
        if (Time.time < _lockUntilTime) return;

        _movementLocked = false;

        InternalUnlock();
    }

    private void InternalUnlock()
    {
        _movementLocked = false;

        if (_agent != null && _agent.enabled)
        {
            _agent.isStopped = false;
        }

        _anim?.SetMove(false);
    }

    // 풀링 / 리스폰 대비 락 상태 강제 초기화
    public void ForceUnlockAll()
    {
        _moveLockCount = 0;
        _lockUntilTime = 0f;
        _movementLocked = false;

        if (_agent != null && _agent.enabled)
        {
            _agent.isStopped = false;
        }
    }

    #endregion

    #region Movement Commands

    public void MoveTo(Vector3 target)
    {
        if (_movementLocked) return;
        if (_agent == null || !_agent.enabled) return;

        _agent.isStopped = false;
        _agent.SetDestination(target);
    }

    public void Stop()
    {
        if (_agent == null || !_agent.enabled) return;

        _agent.isStopped = true;
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

        _anim?.SetMove(false);
    }

    public bool IsArrived(float stopDistance)
    {
        if (!_agent.enabled) return false;
        
        // 경로 계산 중이면 아직 도착하지 않음
        if (_agent.pathPending) return false;
        
        // 경로가 없으면 도착하지 않음 (경로를 찾을 수 없거나 아직 설정 안됨)
        if (!_agent.hasPath) return false;

        return _agent.remainingDistance <= stopDistance;
    }

    #endregion

    #region Rotation

    private void UpdateRotation()
    {
        Vector3 direction = Vector3.zero;

        switch (_rotationMode)
        {
            case ERotationMode.MoveDirection:
                if (_agent.velocity.sqrMagnitude > 0.01f)
                {
                    direction = _agent.velocity;
                }
                break;

            case ERotationMode.LookAtTarget:
                if (_lookTarget != null)
                {
                    direction = _lookTarget.position - transform.position;
                }
                break;
        }

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // 부드럽게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationLerpSpeed);
        }
    }

    public void SetRotationToMoveDirection()
    {
        _rotationMode = ERotationMode.MoveDirection;
        _lookTarget = null;
    }

    public void SetRotationToLookAt(Transform target)
    {
        _rotationMode = ERotationMode.LookAtTarget;
        _lookTarget = target;
    }

    #endregion

    #region Speed Control

    private void UpdateSpeed()
    {
        if (_enemyStatData == null) return;

        _currentSpeedMultiplier = Mathf.Lerp(_currentSpeedMultiplier, _targetSpeedMultiplier, Time.deltaTime * _speedLerpSpeed);

        _agent.speed = _enemyStatData.MoveSpeed * _currentSpeedMultiplier;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _targetSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        _targetSpeedMultiplier = 1f;
    }

    #endregion

}