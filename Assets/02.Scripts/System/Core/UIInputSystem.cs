using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputSystem : SingletonBehaviour<UIInputSystem>
{
    private InputSystem_Actions _inputActions;
    private Dictionary<ESceneType, Action> _pauseActions;

    protected override void Init()
    {
        _inputActions = new InputSystem_Actions();
        InitializePauseActions();
    }

    private void InitializePauseActions()
    {
        _pauseActions = new Dictionary<ESceneType, Action>
        {
            { ESceneType.Stage1_1, () => OpenPauseUI(PauseUIConfig.InGame) },
            { ESceneType.Stage2_1, () => OpenPauseUI(PauseUIConfig.InGame) },
            { ESceneType.TutorialScene, () => OpenPauseUI(PauseUIConfig.InGame) },
            { ESceneType.LobbyScene, () => OpenPauseUI(PauseUIConfig.Lobby) },
            { ESceneType.LoginScene, Exit }
        };
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

        ESceneType currentScene = SceneController.Instance.CurrentScene;
        if (_pauseActions.TryGetValue(currentScene, out Action action))
        {
            action?.Invoke();
        }
    }

    private void OpenGuidePopup(InputAction.CallbackContext context)
    {
        if (ESceneType.Stage1_1 != SceneController.Instance.CurrentScene) return;
        if (ESceneType.Stage2_1 != SceneController.Instance.CurrentScene) return;
        bool isOpened = UI_GuidePopup.IsOpened;
        
        if (!isOpened)
        {
            _= UIController.Instance.OpenUI<UI_GuidePopup>();
        }
        else
        {
            UIController.Instance.CloseUI<UI_GuidePopup>();
        }
    }
    
    private async void OpenPauseUI(PauseUIConfig config)
    {
        UI_Pause pauseUI = await UIController.Instance.OpenUI<UI_Pause>();
        pauseUI.Setup(config);
    }

    private void Exit()
    {
        _= UIController.Instance.OpenUI<UI_Exit>();
    }
}