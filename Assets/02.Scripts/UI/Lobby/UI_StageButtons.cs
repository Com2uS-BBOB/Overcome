using UnityEngine;

public class UI_StageButtons : MonoBehaviour
{
    [SerializeField] private UI_StageInfo _stageInfoPanel;

    public void OpenStageInfoPanel(int chapter, int level)
    {
        _stageInfoPanel.gameObject.SetActive(true);
        _stageInfoPanel.OpenStageInfoPanel(chapter, level);
    }
}
