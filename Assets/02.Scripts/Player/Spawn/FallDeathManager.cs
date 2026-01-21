using System;
using System.Linq;
using UnityEngine;
using _02.Scripts.Player.Movement;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Spawn
{
    /// <summary>
    /// 낙사 판정 및 리스폰 관리
    /// </summary>
    public class FallDeathManager : MonoBehaviour
    {
        [Header("Fall Death Settings")]
        [SerializeField] private float _deathHeight = -10f;
        [SerializeField] private float _safePositionUpdateInterval = 0.5f;

        [Header("References")]
        [SerializeField] private Transform _player;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerStateMachine _playerStateMachine;

        [Header("Spawn Points")]
        [SerializeField] private SpawnPoint[] _spawnPoints;
        [SerializeField] private bool _autoCollectSpawnPoints = true;

        private Vector3 _lastSafePosition;
        private float _safePositionTimer;

        public event Action OnFallDeath;
        public event Action<Vector3> OnRespawn;

        private void Start()
        {
            if (_autoCollectSpawnPoints)
                CollectSpawnPoints();

            if (_player == null)
                FindPlayerReferences();

            if (_player != null)
                _lastSafePosition = _player.position;
        }

        private void Update()
        {
            if (_player == null) return;

            UpdateSafePosition();
            CheckFallDeath();
        }

        private void CollectSpawnPoints()
        {
            _spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            if (_spawnPoints.Length == 0)
                Debug.LogWarning("[FallDeathManager] No SpawnPoints found in scene!");
        }

        private void FindPlayerReferences()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[FallDeathManager] Player not found!");
                return;
            }

            _player = player.transform;
            _characterController = player.GetComponent<CharacterController>();
            _playerMovement = player.GetComponent<PlayerMovement>();
            _playerStateMachine = player.GetComponent<PlayerStateMachine>();
        }

        private void UpdateSafePosition()
        {
            if (_playerMovement == null || !_playerMovement.IsGrounded) return;

            _safePositionTimer += Time.deltaTime;
            if (_safePositionTimer >= _safePositionUpdateInterval)
            {
                _safePositionTimer = 0f;
                _lastSafePosition = _player.position;
            }
        }

        private void CheckFallDeath()
        {
            if (_player.position.y < _deathHeight)
            {
                HandleFallDeath();
            }
        }

        private void HandleFallDeath()
        {
            OnFallDeath?.Invoke();

            var closestSpawn = FindClosestSpawnPoint(_lastSafePosition);
            if (closestSpawn != null)
            {
                Respawn(closestSpawn.Position);
            }
            else
            {
                // 스폰 포인트가 없으면 마지막 안전 위치로 복귀
                Respawn(_lastSafePosition);
            }
        }

        private SpawnPoint FindClosestSpawnPoint(Vector3 referencePosition)
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
                return null;

            // XZ 평면 기준 거리 계산 (Y축 무시)
            return _spawnPoints
                .OrderBy(sp => Vector2.Distance(
                    new Vector2(referencePosition.x, referencePosition.z),
                    new Vector2(sp.Position.x, sp.Position.z)))
                .FirstOrDefault();
        }

        private void Respawn(Vector3 position)
        {
            // CharacterController 비활성화 (위치 직접 변경을 위해)
            if (_characterController != null)
                _characterController.enabled = false;

            // 위치 이동
            _player.position = position;

            // CharacterController 재활성화
            if (_characterController != null)
                _characterController.enabled = true;

            // 속도 및 상태 초기화
            if (_playerMovement != null)
                _playerMovement.ResetVelocity();

            if (_playerStateMachine != null)
                _playerStateMachine.ChangeState<IdleState>();

            // 안전 위치 갱신
            _lastSafePosition = position;

            OnRespawn?.Invoke(position);

            Debug.Log($"[FallDeathManager] Player respawned at {position}");
        }

        private void OnDrawGizmosSelected()
        {
            // 낙사 높이 평면 시각화
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Vector3 center = transform.position;
            center.y = _deathHeight;
            Gizmos.DrawCube(center, new Vector3(100f, 0.1f, 100f));
        }
    }
}