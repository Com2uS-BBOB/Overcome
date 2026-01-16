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
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _username = GetComponent<TMP_InputField>();
    }
    
    public void TryLogin()
    {
        bool existingUser = PlayerDataManager.Instance.InitializeStartPlayer(_username.text);
        // todo. 기존 유저 여부에 따라 popup 출력
        SceneController.Instance.LoadSceneAsync(ESceneType.LobbyScene);
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
