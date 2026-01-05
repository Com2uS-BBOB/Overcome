using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("공격 옵션")]
    [SerializeField] private float _attackDamage = 4f;
    [SerializeField] private float _attackCooldown = 4f;

    private float _cooldownTimer;
    private bool _isAttacking;

    private void Update()
    {
        if (!_isAttacking) return;

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    public void TryAttack(IDamageable target)
    {
        if (_cooldownTimer > 0f) return;

        _cooldownTimer = _attackCooldown;

        Debug.Log("적 공격!");
        target.TakeDamage(_attackDamage, gameObject);
    }

    public void StartAttack()
    {
        _isAttacking = true;
    }

    public void Stop()
    {
        _isAttacking = false;
    }
}
