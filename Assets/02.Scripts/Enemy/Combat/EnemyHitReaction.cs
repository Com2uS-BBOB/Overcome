using UnityEngine;
using UnityEngine.AI;

public class EnemyHitReaction : MonoBehaviour
{
    [Header("넉백 옵션")]
    [SerializeField] private float _pushStrength = 18f;   // 한 대당 기본 밀림
    [SerializeField] private float _consecutionHitTime = 0.6f;    // 연속 히트 판정 시간
    [SerializeField] private float _consecutionHitBonus = 0.8f;    // 연속 히트 시 추가
    [SerializeField] private float _eliteStrengthMultiplier = 1.4f;

    [Header("넉백 경로 끊기 (되돌아오기 방지)")]
    [SerializeField] private float _interruptPathTime = 0.18f;

    [Header("플레이어 (Attacker가 null일 때 사용)")]
    [SerializeField] private Transform _player;

    private EnemyBase _enemy;
    private EnemyMovement _movement;
    private NavMeshAgent _agent;

    private float _lastHitTime;
    private int _consecutionHit;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _agent = GetComponent<NavMeshAgent>();

        if (_player == null)
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null) _player = playerGo.transform;
        }
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
        if (!IsValidHit(hitEvent)) return;

        if (!TryGetAttackSource(hitEvent, out Transform source)) return;
        if (!TryGetKnockbackDirection(source, out Vector3 direction)) return;

        PrepareMovementForKnockback();

        UpdateConsecutiveHit();
        float strength = CalculateKnockbackStrength();

        ApplyKnockback(direction, strength);
    }

    private bool IsValidHit(EnemyHitEvent hitEvent)
    {
        if (hitEvent.Enemy != _enemy) return false;
        if (_movement == null) return false;
        return true;
    }

    private bool TryGetAttackSource(EnemyHitEvent hitEvent, out Transform source)
    {
        if (hitEvent.Attacker != null)
        {
            source = hitEvent.Attacker.transform;
            return true;
        }

        if (_player != null)
        {
            source = _player;
            return true;
        }
        source = null;
        return false;
    }

    private bool TryGetKnockbackDirection(Transform source, out Vector3 direction)
    {
        direction = transform.position - source.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return false;

        direction.Normalize();
        return true;
    }

    // 이동 상태 정리
    private void PrepareMovementForKnockback()
    {
        if (_agent != null && _agent.enabled)
        {
            _agent.ResetPath();
        }

        // 잠깐 멈췄다가 바로 풀어서 되돌아붙는 느낌 제거
        _movement.LockMovement(true, _interruptPathTime);
        _movement.LockMovement(false);
    }

    // 연속 히트 누적 관리
    private void UpdateConsecutiveHit()
    {
        float now = Time.time;

        if (now - _lastHitTime <= _consecutionHitTime)
        {
            _consecutionHit++;
        }
        else
        {
            _consecutionHit = 0;
        }

        _lastHitTime = now;
    }

    // 누적된 넉백 힘 계산
    private float CalculateKnockbackStrength()
    {
        float strength = _pushStrength * (1f + _consecutionHit * _consecutionHitBonus);

        if (_enemy.EnemyType == EEnemyType.Elite)
        {
            strength *= _eliteStrengthMultiplier;
        }

        return strength;
    }

    // 넉백 계산
    private void ApplyKnockback(Vector3 direction, float strength)
    {
        _movement.AddKnockback(direction, strength);
    }
}
