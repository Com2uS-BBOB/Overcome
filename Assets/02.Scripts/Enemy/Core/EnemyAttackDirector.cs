using UnityEngine;
using System.Collections.Generic;

public class EnemyAttackDirector : MonoBehaviour
{
    [Header("동시 공격자 제한")]
    [SerializeField] private int _maxAttackers = 3;

    [Header("최근 공격자 패널티")]
    [SerializeField] private float _fairnessCooldown = 2f;

    [Header("공격 승인 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float _grantChance = 0.75f;

    private readonly HashSet<Transform> _activeAttackers = new();
    private readonly Dictionary<Transform, float> _lastAttackTime = new();

    public bool TryReserve(Transform enemy)
    {
        if (enemy == null) return false;

        // 이미 공격 슬롯 보유 중이면 OK
        if (_activeAttackers.Contains(enemy)) return true;

        // 동시 공격자 제한
        if (_activeAttackers.Count >= _maxAttackers) return false;

        // 최근 공격자 패널티
        if (_lastAttackTime.TryGetValue(enemy, out float lastT))
        {
            if (Time.time - lastT < _fairnessCooldown) return false;
        }

        // 확률 승인
        if (Random.value > _grantChance) return false;

        _activeAttackers.Add(enemy);
        return true;
    }

    public void Release(Transform enemy)
    {
        if (enemy == null) return;

        if (_activeAttackers.Remove(enemy))
        {
            _lastAttackTime[enemy] = Time.time;
        }
    }

    public bool IsAttacker(Transform enemy) => enemy != null && _activeAttackers.Contains(enemy);
}