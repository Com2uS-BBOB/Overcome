using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _healthDelay;
    [SerializeField] private Image _healthBorder;

    [Header("딜레이 옵션")]
    [SerializeField] private float _fillSpeed = 8f;
    [SerializeField] private float _delaySpeed = 5f;
    [SerializeField] private float _delayTime = 0.2f;

    private EnemyBase _enemy;

    private Coroutine _fillCoroutine;
    private Coroutine _delayCoroutine;

    private void Awake()
    {
        _enemy = GetComponentInParent<EnemyBase>();

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        if (_enemy == null) return;

        _enemy.OnHpChanged += OnHealthChanged;
        _enemy.OnDeath += HandleDeath;
        SetVisible(true);

        if (_enemy.EnemyStatData == null) return;

        ForceRefresh();
    }

    private void OnDisable()
    {
        if (_enemy == null) return;

        _enemy.OnHpChanged -= OnHealthChanged;
        _enemy.OnDeath -= HandleDeath;

        Cleanup_Coroutines();
    }

    private void HandleDeath()
    {
        Cleanup_Coroutines();
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
            return;
        }
    }

    private void ForceRefresh()
    {
        Cleanup_Coroutines();

        if (_enemy.EnemyStatData == null) return;

        float max = _enemy.MaxHp;
        float current = _enemy.CurrentHp;

        float ratio = max <= 0f ? 0f : current / max;

        if (_healthFill != null) _healthFill.fillAmount = ratio;
        if (_healthDelay != null) _healthDelay.fillAmount = ratio;
    }

    private void Cleanup_Coroutines()
    {
        if (_fillCoroutine != null) { StopCoroutine(_fillCoroutine); _fillCoroutine = null; }
        if (_delayCoroutine != null) { StopCoroutine(_delayCoroutine); _delayCoroutine = null; }
    }

    private void OnHealthChanged(float currentHp, float maxHp)
    {
        float ratio = maxHp <= 0f ? 0f : currentHp / maxHp;

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