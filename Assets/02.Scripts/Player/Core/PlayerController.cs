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
        [SerializeField] private PlayerAnimatorController _playerAnimatorController;

        [Header("Combat System")]
        [SerializeField] private CombatStateHandler _combatStateHandler;
        [SerializeField] private InputBuffer _inputBuffer;

        public PlayerInputHandler Input { get; private set; }
        public CombatStateHandler CombatStateHandler => _combatStateHandler;
        public InputBuffer InputBuffer => _inputBuffer;
        public PlayerMovement Movement { get; private set; }
        public DragonSwordSkill DragonSwordSkill => _dragonSwordSkill;
        public CrescentSkill Crescent => _crescent;
        public DashAttackSkill DashAttack => _dashAttack;
        public GaugeManager GaugeManager => _gaugeManager;
        public CharacterController CharacterController { get; private set; }
        public PlayerStats Stats { get; private set; }
        public PlayerStateMachine StateMachine { get; private set; }
        public CancelManager CancelManager { get; private set; }
        public PlayerAnimatorController PlayerAnimatorController => _playerAnimatorController;

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
            _dragonSwordSkill?.Initialize(Stats, Movement);
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
            StateMachine.RegisterState(new JumpState(this, StateMachine));

            // 지상 전투 상태
            StateMachine.RegisterState(new DragonSwordState(this, StateMachine));
            StateMachine.RegisterState(new CrescentState(this, StateMachine));
            StateMachine.RegisterState(new DashAttackState(this, StateMachine));

            // 공중 전투 상태
            StateMachine.RegisterState(new AirDragonSwordState(this, StateMachine));
            StateMachine.RegisterState(new AirCrescentState(this, StateMachine));
            StateMachine.RegisterState(new AirDashAttackState(this, StateMachine));

            // CancelManager 초기화
            CancelManager = new CancelManager(StateMachine);

            // CombatStateHandler 연결 (있는 경우)
            if (_combatStateHandler != null && _inputBuffer != null)
            {
                CancelManager.SetCombatHandler(_combatStateHandler, _inputBuffer);
                CancelManager.OnActionExecute += HandleActionExecute;
            }

            // PlayerAnimator 초기화 (State 변경 구독)
            _playerAnimatorController?.Initialize(StateMachine);

            StateMachine.Initialize<IdleState>();
        }

        /// <summary>
        /// CancelManager에서 버퍼된 액션 실행 시 호출
        /// </summary>
        private void HandleActionExecute(ActionType action)
        {
            // 다른 스킬에서 전환하는 경우 이전 스킬 정리
            ResetPreviousSkillIfNeeded(action);

            switch (action)
            {
                case ActionType.Attack:
                    ExecuteAttack();
                    break;
                case ActionType.Skill:
                    ExecuteCrescent();
                    break;
                case ActionType.DashAttack:
                    ExecuteDashAttack();
                    break;
                case ActionType.Jump:
                    ExecuteJump();
                    break;
            }
        }

        /// <summary>
        /// 스킬 전환 시 이전 스킬 콤보 리셋
        /// </summary>
        private void ResetPreviousSkillIfNeeded(ActionType newAction)
        {
            var currentState = StateMachine.CurrentState;

            // 크레센트 → 용검 전환
            if (newAction == ActionType.Attack &&
                (currentState is CrescentState || currentState is AirCrescentState))
            {
                _crescent?.ResetCombo();
            }
            // 용검 → 크레센트 전환
            else if (newAction == ActionType.Skill &&
                     (currentState is DragonSwordState || currentState is AirDragonSwordState))
            {
                _dragonSwordSkill?.ResetCombo();
            }
        }

        private void ExecuteAttack()
        {
            if (_dragonSwordSkill == null) return;

            bool isGrounded = Movement.IsGrounded;

            // Case 1: 첫 공격
            if (_dragonSwordSkill.CanAttack)
            {
                if (isGrounded)
                    StateMachine.ChangeState<DragonSwordState>();
                else
                    StateMachine.ChangeState<AirDragonSwordState>();
                _dragonSwordSkill.Attack();
                _combatStateHandler?.SetCurrentAttackFromSkill(_dragonSwordSkill);
            }
            // Case 2: 공격 중 콤보 큐잉 (CombatStateHandler가 윈도우 체크 완료 후 호출됨)
            else if (_dragonSwordSkill.IsAttacking && _dragonSwordSkill.CanContinueCombo)
            {
                _dragonSwordSkill.QueueCombo();
            }
            // Case 3: 콤보 유예
            else if (_dragonSwordSkill.CanComboGrace)
            {
                if (isGrounded)
                    StateMachine.ChangeState<DragonSwordState>();
                else
                    StateMachine.ChangeState<AirDragonSwordState>();
                _dragonSwordSkill.ExecuteGraceCombo();
                _combatStateHandler?.SetCurrentAttackFromSkill(_dragonSwordSkill);
            }
        }

        private void ExecuteCrescent()
        {
            if (_crescent == null) return;

            bool isGrounded = Movement.IsGrounded;

            // Case 1: 첫 스킬 사용
            if (_crescent.CanUse)
            {
                if (isGrounded)
                    StateMachine.ChangeState<CrescentState>();
                else
                    StateMachine.ChangeState<AirCrescentState>();
                _crescent.Attack();
                _combatStateHandler?.SetCurrentAttackFromSkill(_crescent);
            }
            // Case 2: 스킬 사용 중 콤보 큐잉 (CombatStateHandler가 윈도우 체크 완료 후 호출됨)
            else if (_crescent.IsUsing && _crescent.CanContinueCombo)
            {
                _crescent.QueueCombo();
            }
            // Case 3: 콤보 유예
            else if (_crescent.CanComboGrace)
            {
                if (isGrounded)
                    StateMachine.ChangeState<CrescentState>();
                else
                    StateMachine.ChangeState<AirCrescentState>();
                _crescent.ExecuteGraceCombo();
                _combatStateHandler?.SetCurrentAttackFromSkill(_crescent);
            }
        }

        private void ExecuteDashAttack()
        {
            if (_dashAttack == null || !_dashAttack.CanUse) return;

            if (Movement.IsGrounded)
                StateMachine.ChangeState<DashAttackState>();
            else
                StateMachine.ChangeState<AirDashAttackState>();
        }

        private void ExecuteJump()
        {
            int jumpResult = Movement.TryJump();
            switch (jumpResult)
            {
                case 1:
                    _playerAnimatorController?.PlayJump();
                    break;
                case 2:
                    _playerAnimatorController?.PlayDoubleJump();
                    break;
            }
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
            if (_crescent != null) _crescent.OnComboAttack += HandleCrescentCombo;

            // Root Motion 이벤트 연결
            if (_playerAnimatorController != null)
                _playerAnimatorController.OnRootMotionUpdate += HandleRootMotion;
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
            if (_crescent != null) _crescent.OnComboAttack -= HandleCrescentCombo;

            // Root Motion 이벤트 연결 해제
            if (_playerAnimatorController != null)
                _playerAnimatorController.OnRootMotionUpdate -= HandleRootMotion;
        }

        private void Update()
        {
            StateMachine.Update();

            // 지면 상태 동기화
            _playerAnimatorController?.SetGrounded(Movement.IsGrounded);
        }

        private void FixedUpdate() => StateMachine.FixedUpdate();

        private void HandleJump()
        {
            int jumpResult = Movement.TryJump();

            switch (jumpResult)
            {
                case 1: // 1단 점프
                    _playerAnimatorController?.PlayJump();
                    break;
                case 2: // 2단 점프
                    _playerAnimatorController?.PlayDoubleJump();
                    break;
            }
        }

        private void HandleDashAttack()
        {
            if (_dashAttack == null || !_dashAttack.CanUse) return;

            bool isGrounded = Movement.IsGrounded;

            // 지상/공중에 따른 캔슬 체크 및 State 전환
            if (isGrounded)
            {
                if (CancelManager.CanCancelTo<DashAttackState>())
                    StateMachine.ChangeState<DashAttackState>();
            }
            else
            {
                if (CancelManager.CanCancelTo<AirDashAttackState>())
                    StateMachine.ChangeState<AirDashAttackState>();
            }
        }

        private void HandleAttack()
        {
            if (_dragonSwordSkill == null) return;

            bool isGrounded = Movement.IsGrounded;

            // 지상/공중에 따른 캔슬 체크
            if (isGrounded)
            {
                if (!CancelManager.CanCancelTo<DragonSwordState>()) return;
            }
            else
            {
                if (!CancelManager.CanCancelTo<AirDragonSwordState>()) return;
            }

            // Case 1: 첫 공격
            if (_dragonSwordSkill.CanAttack)
            {
                StartNewDragonSwordCombo(isGrounded);
                return;
            }

            // Case 2: 콤보 큐잉 (CombatStateHandler가 윈도우 체크)
            if (_combatStateHandler != null && _combatStateHandler.CanQueueCombo(ActionType.Attack))
            {
                _dragonSwordSkill.QueueCombo();
                return;
            }

            // Case 3: 콤보 유예 (공격 끝난 직후)
            if (_dragonSwordSkill.CanComboGrace)
            {
                if (isGrounded)
                    StateMachine.ChangeState<DragonSwordState>();
                else
                    StateMachine.ChangeState<AirDragonSwordState>();
                _dragonSwordSkill.ExecuteGraceCombo();
                _combatStateHandler?.SetCurrentAttackFromSkill(_dragonSwordSkill);
                return;
            }

            // Case 4: 캔슬 불가 → 입력 버퍼링
            _combatStateHandler?.BufferIfNeeded(ActionType.Attack);
        }

        private void StartNewDragonSwordCombo(bool isGrounded)
        {
            if (isGrounded)
                StateMachine.ChangeState<DragonSwordState>();
            else
                StateMachine.ChangeState<AirDragonSwordState>();

            _dragonSwordSkill.Attack();
            _combatStateHandler?.SetCurrentAttackFromSkill(_dragonSwordSkill);
        }

        private void HandleCrescent()
        {
            if (_crescent == null) return;

            bool isGrounded = Movement.IsGrounded;

            // 지상/공중에 따른 캔슬 체크
            if (isGrounded)
            {
                if (!CancelManager.CanCancelTo<CrescentState>()) return;
            }
            else
            {
                if (!CancelManager.CanCancelTo<AirCrescentState>()) return;
            }

            // Case 1: 첫 공격
            if (_crescent.CanUse)
            {
                StartNewCrescentCombo(isGrounded);
                return;
            }

            // Case 2: 콤보 큐잉 (CombatStateHandler가 윈도우 체크)
            if (_combatStateHandler != null && _combatStateHandler.CanQueueCombo(ActionType.Skill))
            {
                _crescent.QueueCombo();
                return;
            }

            // Case 3: 콤보 유예 (스킬 끝난 직후)
            if (_crescent.CanComboGrace)
            {
                if (isGrounded)
                    StateMachine.ChangeState<CrescentState>();
                else
                    StateMachine.ChangeState<AirCrescentState>();
                _crescent.ExecuteGraceCombo();
                _combatStateHandler?.SetCurrentAttackFromSkill(_crescent);
                return;
            }

            // Case 4: 캔슬 불가 → 입력 버퍼링
            _combatStateHandler?.BufferIfNeeded(ActionType.Skill);
        }

        private void StartNewCrescentCombo(bool isGrounded)
        {
            if (isGrounded)
                StateMachine.ChangeState<CrescentState>();
            else
                StateMachine.ChangeState<AirCrescentState>();

            _crescent.Attack();
            _combatStateHandler?.SetCurrentAttackFromSkill(_crescent);
        }

        // 오버드라이브 발동
        private void HandleOverDrive() => _gaugeManager?.TryActivateOverDrive();

        // 적 적중 시 오버드라이브 게이지 충전 및 UI 이벤트 전달
        private void HandleEnemyHitForOverDrive(IDamageable target, float damage)
        {
            _gaugeManager?.ChargeOverDriveOnHit();
            HitEventManager.Instance?.NotifyDamageDealt(damage);
        }

        // 용검 콤보 공격 애니메이션 (콤보 시작 시점의 IsGrounded 사용)
        private void HandleComboAttack(int comboStep) => _playerAnimatorController?.PlayAttack(comboStep, _dragonSwordSkill.WasGroundedOnComboStart);

        // 크레센트 콤보 공격 애니메이션
        private void HandleCrescentCombo(int comboStep) => _playerAnimatorController?.PlayCrescent(comboStep, Movement.IsGrounded);

        // Root Motion 처리 - 로컬 좌표를 플레이어 기준 월드 좌표로 변환
        private void HandleRootMotion(Vector3 localDelta)
        {
            if (localDelta.sqrMagnitude > 0.000001f)
            {
                // 로컬 방향을 플레이어 기준 월드 좌표로 변환
                Vector3 worldMovement = transform.TransformDirection(localDelta);
                worldMovement.y = 0;  // Y축 제거 (중력 시스템이 담당)
                CharacterController.Move(worldMovement);
            }
        }
    }
}