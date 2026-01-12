using System;
using UnityEngine;

namespace _02.Scripts.Player.Animation
{
    /// 하위 모델의 Root Motion을 상위 Transform으로 전달하는 프록시
    /// Animator가 있는 하위 오브젝트에 부착
    [RequireComponent(typeof(Animator))]
    public class RootMotionProxy : MonoBehaviour
    {
        private Animator _animator;
        private bool _isRootMotionEnabled;

        public event Action<Vector3> OnRootMotionUpdate;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.applyRootMotion = false;
        }

        public void EnableRootMotion()
        {
            _isRootMotionEnabled = true;
            _animator.applyRootMotion = true;
        }

        public void DisableRootMotion()
        {
            _isRootMotionEnabled = false;
            _animator.applyRootMotion = false;
        }

        private void OnAnimatorMove()
        {
            if (!_isRootMotionEnabled) return;
            OnRootMotionUpdate?.Invoke(_animator.deltaPosition);
        }
    }
}
