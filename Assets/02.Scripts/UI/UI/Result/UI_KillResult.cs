using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UI_KillResult : MonoBehaviour
{
    [SerializeField] private Transform _killInfosParent;
    private readonly List<KillInfo> _killInfos = new List<KillInfo>();

    public event Action OnComplete;

    private int _completedCount;

    private void Awake()
    {
        InitKillInfos();
    }

    private void OnDestroy()
    {
        foreach (KillInfo killInfo in _killInfos)
        {
            killInfo.OnComplete -= HandleKillInfoComplete;
        }
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
                killInfo.OnComplete += HandleKillInfoComplete;
            }
        }
    }

    private void HandleKillInfoComplete()
    {
        _completedCount++;
        if (_completedCount >= _killInfos.Count)
        {
            OnComplete?.Invoke();
        }
    }

    public void Show()
    {
        _completedCount = 0;
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

    public void Complete()
    {
        foreach (KillInfo killInfo in _killInfos)
        {
            killInfo.Complete();
        }
    }
}
