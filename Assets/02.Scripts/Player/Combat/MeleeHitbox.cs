using UnityEngine;
using _02.Scripts.Common;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 근접 공격 히트박스
    /// HitboxBase 상속, Collider 활성화/비활성화로 제어
    /// </summary>
    public class MeleeHitbox : HitboxBase
    {
        private Collider _collider;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider>();
            DisableHitDetection();
        }

        /// <summary>
        /// 히트박스 활성화
        /// </summary>
        public override void EnableHitDetection(float damage)
        {
            base.EnableHitDetection(damage);
            _collider.enabled = true;

            Debug.Log($"[MeleeHitbox] 활성화 - 데미지: {damage}");
        }

        /// <summary>
        /// 히트박스 비활성화
        /// </summary>
        public override void DisableHitDetection()
        {
            base.DisableHitDetection();
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            Debug.Log("[MeleeHitbox] 비활성화");
        }

        /// <summary>
        /// 자기 자신(같은 root) 무시
        /// </summary>
        protected override bool ShouldIgnore(Collider other)
        {
            return other.transform.root == transform.root;
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = col.enabled ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.3f);

            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}