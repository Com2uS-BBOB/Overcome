using System;
using UnityEngine;

public class ScoreSystem : SingletonBehaviour<ScoreSystem>, IGameSystem
{
    protected override bool DontDestroy => false;
    
    private int _currentScore;
    private int _highScore;
    private bool _isHighScore;
    
    public event Action<int, int> OnScoreChanged;
    public event Action BreakHighScore;
    
    public int CurrentScore => _currentScore;
    public int HighScore => _highScore;

    private void OnEnable()
    {
        RankingData data = RankingDataManager.Instance.GetOrCreateStageRanking(StageManager.Instance.StageID);
        _highScore = data.Ranks.Count < 1 ? 0 : data.Ranks[0].Score;
    }
    
    public void GameStart()
    {
        EnemyEventController.Enemy.OnKilled += IncreaseScore;
    }

    public void GameEnd()
    {
        EnemyEventController.Enemy.OnKilled -= IncreaseScore;
    }
    
    private void IncreaseScore(EnemyKilledEvent killedEvent)
    {
        IncreaseScore(killedEvent.Score);
    }

    public void IncreaseScore(int score)
    {
        _currentScore += score;
        UpdateHighScore();
        OnScoreChanged?.Invoke(_currentScore, _highScore);
    }

    private void UpdateHighScore()
    {
        if (_currentScore < _highScore) return;
        if (!_isHighScore)
        {
            _isHighScore = true;
            BreakHighScore?.Invoke();
        }
        _highScore = _currentScore;
    }
    
    public GradeConfig GetGradeConfig()
    {
        return StageManager.Instance.GetGradeConfig(_currentScore);
    }
}
