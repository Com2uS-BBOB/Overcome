using UnityEngine;

public class UI_StageSelect : BaseUI
{
    public static bool IsFirst = true;

    [SerializeField] private GameObject _guidePanelObject;

    private void OnDisable()
    {
        _guidePanelObject.SetActive(false);
    }
    
    public override void OnOpen()
    {
        base.OnOpen();
        if (IsFirst)
        {
            IsFirst = false;
            _guidePanelObject.SetActive(true);
        }
    }
    
    public override void OnClose()
    {
        LobbyScene.CloseStageSelect?.Invoke();
        base.OnClose();
    }
}
