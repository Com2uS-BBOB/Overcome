using UnityEngine;
using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Movement;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Core
{
    // 플레이어 메인 컨트롤러 (FSM 기반)
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerCombat _combat;
        [SerializeField] private CrescentSkill _crescent;
        [SerializeField] private DashAttackSkill _dashAttack;

        public PlayerInputHandler Input { get; private set; }
        public PlayerMovement Movement { get; private set; }
        public PlayerCombat Combat => _combat;
        public CrescentSkill Crescent => _crescent;
        public DashAttackSkill DashAttack => _dashAttack;
        public CharacterController CharacterController { get; private set; }
        public PlayerStats Stats { get; private set; }
        public PlayerStateMachine StateMachine { get; private set; }

        private void Awake()
        {
            Input = GetComponent<PlayerInputHandler>();
            Movement = GetComponent<PlayerMovement>();
            CharacterController = GetComponent<CharacterController>();
            Stats = GetComponent<PlayerStats>();

            if (_combat == null) _combat = GetComponent<PlayerCombat>();
            if (_crescent == null) _crescent = GetComponent<CrescentSkill>();
            if (_dashAttack == null) _dashAttack = GetComponent<DashAttackSkill>();

            Movement.Initialize(Stats);
            _combat?.Initialize(Stats);
            _crescent?.Initialize(Stats);
            _dashAttack?.Initialize(Stats);

            InitializeStateMachine();
            LockCursor();
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void InitializeStateMachine()
        {
            StateMachine = new PlayerStateMachine();
            StateMachine.RegisterState(new IdleState(this, StateMachine));
            StateMachine.RegisterState(new MoveState(this, StateMachine));
            StateMachine.RegisterState(new AttackState(this, StateMachine));
            StateMachine.RegisterState(new DashAttackState(this, StateMachine));
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

        private void Update() => StateMachine.Update();
        private void FixedUpdate() => StateMachine.FixedUpdate();

        private void HandleJump() => Movement.Jump();

        private void HandleDashAttack()
        {
            if (_dashAttack != null && _dashAttack.CanUse && !StateMachine.IsCurrentState<DashAttackState>())
                StateMachine.ChangeState<DashAttackState>();
        }

        private void HandleAttack()
        {
            if (_combat != null && _combat.CanAttack &&
                (_dashAttack == null || !_dashAttack.IsDashing) &&
                !StateMachine.IsCurrentState<AttackState>() && !StateMachine.IsCurrentState<DashAttackState>())
                StateMachine.ChangeState<AttackState>();
        }

        private void HandleCrescent()
        {
            if (_crescent != null && _crescent.CanUse) _crescent.Use();
        }
    }
}