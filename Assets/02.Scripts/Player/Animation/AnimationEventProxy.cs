using System;
using UnityEngine;

namespace _02.Scripts.Player.Animation
{
    /// <summary>
    /// 하위 모델의 Animation Event를 상위로 전달하는 프록시
    /// Animator가 있는 하위 오브젝트에 부착
    /// </summary>
    public class AnimationEventProxy : MonoBehaviour
    {
        // === 크레센트 이벤트 ===
        public event Action OnCrescentFire;

        // === 용검 이벤트 ===
        public event Action OnAttackHitboxEnable;
        public event Action OnAttackHitboxDisable;
        public event Action OnAttackEnd;           // 공격 애니메이션 종료
        public event Action OnCancelWindowEnter;   // 캔슬 가능 시작
        public event Action OnCancelWindowExit;    // 캔슬 가능 종료

        // === 공통 이벤트 ===
        public event Action<string> OnAnimationEvent;

        // === Overdrive 이벤트 ===
        public event Action OnOverdriveReady;    // VFX 시작 타이밍
        public event Action OnOverdriveEnd;      // 애니메이션 종료

        // === 발소리 이벤트 ===
        public event Action OnFootstep;

        // Animation Event에서 호출 (메서드 이름 = 이벤트 이름)
        public void FireCrescent()
        {
            OnCrescentFire?.Invoke();
        }

        public void EnableAttackHitbox()
        {
            OnAttackHitboxEnable?.Invoke();
        }

        public void DisableAttackHitbox()
        {
            OnAttackHitboxDisable?.Invoke();
        }

        public void EndAttack()
        {
            Debug.Log("[AnimEventProxy] EndAttack called");
            OnAttackEnd?.Invoke();
        }

        public void EnterCancelWindow()
        {
            OnCancelWindowEnter?.Invoke();
        }

        public void ExitCancelWindow()
        {
            OnCancelWindowExit?.Invoke();
        }

        // 범용 이벤트 (문자열 파라미터)
        public void AnimEvent(string eventName)
        {
            OnAnimationEvent?.Invoke(eventName);
        }

        // === Overdrive 이벤트 메서드 ===
        public void OverdriveReady()
        {
            Debug.Log("[AnimEventProxy] OverdriveReady called");
            OnOverdriveReady?.Invoke();
        }

        public void OverdriveEnd()
        {
            Debug.Log("[AnimEventProxy] OverdriveEnd called");
            OnOverdriveEnd?.Invoke();
        }

        // === 발소리 이벤트 메서드 ===
        public void Footstep()
        {
            OnFootstep?.Invoke();
        }
    }
}