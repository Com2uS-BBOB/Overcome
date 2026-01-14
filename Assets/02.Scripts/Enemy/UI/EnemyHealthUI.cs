using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _healthDelay;
    [SerializeField] private Image _healthBorder;

    [Header("체력바 테두리 색 옵션")]
    [SerializeField] private Color _normalBorderColor;
    [SerializeField] private Color _lowHpBorderColor = new Color(1f, 0.55f, 0f, 0.7058824f);
    [SerializeField, Range(0f, 1f)] private float _lowHpThreshold = 0.5f;
    [SerializeField] private float _borderLerpSpeed = 6f;

    [Header("펄스 옵션 (엘리트)")]
    [SerializeField] private bool _pulseOnlyElite = true;
    [SerializeField] private float _pulseSpeed = 6f;
    [SerializeField] private float _pulseAmount = 0.25f; // 색 밝기 변화량

    private float _pulseRangeScaling = 0.5f;

    [Header("딜레이 옵션")]
    [SerializeField] private float _fillSpeed = 8f;
    [SerializeField] private float _delaySpeed = 5f;
    [SerializeField] private float _delayTime = 0.2f;

    private EnemyBase _enemy;

    private bool _isLowHp;

    private Coroutine _borderLerpCoroutine;
    private Coroutine _borderPulseCoroutine;
    private Coroutine _fillCoroutine;
    private Coroutine _delayCoroutine;

    private void Awake()
    {
        _enemy = GetComponentInParent<EnemyBase>();
        if (_healthBorder != null)
        {
            _normalBorderColor = _healthBorder.color;
        }
    }

    private void OnEnable()
    {
        if (_enemy != null)
        {
            _enemy.OnHpChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnHpChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(float currentHp, float maxHp)
    {
        float ratio = maxHp <= 0f ? 0f : currentHp / maxHp;

        // 일정 체력 이하면 테두리 색 변경
        UpdateBorderColor(ratio);

        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
        }

        if (_delayCoroutine != null)
        {
            StopCoroutine(_delayCoroutine);
        }

        _fillCoroutine = StartCoroutine(SmoothFill_Coroutine(_healthFill, ratio, _fillSpeed));
        _delayCoroutine = StartCoroutine(SmoothDelay_Coroutine(_healthDelay, ratio));
    }

    private void UpdateBorderColor(float ratio)
    {
        bool shouldLowHp = ratio <= _lowHpThreshold;

        // 상태가 바뀔 때만 적용
        if (_isLowHp == shouldLowHp) return;
        _isLowHp = shouldLowHp;

        Color targetColor = _isLowHp ? _lowHpBorderColor : _normalBorderColor;

        if (_borderLerpCoroutine != null)
        {
            StopCoroutine(_borderLerpCoroutine);
        }

        _borderLerpCoroutine = StartCoroutine(BorderColorLerp_Coroutine(targetColor));

        if (_isLowHp && ShouldPulse())
        {
            StartPulse();
        }
        else
        {
            StopPulse();
        }
    }

    // 적 체력이 특정 퍼센트 이하가 되면 UI 테두리 색 변환
    private IEnumerator BorderColorLerp_Coroutine(Color targetColor)
    {
        if (_healthBorder == null) yield break;

        Color startColor = _healthBorder.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * _borderLerpSpeed;
            _healthBorder.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        _healthBorder.color = targetColor;
    }

    private bool ShouldPulse()
    {
        if (_healthBorder == null || _enemy == null) return false;

        if (!_pulseOnlyElite) return true;

        // 엘리트 구분
        return _enemy.EnemyType == EEnemyType.Elite;
    }

    private void StartPulse()
    {
        if (_borderPulseCoroutine != null) return; // 이미 돌고 있으면 중복 방지
        _borderPulseCoroutine = StartCoroutine(BorderPulse_Coroutine());
    }

    private void StopPulse()
    {
        if (_borderPulseCoroutine != null)
        {
            StopCoroutine(_borderPulseCoroutine);
            _borderPulseCoroutine = null;
        }

        // 펄스 끄면 현재 상태에 맞는 색으로 정리
        if (_healthBorder != null)
        {
            _healthBorder.color = _isLowHp ? _lowHpBorderColor : _normalBorderColor;
        }
    }


    private IEnumerator BorderPulse_Coroutine()
    {
        // 기준 색은 LowHpColor로 두고, 거기서 밝게/어둡게 반복
        while (true)
        {
            if (_healthBorder == null) yield break;
            if (!_isLowHp) yield break; // LowHP 아니면 종료

            // 0~1 반복
            float scaling = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * _pulseRangeScaling;
            float multiplier = 1f + (scaling * _pulseAmount);

            // 밝기만 올려서 펄스 처리
            Color baseColor = _lowHpBorderColor;
            Color pulse = new Color(
                Mathf.Clamp01(baseColor.r * multiplier),
                Mathf.Clamp01(baseColor.g * multiplier),
                Mathf.Clamp01(baseColor.b * multiplier),
                baseColor.a
            );

            _healthBorder.color = pulse;
            yield return null;
        }
    }

    // 체력 바 먼저 부드럽게 이동
    private IEnumerator SmoothFill_Coroutine(Image image, float target, float speed)
    {
        while (!Mathf.Approximately(image.fillAmount, target))
        {
            image.fillAmount = Mathf.Lerp(image.fillAmount, target, Time.deltaTime * speed);
            yield return null;
        }

        image.fillAmount = target;
    }

    // 흰 잔상 체력 바는 딜레이 후 부드럽게 이동
    private IEnumerator SmoothDelay_Coroutine(Image image, float target)
    {
        yield return new WaitForSeconds(_delayTime);

        while (!Mathf.Approximately(image.fillAmount, target))
        {
            image.fillAmount = Mathf.Lerp(image.fillAmount, target, Time.deltaTime * _delaySpeed);
            yield return null;
        }

        image.fillAmount = target;
    }
}