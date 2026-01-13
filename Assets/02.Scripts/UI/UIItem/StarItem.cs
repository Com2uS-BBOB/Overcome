using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StarItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _emptyTransform; 
    [SerializeField] private RectTransform _filledTransform;
    
    [Header("Animation Settings")]
    [SerializeField] private float _moveDuration = 0.7f;
    [SerializeField] private float _startScale = 2.5f;
    [SerializeField] private Ease _moveEase = Ease.InQuad;

    [Header("Impact Settings")]
    [SerializeField] private float _minImpactScale = 0.8f;
    [SerializeField] private float _recoveryDurationRatio = 0.8f;
    [SerializeField] private float _impactScale = 1.4f;
    [SerializeField] private float _impactDuration = 0.12f;
    
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private Sequence _starSequence;

    private void Awake()
    {
        _startPosition = _filledTransform.localPosition;
        _targetPosition = _emptyTransform.localPosition;
        ResetStar();
    }

    public void ActiveStar()
    {
        _starSequence?.Kill();

        _filledTransform.gameObject.SetActive(true);
        
        _filledTransform.localScale = Vector3.one * _startScale;
        _filledTransform.localPosition = _startPosition;
    
        _starSequence = DOTween.Sequence();
        // 이동하면서 작아지기
        _starSequence.Append(_filledTransform.
                             DOLocalMove(_targetPosition, _moveDuration)
                             .SetEase(_moveEase));
        _starSequence.Join(_filledTransform
                           .DOScale(_minImpactScale, _moveDuration)
                           .SetEase(Ease.InQuad));
    
        // 박히는 순간 임팩트
        _starSequence.Append(_filledTransform
                             .DOScale(_impactScale, _impactDuration)
                             .SetEase(Ease.OutCubic));
        _starSequence.Append(_filledTransform
                             .DOScale(1f, _impactDuration * _recoveryDurationRatio)
                             .SetEase(Ease.InCubic));
    }
    
    public void ResetStar()
    {
        _starSequence?.Kill();
        _filledTransform.localPosition = _startPosition;
        _filledTransform.localScale = Vector3.one * _startScale;
        _filledTransform.gameObject.SetActive(false);
    }
}