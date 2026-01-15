using System;
using UnityEngine;
using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Interfaces;

namespace _02.Scripts.Player.Data
{
    // 플레이어 스탯 (IDamageable 구현)
    public class PlayerStats : MonoBehaviour, IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] private PlayerBaseStats _baseStats;

        [Header("Guard System")]
        [SerializeField] private GuardManager _guardManager;

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

        // 피격/가드 이벤트
        public event Action<AttackInfo> OnHit;
        public event Action<AttackInfo> OnJustGuardSuccess;
        public event Action<AttackInfo> OnNormalGuardSuccess;
        public event Action<AttackInfo> OnGuardBreak;

        private void Awake()
        {
            if (_baseStats != null) CurrentHp = MaxHp;
        }

        /// <summary>
        /// 피해 처리 (가드 시스템 연동)
        /// </summary>
        public void TakeDamage(AttackInfo attackInfo)
        {
            if (IsDead) return;

            // 가드 판정
            GuardResult guardResult = GuardResult.None;
            if (_guardManager != null)
            {
                guardResult = _guardManager.TryBlock(attackInfo, transform);
            }

            // 결과에 따른 처리
            switch (guardResult)
            {
                case GuardResult.JustGuard:
                    // 저스트 가드 - 데미지/피격 없음, VFX/SFX만
                    OnJustGuardSuccess?.Invoke(attackInfo);
                    return;

                case GuardResult.NormalBlock:
                    // 일반 가드 - 데미지/피격 없음
                    OnNormalGuardSuccess?.Invoke(attackInfo);
                    return;

                case GuardResult.GuardBreak:
                    // 가드 브레이크 - 별도 상태로 전환
                    OnGuardBreak?.Invoke(attackInfo);
                    return;

                case GuardResult.RearAttack:
                    // 후방 공격 - 가드 실패, 피격 처리
                    break;

                default:
                    // 가드 없음 - 피격 처리
                    break;
            }

            // 피격 처리
            TimeSystem.Instance.SubtractTimeLimit(attackInfo.Damage);
            OnHit?.Invoke(attackInfo);
        }

        /// <summary>
        /// Legacy 호환성 유지
        /// </summary>
        public void TakeDamage(float damage, GameObject attacker = null)
        {
            if (IsDead) return;

            // 공격 방향 계산
            Vector3 direction = Vector3.zero;
            if (attacker != null)
            {
                direction = (transform.position - attacker.transform.position);
                direction.y = 0f;
                direction.Normalize();
            }

            TakeDamage(new AttackInfo(damage, KnockbackLevel.Light, direction, attacker));
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