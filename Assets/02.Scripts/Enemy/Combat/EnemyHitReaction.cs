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
        if (hitEvent.Enemy != _enemy) return;
        if (_movement == null) return;

        Transform source = null;

        if (hitEvent.Attacker != null)
        {
            source = hitEvent.Attacker.transform;
        }
        else if (_player != null)
        {
            source = _player;
        }

        if (source == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[EnemyHitReaction] attacker/player 둘 다 없어서 넉백 스킵");
#endif
            return;
        }

        Vector3 direction = transform.position - source.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;
        direction.Normalize();

        // 넉백 시 바로 원래 경로로 붙는 느낌 최소화
        if (_agent != null && _agent.enabled)
        {
            _agent.ResetPath();
        }

        // 최소 시간만큼 멈추게 하고, Unlock은 바로 요청
        _movement.LockMovement(true, _interruptPathTime);
        _movement.LockMovement(false);

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

        float strength = _pushStrength * (1f + _consecutionHit * _consecutionHitBonus);

        // 엘리트는 취향껏
        float eliteMultiplier = (_enemy.EnemyType == EEnemyType.Elite) ? _eliteStrengthMultiplier : 1f;
        strength *= eliteMultiplier;

        _movement.AddKnockback(direction, strength);
    }
}
