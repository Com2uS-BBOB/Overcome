using System;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Movement;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 용검 (기본 공격) 스킬
    /// 지상 콤보와 공중 콤보가 SkillDataSet으로 분리되어 관리됨
    /// </summary>
    public class DragonSwordSkill : BaseSkill
    {
        private const string CooldownKey = "Attack";

        [Header("Hitbox")]
        [SerializeField] private MeleeHitbox _hitbox;

        private PlayerStats _stats;
        private PlayerMovement _movement;
        private CooldownManager _cooldownManager;

        // === ISkill 구현 ===
        public override string SkillName => "용검";
        public override float Cooldown => _stats != null ? _stats.AttackCooldown : 0.5f;
        public override bool CanUse => CanAttack;

        // === Properties ===

        /// <summary>
        /// 공격 가능 조건: 공격 중 아님 + 쿨다운 OK + 유예 구간 아님
        /// </summary>
        public bool CanAttack => !_isActive && !_inComboGrace && _cooldownManager.IsReady(CooldownKey, Cooldown);

        /// <summary>
        /// 공격 중 여부 (State 체크용)
        /// </summary>
        public bool IsAttacking => _isActive;

        private float AttackDamage => _stats != null ? _stats.AttackDamage : 10f;

        // === Events ===
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;

        // === Initialization ===

        private void Awake()
        {
            _cooldownManager = new CooldownManager();
        }

        public void Initialize(PlayerStats stats, PlayerMovement movement)
        {
            _stats = stats;
            _movement = movement;
        }

        private void OnEnable()
        {
            if (_hitbox != null) _hitbox.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            if (_hitbox != null) _hitbox.OnHit -= HandleHit;
        }

        // === BaseSkill Override ===

        protected override bool GetIsGrounded()
        {
            return _movement != null && _movement.IsGrounded;
        }

        protected override float GetDamage()
        {
            return AttackDamage;
        }

        protected override void OnSkillStart()
        {
            _cooldownManager.Use(CooldownKey);
            OnAttackStarted?.Invoke();
        }

        protected override void EnableHitbox()
        {
            if (_hitbox != null)
                _hitbox.EnableHitDetection(GetDamage());
        }

        protected override void DisableHitbox()
        {
            if (_hitbox != null)
                _hitbox.DisableHitDetection();
        }

        // === Public API (호환성 유지) ===

        /// <summary>
        /// 첫 공격 실행 (콤보 1타)
        /// </summary>
        public void Attack()
        {
            StartSkill(GetIsGrounded());
        }

        /// <summary>
        /// 콤보 리셋 Override (히트박스 정리 추가)
        /// </summary>
        public override void ResetCombo()
        {
            base.ResetCombo();
            DisableHitbox();
        }

        // === Event Handlers ===

        private void HandleHit(IDamageable target, float damage)
        {
            InvokeEnemyHit(target, damage);
        }
    }
}