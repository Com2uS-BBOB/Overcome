using System;
using UnityEngine;
using DG.Tweening;

public class KillInfo : MonoBehaviour
{
    [Serializable]
    public struct KillInfoData
    {
        public EEnemyType Type;
        public int KillCount;
    }

    [SerializeField] private KillInfoData _killInfo;
    [SerializeField] private AnimatedNumber _number;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Animation")]
    [SerializeField] private float _showDuration = 0.25f;
    [SerializeField] private Ease _showEase = Ease.OutBack;

    public event Action OnShowComplete;

    private void Awake()
    {
        _number.Init();
    }

    private void OnDestroy()
    {
        _number.Clear();
    }

    public void Show()
    {
        _killInfo.KillCount = KillLogSystem.Instance.GetKillCount(_killInfo.Type);
        gameObject.SetActive(true);

        transform.localScale = Vector3.zero;
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.DOFade(1f, _showDuration);
        }

        transform
            .DOScale(1f, _showDuration)
            .SetEase(_showEase)
            .OnComplete(() =>
            {
                _number.OnCompleteChanging += OnShowComplete;
                _number.SetValue(_killInfo.KillCount);
            });
    }

    public void HideResult()
    {
        _number.DeactivateTextUI();
    }
    
    public void HideUI()
    {
        gameObject.SetActive(false);
    }
}