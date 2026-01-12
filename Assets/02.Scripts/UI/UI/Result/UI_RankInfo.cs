using System;
using UnityEngine;

public class UI_RankInfo : MonoBehaviour
{
    public event Action OnShowComplete;
    public void Show()
    {
        RankConfig config = ScoreSystem.Instance.GetRanking();
        Debug.Log(config.Grade);
        Debug.Log(config.RequiredScore);
        Debug.Log(config.RewardStars);
    }
    public void Hide()
    {
        throw new NotImplementedException();
    }
}
