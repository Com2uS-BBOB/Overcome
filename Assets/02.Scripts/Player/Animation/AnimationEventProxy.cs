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

        // === 공통 이벤트 ===
        public event Action<string> OnAnimationEvent;

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

        // 범용 이벤트 (문자열 파라미터)
        public void AnimEvent(string eventName)
        {
            OnAnimationEvent?.Invoke(eventName);
        }
    }
}