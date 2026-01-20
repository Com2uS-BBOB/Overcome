using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace _02.Scripts.CameraFX
{
    /// <summary>
    /// 스테이지 결과 연출 관리자
    /// 플레이어 위치 이동, 카메라 전환, 랭크별 애니메이션 재생
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

        [Header("Transition Settings")]
        [SerializeField] private float _transitionDelay = 0.5f;
        [SerializeField] private bool _teleportPlayer = true;

        [Header("Skip Settings")]
        [SerializeField] private bool _allowSkip = true;
        [SerializeField] private Key _skipKey = Key.Space;

        private bool _isPlaying;
        private bool _isSkipping;
        private GradeConfig _currentGrade;
        private Coroutine _sequenceCoroutine;

        public bool IsPlaying => _isPlaying;

        public event Action OnResultStarted;
        public event Action OnResultAnimationComplete;
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

        private void Update()
        {
            if (_isPlaying && _allowSkip && Keyboard.current != null && Keyboard.current[_skipKey].wasPressedThisFrame)
            {
                SkipResult();
            }
        }

        /// <summary>
        /// 결과 연출 시작
        /// </summary>
        public void ShowResult(GradeConfig gradeConfig)
        {
            if (_isPlaying) return;

            _currentGrade = gradeConfig;
            _isPlaying = true;
            _isSkipping = false;

            SetPlayerInputEnabled(false);

            OnResultStarted?.Invoke();

            _sequenceCoroutine = StartCoroutine(ResultSequenceCoroutine());
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
            // 1. 잠시 대기 (게임 종료 후 버퍼)
            yield return new WaitForSeconds(_transitionDelay);

            // 2. 플레이어를 결과 위치로 이동
            MovePlayerToResultPosition();

            // 3. 결과 카메라 활성화
            ActivateResultCamera();

            // 4. 카메라 전환 대기
            yield return new WaitForSeconds(0.3f);

            // 5. 랭크별 애니메이션 재생
            PlayRankAnimation();

            // 6. 애니메이션 완료 대기 (또는 일정 시간 후)
            yield return new WaitForSeconds(1f);

            // 7. 결과 UI 표시 트리거
            OnResultAnimationComplete?.Invoke();

            _isPlaying = false;
            _sequenceCoroutine = null;
        }

        private void MovePlayerToResultPosition()
        {
            if (_playerTransform == null || _resultPosition == null) return;

            if (_teleportPlayer)
            {
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
            if (_playerAnimator == null || _currentGrade == null) return;

            string trigger = _animationData != null
                ? _animationData.GetAnimationTrigger(_currentGrade.Grade)
                : "Victory_Normal";

            _playerAnimator.SetTrigger(trigger);
        }

        private void CompleteResultSequence()
        {
            // 플레이어 위치 즉시 이동
            MovePlayerToResultPosition();

            // 결과 카메라 즉시 활성화
            ActivateResultCamera();

            // 애니메이션 즉시 재생
            PlayRankAnimation();

            // 결과 UI 트리거
            OnResultAnimationComplete?.Invoke();

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

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}