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
        Debug.Log("[RankingDataManager] 종료 시 자동 저장 완료");
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
        Debug.Log($"[RankingDataManager] 저장 완료: {Application.persistentDataPath}/Rankings.json");
    }

    [ContextMenu("Test/Load Data")]
    private void TestLoad()
    {
        LoadData();
        Debug.Log($"[RankingDataManager] 로드 완료 - 랭킹 수: {_rankingsData.Rankings.Count}");
    }

    [ContextMenu("Test/Print All Rankings")]
    private void TestPrintAllRankings()
    {
        Debug.Log($"=== 전체 랭킹 데이터 ({_rankingsData.Rankings.Count}개 스테이지) ===");
        foreach (var ranking in _rankingsData.Rankings)
        {
            Debug.Log($"\n[Stage {ranking.StageID}] - {ranking.Ranks.Count}개 기록");
            for (int i = 0; i < ranking.Ranks.Count; i++)
            {
                var rank = ranking.Ranks[i];
                Debug.Log($"  {i + 1}위. {rank.UserID}: {rank.Score}점");
            }
        }
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

        Debug.Log("[RankingDataManager] 샘플 데이터 추가 완료");
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
        Debug.Log($"[RankingDataManager] Stage 1에 {playerName} ({randomScore}점) 추가");
        TestPrintAllRankings();
    }

    [ContextMenu("Test/Clear All Rankings")]
    private void TestClearAllRankings()
    {
        _rankingsData.Rankings.Clear();
        SaveData();
        Debug.Log("[RankingDataManager] 모든 랭킹 데이터 삭제 완료");
    }

    [ContextMenu("Test/Delete Save File")]
    private void TestDeleteSaveFile()
    {
        JsonSaveService.Delete(SaveKey);
        LoadData();
        Debug.Log("[RankingDataManager] 저장 파일 삭제 및 초기화 완료");
    }

    #endregion
#endif
}
