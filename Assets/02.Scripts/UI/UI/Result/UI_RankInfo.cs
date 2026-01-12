using System;
using TMPro;
using UnityEngine;

public class UI_RankInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _gradeText;
    [SerializeField] private StarItem[] _starItems;

    private void OnEnable()
    {
        Show();
    }

    private void OnDisable()
    {
        Reset();
    }
    
    public void Show()
    {
        RankConfig config = ScoreSystem.Instance.GetRanking();
        _gradeText.text = config.Grade;
        for (var i = 0; i < config.RewardStars; ++i)
        {
            _starItems[i].ActiveStar();
        }
    }

    private void Reset()
    {
        foreach (StarItem item in _starItems)
        {
            item.DeactiveColor();
        }
    }
}
