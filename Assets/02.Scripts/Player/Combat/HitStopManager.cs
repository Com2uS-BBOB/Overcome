using System.Collections;
using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 히트스톱(프리즈 프레임) 관리자
    /// 타격 시 순간적인 게임 정지로 타격감 강화
    /// </summary>
    public class HitStopManager : MonoBehaviour
    {
        public static HitStopManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float _duration = 0.05f;
        [SerializeField] private float _minInterval = 0.1f;

        private float _lastStopTime;
        private Coroutine _currentCoroutine;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// 히트스톱 트리거
        /// </summary>
        public void TriggerHitStop()
        {
            if (Time.unscaledTime - _lastStopTime < _minInterval)
                return;

            if (_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);

            _currentCoroutine = StartCoroutine(HitStopCoroutine());
            _lastStopTime = Time.unscaledTime;
        }

        private IEnumerator HitStopCoroutine()
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            yield return new WaitForSecondsRealtime(_duration);

            Time.timeScale = originalTimeScale;
            _currentCoroutine = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            Time.timeScale = 1f;
        }
    }
}