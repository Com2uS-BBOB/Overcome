using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingDataManager : SingletonBehaviour<RankingDataManager>
{
    private const string SaveKey = "Rankings";
    private AllRankingsData _rankingsData;
    private const int MaxRanking = 5;

    protected override void Init()
    {
        LoadData();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    #region Save/Load

    private void SaveData()
    {
        JsonSaveService.Save(SaveKey, _rankingsData);
    }

    private void LoadData()
    {
        _rankingsData = JsonSaveService.Load<AllRankingsData>(SaveKey) ?? new AllRankingsData();
        _rankingsData.Rankings ??= new List<RankingData>();
    }
    #endregion

    #region Ranking Management

    public RankingData GetOrCreateStageRanking(int stageID)
    {
        RankingData ranking = _rankingsData.Rankings.FirstOrDefault(r => r.StageID == stageID);

        if (ranking != null) return ranking;

        ranking = new RankingData(stageID);
        _rankingsData.Rankings.Add(ranking);
        return ranking;
    }

    public void UpdateRanking(int stageID, string userID, int score)
    {
        RankingData ranking = GetOrCreateStageRanking(stageID);
        RankConfig existingEntry = ranking.Ranks.FirstOrDefault(r => r.UserID == userID);
    
        if (existingEntry != null)
        {
            if (score <= existingEntry.Score) return;
            existingEntry.Score = score;
        }
        else
        {
            if (!CanEnterRanking(ranking, score)) return;
            AddNewEntry(ranking, userID, score);
        }
    
        FinalizeRanking(stageID, ranking);
    }

    private bool CanEnterRanking(RankingData ranking, int score)
    {
        if (ranking.Ranks.Count < MaxRanking) return true;
    
        int lowestScore = ranking.Ranks.Min(r => r.Score);
        return score > lowestScore;
    }

    private void AddNewEntry(RankingData ranking, string userID, int score)
    {
        ranking.Ranks.Add(new RankConfig(userID, score));
    }

    private void FinalizeRanking(int stageID, RankingData ranking)
    {
        ranking.Ranks = ranking.Ranks.OrderByDescending(r => r.Score).ToList();
        TrimRanking(stageID);
        SaveData();
    }
    
    private void TrimRanking(int stageID)
    {
        RankingData ranking = GetOrCreateStageRanking(stageID);

        if (ranking.Ranks.Count > MaxRanking)
        {
            ranking.Ranks = ranking.Ranks.Take(MaxRanking).ToList();
        }
    }
    #endregion

#if UNITY_EDITOR
    #region Editor Test

    [ContextMenu("Test/Save Data")]
    private void TestSave()
    {
        SaveData();
    }

    [ContextMenu("Test/Load Data")]
    private void TestLoad()
    {
        LoadData();
    }

    [ContextMenu("Test/Print All Rankings")]
    private void TestPrintAllRankings()
    {
    }

    [ContextMenu("Test/Add Sample Data")]
    private void TestAddSampleData()
    {
        UpdateRanking(11, "Alice", 1500);
        UpdateRanking(11, "Bob", 1200);
        UpdateRanking(11, "Charlie", 1800);
        UpdateRanking(11, "David", 1000);
        UpdateRanking(11, "Eve", 1600);

        UpdateRanking(12, "Alice", 2500);
        UpdateRanking(12, "Bob", 2200);
        UpdateRanking(12, "Frank", 2800);

        TestPrintAllRankings();
    }

    [ContextMenu("Test/Add Ranking to Stage 1")]
    private void TestAddRankingStage1()
    {
        string playerName = PlayerDataManager.Instance.GetPlayerID();
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "TestPlayer";
        }

        int randomScore = UnityEngine.Random.Range(10000, 50000);
        UpdateRanking(11, playerName, randomScore);
        TestPrintAllRankings();
    }

    [ContextMenu("Test/Clear All Rankings")]
    private void TestClearAllRankings()
    {
        _rankingsData.Rankings.Clear();
        SaveData();
    }

    [ContextMenu("Test/Delete Save File")]
    private void TestDeleteSaveFile()
    {
        JsonSaveService.Delete(SaveKey);
        LoadData();
    }

    #endregion
#endif
}
