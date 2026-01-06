using UnityEngine;
using _02.Scripts.Player.Interfaces;

public class EnemyAttack : MonoBehaviour
{
    [Header("스탯")]
    private EnemyBase _enemy;

    public float Damage => _enemy.EnemyStatData.Damage;

    [Header("공격 옵션")]
    [SerializeField] private float _attackCooldown = 4f;

    private float _cooldownTimer;
    private bool _isAttacking;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
    }

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
        if (target == null) return;
        if (_enemy == null || _enemy.EnemyStatData == null)
        {
            Debug.LogError("EnemyStatData가 설정되지 않았습니다.");
            return;
        }

        if (_cooldownTimer > 0f) return;

        _cooldownTimer = _attackCooldown;

        Debug.Log("적 공격!");
        target.TakeDamage(Damage, gameObject);  // 추후 점수 시스템에 연결 (일단 위치만)
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
