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

    [Header("적 피격 넉백 옵션")]
    [SerializeField] private bool _allowKnockbackWhileLocked = true;
    [SerializeField] private float _knockbackDamping = 12f;   // 클수록 빨리 멈춤
    [SerializeField] private float _maxKnockbackSpeed = 3.2f;   // 넉백 최대치
    private Vector3 _knockbackVelocity;

    private bool _movementLocked;

    private NavMeshAgent _agent;
    private EnemyAnimatorController _anim;

    private float _currentSpeedMultiplier = 1f;
    private float _targetSpeedMultiplier = 1f;

    public bool IsMoving => _agent.enabled && _agent.velocity.sqrMagnitude > 0.01f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemy = GetComponent<EnemyBase>();
        _anim = GetComponent<EnemyAnimatorController>();

        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void OnEnable()
    {
        // 풀링 재스폰 대비 넉백 초기화
        _knockbackVelocity = Vector3.zero;
    }

    private void OnDisable()
    {
        _knockbackVelocity = Vector3.zero;
    }

    public void Initialize()
    {
        _enemyStatData = _enemy.EnemyStatData;
        _agent.speed = _enemyStatData.MoveSpeed;
    }

    private void Update()
    {
        // 경직 중에도 밀리게 넉백 먼저 처리
        ApplyKnockback();

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

        // 누적 (연속 히트 시 "경직되듯 조금씩 밀림")
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

        // agent.Move는 isStopped 상태에서도 위치를 움직일 수 있음 (경직 느낌 살리기)
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

    #region Movement

    public void LockMovement(bool locked)
    {
        _movementLocked = locked;

        if (_agent != null && _agent.enabled)
        {
            if (locked)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                _agent.velocity = Vector3.zero;
            }
            else
            {
                _agent.isStopped = false;
            }
        }

        // 애니도 확실히 Move=false로 고정
        _anim?.SetMove(false);
    }

    public void MoveTo(Vector3 target)
    {
        if (_movementLocked) return;
        if (!_agent.enabled) return;

        _agent.isStopped = false;
        _agent.SetDestination(target);
    }

    public void Stop()
    {
        if (!_agent.enabled) return;

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
            transform.rotation = Quaternion.LookRotation(direction.normalized);
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