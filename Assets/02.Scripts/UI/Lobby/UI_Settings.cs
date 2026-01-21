using UnityEngine;

public class UI_Settings : BaseUI
{
    private static readonly int _close = Animator.StringToHash("Close");
    private bool _isOpen;
    [SerializeField] private Animator _animator;
    
    protected override void PlayCloseAnimation()
    {
        _animator.SetTrigger(_close);
    }

    public void ExitPanel()
    {
        UIController.Instance.CloseUI(this);
    }
    
    public void DeactivatePanel()
    {
        gameObject.SetActive(false);
    }
}
