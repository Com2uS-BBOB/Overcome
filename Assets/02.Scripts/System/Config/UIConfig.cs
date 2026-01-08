using System;
using UnityEngine;

[Serializable]
public class UIConfig
{
    [Header("기본 정보")]
    public string UIName;
    public EUIType UIType;

    [Header("동작 설정")]
    public bool PauseGame;
    public bool UseTransition;
    public bool ShowCursor;
}