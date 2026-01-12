using UnityEngine;

[DisallowMultipleComponent]
public class FlyingEnemyDeathFall : MonoBehaviour
{
    [Header("참조용")]
    [SerializeField] private EnemyBase _enemy;
    [SerializeField] private EnemyAnimatorController _anim;

    [Header("레이캐스트")]
    [SerializeField] private float _rayOriginUp = 0.2f;          // 레이 시작점 높이
    [SerializeField] private float _groundCheckDistance = 0.8f;  // 바닥 탐지 거리
    [SerializeField] private Collider[] _selfColliders;          // 자기 콜라이더를 레이캐스트에서 제외

    [Header("낙하 이동")]
    [SerializeField] private float _initialFallSpeed = 0f;  // 초기 낙하 속도
    [SerializeField] private float _gravity = 2f;
    [SerializeField] private float _maxFallSpeed = 5f;     // 최대 낙하 속도

    [Header("착지 관련")]
    [SerializeField] private float _groundSnapOffset = 0.01f;  // 착지 시 약간 띄우기 (뚫림 방지)
    [SerializeField] private bool _callOnGroundOnce = true;    // 착지 애니메이션 1회만 재생

    private bool _fallingActive;
    private bool _landed;
    private float _fallSpeed;
    private float _marginHitDistance = 0.05f; // 착지 인식 여유 거리

    private void Awake()
    {
        if (_enemy == null) _enemy = GetComponent<EnemyBase>();
        if (_anim == null) _anim = GetComponent<EnemyAnimatorController>();

        if (_selfColliders == null || _selfColliders.Length == 0)
        {
            _selfColliders = GetComponentsInChildren<Collider>(true);
        }
    }

    private void OnEnable()
    {
        ResetRuntime();
    }

    private void Update()
    {
        if (_enemy == null || _anim == null) return;

        // 살아있으면 동작 안 함 (풀링 재사용 안전)
        if (!_enemy.IsDead)
        {
            ResetRuntime();
            return;
        }

        // 죽는 순간부터 낙하 시작
        if (!_fallingActive)
        {
            BeginFalling();
        }

        if (_landed) return;

        // 이동 전 바닥 근접이면 바로 착지 처리
        if (TryGetGroundHit(out RaycastHit hitBefore))
        {
            if (IsCloseEnoughToLand(hitBefore.distance))
            {
                SnapToGround(hitBefore.point);
                Land();
                return;
            }
        }

        // 2) 낙하 이동
        StepFall();

        // 3) 이동 후 바닥 체크(뚫림 방지)
        if (TryGetGroundHit(out RaycastHit hitAfter))
        {
            if (IsCloseEnoughToLand(hitAfter.distance))
            {
                SnapToGround(hitAfter.point);
                Land();
            }
        }
    }

    private void BeginFalling()
    {
        _fallingActive = true;
        _landed = false;
        _fallSpeed = _initialFallSpeed;
    }

    private void StepFall()
    {
        _fallSpeed -= _gravity * Time.deltaTime;
        _fallSpeed = Mathf.Max(_fallSpeed, -_maxFallSpeed);

        transform.position += Vector3.up * (_fallSpeed * Time.deltaTime);
    }

    private bool TryGetGroundHit(out RaycastHit bestHit)
    {
        bestHit = default;

        // 레이 시작점 (현재 위치에서 약간 위)
        Vector3 origin = transform.position + Vector3.up * _rayOriginUp;

        // 총 길이 = 위로 올린 만큼 + 체크 거리
        float maxDistance = _rayOriginUp + _groundCheckDistance;

        var hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            maxDistance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (hits == null || hits.Length == 0) return false;

        float best = float.PositiveInfinity;
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null) continue;

            if (IsSelfCollider(h.collider)) continue;

            if (h.distance < best)
            {
                best = h.distance;
                bestHit = h;
            }
        }

        return best < float.PositiveInfinity;
    }

    private bool IsSelfCollider(Collider collider)
    {
        for (int i = 0; i < _selfColliders.Length; i++)
        {
            if (_selfColliders[i] == collider) return true;
        }
        return false;
    }

    private bool IsCloseEnoughToLand(float hitDistance)
    {
        // 거리를 여유 있게 잡기 위해 임계값 추가
        return hitDistance <= (_rayOriginUp + _marginHitDistance);
    }

    private void SnapToGround(Vector3 groundPoint)
    {
        Vector3 point = transform.position;
        point.y = groundPoint.y + _groundSnapOffset;
        transform.position = point;
    }

    private void Land()
    {
        _landed = true;

        if (_callOnGroundOnce)
        {
            _anim.PlayOnGroundDeath();
        }
        else
        {
            _anim.PlayOnGroundDeath();
        }
    }

    private void ResetRuntime()
    {
        _fallingActive = false;
        _landed = false;
        _fallSpeed = 0f;
    }
}
