using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class KillInfo : MonoBehaviour
{
    private readonly Dictionary<EEnemyType, string> _enemyTypeNames = new Dictionary<EEnemyType, string>
    {
        { EEnemyType.Normal, "Normal" },
        { EEnemyType.Small, "Small" },
        { EEnemyType.Elite, "Elite" },
        { EEnemyType.FloatSmall, "FloatSmall" }
    };
    [Serializable]
    public struct KillInfoData
    {
        public EEnemyType Type;
        public int KillCount;
    }

    [SerializeField] private KillInfoData _killInfo;
    [SerializeField] private ProgressiveScrambleText _enemyType;
    [SerializeField] private ProgressiveScrambleText _killCountText;

    public event Action OnComplete;

    private bool _enemyTypeComplete;
    private bool _killCountComplete;

    private void Awake()
    {
        _enemyType.Init(this);
        _killCountText.Init(this);
        _enemyType.OnComplete += HandleEnemyTypeComplete;
        _killCountText.OnComplete += HandleKillCountComplete;
    }

    private void OnDestroy()
    {
        _enemyType.OnComplete -= HandleEnemyTypeComplete;
        _killCountText.OnComplete -= HandleKillCountComplete;
    }

    private void OnEnable()
    {
        Show();
    }

    private void OnDisable()
    {
        _enemyType.Stop();
        _killCountText.Stop();
    }

    private void HandleEnemyTypeComplete()
    {
        _enemyTypeComplete = true;
        CheckAllComplete();
    }

    private void HandleKillCountComplete()
    {
        _killCountComplete = true;
        CheckAllComplete();
    }

    private void CheckAllComplete()
    {
        if (_enemyTypeComplete && _killCountComplete)
        {
            OnComplete?.Invoke();
        }
    }

    private void Show()
    {
        _enemyTypeComplete = false;
        _killCountComplete = false;
        _killInfo.KillCount = KillLogSystem.Instance.GetKillCount(_killInfo.Type);
        gameObject.SetActive(true);
        _enemyType.Play(_enemyTypeNames[_killInfo.Type]);
        _killCountText.Play(_killInfo.KillCount);
    }

    public void Hide()
    {
        _killCountText.Stop();
        _enemyType.Stop();
        gameObject.SetActive(false);
    }

    public void Complete()
    {
        _enemyType.Stop();
        _killCountText.Stop();
        _enemyType.SetTextImmediate(_enemyTypeNames[_killInfo.Type]);
        _killCountText.SetTextImmediate(_killInfo.KillCount);

        if (!_enemyTypeComplete || !_killCountComplete)
        {
            _enemyTypeComplete = true;
            _killCountComplete = true;
            OnComplete?.Invoke();
        }
    }
}