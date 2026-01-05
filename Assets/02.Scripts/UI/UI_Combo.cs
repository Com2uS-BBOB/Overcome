using TMPro;
using UnityEngine;

public class UI_Combo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private TextMeshProUGUI _gradeText;
    private ComboSystem _comboSystem;
    
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
        
        _comboText.text = $"{_comboSystem.ComboCount} Combo!";
        _gradeText.text = _comboSystem.ComboText;
    }

    private void ClearComboUI()
    {
        _comboText.text = "";
        _gradeText.text = "";
    }
}
