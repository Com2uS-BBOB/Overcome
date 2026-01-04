using System;
using UnityEngine;

namespace _02.Scripts.Player.Data
{
    /// <summary>
    /// 플레이어 런타임 스탯 관리
    /// BaseStats를 기반으로 실제 게임 내 스탯 처리
    /// </summary>
    public class PlayerRuntimeStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private PlayerBaseStats _baseStats;

        // 현재 HP
        public float CurrentHp { get; private set; }

        // 스탯 프로퍼티 (BaseStats에서 가져옴)
        public float MaxHp => _baseStats.maxHp;
        public float AttackDamage => _baseStats.attackDamage;
        public float Defense => _baseStats.defense;
        public float MoveSpeed => _baseStats.moveSpeed;
        public float JumpForce => _baseStats.jumpForce;
        public float DashDistance => _baseStats.dashDistance;
        public float DashCooldown => _baseStats.dashCooldown;
        public float AttackCooldown => _baseStats.attackCooldown;

        // 이벤트
        public event Action<float, float> OnHpChanged; // current, max
        public event Action OnDeath;

        private void Awake()
        {
            if (_baseStats == null)
            {
                Debug.LogError("[PlayerRuntimeStats] BaseStats가 할당되지 않았습니다!");
                return;
            }

            CurrentHp = MaxHp;
        }

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(float damage)
        {
            // 방어력 적용
            float actualDamage = Mathf.Max(0, damage - Defense);
            CurrentHp = Mathf.Max(0, CurrentHp - actualDamage);

            Debug.Log($"[Stats] 데미지: {actualDamage} (원본: {damage}, 방어: {Defense}) → HP: {CurrentHp}/{MaxHp}");

            OnHpChanged?.Invoke(CurrentHp, MaxHp);

            if (CurrentHp <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 회복
        /// </summary>
        public void Heal(float amount)
        {
            float previousHp = CurrentHp;
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + amount);

            float actualHeal = CurrentHp - previousHp;
            Debug.Log($"[Stats] 회복: {actualHeal} → HP: {CurrentHp}/{MaxHp}");

            OnHpChanged?.Invoke(CurrentHp, MaxHp);
        }

        /// <summary>
        /// HP 전체 회복
        /// </summary>
        public void FullHeal()
        {
            CurrentHp = MaxHp;
            OnHpChanged?.Invoke(CurrentHp, MaxHp);
            Debug.Log($"[Stats] 전체 회복 → HP: {CurrentHp}/{MaxHp}");
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        private void Die()
        {
            Debug.Log("[Stats] 플레이어 사망!");
            OnDeath?.Invoke();
        }

        /// <summary>
        /// HP 비율 (0~1)
        /// </summary>
        public float HpRatio => MaxHp > 0 ? CurrentHp / MaxHp : 0f;

        /// <summary>
        /// 생존 여부
        /// </summary>
        public bool IsAlive => CurrentHp > 0;
    }
}