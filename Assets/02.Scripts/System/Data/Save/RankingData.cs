using System;
using System.Collections.Generic;

[Serializable]
public class RankingData
{
    public int StageID;
    public List<RankConfig> Ranks;

    public RankingData()
    {
        Ranks = new List<RankConfig>();
    }

    public RankingData(int stageID)
    {
        StageID = stageID;
        Ranks = new List<RankConfig>();
    }
}

[Serializable]
public class RankConfig
{
    public string UserID;
    public int Score;

    public RankConfig() { }

    public RankConfig(string userID, int score)
    {
        UserID = userID;
        Score = score;
    }
}
