using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 씬뷰처럼 자유롭게 이동할 수 있는 카메라 컨트롤러입니다.
/// Main Camera에 직접 붙여서 사용합니다.
/// 
/// New Input System 사용
/// 
/// 조작법:
/// - WASD: 전후좌우 이동
/// - 마우스: 시점 회전 (우클릭 또는 상시)
/// - Space: 상승
/// - Ctrl: 하강
/// - Shift: 빠른 이동
/// - 마우스 휠: 이동 속도 조절
/// </summary>
public class FreeCameraController : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("기본 이동 속도 (m/s)")]
    [SerializeField] private float _moveSpeed = 10f;
    
    [Tooltip("Shift 누를 때 속도 배율")]
    [SerializeField] private float _sprintMultiplier = 3f;
    
    [Tooltip("마우스 휠로 조절 가능한 최소 속도")]
    [SerializeField] private float _minSpeed = 1f;
    
    [Tooltip("마우스 휠로 조절 가능한 최대 속도")]
    [SerializeField] private float _maxSpeed = 100f;
    
    [Header("회전 설정")]
    [Tooltip("마우스 감도")]
    [SerializeField] private float _mouseSensitivity = 0.1f;
    
    [Tooltip("상하 회전 제한 (도)")]
    [SerializeField] private float _pitchLimit = 89f;
    
    [Header("조작 모드")]
    [Tooltip("체크: 우클릭 시에만 회전 / 해제: 항상 회전")]
    [SerializeField] private bool _requireRightClick = true;
    
    [Tooltip("게임 시작 시 커서 숨김")]
    [SerializeField] private bool _hideCursorOnStart = false;
    
    [Header("디버그")]
    [SerializeField] private bool _showSpeedOnScreen = true;
    
    // 내부 변수
    private float _pitch;  // 상하 회전 (X축)
    private float _yaw;    // 좌우 회전 (Y축)
    private bool _isRotating;
    
    // Input System 참조
    private Keyboard _keyboard;
    private Mouse _mouse;
    
    private void Start()
    {
        // Input System 디바이스 캐싱
        _keyboard = Keyboard.current;
        _mouse = Mouse.current;
        
        if (_keyboard == null || _mouse == null)
        {
            Debug.LogError("[FreeCameraController] 키보드 또는 마우스를 찾을 수 없습니다.");
            enabled = false;
            return;
        }
        
        // 현재 카메라 회전값으로 초기화
        Vector3 euler = transform.eulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;
        
        // pitch가 180도 이상이면 음수로 변환 (Unity euler 특성)
        if (_pitch > 180f)
        {
            _pitch -= 360f;
        }
        
        // 커서 설정
        if (_hideCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void Update()
    {
        if (_keyboard == null || _mouse == null) return;
        
        HandleSpeedChange();
        HandleRotation();
        HandleMovement();
        HandleCursor();
    }
    
    /// <summary>
    /// 마우스 휠로 이동 속도를 조절합니다.
    /// </summary>
    private void HandleSpeedChange()
    {
        // 마우스 휠 입력 (y값이 스크롤)
        float scroll = _mouse.scroll.ReadValue().y;
        
        if (scroll != 0f)
        {
            // 휠 위: 속도 증가, 휠 아래: 속도 감소
            float scrollNormalized = scroll * 0.01f;  // 값 정규화
            _moveSpeed *= (1f + scrollNormalized);
            _moveSpeed = Mathf.Clamp(_moveSpeed, _minSpeed, _maxSpeed);
        }
    }
    
    /// <summary>
    /// 마우스로 시점을 회전합니다.
    /// </summary>
    private void HandleRotation()
    {
        // 우클릭 모드: 우클릭 시에만 회전
        if (_requireRightClick)
        {
            _isRotating = _mouse.rightButton.isPressed;
            
            if (!_isRotating) return;
        }
        else
        {
            _isRotating = true;
        }
        
        // 마우스 델타 입력
        Vector2 mouseDelta = _mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * _mouseSensitivity;
        float mouseY = mouseDelta.y * _mouseSensitivity;
        
        // 회전 적용
        _yaw += mouseX;
        _pitch -= mouseY;
        
        // 상하 회전 제한
        _pitch = Mathf.Clamp(_pitch, -_pitchLimit, _pitchLimit);
        
        // 카메라에 회전 적용
        transform.eulerAngles = new Vector3(_pitch, _yaw, 0f);
    }
    
    /// <summary>
    /// WASD + Space/Ctrl로 이동합니다.
    /// </summary>
    private void HandleMovement()
    {
        // 입력 받기
        float horizontal = 0f;
        float vertical = 0f;
        float upDown = 0f;
        
        // WASD 입력
        if (_keyboard.wKey.isPressed) vertical += 1f;
        if (_keyboard.sKey.isPressed) vertical -= 1f;
        if (_keyboard.dKey.isPressed) horizontal += 1f;
        if (_keyboard.aKey.isPressed) horizontal -= 1f;
        
        // 상승/하강
        if (_keyboard.spaceKey.isPressed) upDown += 1f;
        if (_keyboard.ctrlKey.isPressed) upDown -= 1f;
        
        // 이동 방향 계산
        Vector3 moveDirection = transform.right * horizontal 
                              + transform.forward * vertical 
                              + Vector3.up * upDown;
        
        // 속도 계산 (Shift 누르면 빠르게)
        float currentSpeed = _moveSpeed;
        if (_keyboard.shiftKey.isPressed)
        {
            currentSpeed *= _sprintMultiplier;
        }
        
        // 이동 적용
        transform.position += moveDirection.normalized * currentSpeed * Time.deltaTime;
    }
    
    /// <summary>
    /// ESC 키로 커서를 토글합니다.
    /// </summary>
    private void HandleCursor()
    {
        // ESC 키로 커서 표시/숨김 토글
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
    
    // 화면에 현재 속도 표시 (디버그용)
    private void OnGUI()
    {
        if (!_showSpeedOnScreen) return;
        
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        
        string controls = _requireRightClick 
            ? "[우클릭+마우스] 회전" 
            : "[마우스] 회전";
        
        GUI.Label(new Rect(10, 10, 300, 100),
            $"=== Free Camera ===\n" +
            $"속도: {_moveSpeed:F1} m/s (휠로 조절)\n" +
            $"[WASD] 이동 | [Space/Ctrl] 상승/하강\n" +
            $"[Shift] 빠른 이동 | {controls}\n" +
            $"[ESC] 커서 토글",
            style);
    }
}
