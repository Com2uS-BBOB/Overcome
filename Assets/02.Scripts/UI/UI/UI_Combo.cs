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
    }

    private void ClearComboUI()
    {
        _comboText.text = "";
        _comboText.colorGradient = new VertexGradient(Color.black);
        _gradeText.text = "";
        _gradeText.colorGradient = new VertexGradient(Color.black);
        _canvasGroup.alpha = 0;
    }
}
