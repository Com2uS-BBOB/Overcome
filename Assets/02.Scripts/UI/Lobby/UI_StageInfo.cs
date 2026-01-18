using System.Text;
using TMPro;
using UnityEngine;

public class UI_StageInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _scoreRequiredText;
    [SerializeField] private UI_EnemyInfo[] _enemyInfos;
    private readonly StringBuilder _stringBuilder = new StringBuilder();

    private int _chapter;
    private int _level;
    
    public void GameStart()
    {
        StageManager.Instance.SetCurrentStage(_chapter, _level);
        SceneController.Instance.LoadScene(ESceneType.SampleScene);
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
            // C 등급 (0점 기준)은 출력하지 않음
            if (config.RequiredScore <= 0)
                continue;

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
        gameObject.SetActive(false);
    }
}