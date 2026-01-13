using System;
using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 전투 상태 핸들러
    /// Animator 정규화 시간 기반 캔슬 윈도우 관리
    /// </summary>
    public class CombatStateHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private InputBuffer _inputBuffer;

        [Header("Attack Data")]
        [SerializeField] private AttackData[] _attackDataList;

        private Dictionary<string, AttackData> _attackDataMap;
        private AttackData _currentAttack;
        private bool _isInCancelWindow;
        private bool _isInHitboxWindow;

        public bool IsInCancelWindow => _isInCancelWindow;
        public bool IsInHitboxWindow => _isInHitboxWindow;
        public AttackData CurrentAttack => _currentAttack;
        public InputBuffer InputBuffer => _inputBuffer;

        public event Action OnCancelWindowEnter;
        public event Action OnCancelWindowExit;
        public event Action OnHitboxWindowEnter;
        public event Action OnHitboxWindowExit;

        private void Awake()
        {
            InitializeAttackDataMap();
        }

        private void InitializeAttackDataMap()
        {
            _attackDataMap = new Dictionary<string, AttackData>();

            if (_attackDataList == null) return;

            foreach (var data in _attackDataList)
            {
                if (data != null && !string.IsNullOrEmpty(data.AttackName))
                {
                    _attackDataMap[data.AttackName] = data;
                }
            }
        }

        private void Update()
        {
            if (_currentAttack != null)
            {
                CheckWindows();
            }
        }

        private void CheckWindows()
        {
            if (_animator == null || _currentAttack == null) return;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime % 1f;

            // 캔슬 윈도우 체크
            bool wasInCancelWindow = _isInCancelWindow;
            _isInCancelWindow = _currentAttack.IsInCancelWindow(normalizedTime);

            if (!wasInCancelWindow && _isInCancelWindow)
            {
                OnCancelWindowEnter?.Invoke();
                TryConsumeBuffer();
            }
            else if (wasInCancelWindow && !_isInCancelWindow)
            {
                OnCancelWindowExit?.Invoke();
            }

            // 히트박스 윈도우 체크
            bool wasInHitboxWindow = _isInHitboxWindow;
            _isInHitboxWindow = _currentAttack.IsInHitboxWindow(normalizedTime);

            if (!wasInHitboxWindow && _isInHitboxWindow)
            {
                OnHitboxWindowEnter?.Invoke();
            }
            else if (wasInHitboxWindow && !_isInHitboxWindow)
            {
                OnHitboxWindowExit?.Invoke();
            }
        }

        private void TryConsumeBuffer()
        {
            if (_inputBuffer != null && _inputBuffer.TryConsume(out var buffered))
            {
                // 버퍼에서 꺼낸 입력이 현재 공격에서 캔슬 가능한지 확인
                if (CanCancelInto(buffered.Action))
                {
                    OnBufferedActionReady?.Invoke(buffered);
                }
            }
        }

        public event Action<InputBuffer.BufferedInput> OnBufferedActionReady;

        /// <summary>
        /// 현재 공격 설정
        /// </summary>
        public void SetCurrentAttack(string attackName)
        {
            if (_attackDataMap.TryGetValue(attackName, out var data))
            {
                _currentAttack = data;
                _isInCancelWindow = false;
                _isInHitboxWindow = false;
            }
            else
            {
                Debug.LogWarning($"[CombatStateHandler] AttackData not found: {attackName}");
                _currentAttack = null;
            }
        }

        /// <summary>
        /// 콤보 단계로 현재 공격 설정
        /// </summary>
        public void SetCurrentAttackByCombo(string skillName, int comboStep)
        {
            string attackName = $"{skillName}_{comboStep}";
            SetCurrentAttack(attackName);
        }

        /// <summary>
        /// 현재 공격 초기화
        /// </summary>
        public void ClearCurrentAttack()
        {
            _currentAttack = null;
            _isInCancelWindow = false;
            _isInHitboxWindow = false;
        }

        /// <summary>
        /// 특정 액션으로 캔슬 가능한지 확인
        /// </summary>
        public bool CanCancelInto(ActionType targetAction)
        {
            // 현재 공격이 없으면 항상 가능
            if (_currentAttack == null) return true;

            // 캔슬 윈도우 밖이면 불가
            if (!_isInCancelWindow) return false;

            // 해당 액션이 허용되는지 확인
            return _currentAttack.CanCancelInto(targetAction);
        }

        /// <summary>
        /// 캔슬 불가 시 입력을 버퍼에 저장
        /// </summary>
        public void BufferIfNeeded(ActionType action, Vector2 direction = default)
        {
            if (_inputBuffer == null) return;

            // 이미 캔슬 가능하면 버퍼링 불필요
            if (CanCancelInto(action)) return;

            _inputBuffer.Buffer(action, direction);
        }

        /// <summary>
        /// AttackData 가져오기
        /// </summary>
        public AttackData GetAttackData(string attackName)
        {
            return _attackDataMap.TryGetValue(attackName, out var data) ? data : null;
        }
    }
}