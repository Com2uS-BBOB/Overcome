using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("공격 옵션")]  // 임의의 값, 필요시 EnemyStatData로 이동
    [SerializeField] private float _attackDamage = 4f;
    [SerializeField] private float _attackRange = 6f;
    [SerializeField] private float _attackCooldown = 4f;

    private bool _isAttack;
    private Vector3 _targetPosition;

    public void TryAttack(Vector3 target)
    {
        _targetPosition = target;
        _isAttack = true;

        Attack();
    }

    private void Attack()
    {
        if (!_isAttack) return;

        // 플레이어 공격
    }

    public void Stop()
    {
        _isAttack = false;
    }
}
