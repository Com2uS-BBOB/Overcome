using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    [Header("플레이어와의 간격")]
    [SerializeField] private float _maxDistance = 1.2f;
    [SerializeField] private float _minDistance = 0.001f;

    private float _moveSpeed;
    private float _stopMagnitude = 0.01f;

    private bool _isMoving;
    private Vector3 _targetPosition;

    private void Start()
    {
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
        ApplyPlayerSeparation();
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

    // 플레이어와 일정 거리 유지
    private void ApplyPlayerSeparation()
    {
        if (_targetPosition == null) return;

        Vector3 different = transform.position - _targetPosition;
        different.y = 0f;

        float distance = different.magnitude;
        if (distance < _maxDistance && distance > _minDistance)
        {
            Vector3 direction = different.normalized;
            float pushAmount = (_maxDistance - distance);

            transform.position += direction * pushAmount;
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
}
