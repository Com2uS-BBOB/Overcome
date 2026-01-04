using UnityEngine;
using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Movement;

namespace _02.Scripts.Player.Core
{
    /// <summary>
    /// 플레이어 메인 컨트롤러
    /// 입력 → 이동/전투 연결
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerCombat _combat;

        private PlayerInputHandler _input;
        private PlayerMovement _movement;
        private CharacterController _controller;

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();
            _movement = GetComponent<PlayerMovement>();
            _controller = GetComponent<CharacterController>();

            if (_combat == null)
            {
                _combat = GetComponent<PlayerCombat>();
            }
        }

        private void OnEnable()
        {
            _input.OnJumpPerformed += HandleJump;
            _input.OnDashAttackPerformed += HandleDashAttack;
            _input.OnAttackPerformed += HandleAttack;
        }

        private void OnDisable()
        {
            _input.OnJumpPerformed -= HandleJump;
            _input.OnDashAttackPerformed -= HandleDashAttack;
            _input.OnAttackPerformed -= HandleAttack;
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            _movement.Move(_input.MoveInput);
        }

        private void HandleJump()
        {
            _movement.Jump();
        }

        private void HandleDashAttack()
        {
            _movement.DashAttack();
        }

        private void HandleAttack()
        {
            if (_combat != null)
            {
                _combat.Attack();
            }
        }
    }
}