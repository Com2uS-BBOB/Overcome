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
    
    private void IncreaseScore(EnemyStatData enemyData)
    {
        _currentScore += enemyData.Score;
        OnScoreChanged?.Invoke(_currentScore, _highScore);
        if (!_isHighScore && _currentScore > _highScore)
        {
            BreakHighScore?.Invoke();
        }
    }

    public void TestIncrease()
    {
        _currentScore += 10000;
        if (_currentScore > _highScore)
        {
            _highScore = _currentScore;
        }
        OnScoreChanged?.Invoke(_currentScore, _highScore);
    }
}
