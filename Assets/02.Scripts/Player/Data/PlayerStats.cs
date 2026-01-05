using System;
using UnityEngine;
using _02.Scripts.Player.Interfaces;

namespace _02.Scripts.Player.Data
{
    // 플레이어 스탯 (IDamageable 구현)
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] private PlayerBaseStats _baseStats;

        public float CurrentHp { get; private set; }
        public float MaxHp => _baseStats.MaxHp;
        public bool IsDead => CurrentHp <= 0;
        public bool IsAlive => CurrentHp > 0;
        public float HpRatio => MaxHp > 0 ? CurrentHp / MaxHp : 0f;

        // 기본 스탯
        public float AttackDamage => _baseStats.AttackDamage;
        public float MoveSpeed => _baseStats.MoveSpeed;
        public float JumpForce => _baseStats.JumpForce;
        public float DashDistance => _baseStats.DashDistance;
        public float DashCooldown => _baseStats.DashCooldown;
        public float AttackCooldown => _baseStats.AttackCooldown;

        // 크레센트 스탯
        public float CrescentDamage => _baseStats.CrescentDamage;
        public float CrescentSpeed => _baseStats.CrescentSpeed;
        public float CrescentRange => _baseStats.CrescentRange;

        public event Action<float, float> OnHpChanged;
        public event Action OnDeath;

        private void Awake()
        {
            if (_baseStats != null) CurrentHp = MaxHp;
        }

        public void TakeDamage(float damage, GameObject attacker = null)
        {
            if (IsDead) return;

            CurrentHp = Mathf.Max(0, CurrentHp - damage);
            OnHpChanged?.Invoke(CurrentHp, MaxHp);

            if (CurrentHp <= 0) OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);
            OnHpChanged?.Invoke(CurrentHp, MaxHp);
        }

        public void FullHeal()
        {
            CurrentHp = MaxHp;
            OnHpChanged?.Invoke(CurrentHp, MaxHp);
        }
    }
}