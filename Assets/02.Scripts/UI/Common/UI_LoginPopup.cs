using TMPro;
using UnityEngine;

public class UI_LoginPopup : BaseUI
{
    [SerializeField] private TextMeshProUGUI _welcomeText;
    
    public void SetTitle(string title)
    {
        _welcomeText.text = title;
    }
}
