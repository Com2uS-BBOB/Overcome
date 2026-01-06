using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    [Header("플레이어와의 간격")]
    [SerializeField] private float _maxDistance = 1.4f;
    [SerializeField] private float _minDistance = 0.001f;

    private float _moveSpeed;
    private float _stopMagnitude = 0.01f;

    private bool _isMoving;
    private bool _usePlayerSeparation;

    private Rigidbody _rigidbody;

    private Vector3 _targetPosition;
    private Transform _separationTarget;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _moveSpeed = EnemyStatData.MoveSpeed;
    }

    private void Update()
    {
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
        transform.position += moveDirection * _moveSpeed * Time.deltaTime;

        // 회전 설정
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    public void Stop()
    {
        _isMoving = false;
    }

    // 목표 지점에 도착했는지 확인
    public bool IsArrived(Vector3 target, float stopDistance)
    {
        Vector3 different = target - transform.position;
        different.y = 0f;
        return different.sqrMagnitude <= stopDistance * stopDistance;
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

        Vector3 different = transform.position - _separationTarget.position;
        different.y = 0f;

        float distance = different.magnitude;
        if (distance < _maxDistance && distance > _minDistance)
        {
            transform.position += different.normalized * (_maxDistance - distance);
        }
    }
}
