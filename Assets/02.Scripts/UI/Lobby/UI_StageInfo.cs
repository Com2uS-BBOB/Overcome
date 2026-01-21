using System;
using System.Text;
using TMPro;
using UnityEngine;

public class UI_StageInfo : BaseUI
{
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _scoreRequiredText;
    [SerializeField] private UI_EnemyInfo[] _enemyInfos;
    private readonly StringBuilder _stringBuilder = new StringBuilder();

    private int _chapter;
    private int _level;

    public event Action<int, int> OnGameStartRequested;
    
    public void GameStart()
    {
        OnGameStartRequested?.Invoke(_chapter, _level);
    }
    
    public void OpenStageInfoPanel(int chapter, int level)
    {
        _chapter = chapter;
        _level = level;
        
        SetEnemyInfo();
        SetStageInfo(chapter, level);
    }

    private void SetStageInfo(int chapter, int level)
    {
        string stageID = $"{chapter}_{level}";
        _title.text = $"Stage : {chapter}-{level}";
        StageGradeConfig gradeConfigs = StageManager.Instance.GetStageGradeConfig(stageID);

        _stringBuilder.Clear();
        bool isFirst = true;

        foreach (var config in gradeConfigs.Grades)
        {
            if (config.RequiredScore <= 0) continue;

            if (!isFirst)
                _stringBuilder.Append(" / ");

            _stringBuilder.Append(config.Grade);
            _stringBuilder.Append(" : ");
            _stringBuilder.Append(config.RequiredScore.ToString("N0"));

            isFirst = false;
        }

        _scoreRequiredText.SetText(_stringBuilder.ToString());
    }

    private void SetEnemyInfo()
    {
        foreach (UI_EnemyInfo enemyInfo in _enemyInfos)
        {
            enemyInfo.SetInfo();
        }
    }

    public void ClosePanel()
    {
        UIController.Instance.CloseUI(this);
    }

    public async void OpenRankingPanel()
    {
        UI_Ranking ranking = await UIController.Instance.OpenUI<UI_Ranking>();
        ranking.SetStageInfo(_chapter, _level);
    }
}