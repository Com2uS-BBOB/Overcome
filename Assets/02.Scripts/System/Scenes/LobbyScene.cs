using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneController.Instance.LoadScene(ESceneType.SampleScene);
    }

    public void LoadTutorialScene()
    {
        SceneController.Instance.LoadScene(ESceneType.TutorialScene);
    }
}
