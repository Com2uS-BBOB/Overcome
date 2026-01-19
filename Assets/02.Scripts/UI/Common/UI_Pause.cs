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
        SceneController.Instance.ReloadCurrentScene();
    }

    public void OpenControls()
    {
        _= UIController.Instance.OpenUI<UI_ControlGuide>();
        UIController.Instance.CloseUI(this);
    }

    public void OpenSettings()
    {
        _= UIController.Instance.OpenUI<UI_Settings>();
        UIController.Instance.CloseUI(this);
    }

    public void GotoLobby()
    {
        SceneController.Instance.LoadScene(ESceneType.LobbyScene);
    }

    public void GotoTitle()
    {
        SceneController.Instance.LoadScene(ESceneType.LoginScene);
    }

    public void ExitGame()
    {
        _= UIController.Instance.OpenUI<UI_Exit>();
    }
}