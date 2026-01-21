using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace _02.Scripts.CameraFX
{
    /// <summary>
    /// 스테이지 결과 연출 관리자
    /// 페이드 전환, 플레이어 위치 이동, 카메라 전환, 랭크별 애니메이션 재생
    /// </summary>
    public class ResultSequenceManager : MonoBehaviour
    {
        public static ResultSequenceManager Instance { get; private set; }

        [Header("Cameras")]
        [SerializeField] private CinemachineCamera _resultCamera;
        [SerializeField] private CinemachineCamera _gameplayCamera;

        [Header("Camera Priority")]
        [SerializeField] private int _resultCameraPriority = 20;
        [SerializeField] private int _gameplayCameraPriority = 10;

        [Header("Player Setup")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private CharacterController _playerCharacterController;
        [SerializeField] private Transform _resultPosition;
        [SerializeField] private Transform _resultLookAt;

        [Header("Animation")]
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private ResultAnimationData _animationData;

        [Header("Fade Settings")]
        [SerializeField] private CanvasGroup _fadeCanvasGroup;
        [SerializeField] private float _fadeOutDuration = 0.5f;
        [SerializeField] private float _fadeInDuration = 0.5f;

        [Header("Transition Settings")]
        [SerializeField] private float _preDelay = 0.3f;

        [Header("Skip Settings")]
        [SerializeField] private bool _allowSkip = true;
        [SerializeField] private Key _skipKey = Key.Space;

        private bool _isPlaying;
        private bool _isSkipping;
        private GradeConfig _currentGrade;
        private Coroutine _sequenceCoroutine;

        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// 페이드 인 완료 후 발생 (애니메이션 + UI 동시 시작)
        /// </summary>
        public event Action OnResultReady;
        public event Action OnResultSkipped;

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
            // TimeSystem의 게임 오버 이벤트 구독
            if (TimeSystem.Instance != null)
            {
                TimeSystem.Instance.OnGameOver += HandleGameOver;
            }

            // 페이드 캔버스 초기화
            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.alpha = 0f;
            }
        }

        private void Update()
        {
            if (_isPlaying && _allowSkip && Keyboard.current != null && Keyboard.current[_skipKey].wasPressedThisFrame)
            {
                SkipResult();
            }
        }

        private void OnDestroy()
        {
            if (TimeSystem.Instance != null)
            {
                TimeSystem.Instance.OnGameOver -= HandleGameOver;
            }

            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 게임 오버 시 호출
        /// </summary>
        private void HandleGameOver()
        {
            // ScoreSystem에서 현재 등급 가져오기
            _currentGrade = ScoreSystem.Instance?.GetGradeConfig();
            StartResultSequence();
        }

        /// <summary>
        /// 결과 연출 시작
        /// </summary>
        public void StartResultSequence()
        {
            if (_isPlaying) return;

            _isPlaying = true;
            _isSkipping = false;

            SetPlayerInputEnabled(false);

            _sequenceCoroutine = StartCoroutine(ResultSequenceCoroutine());
        }

        /// <summary>
        /// 외부에서 GradeConfig를 지정하여 시작
        /// </summary>
        public void StartResultSequence(GradeConfig gradeConfig)
        {
            _currentGrade = gradeConfig;
            StartResultSequence();
        }

        /// <summary>
        /// 결과 연출 스킵
        /// </summary>
        public void SkipResult()
        {
            if (!_isPlaying || _isSkipping) return;

            _isSkipping = true;

            if (_sequenceCoroutine != null)
            {
                StopCoroutine(_sequenceCoroutine);
            }

            CompleteResultSequence();

            OnResultSkipped?.Invoke();
        }

        private IEnumerator ResultSequenceCoroutine()
        {
            // 1. 잠시 대기
            yield return new WaitForSeconds(_preDelay);

            // 2. 페이드 아웃 (화면 어두워짐)
            yield return StartCoroutine(FadeOutCoroutine());

            // 3. 플레이어를 결과 위치로 이동
            MovePlayerToResultPosition();

            // 4. 결과 카메라 활성화
            ActivateResultCamera();

            // 5. 페이드 인 (화면 밝아짐)
            yield return StartCoroutine(FadeInCoroutine());

            // 6. 애니메이션 + UI 동시 시작
            PlayRankAnimation();
            OnResultReady?.Invoke();

            _isPlaying = false;
            _sequenceCoroutine = null;
        }

        private IEnumerator FadeOutCoroutine()
        {
            if (_fadeCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < _fadeOutDuration)
            {
                _fadeCanvasGroup.alpha = elapsed / _fadeOutDuration;
                elapsed += Time.deltaTime;
                yield return null;
            }
            _fadeCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeInCoroutine()
        {
            if (_fadeCanvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                _fadeCanvasGroup.alpha = 1f - (elapsed / _fadeInDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _fadeCanvasGroup.alpha = 0f;
        }

        private void MovePlayerToResultPosition()
        {
            if (_playerTransform == null || _resultPosition == null) return;

            // CharacterController 비활성화 후 위치 이동
            if (_playerCharacterController != null)
            {
                _playerCharacterController.enabled = false;
            }

            _playerTransform.position = _resultPosition.position;

            // LookAt 방향으로 회전
            if (_resultLookAt != null)
            {
                Vector3 lookDirection = _resultLookAt.position - _playerTransform.position;
                lookDirection.y = 0;
                if (lookDirection.sqrMagnitude > 0.01f)
                {
                    _playerTransform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }

            if (_playerCharacterController != null)
            {
                _playerCharacterController.enabled = true;
            }
        }

        private void ActivateResultCamera()
        {
            if (_resultCamera != null)
            {
                _resultCamera.gameObject.SetActive(true);
                _resultCamera.Priority = _resultCameraPriority;
            }

            if (_gameplayCamera != null)
            {
                _gameplayCamera.Priority = _gameplayCameraPriority;
            }
        }

        private void PlayRankAnimation()
        {
            if (_playerAnimator == null) return;

            string trigger = "Victory_Normal";

            if (_currentGrade != null && _animationData != null)
            {
                trigger = _animationData.GetAnimationTrigger(_currentGrade.Grade);
            }

            _playerAnimator.SetTrigger(trigger);
        }

        private void CompleteResultSequence()
        {
            // 페이드 즉시 해제
            if (_fadeCanvasGroup != null)
            {
                _fadeCanvasGroup.alpha = 0f;
            }

            // 플레이어 위치 즉시 이동
            MovePlayerToResultPosition();

            // 결과 카메라 즉시 활성화
            ActivateResultCamera();

            // 애니메이션 즉시 재생
            PlayRankAnimation();

            // UI 트리거
            OnResultReady?.Invoke();

            _isPlaying = false;
            _sequenceCoroutine = null;
        }

        private void SetPlayerInputEnabled(bool isEnabled)
        {
            var inputHandler = _playerTransform?.GetComponent<_02.Scripts.Player.Core.PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.SetInputEnabled(isEnabled);
            }
        }

        /// <summary>
        /// 결과 화면 종료 후 게임플레이 카메라로 복귀 (필요시)
        /// </summary>
        public void ReturnToGameplay()
        {
            if (_resultCamera != null)
            {
                _resultCamera.gameObject.SetActive(false);
            }

            if (_gameplayCamera != null)
            {
                _gameplayCamera.Priority = _resultCameraPriority;
            }

            SetPlayerInputEnabled(true);
        }
    }
}