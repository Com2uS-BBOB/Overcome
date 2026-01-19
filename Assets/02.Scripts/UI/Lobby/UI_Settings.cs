using UnityEngine;

public class UI_Settings : BaseUI
{
    private bool _isOpen;
    private Animator _animator;
    protected override void Init()
    {
        _animator = GetComponent<Animator>();
        gameObject.SetActive(false);
    }

    protected override void PlayOpenAnimation()
    {
        _animator.SetTrigger("Panel In");
    }

    protected override void PlayCloseAnimation()
    {
        _animator.SetTrigger("Panel Out");
    }

    public void ExitPanel()
    {
        UIController.Instance.CloseUI(this);
    }
    
    public void DeactivePanel()
    {
        gameObject.SetActive(false);
    }
}
