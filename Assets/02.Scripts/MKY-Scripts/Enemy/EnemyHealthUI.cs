using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _healthDelay;

    [Header("딜레이 옵션")]
    [SerializeField] private float _delayTime = 0.3f;
    [SerializeField] private float _smoothSpeed = 2f;

    private EnemyBase _enemy;

    private float _maxHealth;
    private Coroutine _delayCoroutine;

    private void Awake()
    {
        _enemy = GetComponentInParent<EnemyBase>();
    }

    private void OnEnable()
    {
        _enemy.OnHealthChanged += OnHealthChanged;
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
        _maxHealth = maxHp;

        float target = currentHp / maxHp;
        _healthFill.fillAmount = target;

        if (_delayCoroutine != null)
        {
            StopCoroutine(_delayCoroutine);
        }

        _delayCoroutine = StartCoroutine(DelayBar(target));
    }

    private IEnumerator DelayBar(float target)
    {
        yield return new WaitForSeconds(_delayTime);

        while (_healthDelay.fillAmount > target)
        {
            _healthDelay.fillAmount = Mathf.Lerp(_healthDelay.fillAmount,target,Time.deltaTime * _smoothSpeed);

            yield return null;
        }

        _healthDelay.fillAmount = target;
    }
}