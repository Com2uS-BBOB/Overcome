using UnityEngine;

public class LoginScene : MonoBehaviour
{
    public void HandleLoginCompleted()
    {
        LoadLobbyScene();
    }

    public void LoadLobbyScene()
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.LobbyScene);
    }
}
