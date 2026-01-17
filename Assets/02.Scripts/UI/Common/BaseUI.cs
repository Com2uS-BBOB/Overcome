using System;
using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public UIConfig Config;
    
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
    }

    public virtual void OnClose()
    {
        if (Config.UseTransition)
        {
            PlayCloseAnimation();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    protected virtual void PlayOpenAnimation() { }

    protected virtual void PlayCloseAnimation() { }
}
