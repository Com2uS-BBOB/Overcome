using System;
using UnityEngine;
using DG.Tweening;

public class KillInfo : MonoBehaviour
{
    private static readonly string[] EnemyTypeNames = 
        { "Normal", "Small", "Elite", "FloatSmall" };

    [Serializable]
    public struct KillInfoData
    {
        public EEnemyType Type;
        public int KillCount;
    }

    [SerializeField] private KillInfoData _killInfo;
    [SerializeField] private ProgressiveScrambleText _enemyType;
    [SerializeField] private ProgressiveScrambleText _killCountText;
    
    private void Awake()
    {
        _enemyType.Init(this);
        _killCountText.Init(this);
    }

    private void OnEnable()
    {
        Show();
    }
    
    private void OnDisable()
    {
        _enemyType.Stop();
        _killCountText.Stop();
    }

    private void Show()
    {
        _killInfo.KillCount = KillLogSystem.Instance.GetKillCount(_killInfo.Type);
        gameObject.SetActive(true);
        _enemyType.Play(EnemyTypeNames[(int)_killInfo.Type]);
        _killCountText.Play(_killInfo.KillCount);
    }

    public void Hide()
    {
        _killCountText.Stop();
        _enemyType.Stop();
        gameObject.SetActive(false);
    }
}