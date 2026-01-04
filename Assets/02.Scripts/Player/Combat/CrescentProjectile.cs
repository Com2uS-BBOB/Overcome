using System;
using UnityEngine;
using _02.Scripts.Common;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 크레센트 프로젝타일
    /// HitboxBase 상속, 관통 (다중 피격), 오브젝트 풀링 지원
    /// </summary>
    public class CrescentProjectile : HitboxBase
    {
        private float _speed;
        private float _maxDistance;
        private Vector3 _startPosition;
        private Vector3 _direction;
        private GameObject _owner;

        // 풀 반환 콜백
        private Action<CrescentProjectile> _returnToPool;

        /// <summary>
        /// 프로젝타일 초기화
        /// </summary>
        public void Initialize(float damage, float speed, float maxDistance, Vector3 direction, GameObject owner, Action<CrescentProjectile> returnCallback = null)
        {
            _speed = speed;
            _maxDistance = maxDistance;
            _direction = direction.normalized;
            _startPosition = transform.position;
            _owner = owner;
            _returnToPool = returnCallback;

            // 히트 감지 활성화
            EnableHitDetection(damage);

            // 방향으로 회전
            if (_direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_direction);
            }

            Debug.Log($"[Crescent] 발사! 데미지: {damage}, 속도: {speed}, 사거리: {maxDistance}");
        }

        private void Update()
        {
            if (!_isActive) return;

            // 이동
            transform.position += _direction * _speed * Time.deltaTime;

            // 최대 거리 체크
            float traveled = Vector3.Distance(_startPosition, transform.position);
            if (traveled >= _maxDistance)
            {
                ReturnOrDestroy();
            }
        }

        /// <summary>
        /// Player 태그 무시
        /// </summary>
        protected override bool ShouldIgnore(Collider other)
        {
            return other.CompareTag(GameTags.Player);
        }

        /// <summary>
        /// 공격자 반환
        /// </summary>
        protected override GameObject GetOwner()
        {
            return _owner;
        }

        /// <summary>
        /// 풀 반환 또는 파괴
        /// </summary>
        private void ReturnOrDestroy()
        {
            DisableHitDetection();

            if (_returnToPool != null)
            {
                Debug.Log("[Crescent] 풀로 반환");
                _returnToPool.Invoke(this);
            }
            else
            {
                Debug.Log("[Crescent] 소멸");
                Destroy(gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, _direction * 2f);
        }
    }
}