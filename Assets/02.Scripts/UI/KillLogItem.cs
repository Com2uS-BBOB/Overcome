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

        float elapsed = 0f;
        while (elapsed < _fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeOutDuration);
            transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1f, 0f, 1f), elapsed / _fadeOutDuration);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one;
        OnExpired?.Invoke(this);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        _slideInAnimation.DORewind();
        if (_lifetimeCoroutine == null) return;
        StopCoroutine(_lifetimeCoroutine);
        _lifetimeCoroutine = null;
    }
}
