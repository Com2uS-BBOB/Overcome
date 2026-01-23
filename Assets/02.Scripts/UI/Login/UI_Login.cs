using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TMP_InputField))]
public class UI_Login : MonoBehaviour
{
    [SerializeField] private LoginScene _loginScene;
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
            UI_StageSelect.IsFirst = false;
            loginPopup?.SetTitle(WelcomeMessages[0]);
        }
        else
        {
            UI_StageSelect.IsFirst = true;
            string text = WelcomeMessages[1];
            loginPopup?.SetTitle($"{text} {_username.text}");
        }
        loginPopup.OnLoginCompleted += _loginScene.HandleLoginCompleted;
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
