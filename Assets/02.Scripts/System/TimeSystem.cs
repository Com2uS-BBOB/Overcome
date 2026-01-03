using System;
using UnityEngine;

public class TimeSystem : SingletonBehaviour<TimeSystem>
{
    protected override bool DontDestroy => false;

    [Header("시간 제한")]
    [SerializeField] private float _startTime;
    [SerializeField] private bool _hasTimeLimit;

    [Header("시간 관련 설정")]
    [SerializeField] private float _maxPlayTime;
    [SerializeField] private float _difficultyOffset;
    [SerializeField] private float _timeDecayStartTime = 120.0f;
    [SerializeField] private float _decayRatio = 0.5f;

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
        if (!_hasTimeLimit) return;
        if (_isGameOver) return;
        if (_remainTime > 0f) return;

        _isGameOver = true;
        OnGameOver?.Invoke();
    }

    private void SetInfoByDifficulty()
    {
        // todo. 난이도에 따른 초기 값 설정
        _remainTime = (_hasTimeLimit) ? _startTime : float.MaxValue;
        _prevRemainTimeInt = Mathf.FloorToInt(_remainTime);
        _prevPlayTimeInt = Mathf.FloorToInt(_playTime);
    }

    public void AddTimeLimit(float additionalTime)
    {
        if (!_hasTimeLimit) return;

        float additionalValue = additionalTime + _difficultyOffset;
        if (_playTime >= _timeDecayStartTime)
        {
            additionalValue *= _decayRatio;
        }

        _remainTime += additionalValue;

        OnRemainTimeChanged?.Invoke();
        OnRemainTimeDelta?.Invoke(additionalValue);
    }

    public void SubtractTimeLimit(float reducedTime)
    {
        if (!_hasTimeLimit) return;

        _remainTime -= reducedTime;

        OnRemainTimeChanged?.Invoke();
        OnRemainTimeDelta?.Invoke(-reducedTime);

        CheckGameOver();
    }
}