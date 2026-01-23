using System;
using UnityEngine;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Gauge;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 가드 시스템 중앙 관리
    /// - 저스트 가드 타이밍 계산
    /// - 방향 체크 (전방 180도)
    /// - 게이지 소모/회복 처리
    /// </summary>
    public class GuardManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GuardSettings _settings;
        [SerializeField] private GaugeManager _gaugeManager;

        private bool _isGuarding;
        private float _guardStartTime;
        private float _guardReleaseTime;
        private bool _isInInputBuffer;

        // Properties
        public bool IsGuarding => _isGuarding || _isInInputBuffer;
        public bool IsActivelyGuarding => _isGuarding;
        public GuardSettings Settings => _settings;

        // Events
        public event Action OnGuardStart;
        public event Action OnGuardEnd;
        public event Action<GuardResult> OnGuardResult;
        public event Action OnJustGuard;
        public event Action OnNormalGuard;
        public event Action OnGuardBreak;

        private void Update()
        {
            // 입력 버퍼 체크
            if (_isInInputBuffer && !_isGuarding)
            {
                float timeSinceRelease = Time.time - _guardReleaseTime;
                if (timeSinceRelease >= _settings.GuardInputBuffer)
                {
                    _isInInputBuffer = false;
                }
            }
        }

        /// <summary>
        /// 가드 시작
        /// </summary>
        public void StartGuard()
        {
            _isGuarding = true;
            _isInInputBuffer = true;
            _guardStartTime = Time.time;
            OnGuardStart?.Invoke();
        }

        /// <summary>
        /// 가드 종료 (키 해제)
        /// </summary>
        public void EndGuard()
        {
            if (!_isGuarding) return;

            _isGuarding = false;
            _guardReleaseTime = Time.time;
            // _isInInputBuffer는 유지 (버퍼 시간 동안)
            OnGuardEnd?.Invoke();
        }

        /// <summary>
        /// 가드 완전 종료 (상태 전환 시)
        /// </summary>
        public void ForceEndGuard()
        {
            _isGuarding = false;
            _isInInputBuffer = false;
            OnGuardEnd?.Invoke();
        }

        /// <summary>
        /// 공격 가드 시도
        /// </summary>
        /// <param name="attackInfo">공격 정보</param>
        /// <param name="playerTransform">플레이어 Transform</param>
        /// <returns>가드 결과</returns>
        public GuardResult TryBlock(AttackInfo attackInfo, Transform playerTransform)
        {
            if (!IsGuarding)
                return GuardResult.None;

            // 후방 공격 체크
            if (!IsAttackFromFront(attackInfo.Direction, playerTransform))
            {
                OnGuardResult?.Invoke(GuardResult.RearAttack);
                return GuardResult.RearAttack;
            }

            // 저스트 가드 판정
            float timeSinceGuardStart = Time.time - _guardStartTime;
            bool isJustGuard = timeSinceGuardStart <= _settings.JustGuardWindow;

            if (isJustGuard)
            {
                // 저스트 가드 성공 - 게이지 회복
                _gaugeManager.AddCrescentCharge(_settings.JustGuardReward);
                OnJustGuard?.Invoke();
                OnGuardResult?.Invoke(GuardResult.JustGuard);
                return GuardResult.JustGuard;
            }

            // 일반 가드 - 게이지 체크
            if (_gaugeManager.CrescentCurrent < _settings.NormalGuardCost)
            {
                // 가드 브레이크
                OnGuardBreak?.Invoke();
                OnGuardResult?.Invoke(GuardResult.GuardBreak);
                return GuardResult.GuardBreak;
            }

            // 일반 가드 성공 - 게이지 소모
            _gaugeManager.ConsumeCrescentForGuard(_settings.NormalGuardCost);
            OnNormalGuard?.Invoke();
            OnGuardResult?.Invoke(GuardResult.NormalBlock);
            return GuardResult.NormalBlock;
        }

        /// <summary>
        /// 공격이 전방에서 오는지 확인 (180도 범위)
        /// </summary>
        private bool IsAttackFromFront(Vector3 attackDirection, Transform playerTransform)
        {
            // 공격 방향이 없으면 전방으로 간주
            if (attackDirection.sqrMagnitude < 0.01f)
                return true;

            // 공격자 방향 (공격 방향의 반대)
            Vector3 toAttacker = -attackDirection;
            toAttacker.y = 0;
            toAttacker.Normalize();

            Vector3 playerForward = playerTransform.forward;
            playerForward.y = 0;
            playerForward.Normalize();

            float angle = Vector3.Angle(playerForward, toAttacker);

            return angle <= _settings.FrontGuardAngle;
        }

        /// <summary>
        /// 저스트 가드 가능 상태인지 (현재 가드 중 + 윈도우 내)
        /// </summary>
        public bool IsInJustGuardWindow()
        {
            if (!IsGuarding) return false;

            float timeSinceGuardStart = Time.time - _guardStartTime;
            return timeSinceGuardStart <= _settings.JustGuardWindow;
        }
    }
}