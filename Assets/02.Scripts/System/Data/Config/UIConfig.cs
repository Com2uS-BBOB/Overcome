using System;
using UnityEngine;

[Serializable]
public class UIConfig
{
    [Header("동작 설정")]
    public bool PauseGame;
    public bool UseTransition;
    public bool ShowCursor;
}