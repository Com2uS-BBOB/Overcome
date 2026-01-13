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

    public void Initialize()
    {
        _enemyStatData = _enemy.EnemyStatData;
        _agent.speed = _enemyStatData.MoveSpeed;
    }

    private void Update()
    {
        if (_movementLocked) return;

        UpdateSpeed();
        UpdateRotation();
        _anim?.SetMove(IsMoving);
    }

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