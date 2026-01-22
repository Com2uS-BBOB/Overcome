using System;
using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public UIConfig Config;
    
    private float _prevTimeScale;
    private CursorLockMode _cursorState;
    private bool _cursorVisible;
    
    private void Awake()
    {
        Init();
    }

    protected virtual void Init() { }
    
    public virtual void OnOpen()
    {
        gameObject.SetActive(true);

        if (Config.HasSound)
        {
            SoundManager.Instance.PlaySfx("SFX_UIOpen");
        }
        if (Config.PauseGame)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        if (Config.ShowCursor)
        {
            _cursorState = Cursor.lockState;
            _cursorVisible = Cursor.visible;
            
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        if (Config.UseTransition)
        {
            PlayOpenAnimation();
        }
    }

    public virtual void OnClose()
    {
        if (Config.HasSound)
        {
            SoundManager.Instance.PlaySfx("SFX_UIClose");
        }
        if (Config.PauseGame)
        {
            Time.timeScale = _prevTimeScale;
        }
        if (Config.ShowCursor)
        {
            Cursor.lockState = _cursorState;
            Cursor.visible = _cursorVisible;
        }
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
