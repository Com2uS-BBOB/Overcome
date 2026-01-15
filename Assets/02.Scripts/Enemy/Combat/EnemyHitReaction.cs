using UnityEngine;

public class EnemyHitReaction : MonoBehaviour
{
    [Header("넉백 옵션")]
    [SerializeField] private float _pushStrength = 0.7f;   // 한 대당 기본 밀림
    [SerializeField] private float _consecutionHitTime = 0.3f;    // 연속 히트 판정 시간
    [SerializeField] private float _consecutionHitBonus = 0.2f;     // 연속 히트 시 추가

    private EnemyBase _enemy;
    private EnemyMovement _movement;

    private float _lastHitTime;
    private int _consecutionHit;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
    }

    private void OnEnable()
    {
        EnemyEventController.Enemy.OnHit += OnHit;
        _consecutionHit = 0;
        _lastHitTime = 0f;
    }

    private void OnDisable()
    {
        EnemyEventController.Enemy.OnHit -= OnHit;
    }

    private void OnHit(EnemyHitEvent hitEvent)
    {
        if (hitEvent.Enemy != _enemy) return;
        if (_movement == null) return;

        // attacker 없으면 방향을 못 잡으니 스킵
        if (hitEvent.Attacker == null) return;

        Vector3 direction = transform.position - hitEvent.Attacker.transform.position;
        direction.y = 0f;

        float now = Time.time;
        if (now - _lastHitTime <= _consecutionHitTime) _consecutionHit++;
        else _consecutionHit = 0;

        _lastHitTime = now;

        float strength = _pushStrength * (1f + _consecutionHit * _consecutionHitBonus);
        float eliteMultiplier = (_enemy.EnemyType == EEnemyType.Elite) ? 1.3f : 1f;
        strength *= eliteMultiplier;
        _movement.AddKnockback(direction, strength);
    }
}
