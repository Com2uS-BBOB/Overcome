using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("스탯")]
    public EnemyStatData EnemyStatData;

    private float _moveSpeed;
    private float _stopMagnitude = 0.01f;

    private bool _isMoving;
    private Vector3 _targetPosition;

    private void Start()
    {
        _moveSpeed = EnemyStatData.MoveSpeed;
    }

    public void MoveTo(Vector3 target)
    {
        _targetPosition = target;
        _isMoving = true;

        Move();
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

    private void Update()
    {
        if (_isMoving)
        {
            Move();
        }
    }

    public void Stop()
    {
        _isMoving = false;
    }
}
