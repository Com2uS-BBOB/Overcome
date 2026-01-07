using System.Collections;
using UnityEngine;
using DG.Tweening;

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
        ProcessHitFeedback();
    }

    private void ProcessHitFeedback()
    {
        Sequence hitSequence = DOTween.Sequence();

        // 먼저 모든 라인을 동시에 벌어지게 함
        foreach (var line in _lines)
        {
            line.Transform.DOKill();

            Vector2 targetPos = line.OriginalPos + line.SpreadDirection * _spreadDistance;

            hitSequence.Join(
                line.Transform.DOAnchorPos(targetPos, _hitDuration)
                    .SetEase(Ease.OutBack)
            );
        }

        // 그 다음 모든 라인을 동시에 원래 위치로 돌아오게 함
        bool isFirst = true;
        foreach (var line in _lines)
        {
            var returnTween = line.Transform.DOAnchorPos(line.OriginalPos, _returnDuration)
                .SetEase(Ease.InOutQuad);

            if (isFirst)
            {
                hitSequence.Append(returnTween); // 첫 번째는 Append (순차 실행)
                isFirst = false;
            }
            else
            {
                hitSequence.Join(returnTween); // 나머지는 Join (동시 실행)
            }
        }
    }
}
