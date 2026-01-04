using UnityEngine;
using UnityEngine.UI;

public class TestComboUI : MonoBehaviour
{
    public Text ComboText;
    private ComboSystem _comboSystem;
    
    private void Start()
    {
        _comboSystem = ComboSystem.Instance;
        _comboSystem.OnComboChanged += UpdateComboUI;
    }

    private void OnDestroy()
    {
        _comboSystem.OnComboChanged -= UpdateComboUI;
    }

    public void UpdateComboUI()
    {
        ComboText.text = $"Combo : {_comboSystem.ComboCount}\n {_comboSystem.ComboText}";
    }
}
