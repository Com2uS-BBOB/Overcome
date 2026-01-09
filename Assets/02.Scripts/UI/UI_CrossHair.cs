using System.Collections;
using UnityEngine;
// using DG.Tweening;

public class UI_CrossHair : MonoBehaviour
{
    [Header("Crosshair Lines (상하좌우 라인)")]
    [SerializeField] private RectTransform _topLine;
    [SerializeField] private RectTransform _bottomLine;
    [SerializeField] private RectTransform _leftLine;
    [SerializeField] private RectTransform _rightLine;

    [Header("타격 효과 세팅")]
    [SerializeField] private float _spreadDistance = 10f;
    [SerializeField] private float _hitDuration = 0.1f;
    [SerializeField] private float _returnDuration = 0.15f;

    private struct CrosshairLine
    {
        public RectTransform Transform;
        public Vector2 OriginalPos;
        public Vector2 SpreadDirection;
    }

    private CrosshairLine[] _lines;

    private void Awake()
    {
        _lines = new CrosshairLine[]
        {
            new CrosshairLine { Transform = _topLine, OriginalPos = _topLine.anchoredPosition, SpreadDirection = Vector2.up },
            new CrosshairLine { Transform = _bottomLine, OriginalPos = _bottomLine.anchoredPosition, SpreadDirection = Vector2.down },
            new CrosshairLine { Transform = _leftLine, OriginalPos = _leftLine.anchoredPosition, SpreadDirection = Vector2.left },
            new CrosshairLine { Transform = _rightLine, OriginalPos = _rightLine.anchoredPosition, SpreadDirection = Vector2.right }
        };
    }

    public void OnEnemyHit()
    {
        // PlayHitFeedback_DOTween();
        PlayHitFeedback_Test();
    }

    // todo. DOTween을 적용 이후 사용
    // private void PlayHitFeedback_DOTween()
    // {
    //     Sequence hitSequence = DOTween.Sequence();
    //
    //     foreach (var line in _lines)
    //     {
    //         line.Transform.DOKill();
    //
    //         Vector2 targetPos = line.OriginalPos + line.SpreadDirection * _spreadDistance;
    //
    //         hitSequence.Join(
    //             line.Transform.DOAnchorPos(targetPos, _hitDuration)
    //                 .SetEase(Ease.OutBack)
    //         );
    //     }
    //
    //     foreach (var line in _lines)
    //     {
    //         hitSequence.Join(
    //             line.Transform.DOAnchorPos(line.OriginalPos, _returnDuration)
    //                 .SetEase(Ease.InOutQuad)
    //         );
    //     }
    // }

    #region TestCode
    
    private void PlayHitFeedback_Test()
    {
        StopAllCoroutines();
        StartCoroutine(HitFeedbackCoroutine());
    }

    private IEnumerator HitFeedbackCoroutine()
    {
        Vector2[] targetPositions = new Vector2[_lines.Length];
        for (int i = 0; i < _lines.Length; i++)
        {
            targetPositions[i] = _lines[i].OriginalPos + _lines[i].SpreadDirection * _spreadDistance;
        }

        float elapsed = 0f;

        while (elapsed < _hitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _hitDuration;

            for (int i = 0; i < _lines.Length; i++)
            {
                _lines[i].Transform.anchoredPosition = Vector2.Lerp(_lines[i].OriginalPos, targetPositions[i], t);
            }

            yield return null;
        }

        for (int i = 0; i < _lines.Length; i++)
        {
            _lines[i].Transform.anchoredPosition = targetPositions[i];
        }

        elapsed = 0f;

        while (elapsed < _returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _returnDuration;

            for (int i = 0; i < _lines.Length; i++)
            {
                _lines[i].Transform.anchoredPosition = Vector2.Lerp(targetPositions[i], _lines[i].OriginalPos, t);
            }

            yield return null;
        }
        
        for (int i = 0; i < _lines.Length; i++)
        {
            _lines[i].Transform.anchoredPosition = _lines[i].OriginalPos;
        }
    }
    #endregion
}
