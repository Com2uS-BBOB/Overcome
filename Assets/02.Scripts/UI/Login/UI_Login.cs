using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TMP_InputField))]
public class UI_Login : MonoBehaviour
{
    private static readonly int _onSelectLogin = Animator.StringToHash("OnSelectLogin");
    private TMP_InputField _username;
    private Animator _animator;

    private static readonly string[] WelcomeMessages = new string[]
    {
        "Oh, it's good\nto be back!",
        "WELCOME TO THE OVERCOME\n"
    };

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _username = GetComponent<TMP_InputField>();
    }
    
    public async void TryLogin()
    {
        if (_username.text == "") return;
        bool existingUser = PlayerDataManager.Instance.InitializeStartPlayer(_username.text);
        UI_LoginPopup loginPopup = await UIController.Instance.OpenUI<UI_LoginPopup>();
        if (existingUser)
        {
            loginPopup?.SetTitle(WelcomeMessages[0]);
        }
        else
        {
            string text = WelcomeMessages[1];
            loginPopup?.SetTitle($"{text} {_username.text}");
        }
    }

    public void OnSelectLogin()
    {
        if (_username.text != "") return;
        _animator.SetBool(_onSelectLogin, true);
    }

    public void OnDeselectLogin()
    {
        if (_username.text != "") return;
        _animator.SetBool(_onSelectLogin, false);
    }
}
