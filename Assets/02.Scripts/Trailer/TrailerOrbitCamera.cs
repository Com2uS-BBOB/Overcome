using UnityEngine;
using UnityEngine.Splines;
using Unity.Cinemachine;

namespace _02.Scripts.Trailer
{
    /// <summary>
    /// 트레일러용 맵 오빗 카메라
    /// Spline 경로를 따라 맵 전체를 한바퀴 도는 연출
    /// </summary>
    public class TrailerOrbitCamera : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private CinemachineCamera _orbitCamera;
        [SerializeField] private int _activePriority = 20;

        [Header("Spline Dolly")]
        [SerializeField] private CinemachineSplineDolly _dolly;

        [Header("Orbit Settings")]
        [SerializeField] private float _orbitDuration = 15f;
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Options")]
        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private bool _loop = false;

        private bool _isPlaying;
        private float _elapsed;

        private void Start()
        {
            if (_playOnStart)
                Play();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            if (_dolly == null) return;

            _elapsed += Time.deltaTime;
            float t = _elapsed / _orbitDuration;

            if (t >= 1f)
            {
                if (_loop)
                {
                    _elapsed = 0f;
                    t = 0f;
                }
                else
                {
                    t = 1f;
                    _isPlaying = false;
                }
            }

            // Spline 위치 업데이트
            float curveValue = _speedCurve.Evaluate(t);
            _dolly.CameraPosition = curveValue;
        }

        [ContextMenu("Play Orbit")]
        public void Play()
        {
            _isPlaying = true;
            _elapsed = 0f;

            if (_orbitCamera != null)
                _orbitCamera.Priority = _activePriority;

            if (_dolly != null)
                _dolly.CameraPosition = 0f;
        }

        [ContextMenu("Stop Orbit")]
        public void Stop()
        {
            _isPlaying = false;

            if (_orbitCamera != null)
                _orbitCamera.Priority = 0;
        }

        [ContextMenu("Reset Position")]
        public void ResetPosition()
        {
            _elapsed = 0f;
            if (_dolly != null)
                _dolly.CameraPosition = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Dolly 자동 찾기
            if (_dolly == null && _orbitCamera != null)
                _dolly = _orbitCamera.GetComponent<CinemachineSplineDolly>();
        }
#endif
    }
}