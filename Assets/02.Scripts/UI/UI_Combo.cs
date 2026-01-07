using DG.Tweening;
using TMPro;
using UnityEngine;

public class UI_Combo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private TextMeshProUGUI _gradeText;
    [SerializeField] private CanvasGroup _canvasGroup;
    private ComboSystem _comboSystem;

    [Header("Animation")]
    [SerializeField] private DOTweenAnimation _animation;
    [SerializeField] private RectTransform _comboTextTransform;
    [SerializeField] private RectTransform _gradeTextTransform;

    private void Awake()
    {
        if (_animation == null)
        {
            _animation = GetComponent<DOTweenAnimation>();
        }
        _animation.autoKill = false;

        // RectTransform 자동 설정
        if (_comboTextTransform == null && _comboText != null)
        {
            _comboTextTransform = _comboText.GetComponent<RectTransform>();
        }
        if (_gradeTextTransform == null && _gradeText != null)
        {
            _gradeTextTransform = _gradeText.GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        _comboSystem = ComboSystem.Instance;
        _comboSystem.OnComboChanged += UpdateComboUI;
        ClearComboUI();
    }

    private void OnDestroy()
    {
        _comboSystem.OnComboChanged -= UpdateComboUI;
    }

    private void UpdateComboUI()
    {
        if (_comboSystem.ComboCount == 0)
        {
            ClearComboUI();
            return;
        }
        if (_canvasGroup.alpha == 0)
        {
            _canvasGroup.alpha = 1;
            _animation.DORestart();
        }

        _comboText.text = _comboSystem.ComboCount.ToString();
        _comboText.color = _comboSystem.ComboColor;
        _gradeText.text = _comboSystem.ComboText;
        _gradeText.color = _comboSystem.GradeColor;
    }

    private void ClearComboUI()
    {
        _comboText.text = "";
        _comboText.color = Color.black;
        _gradeText.text = "";
        _gradeText.color = Color.black;
        _canvasGroup.alpha = 0;
    }
}
