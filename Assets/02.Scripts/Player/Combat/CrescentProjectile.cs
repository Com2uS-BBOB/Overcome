using System;
using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 크레센트 프로젝타일
    /// 직선 이동, 관통 (다중 피격)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CrescentProjectile : MonoBehaviour
    {
        private float _damage;
        private float _speed;
        private float _maxDistance;
        private Vector3 _startPosition;
        private Vector3 _direction;
        private HashSet<Collider> _hitTargets = new HashSet<Collider>();

        // 피격 이벤트
        public event Action<Collider, float> OnHit;

        /// <summary>
        /// 프로젝타일 초기화
        /// </summary>
        public void Initialize(float damage, float speed, float maxDistance, Vector3 direction)
        {
            _damage = damage;
            _speed = speed;
            _maxDistance = maxDistance;
            _direction = direction.normalized;
            _startPosition = transform.position;
            _hitTargets.Clear();

            // 방향으로 회전
            if (_direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(_direction);
            }

            Debug.Log($"[Crescent] 발사! 데미지: {damage}, 속도: {speed}, 사거리: {maxDistance}");
        }

        private void Update()
        {
            // 이동
            transform.position += _direction * _speed * Time.deltaTime;

            // 최대 거리 체크
            float traveled = Vector3.Distance(_startPosition, transform.position);
            if (traveled >= _maxDistance)
            {
                DestroyProjectile();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // 이미 피격된 대상 무시 (관통 - 중복 방지)
            if (_hitTargets.Contains(other))
            {
                return;
            }

            // 자기 자신(플레이어) 무시
            if (other.CompareTag("Player"))
            {
                return;
            }

            _hitTargets.Add(other);

            Debug.Log($"[Crescent] 피격! 대상: {other.name}, 데미지: {_damage}");

            // 피격 이벤트 발행
            OnHit?.Invoke(other, _damage);

            // TODO: IDamageable 인터페이스로 데미지 전달
            // var damageable = other.GetComponent<IDamageable>();
            // damageable?.TakeDamage(_damage, ...);
        }

        private void DestroyProjectile()
        {
            Debug.Log("[Crescent] 소멸");
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            // 프로젝타일 시각화
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // 이동 방향
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, _direction * 2f);
        }
    }
}