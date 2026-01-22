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
    [SerializeField] private DOTweenAnimation _openComboPanelAnimation;
    [SerializeField] private DOTweenAnimation _increaseComboAnimation;

    private Tweener _fadeTweener;

    private void Awake()
    {
        _openComboPanelAnimation.autoKill = false;
        _increaseComboAnimation.autoKill = false;
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
        _fadeTweener?.Kill();
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
            _openComboPanelAnimation.DORestart();
        }

        _comboText.text = _comboSystem.ComboCount.ToString();
        _comboText.colorGradient = _comboSystem.ComboColorGradient;
        _gradeText.text = _comboSystem.ComboText;
        _gradeText.colorGradient = _comboSystem.GradeColorGradient;
        _increaseComboAnimation.DORestart();

        _fadeTweener?.Kill();
        _canvasGroup.alpha = 1;
        _fadeTweener = _canvasGroup.DOFade(0, _comboSystem.ComboDuration)
                                   .SetEase(Ease.InQuad);
    }

    private void ClearComboUI()
    {
        _fadeTweener?.Kill();
        _comboText.text = "";
        _comboText.colorGradient = new VertexGradient(Color.black);
        _gradeText.text = "";
        _gradeText.colorGradient = new VertexGradient(Color.black);
        _canvasGroup.alpha = 0;
    }
}
