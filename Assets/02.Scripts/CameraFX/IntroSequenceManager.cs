using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace _02.Scripts.CameraFX
{
    /// <summary>
    /// 게임 시작 시 플레이어 주변 카메라 연출 관리자
    /// Cinemachine Spline Dolly를 사용한 시네마틱 인트로
    /// </summary>
    public class IntroSequenceManager : MonoBehaviour
    {
        public static IntroSequenceManager Instance { get; private set; }

        [Header("Cameras")]
        [SerializeField] private CinemachineCamera _introCamera;
        [SerializeField] private CinemachineCamera _gameplayCamera;

        [Header("Dolly Settings")]
        [SerializeField] private CinemachineSplineDolly _dollyComponent;
        [SerializeField] private float _introDuration = 4f;
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Camera Priority")]
        [SerializeField] private int _introCameraPriority = 20;
        [SerializeField] private int _gameplayCameraPriority = 10;

        [Header("Blend Settings")]
        [SerializeField] private float _cameraBlendTime = 2f;

        [Header("FOV Settings")]
        [SerializeField] private bool _useFOVTransition = true;
        [SerializeField] private float _introStartFOV = 70f;
        [SerializeField] private float _introEndFOV = 60f;

        [Header("Fade Settings")]
        [SerializeField] private bool _useFadeIn = true;
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private CanvasGroup _fadeCanvasGroup;

        [Header("Skip Settings")]
        [SerializeField] private bool _allowSkip = true;
        [SerializeField] private Key _skipKey = Key.Space;

        [Header("References")]
        [SerializeField] private Transform _playerTransform;

        [Header("Player Animation")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private string _introAnimationTrigger = "Intro";
        [SerializeField] private string _idleAnimationTrigger = "Idle";

        private bool _isPlaying;
        private bool _isSkipping;
        private Coroutine _introCoroutine;

        public bool IsPlaying => _isPlaying;

        public event Action OnIntroStarted;
        public event Action OnIntroCompleted;
        public event Action OnIntroSkipped;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 자동 시작 (씬 로드 시)
            if (_introCamera != null && _gameplayCamera != null)
            {
                PlayIntroSequence();
            }
        }

        private void Update()
        {
            // 스킵 입력 체크 (Input System)
            if (_isPlaying && _allowSkip && Keyboard.current != null && Keyboard.current[_skipKey].wasPressedThisFrame)
            {
                SkipIntro();
            }
        }

        /// <summary>
        /// 인트로 시퀀스 시작
        /// </summary>
        public void PlayIntroSequence()
        {
            if (_isPlaying) return;
            if (_introCamera == null || _gameplayCamera == null)
            {
                Debug.LogWarning("[IntroSequenceManager] Cameras not assigned.");
                return;
            }

            _isPlaying = true;
            _isSkipping = false;

            // 인트로 카메라 활성화
            _introCamera.Priority = _introCameraPriority;
            _gameplayCamera.Priority = _gameplayCameraPriority;

            // 플레이어 입력 비활성화
            SetPlayerInputEnabled(false);

            // 인트로 애니메이션 재생
            PlayPlayerAnimation(_introAnimationTrigger);

            OnIntroStarted?.Invoke();

            _introCoroutine = StartCoroutine(IntroSequenceCoroutine());
        }

        /// <summary>
        /// 인트로 스킵
        /// </summary>
        public void SkipIntro()
        {
            if (!_isPlaying || _isSkipping) return;

            _isSkipping = true;

            if (_introCoroutine != null)
            {
                StopCoroutine(_introCoroutine);
            }

            // 즉시 게임플레이 카메라로 전환
            TransitionToGameplay();

            OnIntroSkipped?.Invoke();
        }

        private IEnumerator IntroSequenceCoroutine()
        {
            // 페이드 인
            if (_useFadeIn && _fadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeInCoroutine());
            }

            // Dolly 시작 위치 설정
            if (_dollyComponent != null)
            {
                _dollyComponent.CameraPosition = 0f;
            }

            // 초기 FOV 설정
            if (_useFOVTransition && _introCamera != null)
            {
                _introCamera.Lens.FieldOfView = _introStartFOV;
            }

            // Dolly Track 이동
            float elapsed = 0f;
            while (elapsed < _introDuration)
            {
                float normalizedTime = elapsed / _introDuration;
                float curveValue = _speedCurve.Evaluate(normalizedTime);

                // Dolly 위치 업데이트
                if (_dollyComponent != null)
                {
                    _dollyComponent.CameraPosition = curveValue;
                }

                // FOV 전환
                if (_useFOVTransition && _introCamera != null)
                {
                    _introCamera.Lens.FieldOfView = Mathf.Lerp(_introStartFOV, _introEndFOV, curveValue);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 인트로 완료
            TransitionToGameplay();
        }

        private IEnumerator FadeInCoroutine()
        {
            _fadeCanvasGroup.alpha = 1f;
            float elapsed = 0f;

            while (elapsed < _fadeInDuration)
            {
                _fadeCanvasGroup.alpha = 1f - (elapsed / _fadeInDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _fadeCanvasGroup.alpha = 0f;
        }

        private void TransitionToGameplay()
        {
            // 게임플레이 카메라로 전환
            if (_gameplayCamera != null)
                _gameplayCamera.Priority = _introCameraPriority;

            // 블렌딩 완료 후 IntroCam 비활성화 (블렌딩 중 비활성화 시 버벅거림 발생)
            if (_introCamera != null)
                StartCoroutine(DisableIntroCamAfterBlend());

            // Idle 애니메이션으로 전환
            PlayPlayerAnimation(_idleAnimationTrigger);

            // 플레이어 입력 활성화
            SetPlayerInputEnabled(true);

            _isPlaying = false;
            _introCoroutine = null;

            OnIntroCompleted?.Invoke();
            GameEventHandler.GameStart();
        }

        private IEnumerator DisableIntroCamAfterBlend()
        {
            yield return new WaitForSeconds(_cameraBlendTime);

            if (_introCamera != null)
                _introCamera.gameObject.SetActive(false);
        }

        private void PlayPlayerAnimation(string triggerName)
        {
            if (_playerAnimator != null && !string.IsNullOrEmpty(triggerName))
            {
                _playerAnimator.SetTrigger(triggerName);
            }
        }

        private void SetPlayerInputEnabled(bool enabled)
        {
            // PlayerInputHandler의 입력 활성화/비활성화
            var inputHandler = _playerTransform?.GetComponent<_02.Scripts.Player.Core.PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.SetInputEnabled(enabled);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

#if UNITY_EDITOR
        [Header("Editor Debug")]
        [SerializeField] private bool _previewInEditor;

        private void OnValidate()
        {
            // Dolly 경로 자동 찾기
            if (_dollyComponent == null && _introCamera != null)
            {
                _dollyComponent = _introCamera.GetComponent<CinemachineSplineDolly>();
            }
        }
#endif
    }
}