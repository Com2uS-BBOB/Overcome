using UnityEngine;
using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Movement;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Core
{
    /// <summary>
    /// 플레이어 메인 컨트롤러
    /// FSM 기반 상태 관리
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerRuntimeStats))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerCombat _combat;
        [SerializeField] private CrescentSkill _crescent;

        // 컴포넌트 참조 (public for states)
        public PlayerInputHandler Input { get; private set; }
        public PlayerMovement Movement { get; private set; }
        public PlayerCombat Combat => _combat;
        public CrescentSkill Crescent => _crescent;
        public CharacterController CharacterController { get; private set; }
        public PlayerRuntimeStats Stats { get; private set; }

        // State Machine
        public PlayerStateMachine StateMachine { get; private set; }

        private void Awake()
        {
            Input = GetComponent<PlayerInputHandler>();
            Movement = GetComponent<PlayerMovement>();
            CharacterController = GetComponent<CharacterController>();
            Stats = GetComponent<PlayerRuntimeStats>();

            if (_combat == null)
            {
                _combat = GetComponent<PlayerCombat>();
            }

            if (_crescent == null)
            {
                _crescent = GetComponent<CrescentSkill>();
            }

            // Stats 주입
            Movement.Initialize(Stats);
            if (_combat != null)
            {
                _combat.Initialize(Stats);
            }
            if (_crescent != null)
            {
                _crescent.Initialize(Stats);
            }

            InitializeStateMachine();
        }

        private void InitializeStateMachine()
        {
            StateMachine = new PlayerStateMachine();

            // 상태 등록
            StateMachine.RegisterState(new IdleState(this, StateMachine));
            StateMachine.RegisterState(new MoveState(this, StateMachine));
            StateMachine.RegisterState(new AttackState(this, StateMachine));
            StateMachine.RegisterState(new DashAttackState(this, StateMachine));

            // 초기 상태
            StateMachine.Initialize<IdleState>();
        }

        private void OnEnable()
        {
            Input.OnJumpPerformed += HandleJump;
            Input.OnDashAttackPerformed += HandleDashAttack;
            Input.OnAttackPerformed += HandleAttack;
            Input.OnCrescentPerformed += HandleCrescent;
        }

        private void OnDisable()
        {
            Input.OnJumpPerformed -= HandleJump;
            Input.OnDashAttackPerformed -= HandleDashAttack;
            Input.OnAttackPerformed -= HandleAttack;
            Input.OnCrescentPerformed -= HandleCrescent;
        }

        private void Update()
        {
            StateMachine.Update();
        }

        private void FixedUpdate()
        {
            StateMachine.FixedUpdate();
        }

        private void HandleJump()
        {
            // 점프는 모든 상태에서 가능 (공중 체크는 Movement에서)
            Movement.Jump();
        }

        private void HandleDashAttack()
        {
            // 질풍참 가능할 때만 상태 전환
            if (Movement.CanDash && !StateMachine.IsCurrentState<DashAttackState>())
            {
                StateMachine.ChangeState<DashAttackState>();
            }
        }

        private void HandleAttack()
        {
            // 공격 가능할 때만 상태 전환
            if (_combat != null &&
                _combat.CanAttack &&
                !Movement.IsDashing &&
                !StateMachine.IsCurrentState<AttackState>() &&
                !StateMachine.IsCurrentState<DashAttackState>())
            {
                StateMachine.ChangeState<AttackState>();
            }
        }

        private void HandleCrescent()
        {
            // 크레센트 발사 (상태 전환 없이 즉시 발사)
            if (_crescent != null && _crescent.CanFire)
            {
                _crescent.Fire();
            }
        }
    }
}