using System;
using UnityEngine;

public class ScoreSystem : SingletonBehaviour<ScoreSystem>
{
    protected override bool DontDestroy => false;
    
    private int _currentScore;
    private int _highScore;
    private bool _isHighScore;
    
    public event Action<int, int> OnScoreChanged;
    public event Action BreakHighScore;
    
    // Test Code
    [SerializeField] private int _testHighScore;

    public int CurrentScore => _currentScore;
    public int HighScore => _highScore;
    
    protected override void Init()
    {
        _highScore = _testHighScore;
    }
    
    private void OnEnable()
    {
        EnemyEventController.Enemy.OnKilled += IncreaseScore;
    }

    private void OnDisable()
    {
        EnemyEventController.Enemy.OnKilled -= IncreaseScore;
    }
    
    private void IncreaseScore(EnemyKilledEvent killedEvent)
    {
        _currentScore += killedEvent.Score;
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
}
