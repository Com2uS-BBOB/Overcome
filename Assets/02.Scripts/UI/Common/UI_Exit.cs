using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UI_Exit : BaseUI
{
    private static readonly int _open = Animator.StringToHash("Open");
    private static readonly int _close = Animator.StringToHash("Close");
    private Animator _animator;
    
    protected override void Init()
    {
        _animator = GetComponent<Animator>();
    }
    
    protected override void PlayCloseAnimation()
    {
        _animator.SetTrigger(_close);
    }

    private void DeactiveUI()
    {
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        UIController.Instance.CloseUI(this);
    }
    
    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
