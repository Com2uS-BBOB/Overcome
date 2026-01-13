using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TPS(3인칭) 뷰 플레이어 컨트롤러.
/// 카메라가 플레이어 뒤에서 따라다니며, 콜라이더 충돌을 테스트한다.
/// 
/// 조작법:
/// - WASD: 이동 (카메라 기준 방향)
/// - Space: 점프
/// - Shift: 달리기
/// - 마우스: 카메라 회전
/// - ESC: 커서 토글
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class TestPlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("기본 이동 속도 (m/s)")]
    [SerializeField] private float _moveSpeed = 6f;
    
    [Tooltip("달리기 속도 배율")]
    [SerializeField] private float _sprintMultiplier = 2f;
    
    [Tooltip("점프 높이 (m)")]
    [SerializeField] private float _jumpHeight = 1.5f;
    
    [Tooltip("중력 가속도")]
    [SerializeField] private float _gravity = -20f;
    
    [Tooltip("플레이어 회전 속도")]
    [SerializeField] private float _rotationSpeed = 10f;
    
    [Header("카메라 설정")]
    [Tooltip("플레이어 시점 카메라")]
    [SerializeField] private Transform _cameraTransform;
    
    [Tooltip("카메라와 플레이어 사이 거리")]
    [SerializeField] private float _cameraDistance = 5f;
    
    [Tooltip("카메라 높이 오프셋")]
    [SerializeField] private float _cameraHeight = 2f;
    
    [Tooltip("카메라가 바라볼 플레이어 높이 오프셋")]
    [SerializeField] private float _lookAtHeight = 1f;
    
    [Header("회전 설정")]
    [Tooltip("마우스 감도")]
    [SerializeField] private float _mouseSensitivity = 0.2f;
    
    [Tooltip("상하 회전 최소 각도")]
    [SerializeField] private float _pitchMin = -20f;
    
    [Tooltip("상하 회전 최대 각도")]
    [SerializeField] private float _pitchMax = 60f;
    
    [Header("디버그")]
    [SerializeField] private bool _showDebugInfo = true;
    
    // 컴포넌트 참조
    private CharacterController _controller;
    
    // 카메라 회전 상태
    private float _yaw;     // 좌우 회전 (Y축)
    private float _pitch;   // 상하 회전 (X축)
    
    // 이동 상태
    private Vector3 _velocity;
    private bool _isGrounded;
    
    // Input System 참조
    private Keyboard _keyboard;
    private Mouse _mouse;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        
        if (_controller == null)
        {
            Debug.LogError("[TestPlayerController] CharacterController가 필요합니다.");
            enabled = false;
        }
    }

    private void Start()
    {
        // Input System 디바이스 캐싱
        _keyboard = Keyboard.current;
        _mouse = Mouse.current;
        
        if (_keyboard == null || _mouse == null)
        {
            Debug.LogError("[TestPlayerController] 키보드 또는 마우스를 찾을 수 없습니다.");
            enabled = false;
            return;
        }
        
        // 카메라 자동 탐색
        if (_cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                _cameraTransform = mainCam.transform;
                
                // 카메라가 플레이어 자식이면 분리 (TPS는 독립 카메라 필요)
                if (_cameraTransform.parent == transform)
                {
                    _cameraTransform.SetParent(null);
                }
            }
            else
            {
                Debug.LogError("[TestPlayerController] 카메라를 찾을 수 없습니다.");
            }
        }
        
        // 초기 카메라 각도 설정
        _yaw = transform.eulerAngles.y;
        _pitch = 20f; // 약간 위에서 내려다보는 각도
        
        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 초기 카메라 위치 설정
        UpdateCameraPosition();
    }

    private void Update()
    {
        if (_keyboard == null || _mouse == null) return;
        
        CheckGrounded();
        HandleCameraRotation();
        HandleMovement();
        HandleCursor();
    }

    private void LateUpdate()
    {
        // 카메라 위치는 LateUpdate에서 (플레이어 이동 후)
        UpdateCameraPosition();
    }

    /// <summary>
    /// 땅에 닿아있는지 확인한다.
    /// </summary>
    private void CheckGrounded()
    {
        _isGrounded = _controller.isGrounded;
        
        if (_isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f;
        }
    }

    /// <summary>
    /// 마우스로 카메라를 회전한다.
    /// 카메라가 플레이어 주위를 공전하는 방식.
    /// </summary>
    private void HandleCameraRotation()
    {
        Vector2 mouseDelta = _mouse.delta.ReadValue();
        
        // 좌우 회전
        _yaw += mouseDelta.x * _mouseSensitivity;
        
        // 상하 회전 (제한 있음)
        _pitch -= mouseDelta.y * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);
    }

    /// <summary>
    /// WASD로 이동한다.
    /// 카메라가 바라보는 방향 기준으로 이동한다.
    /// </summary>
    private void HandleMovement()
    {
        // 입력 받기
        float horizontal = 0f;
        float vertical = 0f;
        
        if (_keyboard.wKey.isPressed) vertical += 1f;
        if (_keyboard.sKey.isPressed) vertical -= 1f;
        if (_keyboard.dKey.isPressed) horizontal += 1f;
        if (_keyboard.aKey.isPressed) horizontal -= 1f;
        
        // 입력이 있을 때만 이동 처리
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        if (inputDirection.magnitude >= 0.1f)
        {
            // 카메라 기준 이동 방향 계산
            // Y축 회전만 사용 (수평 이동)
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _yaw;
            
            // 플레이어를 이동 방향으로 부드럽게 회전
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, _rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            // 이동 방향
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            // 속도 계산
            float currentSpeed = _moveSpeed;
            if (_keyboard.shiftKey.isPressed)
            {
                currentSpeed *= _sprintMultiplier;
            }
            
            // 수평 이동
            Vector3 horizontalMove = moveDirection * currentSpeed * Time.deltaTime;
            
            // CharacterController로 이동
            _controller.Move(horizontalMove);
        }
        
        // 점프
        if (_keyboard.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }
        
        // 중력 적용
        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    /// <summary>
    /// ESC 키로 커서를 토글한다.
    /// </summary>
    private void HandleCursor()
    {
        if (_keyboard.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    /// <summary>
    /// 카메라를 플레이어 뒤에 위치시킨다.
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (_cameraTransform == null) return;
        
        // 플레이어 기준점 (허리 높이)
        Vector3 lookAtPoint = transform.position + Vector3.up * _lookAtHeight;
        
        // 카메라 회전값으로 방향 계산
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        
        // 플레이어 뒤쪽 위치 계산
        Vector3 cameraOffset = rotation * new Vector3(0f, 0f, -_cameraDistance);
        cameraOffset.y += _cameraHeight;
        
        // 카메라 위치 설정
        _cameraTransform.position = lookAtPoint + cameraOffset;
        
        // 카메라가 플레이어를 바라보도록
        _cameraTransform.LookAt(lookAtPoint);
    }

    // 디버그 정보 표시
    private void OnGUI()
    {
        if (!_showDebugInfo) return;
        
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14
        };
        style.normal.textColor = Color.white;
        
        string groundedText = _isGrounded ? "예" : "아니오";
        
        GUI.Label(new Rect(10, 10, 350, 150),
            $"=== TPS Player (충돌 테스트) ===\n" +
            $"위치: {transform.position:F1}\n" +
            $"속도: {_moveSpeed:F1} m/s\n" +
            $"땅에 닿음: {groundedText}\n" +
            $"[WASD] 이동 | [Space] 점프\n" +
            $"[Shift] 달리기 | [마우스] 카메라 회전\n" +
            $"[ESC] 커서 토글",
            style);
    }
}
