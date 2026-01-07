using UnityEngine;
using _02.Scripts.Player.Animation;
using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Gauge;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Movement;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Core
{
    // 플레이어 메인 컨트롤러 
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DragonSwordSkill _dragonSwordSkill;
        [SerializeField] private CrescentSkill _crescent;
        [SerializeField] private DashAttackSkill _dashAttack;
        [SerializeField] private GaugeManager _gaugeManager;

        [Header("Animation")]
        [SerializeField] private PlayerAnimator _playerAnimator;

        public PlayerInputHandler Input { get; private set; }
        public PlayerMovement Movement { get; private set; }
        public DragonSwordSkill DragonSwordSkill => _dragonSwordSkill;
        public CrescentSkill Crescent => _crescent;
        public DashAttackSkill DashAttack => _dashAttack;
        public GaugeManager GaugeManager => _gaugeManager;
        public CharacterController CharacterController { get; private set; }
        public PlayerStats Stats { get; private set; }
        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerAnimator PlayerAnimator => _playerAnimator;

        private void Awake()
        {
            Input = GetComponent<PlayerInputHandler>();
            Movement = GetComponent<PlayerMovement>();
            CharacterController = GetComponent<CharacterController>();
            Stats = GetComponent<PlayerStats>();

            if (_dragonSwordSkill == null) _dragonSwordSkill = GetComponent<DragonSwordSkill>();
            if (_crescent == null) _crescent = GetComponent<CrescentSkill>();
            if (_dashAttack == null) _dashAttack = GetComponent<DashAttackSkill>();
            if (_gaugeManager == null) _gaugeManager = GetComponent<GaugeManager>();

            Movement.Initialize(Stats);
            _dragonSwordSkill?.Initialize(Stats);
            _crescent?.Initialize(Stats, _gaugeManager);
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
            StateMachine.RegisterState(new DragonSwordState(this, StateMachine));
            StateMachine.RegisterState(new DashAttackState(this, StateMachine));

            // PlayerAnimator 초기화 (State 변경 구독)
            _playerAnimator?.Initialize(StateMachine);

            StateMachine.Initialize<IdleState>();
        }

        private void OnEnable()
        {
            Input.OnJumpPerformed += HandleJump;
            Input.OnDashAttackPerformed += HandleDashAttack;
            Input.OnAttackPerformed += HandleAttack;
            Input.OnCrescentPerformed += HandleCrescent;
            Input.OnOverDrivePerformed += HandleOverDrive;

            // 오버드라이브 게이지 충전 연결
            if (_dragonSwordSkill != null) _dragonSwordSkill.OnEnemyHit += HandleEnemyHitForOverDrive;
            if (_crescent != null) _crescent.OnEnemyHit += HandleEnemyHitForOverDrive;

            // 콤보 애니메이션 연결
            if (_dragonSwordSkill != null) _dragonSwordSkill.OnComboAttack += HandleComboAttack;
        }

        private void OnDisable()
        {
            Input.OnJumpPerformed -= HandleJump;
            Input.OnDashAttackPerformed -= HandleDashAttack;
            Input.OnAttackPerformed -= HandleAttack;
            Input.OnCrescentPerformed -= HandleCrescent;
            Input.OnOverDrivePerformed -= HandleOverDrive;

            if (_dragonSwordSkill != null) _dragonSwordSkill.OnEnemyHit -= HandleEnemyHitForOverDrive;
            if (_crescent != null) _crescent.OnEnemyHit -= HandleEnemyHitForOverDrive;

            // 콤보 애니메이션 연결 해제
            if (_dragonSwordSkill != null) _dragonSwordSkill.OnComboAttack -= HandleComboAttack;
        }

        private void Update()
        {
            StateMachine.Update();

            // 지면 상태 동기화
            _playerAnimator?.SetGrounded(Movement.IsGrounded);
        }

        private void FixedUpdate() => StateMachine.FixedUpdate();

        private void HandleJump()
        {
            if (Movement.IsGrounded)
            {
                Movement.Jump();
                _playerAnimator?.PlayJump();
            }
        }

        private void HandleDashAttack()
        {
            if (_dashAttack != null && _dashAttack.CanUse && !StateMachine.IsCurrentState<DashAttackState>())
                StateMachine.ChangeState<DashAttackState>();
        }

        private void HandleAttack()
        {
            if (_dragonSwordSkill == null) return;
            if (_dashAttack != null && _dashAttack.IsDashing) return;
            if (StateMachine.IsCurrentState<DashAttackState>()) return;

            if (_dragonSwordSkill.CanAttack)
            {
                StateMachine.ChangeState<DragonSwordState>();
                _dragonSwordSkill.Attack();
            }
        }

        private void HandleCrescent()
        {
            if (_crescent != null && _crescent.CanUse)
            {
                Movement.RotateToCamera();
                _crescent.Use();
                _playerAnimator?.PlayCrescent(Movement.IsGrounded);
            }
        }

        // 오버드라이브 발동
        private void HandleOverDrive() => _gaugeManager?.TryActivateOverDrive();

        // 적 적중 시 오버드라이브 게이지 충전
        private void HandleEnemyHitForOverDrive(IDamageable target, float damage) => _gaugeManager?.ChargeOverDriveOnHit();

        // 콤보 공격 애니메이션
        private void HandleComboAttack(int comboStep) => _playerAnimator?.PlayAttack(comboStep, Movement.IsGrounded);
    }
}