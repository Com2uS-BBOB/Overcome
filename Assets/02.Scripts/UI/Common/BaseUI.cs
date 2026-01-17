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

    private void Awake()
    {
        Init();
    }

    protected virtual void Init() { }
    
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
}
