using UnityEngine;
using Drakkar.GameUtils;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 프로젝타일의 VFX 라이프사이클을 관리하는 컴포넌트
    /// 메인 VFX, Trail, Hit 이펙트를 통합 관리
    /// </summary>
    public class ProjectileVFXController : MonoBehaviour
    {
        [Header("Main VFX")]
        [SerializeField] private ParticleSystem _mainVFX;

        [Header("Trail")]
        [SerializeField] private DrakkarTrail _trail;

        [Header("Hit Effect")]
        [SerializeField] private ParticleSystem _hitEffectPrefab;

        private bool _isActive;

        /// <summary>
        /// 프로젝타일 발사 시 호출 - VFX와 Trail 활성화
        /// </summary>
        public void OnFired()
        {
            _isActive = true;

            if (_mainVFX != null)
            {
                _mainVFX.Play(true);
            }

            if (_trail != null)
            {
                _trail.Begin();
            }
        }

        /// <summary>
        /// 프로젝타일 반환 시 호출 - VFX와 Trail 비활성화
        /// </summary>
        public void OnReturned()
        {
            _isActive = false;

            if (_mainVFX != null)
            {
                _mainVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (_trail != null)
            {
                _trail.End();
            }
        }

        /// <summary>
        /// Hit 이펙트 스폰
        /// </summary>
        public void SpawnHitEffect(Vector3 position)
        {
            if (_hitEffectPrefab == null) return;

            var hitEffect = Instantiate(_hitEffectPrefab, position, Quaternion.identity);

            // 파티클 종료 후 자동 파괴
            var main = hitEffect.main;
            main.stopAction = ParticleSystemStopAction.Destroy;

            hitEffect.Play();
        }

        /// <summary>
        /// 풀 재사용 시 상태 리셋
        /// </summary>
        public void Reset()
        {
            _isActive = false;

            if (_mainVFX != null)
            {
                _mainVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_trail != null)
            {
                _trail.Clear();
            }
        }

        private void OnEnable()
        {
            // 풀에서 재활성화될 때 리셋
            Reset();
        }
    }
}