using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _healthDelay;
    [SerializeField] private Image _healthBorder;

    [Header("체력바 테두리 색")]
    [SerializeField] private Color _normalBorderColor;
    [SerializeField] private Color _lowHpBorderColor = new Color(1f, 0.55f, 0f, 0.7058824f);
    [SerializeField, Range(0f, 1f)] private float _lowHpThreshold = 0.5f;

    [Header("딜레이 옵션")]
    [SerializeField] private float _fillSpeed = 8f;
    [SerializeField] private float _delaySpeed = 5f;
    [SerializeField] private float _delayTime = 0.2f;

    private EnemyBase _enemy;

    private bool _isLowHp;

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

        if (_healthBorder != null)
        {
            _healthBorder.color = _isLowHp ? _lowHpBorderColor : _normalBorderColor;
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