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
        for (var i = 0; i < _rankItems.Length; i++)
        {
            if (i < rankData.Ranks.Count)
            {
                RankConfig rankConfig = rankData.Ranks[i];
                _rankItems[i].SetRankInfo(rankConfig);
            }
            else
            {
                _rankItems[i].SetDefaultInfo();
            }
        }
    }

    public void CloseUI()
    {
        UIController.Instance.CloseUI(this);
    }
}
