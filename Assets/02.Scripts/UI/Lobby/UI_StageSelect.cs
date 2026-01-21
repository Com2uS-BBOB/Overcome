public class UI_StageSelect : BaseUI
{
    public override void OnClose()
    {
        LobbyScene.CloseStageSelect?.Invoke();
        base.OnClose();
    }
}
