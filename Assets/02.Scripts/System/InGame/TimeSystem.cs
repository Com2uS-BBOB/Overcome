using System;
using UnityEngine;

public class TimeSystem : SingletonBehaviour<TimeSystem>
{
    protected override bool DontDestroy => false;

    [Header("난이도 설정")]
    [SerializeField] private DifficultyConfigData _difficultyConfigData;
    [SerializeField] private EDifficultyType _currentDifficulty = EDifficultyType.Normal;
    private DifficultyConfig _difficultyConfig;
    
    // Ingame Data
    private float _remainTime;
    private float _playTime;
    private bool _isGameOver;
    private float _updateTimer;
    private const float UpdateInterval = 0.01f;

    // Property
    public float RemainTime => _remainTime;
    public float PlayTime => _playTime;

    // Event
    public event Action<float> OnRemainTimeDelta; // 변화량 기반(부호 명시 필요)
    public event Action OnTimeChanged;
    public event Action OnGameOver;
    public event Action<int> OnClearGame;

    protected override void Init()
    {
        SetInfoByDifficulty();
    }
    
    private void OnEnable()
    {
        EnemyEventController.Enemy.OnKilled += KillEnemy;
    }

    private void OnDisable()
    {
        EnemyEventController.Enemy.OnKilled -= KillEnemy;
    }
    
    private void SetInfoByDifficulty()
    {
        _difficultyConfig = _difficultyConfigData.GetConfig(_currentDifficulty);
        if (_difficultyConfig == null)
        {
            Debug.LogError($"[TimeSystem] {_currentDifficulty} Difficulty Config not found");
            enabled = false;
            return;
        }
        _remainTime = _difficultyConfig.StartTime;
    }

    private void Update()
    {
        if (_isGameOver) return;
        _updateTimer += Time.deltaTime;
        if (_updateTimer < UpdateInterval) return;
        
        UpdateTimers(_updateTimer);
        CheckGameClear();
        CheckGameOver();
        _updateTimer = 0f;
    }

    private void UpdateTimers(float deltaTime)
    {
        _playTime += deltaTime;
        if (_difficultyConfig.HasTimeLimit)
        {
            _remainTime -= deltaTime;
        }
        OnTimeChanged?.Invoke();
    }

    private void CheckGameOver()
    {
        if (!_difficultyConfig.HasTimeLimit) return;
        if (_isGameOver) return;
        if (_remainTime > 0f) return;

        _remainTime = 0f;
        _isGameOver = true;
        OnGameOver?.Invoke();
    }

    private void CheckGameClear()
    {
        if (_isGameOver) return;
        if (_playTime < _difficultyConfig.MaxPlayTime) return;
        
        _isGameOver = true;
        OnClearGame?.Invoke(_difficultyConfig.ClearBonus);
        OnGameOver?.Invoke();
    }
    
    private void KillEnemy(EnemyKilledEvent killedEvent)
    {
        AddTimeLimit(killedEvent.Playtime);
    }
    
    public void AddTimeLimit(float additionalTime)
    {
        if (!_difficultyConfig.HasTimeLimit) return;

        float additionalValue = additionalTime + _difficultyConfig.TimeAdjustment;
        if (_playTime >= _difficultyConfig.AdjustmentDecayStart)
        {
            additionalValue *= _difficultyConfig.AdjustmentDecayRatio;
        }
        _remainTime += additionalValue;
        OnRemainTimeDelta?.Invoke(additionalValue);
        OnTimeChanged?.Invoke();
    }

    public void SubtractTimeLimit(float reducedTime)
    {
        if (!_difficultyConfig.HasTimeLimit) return;
        _remainTime -= reducedTime;
        OnRemainTimeDelta?.Invoke(-reducedTime);
        OnTimeChanged?.Invoke();
        CheckGameOver();
    }
}