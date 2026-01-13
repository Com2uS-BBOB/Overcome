using System.Linq;
using UnityEngine;

public class UI_Result : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    
    [Header("UI References")]
    [SerializeField] private UI_TimeResult _timeResult;
    [SerializeField] private UI_ScoreResult _scoreResult;
    [SerializeField] private UI_KillResult _killResult;
    [SerializeField] private UI_RankInfo _rankInfo;
    

    private Transform[] _resultUiObjects;
    
    private bool _timeComplete;
    private bool _scoreComplete;
    private bool _killComplete;
    private bool _allComplete;

    private void Awake()
    {
        _timeResult.OnComplete += HandleTimeComplete;
        _scoreResult.OnComplete += HandleScoreComplete;
        _killResult.OnComplete += HandleKillComplete;
        _resultUiObjects = GetComponentsInChildren<Transform>(true)
            .Where(t => t != transform)
            .ToArray();
        foreach (Transform child in _resultUiObjects)
        {
            child.gameObject.SetActive(false);
        }
        _canvasGroup.alpha = 0;
    }

    private void Start()
    {
        TimeSystem.Instance.OnGameOver += ShowResultUI;
    }

    private void OnDestroy()
    {
        _timeResult.OnComplete -= HandleTimeComplete;
        _scoreResult.OnComplete -= HandleScoreComplete;
        _killResult.OnComplete -= HandleKillComplete;
    }
    
    private void ShowResultUI()
    {
        _canvasGroup.alpha = 1;
        ResetState();
        foreach (Transform child in _resultUiObjects)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void ResetState()
    {
        _timeComplete = false;
        _scoreComplete = false;
        _killComplete = false;
        _allComplete = false;
        _rankInfo.gameObject.SetActive(false);
    }

    private void HandleTimeComplete()
    {
        _timeComplete = true;
        CheckAllComplete();
    }

    private void HandleScoreComplete()
    {
        _scoreComplete = true;
        CheckAllComplete();
    }

    private void HandleKillComplete()
    {
        _killComplete = true;
        CheckAllComplete();
    }

    private void CheckAllComplete()
    {
        if (_allComplete) return;

        if (_timeComplete && _scoreComplete && _killComplete)
        {
            _allComplete = true;
            ShowRankInfo();
        }
    }

    private void ShowRankInfo()
    {
        _rankInfo.gameObject.SetActive(true);
    }

    public void Skip()
    {
        if (!_allComplete)
        {
            _timeResult.Complete();
            _scoreResult.Complete();
            _killResult.Complete();
        }
        // todo. Lobby Scene으로 넘어가기
    }
}
