using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputSystem : SingletonBehaviour<UIInputSystem>
{
    [SerializeField] private InputActionAsset _inputActionAsset;
    private InputAction _pauseAction;

    protected override void Init()
    {
        if (_inputActionAsset != null)
        {
            _pauseAction = _inputActionAsset.FindActionMap("UI")?.FindAction("Cancel");
        }
    }

    private void OnEnable()
    {
        if (_pauseAction == null) return;
        _pauseAction.performed += OnPausePerformed;
        _pauseAction.Enable();
    }

    private void OnDisable()
    {
        if (_pauseAction == null) return;
        _pauseAction.performed -= OnPausePerformed;
        _pauseAction.Disable();
    }
    
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (!CanPause()) return;
        if (!UIController.Instance.CloseLastUI())
        {
            // 닫을 UI가 없으면 Pause UI를 연다
            Pause();
        }
    }
    
    private bool CanPause()
    {
        ESceneType currentScene = SceneController.Instance.CurrentScene;
        return currentScene != ESceneType.LoginScene;
    }
    
    private void Pause()
    {
        UIController.Instance.OpenUI<UI_Pause>();
    }
}