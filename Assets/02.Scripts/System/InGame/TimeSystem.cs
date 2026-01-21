using System;
using UnityEngine;

public class TimeSystem : SingletonBehaviour<TimeSystem>, IGameSystem
{
    protected override bool DontDestroy => false;

    [SerializeField] private GameObject _hudObject;
    
    [Header("난이도 설정")]
    [SerializeField] private DifficultyConfigData _difficultyConfigData;
    [SerializeField] private EDifficultyType _currentDifficulty = EDifficultyType.Normal;
    private DifficultyConfig _difficultyConfig;
    
    // Ingame Data
    private float _remainTime;
    private float _playTime;
    private bool _isOnGame;
    private float _updateTimer;
    private const float UpdateInterval = 0.01f;

    // Property
    public float RemainTime => _remainTime;
    public float PlayTime => _playTime;

    // Event
    public event Action<float> OnRemainTimeDelta; // 변화량 기반(부호 명시 필요)
    public event Action OnTimeChanged;
    
    protected override void Init()
    {
        SetInfoByDifficulty();
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
        if (!_isOnGame) return;
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
        if (!_isOnGame) return;
        if (_remainTime > 0f) return;

        _remainTime = 0f;
        _isOnGame = false;
        _hudObject.gameObject.SetActive(false);
        GameEventHandler.GameEnd();
    }

    private void CheckGameClear()
    {
        if (!_isOnGame) return;
        if (_playTime < _difficultyConfig.MaxPlayTime) return;
        
        _isOnGame = false;
        _hudObject.gameObject.SetActive(false);
        GameEventHandler.GameClear(_difficultyConfig.ClearBonus);
        GameEventHandler.GameEnd();
    }
    
    private void KillEnemy(EnemyKilledEvent killedEvent)
    {
        if (!_isOnGame) return;
        AddTimeLimit(killedEvent.Playtime);
    }
    
    public void AddTimeLimit(float additionalTime)
    {
        if (!_isOnGame) return;
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

    public void GameStart()
    {
        _isOnGame = true;
        EnemyEventController.Enemy.OnKilled += KillEnemy;
    }

    public void GameEnd()
    {
        _isOnGame = false;
        EnemyEventController.Enemy.OnKilled -= KillEnemy;
    }
}