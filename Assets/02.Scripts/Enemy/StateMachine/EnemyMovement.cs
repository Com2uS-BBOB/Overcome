using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    [Header("플레이어와의 간격")]
    [SerializeField] private float _maxDistance = 1.4f;
    [SerializeField] private float _minDistance = 0.001f;

    [Header("바라보는 방향")]
    private ERotationMode _rotationMode = ERotationMode.MoveDirection;
    private Transform _lookTarget;

    private float _moveSpeed;
    private float _stopMagnitude = 0.01f;

    [Header("속도 배율 관련")]
    [SerializeField] private float _speedLerpSpeed = 6f;
    private float _speedMultiplier = 1f;
    private float _targetSpeedMultiplier = 1f;

    private bool _isMoving;
    private bool _usePlayerSeparation;

    private Rigidbody _rigidbody;

    private Vector3 _targetPosition;
    private Transform _separationTarget;

    [Header("적 사이의 간격")]
    [SerializeField] private float _enemySeparationRadius = 0.8f;
    [SerializeField] private float _enemySeparationStrength = 1f;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _moveSpeed = EnemyStatData.MoveSpeed;
    }

    private void Update()
    {
        _speedMultiplier = Mathf.Lerp(_speedMultiplier,_targetSpeedMultiplier,Time.deltaTime * _speedLerpSpeed);

        if (_isMoving)
        {
            Move();
        }
    }

    private void LateUpdate()
    {
        if (_usePlayerSeparation)
        {
            ApplyPlayerSeparation();
        }

        ApplyEnemySeparation();
    }

    // 목표 지점으로 이동 시작
    public void MoveTo(Vector3 target)
    {
        _targetPosition = target;
        _isMoving = true;
    }

    private void Move()
    {
        if (!_isMoving) return;

        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < _stopMagnitude)
        {
            Stop();
            return;
        }

        Vector3 moveDirection = direction.normalized;
        transform.position += moveDirection * (_moveSpeed * _speedMultiplier) * Time.deltaTime;

        // 회전 설정
        ApplyRotation(moveDirection);
    }

    public void Stop()
    {
        _isMoving = false;
    }

    private void ApplyRotation(Vector3 moveDirection)
    {
        switch (_rotationMode)
        {
            case ERotationMode.MoveDirection:
                if (moveDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(moveDirection);
                }
                break;

            case ERotationMode.LookAtTarget:
                if (_lookTarget == null) return;

                Vector3 lookDirection = _lookTarget.position - transform.position;
                lookDirection.y = 0f;

                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection.normalized);
                }
                break;
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

    // 이동 속도 배율 설정
    public void SetSpeedMultiplier(float multiplier)
    {
        _targetSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetSpeedMultiplier()
    {
        _targetSpeedMultiplier = 1f;
    }

    // 목표 지점에 도착했는지 확인
    public bool IsArrived(Vector3 target, float stopDistance)
    {
        Vector3 difference = target - transform.position;
        difference.y = 0f;
        return difference.sqrMagnitude <= stopDistance * stopDistance;
    }

    // 플레이어와 일정 거리 유지 활성화
    public void EnablePlayerSeparation(Transform target)
    {
        _separationTarget = target;
        _usePlayerSeparation = true;
    }

    public void DisablePlayerSeparation()
    {
        _usePlayerSeparation = false;
        _separationTarget = null;
    }

    // 플레이어와 일정 거리 유지
    private void ApplyPlayerSeparation()
    {
        if (_separationTarget == null) return;

        Vector3 difference = transform.position - _separationTarget.position;
        difference.y = 0f;

        float distance = difference.magnitude;
        if (distance < _maxDistance && distance > _minDistance)
        {
            float push = (_maxDistance - distance);
            push = Mathf.Min(push, 0.5f);

            transform.position += difference.normalized * push * Time.deltaTime;
        }
    }

    // 적들 사이의 간격 유지
    private void ApplyEnemySeparation()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            _enemySeparationRadius
        );

        Vector3 pushDirection = Vector3.zero;
        int count = 0;

        foreach (var collider in colliders)
        {
            if (collider.transform == transform) continue;

            // 적인지 판별
            if (!collider.TryGetComponent<EnemyBase>(out var enemy)) continue;

            Vector3 diff = transform.position - collider.transform.position;
            diff.y = 0f;

            float dist = diff.magnitude;
            if (dist <= 0.001f) continue;

            pushDirection += diff.normalized / dist;
            count++;
        }

        if (count > 0)
        {
            pushDirection /= count;
            transform.position += pushDirection * _enemySeparationStrength * Time.deltaTime;
        }
    }
}
