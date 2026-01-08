using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("스탯")]
    [SerializeField] private EnemyStatData _enemyStatData;

    [Header("회전 옵션")]
    [SerializeField] private ERotationMode _rotationMode = ERotationMode.MoveDirection;
    private Transform _lookTarget;

    [Header("스피드 보간")]
    [SerializeField] private float _speedLerpSpeed = 6f;

    private NavMeshAgent _agent;

    private float _currentSpeedMultiplier = 1f;
    private float _targetSpeedMultiplier = 1f;

    public bool IsMoving => _agent.enabled && _agent.velocity.sqrMagnitude > 0.01f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        _agent.speed = _enemyStatData.MoveSpeed;
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void Update()
    {
        UpdateSpeed();
        UpdateRotation();
    }

    #region Movement

    public void MoveTo(Vector3 target)
    {
        if (!_agent.enabled) return;

        _agent.isStopped = false;
        _agent.SetDestination(target);
    }

    public void Stop()
    {
        if (!_agent.enabled) return;

        _agent.isStopped = true;
        _agent.ResetPath();
    }

    public bool IsArrived(float stopDistance)
    {
        if (!_agent.enabled || !_agent.hasPath) return true;

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
        _currentSpeedMultiplier = Mathf.Lerp(
            _currentSpeedMultiplier,
            _targetSpeedMultiplier,
            Time.deltaTime * _speedLerpSpeed
        );

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