using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_GradeInfo : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _gradeName;
    [SerializeField] private TextMeshProUGUI _requiredScore;
    [SerializeField] private GameObject _clearedMark;
    [SerializeField] private Image _starIcon;

    [Header("Visual Settings")]
    [SerializeField] private Color _clearedColor = Color.yellow;
    [SerializeField] private Color _notClearedColor = Color.gray;

    public void SetInfo(string gradeName, int requiredScore, int rewardStars, bool isCleared)
    {
        if (_gradeName != null)
            _gradeName.SetText(gradeName);

        if (_requiredScore != null)
            _requiredScore.SetText(requiredScore.ToString("N0"));

        if (_clearedMark != null)
            _clearedMark.SetActive(isCleared);

        if (_starIcon != null)
            _starIcon.color = isCleared ? _clearedColor : _notClearedColor;
    }

    public void SetInfo(GradeConfig config, bool isCleared)
    {
        SetInfo(config.Grade, config.RequiredScore, config.RewardStars, isCleared);
    }
}
