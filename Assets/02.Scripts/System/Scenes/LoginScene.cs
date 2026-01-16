using UnityEngine;

public class LoginScene : MonoBehaviour
{
    public void LoadLobbyScene()
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.LobbyScene);
    }
}
