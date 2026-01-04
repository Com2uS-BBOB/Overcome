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

    // UI 갱신용 캐시 (초 단위)
    private int _prevRemainTimeInt;
    private int _prevPlayTimeInt;

    // Property
    public float RemainTime => _remainTime;
    public float PlayTime => _playTime;

    // Event
    public event Action OnRemainTimeChanged;
    public event Action OnPlayTimeChanged;
    public event Action<float> OnRemainTimeDelta; // 변화량 기반(부호 명시 필요)
    public event Action OnGameOver;

    protected override void Init()
    {
        SetInfoByDifficulty();
    }
    
    private void SetInfoByDifficulty()
    {
        _difficultyConfig = _difficultyConfigData.GetConfig(_currentDifficulty);
        _remainTime = (_difficultyConfig.HasTimeLimit) ? _difficultyConfig.StartTime : float.MaxValue;
        _prevRemainTimeInt = Mathf.FloorToInt(_remainTime);
        _prevPlayTimeInt = Mathf.FloorToInt(_playTime);
    }

    private void Update()
    {
        UpdateTimers();
        UpdateTimerUI();
        CheckGameOver();
    }

    private void UpdateTimers()
    {
        float deltaTime = Time.deltaTime;
        _remainTime -= deltaTime;
        _playTime += deltaTime;
    }

    private void UpdateTimerUI()
    {
        int remainInt = Mathf.FloorToInt(_remainTime);
        if (remainInt != _prevRemainTimeInt)
        {
            _prevRemainTimeInt = remainInt;
            OnRemainTimeChanged?.Invoke();
        }

        int playInt = Mathf.FloorToInt(_playTime);
        if (playInt != _prevPlayTimeInt)
        {
            _prevPlayTimeInt = playInt;
            OnPlayTimeChanged?.Invoke();
        }
    }

    private void CheckGameOver()
    {
        if (!_difficultyConfig.HasTimeLimit) return;
        if (_isGameOver) return;
        if (_remainTime > 0f) return;

        _isGameOver = true;
        OnGameOver?.Invoke();
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

        OnRemainTimeChanged?.Invoke();
        OnRemainTimeDelta?.Invoke(additionalValue);
    }

    public void SubtractTimeLimit(float reducedTime)
    {
        if (!_difficultyConfig.HasTimeLimit) return;

        _remainTime -= reducedTime;

        OnRemainTimeChanged?.Invoke();
        OnRemainTimeDelta?.Invoke(-reducedTime);

        CheckGameOver();
    }
}