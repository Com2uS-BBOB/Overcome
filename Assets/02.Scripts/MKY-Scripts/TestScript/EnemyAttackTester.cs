using System;
using UnityEngine;

public class EnemyAttackTester : MonoBehaviour, IDamageable
{
    [Header("테스트용 체력")]
    [SerializeField] private float _maxHp = 60f;

    public float CurrentHp { get; private set; }
    public float MaxHp => _maxHp;
    public bool IsDead => CurrentHp <= 0f;

    public event Action<float, float> OnHpChanged;
    public event Action OnDeath;

    private void Awake()
    {
        CurrentHp = _maxHp;
    }

    public void TakeDamage(float damage, GameObject attacker = null)
    {
        if (IsDead) return;

        CurrentHp -= damage;
        CurrentHp = Mathf.Max(CurrentHp, 0f);

        Debug.Log(
            $"[PlayerDummy] 피격! Damage: {damage}, " +
            $"HP: {CurrentHp}/{MaxHp}, " +
            $"Attacker: {(attacker != null ? attacker.name : "None")}"
        );

        OnHpChanged?.Invoke(CurrentHp, MaxHp);

        if (CurrentHp <= 0f)
        {
            Debug.Log("[PlayerDummy] 사망");
            OnDeath?.Invoke();
        }
    }
}