using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    // 질풍참 (대시 공격 스킬)
    public class DashAttackSkill : MonoBehaviour, ISkill, IOverDriveAffected
    {
        private const string CooldownKey = "DashAttack";

        [Header("Settings")]
        [SerializeField] private float _dashDuration = 0.3f;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private PlayerStats _stats;
        private CooldownManager _cooldownManager;
        private bool _isDashing;

        // ISkill
        public string SkillName => "질풍참";
        public float Cooldown => _stats != null ? _stats.DashCooldown : 3f;
        public bool CanUse => !_isDashing && (IsOverDriveActive || _cooldownManager.IsReady(CooldownKey, Cooldown));
        public bool IsDashing => _isDashing;

        // IOverDriveAffected
        public bool IsOverDriveActive { get; set; }

        private float DashDistance => _stats != null ? _stats.DashDistance : 10f;

        public event Action OnSkillUsed;
        public event Action OnDashStarted;
        public event Action OnDashEnded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _stats = GetComponent<PlayerStats>();
            _cooldownManager = new CooldownManager();

            if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
        }

        public void Initialize(PlayerStats stats) => _stats = stats;

        public void Use()
        {
            if (!CanUse) return;
            StartCoroutine(DashCoroutine());
        }

        private IEnumerator DashCoroutine()
        {
            _isDashing = true;
            _cooldownManager.Use(CooldownKey);
            OnDashStarted?.Invoke();
            OnSkillUsed?.Invoke();

            Vector3 dashDirection = _cameraTransform.forward;
            float dashSpeed = DashDistance / _dashDuration;

            float elapsed = 0f;
            while (elapsed < _dashDuration)
            {
                _controller.Move(dashDirection * dashSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _isDashing = false;
            OnDashEnded?.Invoke();
        }

        public void ResetCooldown() => _cooldownManager.Reset(CooldownKey);
    }
}