using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UI_KillResult : MonoBehaviour
{
    [SerializeField] private Transform _killInfosParent;
    private readonly List<KillInfo> _killInfos = new List<KillInfo>();

    private void Awake()
    {
        InitKillInfos();
    }

    private void OnEnable()
    {
        Show();
    }

    private void OnDisable()
    {
        foreach (KillInfo killInfo in _killInfos)
        {
            killInfo.Hide();
        }
    }
    
    private void InitKillInfos()
    {
        _killInfos.Clear();
        foreach (Transform child in _killInfosParent)
        {
            KillInfo killInfo = child.GetComponent<KillInfo>();
            if (killInfo != null)
            {
                _killInfos.Add(killInfo);
            }
        }
    }

    public void Show()
    {
        foreach (KillInfo killInfo in _killInfos)
        {
            killInfo.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        foreach (KillInfo killInfo in _killInfos)
        {
            killInfo.Hide();
        }
        gameObject.SetActive(false);
    }
}
