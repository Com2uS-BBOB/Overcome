using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class KillLogItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image _enemyIcon;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Space(10)]
    [Header("Animation")]
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private DOTweenAnimation _slideInAnimation;
    
    private float _lifetime;
    private Coroutine _lifetimeCoroutine;
    public event Action<KillLogItem> OnExpired;

    private void Awake()
    {
        if (_slideInAnimation == null)
        {
            _slideInAnimation = GetComponentInChildren<DOTweenAnimation>();
        }
    }
    
    public void Initialize(Sprite skillIcon, Sprite enemyIcon, float lifetime)
    {
        transform.localScale = Vector3.one;
        
        _lifetime = lifetime;

        _skillIcon.sprite = skillIcon;
        _enemyIcon.sprite = enemyIcon;

        _canvasGroup.alpha = 1f;
        gameObject.SetActive(true);

        if (_lifetimeCoroutine != null)
        {
            StopCoroutine(_lifetimeCoroutine);
        }

        _slideInAnimation.DOPlay();
        _lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(_lifetime - _fadeOutDuration);

        Sequence fadeOutSequence = DOTween.Sequence();
        fadeOutSequence.Append(_canvasGroup.DOFade(0f, _fadeOutDuration));
        fadeOutSequence.Join(transform.DOScaleY(0f, _fadeOutDuration));
        fadeOutSequence.OnComplete(() =>
        {
            OnExpired?.Invoke(this);
        });
    }

    private void OnDisable()
    {
        _slideInAnimation.DORewind();
        if (_lifetimeCoroutine == null) return;
        StopCoroutine(_lifetimeCoroutine);
        _lifetimeCoroutine = null;
    }
}
