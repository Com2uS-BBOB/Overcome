using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UI_RankInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _gradeText;
    [SerializeField] private StarItem[] _starItems;

    [Header("Stamp Animation")]
    [SerializeField] private float _startScale = 3f;
    [SerializeField] private float _stampDuration = 0.3f;
    [SerializeField] private float _punchScale = 0.15f;
    [SerializeField] private float _punchDuration = 0.2f;
    [SerializeField] private Ease _punchEase = Ease.InQuad;
    
    private Sequence _stampSequence;

    private void OnEnable()
    {
        Show();
    }

    private void OnDisable()
    {
        Reset();
    }

    public void Show()
    {
        RankConfig config = ScoreSystem.Instance.GetRanking();
        PlayStampAnimation(config.Grade);
        for (var i = 0; i < config.RewardStars; ++i)
        {
            _starItems[i].ActiveStar();
        }
    }

    private void PlayStampAnimation(string grade)
    {
        _stampSequence?.Kill();

        _gradeText.text = grade;
        _gradeText.transform.localScale = Vector3.one * _startScale;

        _stampSequence = DOTween.Sequence();
        _stampSequence.Append(_gradeText.transform
            .DOScale(1f, _stampDuration)
            .SetEase(Ease.InQuad));
        _stampSequence.Append(_gradeText.transform
            .DOPunchScale(Vector3.one * _punchScale, _punchDuration, 1, 0f));
    }

    private void Reset()
    {
        _stampSequence?.Kill();
        _gradeText.transform.localScale = Vector3.one;

        foreach (StarItem item in _starItems)
        {
            item.DeactiveColor();
        }
    }
}
