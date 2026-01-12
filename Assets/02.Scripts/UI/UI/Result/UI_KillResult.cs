using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UI_KillResult : MonoBehaviour, ISequentialUI
{
    [SerializeField] private GameObject _title;
    [SerializeField] private Transform _killInfosParent;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Animation")]
    [SerializeField] private float _titleDuration = 0.3f;
    [SerializeField] private float _itemDelay = 0.15f;
    [SerializeField] private Ease _titleEase = Ease.OutBack;

    public event Action OnShowComplete;
    
    private readonly List<KillInfo> _killInfos = new List<KillInfo>();
    private WaitForSeconds _delayTime;

    private void Awake()
    {
        InitKillInfos();
        _delayTime = new WaitForSeconds(_itemDelay);
    }

    private void InitKillInfos()
    {
        _killInfos.Clear();
        foreach (Transform child in _killInfosParent)
        {
            var killInfo = child.GetComponent<KillInfo>();
            if (killInfo != null)
            {
                _killInfos.Add(killInfo);
            }
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        
        // 모든 자식 요소 비활성화
        _title.SetActive(false);
        foreach (var killInfo in _killInfos)
        {
            killInfo.HideUI();
        }

        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.DOFade(1f, _titleDuration);
        }
        
        // Title 설정
        _title.SetActive(true);
        _title.transform.localScale = Vector3.zero;
        _title.transform.DOScale(1f, _titleDuration).SetEase(_titleEase);
        yield return new WaitForSeconds(_titleDuration);

        // Monster Type에 따른 Kill Count 표시
        foreach (var killInfo in _killInfos)
        {
            bool isComplete = false;

            Action onComplete = null;
            onComplete = () =>
            {
                killInfo.OnShowComplete -= onComplete;
                isComplete = true;
            };
            killInfo.OnShowComplete += onComplete;

            killInfo.Show();

            yield return new WaitUntil(() => isComplete);
            yield return _delayTime;
        }

        OnShowComplete?.Invoke();
    }

    public void Hide()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}
