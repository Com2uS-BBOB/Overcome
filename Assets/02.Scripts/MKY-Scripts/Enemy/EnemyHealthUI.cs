using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _healthDelay;

    [Header("딜레이 옵션")]
    [SerializeField] private float _fillSpeed = 8f;
    [SerializeField] private float _delaySpeed = 5f;
    [SerializeField] private float _delayTime = 0.3f;

    private EnemyBase _enemy;

    private Coroutine _fillCoroutine;
    private Coroutine _delayCoroutine;

    private void Awake()
    {
        _enemy = GetComponentInParent<EnemyBase>();
    }

    private void OnEnable()
    {
        if (_enemy != null)
        {
            _enemy.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(float currentHp, float maxHp)
    {
        float target = currentHp / maxHp;

        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
        }

        if (_delayCoroutine != null)
        {
            StopCoroutine(_delayCoroutine);
        }

        _fillCoroutine = StartCoroutine(SmoothFill(_healthFill, target, _fillSpeed));
        _delayCoroutine = StartCoroutine(SmoothDelay(_healthDelay, target));
    }

    private IEnumerator SmoothFill(Image image, float target, float speed)
    {
        while (!Mathf.Approximately(image.fillAmount, target))
        {
            image.fillAmount = Mathf.Lerp(image.fillAmount, target, Time.deltaTime * speed);
            yield return null;
        }

        image.fillAmount = target;
    }

    private IEnumerator SmoothDelay(Image image, float target)
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