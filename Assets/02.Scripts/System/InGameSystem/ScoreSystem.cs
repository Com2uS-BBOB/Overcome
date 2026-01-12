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
    
    // Test Code
    [SerializeField] private int _testHighScore;
    [SerializeField] private int _rankingTestScore;

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
        // todo. TestCode 삭제 필요
        return _rankingData.GetRank(_rankingTestScore);
        // return _rankingData.GetRank(_currentScore);
    }
}
