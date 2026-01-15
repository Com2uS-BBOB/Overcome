using System;
using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 전투 상태 핸들러
    /// Animation Event 기반 캔슬 윈도우 관리
    /// BaseSkill에서 AttackData를 직접 참조 (중복 할당 불필요)
    /// </summary>
    public class CombatStateHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputBuffer _inputBuffer;

        private AttackData _currentAttack;
        private bool _isInCancelWindow;

        public bool IsInCancelWindow => _isInCancelWindow;
        public AttackData CurrentAttack => _currentAttack;
        public InputBuffer InputBuffer => _inputBuffer;

        public event Action OnCancelWindowEnter;
        public event Action OnCancelWindowExit;
        public event Action<InputBuffer.BufferedInput> OnBufferedActionReady;

        #region Animation Event Handlers

        /// <summary>
        /// Animation Event: 캔슬 윈도우 진입
        /// </summary>
        public void OnAnimEventCancelWindowEnter()
        {
            if (_currentAttack == null) return;

            _isInCancelWindow = true;
            OnCancelWindowEnter?.Invoke();
            TryConsumeBuffer();
        }

        /// <summary>
        /// Animation Event: 캔슬 윈도우 종료
        /// </summary>
        public void OnAnimEventCancelWindowExit()
        {
            _isInCancelWindow = false;
            OnCancelWindowExit?.Invoke();
        }

        #endregion

        #region Buffer

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

        #endregion

        #region Attack Management

        /// <summary>
        /// 현재 공격 설정 (AttackData 직접 전달)
        /// </summary>
        public void SetCurrentAttack(AttackData attackData)
        {
            _currentAttack = attackData;
            _isInCancelWindow = false;
        }

        /// <summary>
        /// BaseSkill에서 현재 콤보의 AttackData를 가져와 설정
        /// </summary>
        public void SetCurrentAttackFromSkill(BaseSkill skill)
        {
            if (skill == null || skill.CurrentSkillData == null)
            {
                _currentAttack = null;
                return;
            }

            var attackData = skill.CurrentSkillData.GetComboData(skill.ComboStep);
            SetCurrentAttack(attackData);
        }

        /// <summary>
        /// 현재 공격 초기화
        /// </summary>
        public void ClearCurrentAttack()
        {
            _currentAttack = null;
            _isInCancelWindow = false;
        }

        #endregion

        #region Cancel Queries

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
        /// 콤보 큐잉 가능 여부 (캔슬 윈도우 내 + 콤보 액션 허용)
        /// </summary>
        public bool CanQueueCombo(ActionType comboAction)
        {
            return _isInCancelWindow && CanCancelInto(comboAction);
        }

        /// <summary>
        /// 현재 공격 중인지 (AttackData가 설정되어 있는지)
        /// </summary>
        public bool IsInCombat => _currentAttack != null;

        #endregion

        #region Input Buffer

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

        #endregion
    }
}