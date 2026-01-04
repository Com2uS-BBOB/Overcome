using System;
using UnityEngine;
using _02.Scripts.Interfaces;

namespace _02.Scripts.Player.Data
{
    // 플레이어 런타임 스탯 (IDamageable 구현)
    public class PlayerRuntimeStats : MonoBehaviour, IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] private PlayerBaseStats _baseStats;

        public float CurrentHp { get; private set; }
        public float MaxHp => _baseStats.maxHp;
        public bool IsDead => CurrentHp <= 0;
        public bool IsAlive => CurrentHp > 0;
        public float HpRatio => MaxHp > 0 ? CurrentHp / MaxHp : 0f;

        // 기본 스탯
        public float AttackDamage => _baseStats.attackDamage;
        public float Defense => _baseStats.defense;
        public float MoveSpeed => _baseStats.moveSpeed;
        public float JumpForce => _baseStats.jumpForce;
        public float DashDistance => _baseStats.dashDistance;
        public float DashCooldown => _baseStats.dashCooldown;
        public float AttackCooldown => _baseStats.attackCooldown;

        // 크레센트 스탯
        public float CrescentDamage => _baseStats.crescentDamage;
        public float CrescentSpeed => _baseStats.crescentSpeed;
        public float CrescentRange => _baseStats.crescentRange;
        public float CrescentCooldown => _baseStats.crescentCooldown;

        public event Action<float, float> OnHpChanged;
        public event Action OnDeath;

        private void Awake()
        {
            if (_baseStats != null) CurrentHp = MaxHp;
        }

        public void TakeDamage(float damage, GameObject attacker = null)
        {
            if (IsDead) return;

            float actualDamage = Mathf.Max(0, damage - Defense);
            CurrentHp = Mathf.Max(0, CurrentHp - actualDamage);
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