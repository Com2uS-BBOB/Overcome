using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_EnemyInfo : MonoBehaviour
{
    [SerializeField] private EEnemyType _type;
    
    [Header("Data References")]
    [SerializeField] private Sprite[] _iconInfos;
    [SerializeField] private EnemyStatData[] _enemyStats;
    
    [Space(10)]
    [Header("UI References")]
    [SerializeField] private Image _enemyIcon;
    [SerializeField] private TextMeshProUGUI _score;

    public void SetInfo()
    {
        if (_enemyStats.Length == 0) return;
        for (var i = 0; i < _enemyStats.Length; i++)
        {
            if (_enemyStats[i].EnemyType != _type) continue;
            SetUI(i);
            return;
        }
        SetUI(0);
    }

    private void SetUI(int i)
    {
        _enemyIcon.sprite = _iconInfos[i];
        _score.SetText(_enemyStats[i].Score.ToString("N0"));
    }
}
