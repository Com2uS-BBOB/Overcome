using UnityEngine;

public class UI_Settings : MonoBehaviour
{
    private bool _isOpen;
    private Animator _animator;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OpenPanel()
    {
        if (_isOpen) return;
        _isOpen = true;
        _animator.SetTrigger("Panel In");
    }

    public void ClosePanel()
    {
        if (!_isOpen) return;
        _isOpen = false;
        _animator.SetTrigger("Panel Out");
    }
}
