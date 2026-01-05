using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    // 용검 (기본 공격) 스킬
    public class DragonSwordSkill : MonoBehaviour, ISkill
    {
        private const string CooldownKey = "Attack";

        [Header("Settings")]
        [SerializeField] private float _attackDuration = 0.3f;

        [Header("References")]
        [SerializeField] private MeleeHitbox _hitbox;

        private PlayerStats _stats;
        private CooldownManager _cooldownManager;
        private bool _isAttacking;

        // ISkill
        public string SkillName => "용검";
        public float Cooldown => _stats != null ? _stats.AttackCooldown : 0.5f;
        public bool CanUse => CanAttack;
        public bool CanAttack => !_isAttacking && _cooldownManager.IsReady(CooldownKey, Cooldown);
        public bool IsAttacking => _isAttacking;

        private float AttackDamage => _stats != null ? _stats.AttackDamage : 10f;

        public event Action OnSkillUsed;
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;
        public event Action<IDamageable, float> OnEnemyHit;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _cooldownManager = new CooldownManager();
        }

        public void Initialize(PlayerStats stats) => _stats = stats;

        private void OnEnable() { if (_hitbox != null) _hitbox.OnHit += HandleHit; }
        private void OnDisable() { if (_hitbox != null) _hitbox.OnHit -= HandleHit; }

        public void Use() => Attack();

        public void Attack()
        {
            if (_isAttacking || !_cooldownManager.IsReady(CooldownKey, Cooldown)) return;
            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;
            _cooldownManager.Use(CooldownKey);
            OnAttackStarted?.Invoke();
            OnSkillUsed?.Invoke();

            if (_hitbox != null) _hitbox.EnableHitDetection(AttackDamage);

            yield return new WaitForSeconds(_attackDuration);

            if (_hitbox != null) _hitbox.DisableHitDetection();

            _isAttacking = false;
            OnAttackEnded?.Invoke();
        }

        private void HandleHit(IDamageable target, float damage) => OnEnemyHit?.Invoke(target, damage);
    }
}