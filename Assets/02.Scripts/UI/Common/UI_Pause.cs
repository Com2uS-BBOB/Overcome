using UnityEngine;
using UnityEngine.UI;

public struct PauseUIConfig
{
    public bool ShowRetry;
    public bool ShowToLobby;
    public bool ShowToTitle;

    public static PauseUIConfig InGame => new PauseUIConfig
    {
        ShowRetry = true,
        ShowToLobby = true,
        ShowToTitle = false
    };

    public static PauseUIConfig Lobby => new PauseUIConfig
    {
        ShowRetry = false,
        ShowToLobby = false,
        ShowToTitle = true
    };
}

public class UI_Pause : BaseUI
{
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _controlButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _toLobbyButton;
    [SerializeField] private Button _toTitleButton;
    [SerializeField] private Button _exitGameButton;

    public void Setup(PauseUIConfig config)
    {
        _retryButton.gameObject.SetActive(config.ShowRetry);
        _toLobbyButton.gameObject.SetActive(config.ShowToLobby);
        _toTitleButton.gameObject.SetActive(config.ShowToTitle);
    }
    
    public void Retry()
    {
        if (GameEventHandler.IsOnGame)
        {
            GameEventHandler.GameEnd();
        }
        SceneController.Instance.ReloadCurrentScene();
    }

    public void OpenControls()
    {
        _= UIController.Instance.OpenUI<UI_ControlGuide>();
    }

    public void OpenSettings()
    {
        _= UIController.Instance.OpenUI<UI_Settings>();
    }

    public void GotoLobby()
    {
        if (GameEventHandler.IsOnGame)
        {
            GameEventHandler.GameEnd();
        }
        SceneController.Instance.LoadSceneAsync(ESceneType.LobbyScene);
    }

    public void GotoTitle()
    {
        if (GameEventHandler.IsOnGame)
        {
            GameEventHandler.GameEnd();
        }
        SceneController.Instance.LoadSceneAsync(ESceneType.LoginScene);
    }

    public void ExitGame()
    {
        _= UIController.Instance.OpenUI<UI_Exit>();
    }
}