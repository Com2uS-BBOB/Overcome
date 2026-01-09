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
        // todo. Enemy Die Event에 Increase Score 함수 바인딩
    }
    
    private void IncreaseScore(int score)
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

    public void TestIncrease()
    {
        IncreaseScore(1000);
    }
}
