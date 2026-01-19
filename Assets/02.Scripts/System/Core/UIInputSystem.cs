using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputSystem : SingletonBehaviour<UIInputSystem>
{
    private InputSystem_Actions _inputActions;

    protected override void Init()
    {
        _inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        if (_inputActions == null) return;
        _inputActions.UI.Cancel.performed += OnPausePerformed;
        _inputActions.UI.OpenGuidePopup.performed += OpenGuidePopup;
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        if (_inputActions == null) return;
        _inputActions.UI.Cancel.performed -= OnPausePerformed;
        _inputActions.UI.OpenGuidePopup.performed -= OpenGuidePopup;
        _inputActions.Disable();
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

    private void OpenGuidePopup(InputAction.CallbackContext context)
    {
        // UIController.Instance.OpenUI<UI_GuidePopup>();
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