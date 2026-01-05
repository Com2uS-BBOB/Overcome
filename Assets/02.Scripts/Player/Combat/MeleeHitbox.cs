using UnityEngine;
using _02.Scripts.Player.Common;

namespace _02.Scripts.Player.Combat
{
    // 근접 공격 히트박스
    public class MeleeHitbox : HitboxBase
    {
        private Collider _collider;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider>();
            DisableHitDetection();
        }

        public override void EnableHitDetection(float damage)
        {
            base.EnableHitDetection(damage);
            _collider.enabled = true;
        }

        public override void DisableHitDetection()
        {
            base.DisableHitDetection();
            if (_collider != null) _collider.enabled = false;
        }

        // 같은 root 무시
        protected override bool ShouldIgnore(Collider other) => other.transform.root == transform.root;
    }
}