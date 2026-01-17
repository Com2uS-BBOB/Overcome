using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;
using _02.Scripts.CameraFX;

namespace _02.Scripts.Player.Combat
{
    // 질풍참 (대시 공격 스킬)
    public class DashAttackSkill : MonoBehaviour, ISkill, IOverDriveAffected
    {
        private const string CooldownKey = "DashAttack";

        [Header("Settings")]
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private string _enemyLayerName = "Enemy";

        [Header("Damage")]
        [SerializeField] private float _dashDamage = 15f;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private MeleeHitbox _hitbox;

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
        public event Action<IDamageable, float> OnEnemyHit;
        public event Action OnCooldownReset;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _cooldownManager = new CooldownManager();

            if (_cameraTransform == null) _cameraTransform = Camera.main?.transform;
        }

        private void OnEnable()
        {
            if (_hitbox != null) _hitbox.OnHit += HandleHit;
            EnemyEventController.Enemy.OnKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            if (_hitbox != null) _hitbox.OnHit -= HandleHit;
            EnemyEventController.Enemy.OnKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(EnemyKilledEvent e)
        {
            ResetCooldown();
        }

        public void Initialize(PlayerStats stats) => _stats = stats;

        private void HandleHit(IDamageable target, float damage)
        {
            OnEnemyHit?.Invoke(target, damage);
        }

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

            // 카메라 효과: FOV 확대 (스피드감)
            CameraEffectsManager.Instance?.StartDashFOV();

            // 히트박스 활성화
            _hitbox?.EnableHitDetection(_dashDamage);

            // 적 레이어와 충돌 무시 (관통)
            int playerLayer = gameObject.layer;
            int enemyLayer = LayerMask.NameToLayer(_enemyLayerName);
            bool layerCollisionEnabled = enemyLayer >= 0;

            if (layerCollisionEnabled)
                Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);

            Vector3 dashDirection = _cameraTransform.forward;

            // 플레이어를 대시 방향으로 회전 (Y축만, 수평 방향)
            Vector3 horizontalDirection = dashDirection;
            horizontalDirection.y = 0f;
            if (horizontalDirection.sqrMagnitude > 0.01f)
            {
                horizontalDirection.Normalize();
                transform.rotation = Quaternion.LookRotation(horizontalDirection);
            }

            float dashSpeed = DashDistance / _dashDuration;

            float elapsed = 0f;
            while (elapsed < _dashDuration)
            {
                _controller.Move(dashDirection * dashSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 적 레이어 충돌 복원
            if (layerCollisionEnabled)
                Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);

            // 히트박스 비활성화
            _hitbox?.DisableHitDetection();

            // 카메라 효과: FOV 복귀
            CameraEffectsManager.Instance?.EndDashFOV();

            _isDashing = false;
            OnDashEnded?.Invoke();
        }

        public void ResetCooldown()
        {
            _cooldownManager.Reset(CooldownKey);
            OnCooldownReset?.Invoke();
        }
    }
}