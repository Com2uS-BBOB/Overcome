using System;
using System.Collections;
using UnityEngine;

public class BaseUIEvent
{
    public Action OnOpenComplete;
    public Action OnCloseComplete;
}

public class BaseUI : MonoBehaviour
{
    public UIConfig Config;
    [SerializeField] private Canvas _canvas;

    public BaseUIEvent UIEventHandler;

    private void Start()
    {
        UIController.Instance.RegisterUI(Config.UIName, this);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UIController.Instance?.UnregisterUI(Config.UIName);
    }

    public virtual void OnOpen()
    {
        gameObject.SetActive(true);

        if (Config.UseTransition)
        {
            StartCoroutine(OpenAnimationCoroutine());
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
            StartCoroutine(CloseAnimationCoroutine());
        }
        else
        {
            UIEventHandler?.OnCloseComplete?.Invoke();
            gameObject.SetActive(false);
        }
    }

    protected virtual IEnumerator OpenAnimationCoroutine()
    {
        // todo. UI 활성화 애니메이션 또는 VFX 추가
        UIEventHandler?.OnOpenComplete?.Invoke();
        yield break;
    }

    protected virtual IEnumerator CloseAnimationCoroutine()
    {
        // todo. UI 비활성화 애니메이션 또는 VFX 추가
        UIEventHandler?.OnCloseComplete?.Invoke();
        gameObject.SetActive(false);
        yield break;
    }

    public void SetSortingOrder(int order)
    {
        _canvas.sortingOrder = order;
    }
}