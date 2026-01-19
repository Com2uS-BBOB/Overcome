using UnityEngine;

public class UI_Ranking : BaseUI
{
    [SerializeField] private RankItem[] _rankItems;
    private int _chapter;
    private int _level;
    
    public void SetStageInfo(int chapter, int level)
    {
        _chapter = chapter;
        _level = level;
        LoadData();
    }

    private void LoadData()
    {
        int stageID = _chapter * 10 +  _level;
        RankingData rankData = RankingDataManager.Instance.GetOrCreateStageRanking(stageID);
        for (var i = 0; i < rankData.Ranks.Count; i++)
        {
            RankConfig rankConfig = rankData.Ranks[i];
            _rankItems[i].SetRankInfo(rankConfig);
        }
    }

    public void CloseUI()
    {
        UIController.Instance.CloseUI(this);
    }
}
