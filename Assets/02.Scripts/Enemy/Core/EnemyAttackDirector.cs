using UnityEngine;
using System.Collections.Generic;

public class EnemyAttackDirector : MonoBehaviour
{
    [Header("동시 공격자 제한")]
    [SerializeField] private int _maxAttackers = 3;

    [Header("동시 압박자 제한 (플레이어 바짝 추적)")]
    [SerializeField] private int _maxPressurers = 2;

    [Header("최근 공격자 패널티")]
    [SerializeField] private float _fairnessCooldown = 2f;

    [Header("공격 승인 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float _grantChance = 0.75f;

    private readonly HashSet<Transform> _activeAttackers = new();
    private readonly HashSet<Transform> _activePressurers = new();
    private readonly Dictionary<Transform, float> _lastAttackTime = new();

    // 공격자 예약 시도
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

    // 압박자 예약 시도
    public bool TryReservePressure(Transform enemy)
    {
        if (enemy == null) return false;
        if (_activePressurers.Contains(enemy)) return true;
        if (_activePressurers.Count >= _maxPressurers) return false;

        _activePressurers.Add(enemy);
        return true;
    }

    public void ReleasePressure(Transform enemy)
    {
        if (enemy == null) return;
        _activePressurers.Remove(enemy);
    }

    public bool IsPressurer(Transform enemy) => enemy != null && _activePressurers.Contains(enemy);
}