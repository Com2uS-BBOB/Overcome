using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputSystem : SingletonBehaviour<UIInputSystem>
{
    private InputSystem_Actions _inputActions;
    private Dictionary<ESceneType, bool> _isIngame;
    private bool _isInputBlocked;

    protected override void Init()
    {
        _inputActions = new InputSystem_Actions();
        InitializePauseActions();
    }

    private void InitializePauseActions()
    {
        _isIngame = new Dictionary<ESceneType, bool>
        {
            { ESceneType.Stage1_1, true },
            { ESceneType.Stage1_2, true },
            { ESceneType.Stage1_3, true },
            { ESceneType.Stage2_1, true },
            { ESceneType.Stage2_2, true },
            { ESceneType.Stage2_3, true },
            { ESceneType.TutorialMap, true },
            { ESceneType.LobbyScene, false },
            { ESceneType.LoginScene, false },
            { ESceneType.LoadingScene, false }
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
        if (_isInputBlocked) return;
        if (UIController.Instance.CloseLastUI()) return;
        OpenPauseUI();
    }

    private void OpenGuidePopup(InputAction.CallbackContext context)
    {
        if (_isInputBlocked) return;
        ESceneType currentScene = SceneController.Instance.CurrentScene;
        if (!_isIngame[currentScene]) return;
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
    
    private async void OpenPauseUI()
    {
        ESceneType currentScene = SceneController.Instance.CurrentScene;
        if (currentScene == ESceneType.LoadingScene) return;
        if (currentScene == ESceneType.LoginScene)
        {
            _= UIController.Instance.OpenUI<UI_Exit>();
            return;
        }
        
        UI_Pause pauseUI = await UIController.Instance.OpenUI<UI_Pause>();

        bool isIngame = _isIngame[currentScene];
        PauseUIConfig config = isIngame ? PauseUIConfig.InGame : PauseUIConfig.Lobby;
        pauseUI?.Setup(config);
    }

    private void Exit()
    {
        _= UIController.Instance.OpenUI<UI_Exit>();
    }

    public void BlockInput()
    {
        _isInputBlocked = true;
    }

    public void UnblockInput()
    {
        _isInputBlocked = false;
    }
}