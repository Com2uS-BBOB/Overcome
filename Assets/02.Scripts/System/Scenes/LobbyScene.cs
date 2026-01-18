using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    
    public void LoadGameScene()
    {
        SceneController.Instance.LoadScene(ESceneType.SampleScene);
    }

    public void LoadTutorialScene()
    {
        SceneController.Instance.LoadScene(ESceneType.TutorialScene);
    }
}
