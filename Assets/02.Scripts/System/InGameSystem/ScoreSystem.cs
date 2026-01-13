using System;
using UnityEngine;

public class ScoreSystem : SingletonBehaviour<ScoreSystem>
{
    [SerializeField] private RankingData _rankingData;
    protected override bool DontDestroy => false;
    
    private int _currentScore;
    private int _highScore;
    private bool _isHighScore;
    
    public event Action<int, int> OnScoreChanged;
    public event Action BreakHighScore;
    
    [Header("Test Settings")]
    [SerializeField] private bool _isTest = false;
    [SerializeField] private int _testHighScore;
    [SerializeField] private int _testScore;
    
    public int CurrentScore => _currentScore;
    public int HighScore => _highScore;
    
    protected override void Init()
    {
        if (_isTest)
        {
            _highScore = _testHighScore;
        }
    }
    
    private void OnEnable()
    {
        EnemyEventController.Enemy.OnKilled += IncreaseScore;
    }
    
    private void Start()
    {
        TimeSystem.Instance.OnClearGame += IncreaseScore;
    }

    private void OnDestroy()
    {
        if (TimeSystem.Instance == null) return;
        TimeSystem.Instance.OnClearGame -= IncreaseScore;
    }

    private void OnDisable()
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
    
    public RankConfig GetRanking()
    {
        if (_isTest)
        {
            return _rankingData.GetRank(_testScore);
        }
        else
        {
            return _rankingData.GetRank(_currentScore);
        }
    }
}
