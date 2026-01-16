using System;
using UnityEngine;

public class BaseUIEvent
{
    public Action OnOpenComplete;
    public Action OnCloseComplete;
}

public abstract class BaseUI : MonoBehaviour
{
    public UIConfig Config;

    public BaseUIEvent UIEventHandler;

    protected virtual void Start()
    {
        UIController.Instance.RegisterUI(this);
        gameObject.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        UIController.Instance.UnregisterUI(this);
    }

    public virtual void OnOpen()
    {
        gameObject.SetActive(true);

        if (Config.UseTransition)
        {
            PlayOpenAnimation();
        }
        else
        {
            UIEventHandler?.OnOpenComplete?.Invoke();
        }
    }

    public virtual void OnClose()
    {
        if (Config.UseTransition)
        {
            PlayCloseAnimation();
        }
        else
        {
            UIEventHandler?.OnCloseComplete?.Invoke();
            gameObject.SetActive(false);
        }
    }

    protected virtual void PlayOpenAnimation() { }

    protected virtual void PlayCloseAnimation() { }

    public void BringToFront()
    {
        transform.SetAsLastSibling();
    }
}
