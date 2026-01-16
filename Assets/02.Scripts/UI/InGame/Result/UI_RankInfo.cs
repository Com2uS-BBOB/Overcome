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

    [Header("Star Animation")]
    [SerializeField] private float _starDelay = 0.2f;
    
    private Sequence _stampSequence;
    private Sequence _starSequence;

    private void OnEnable()
    {
        Show();
    }

    private void OnDisable()
    {
        ResetUI();
    }

    private void Show()
    {
        GradeConfig config = ScoreSystem.Instance.GetGradeConfig();
        PlayStampAnimation(config.Grade, config.RewardStars);
    }

    private void PlayStampAnimation(string grade, int starCount)
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
        _stampSequence.OnComplete(() => PlayStarAnimation(starCount));
    }

    private void PlayStarAnimation(int starCount)
    {
        _starSequence?.Kill();
        _starSequence = DOTween.Sequence();

        int activeStarCount = Mathf.Min(starCount, _starItems.Length); 
        for (var i = 0; i < activeStarCount; i++)
        {
            int index = i;
            _starSequence.AppendCallback(() => _starItems[index].ActiveStar());
            _starSequence.AppendInterval(_starDelay);
        }
    }

    private void ResetUI()
    {
        _stampSequence?.Kill();
        _starSequence?.Kill();
        _gradeText.transform.localScale = Vector3.one;

        foreach (StarItem item in _starItems)
        {
            item.ResetStar();
        }
    }
}
