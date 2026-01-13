using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UISequencer : MonoBehaviour
{
    [Serializable]
    public class SequenceItem
    {
        public GameObject target;
        [Tooltip("표시 후 다음 UI까지 대기 시간")]
        public float delayAfter = 0.3f;
        [Tooltip("ISequentialUI 이벤트 완료까지 대기")]
        public bool waitForEvent = false;
    }

    [SerializeField] private GameObject[] _defaultGameObjects;
    
    [SerializeField] private List<SequenceItem> _items;
    [SerializeField] private float _defaultAnimationDuration = 0.3f;
    [SerializeField] private Ease _defaultEase = Ease.OutBack;

    public event Action OnSequenceComplete;

    private int _currentIndex;

    private void Start()
    {
        TimeSystem.Instance.OnGameOver += Play;
    }

    private void OnDestroy()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnGameOver -= Play;
        }
    }

    private void OnEnable()
    {
        Play();
    }
    
    public void Play()
    {
        foreach (GameObject defaultObject in _defaultGameObjects)
        {
            defaultObject.SetActive(true);
        }
        
        foreach (var item in _items)
        {
            item.target.SetActive(false);
        }

        _currentIndex = 0;
        ShowCurrent();
    }

    public void Stop()
    {
        _currentIndex = _items.Count;
        DOTween.Kill(this);
    }

    private void ShowCurrent()
    {
        if (_currentIndex >= _items.Count)
        {
            OnSequenceComplete?.Invoke();
            return;
        }

        var item = _items[_currentIndex];
        var sequentialUI = item.target.GetComponent<ISequentialUI>();

        if (sequentialUI != null && item.waitForEvent)
        {
            Action onComplete = null;
            onComplete = () =>
            {
                sequentialUI.OnShowComplete -= onComplete;
                ProceedToNext(item.delayAfter);
            };
            sequentialUI.OnShowComplete += onComplete;
            sequentialUI.Show();
        }
        else
        {
            ShowWithDefaultAnimation(item.target, () => ProceedToNext(item.delayAfter));
        }
    }

    private void ShowWithDefaultAnimation(GameObject target, Action onComplete)
    {
        target.SetActive(true);
        target.transform.localScale = Vector3.zero;
        target.transform
            .DOScale(1f, _defaultAnimationDuration)
            .SetEase(_defaultEase)
            .SetTarget(this)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void ProceedToNext(float delay)
    {
        _currentIndex++;

        if (delay > 0)
        {
            DOVirtual.DelayedCall(delay, ShowCurrent).SetTarget(this);
        }
        else
        {
            ShowCurrent();
        }
    }
}