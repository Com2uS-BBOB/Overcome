using System;
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
        if (UIController.Instance.CloseLastUI()) return;
        switch (SceneController.Instance.CurrentScene)
        {
            case ESceneType.SampleScene:
            case ESceneType.TutorialScene:
            case ESceneType.LobbyScene:
                Pause();
                break;
            case ESceneType.LoginScene:
                Exit();
                break;
        }
    }
    
    private void Pause()
    {
        _= UIController.Instance.OpenUI<UI_Pause>();
    }

    private void Exit()
    {
        _= UIController.Instance.OpenUI<UI_Exit>();
    }
}