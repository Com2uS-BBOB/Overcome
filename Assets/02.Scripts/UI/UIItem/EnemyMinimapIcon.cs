using UnityEngine;
using UnityEngine.UI;

public class EnemyMinimapIcon : MonoBehaviour
{
    [Header("Icon Elements")]
    [SerializeField] private Image _normalIcon;
    [SerializeField] private Image _directionIndicator;

    private RectTransform _rectTransform;
    private RectTransform _directionRect;
    private Color _inRangeColor;
    private Color _outOfRangeColor;

    private float _minimapRadius;
    private float _minimapRadiusSqr;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _directionRect = _directionIndicator.rectTransform;
    }

    public void Initialize(Color inRangeColor, Color outOfRangeColor, float minimapRadius)
    {
        _inRangeColor = inRangeColor;
        _outOfRangeColor = outOfRangeColor;
        _minimapRadius = minimapRadius;
        _minimapRadiusSqr = _minimapRadius * _minimapRadius;
    }

    public void UpdatePosition(Vector2 mapPosition)
    {
        float sqrDistance = mapPosition.sqrMagnitude;
        bool isInRange = sqrDistance <= _minimapRadiusSqr;

        if (isInRange)
        {
            ShowInRange(mapPosition);
        }
        else
        {
            float distance = Mathf.Sqrt(sqrDistance);
            Vector2 direction = mapPosition / distance;
            Vector2 edgePosition = direction * _minimapRadius;
            ShowOutOfRange(edgePosition, direction);
        }
    }

    private void ShowInRange(Vector2 position)
    {
        _rectTransform.anchoredPosition = position;

        SetIconActive(true);
        SetDirectionIndicatorActive(false);

        _normalIcon.color = _inRangeColor;
    }

    private void ShowOutOfRange(Vector2 edgePosition, Vector2 direction)
    {
        _rectTransform.anchoredPosition = edgePosition;

        SetIconActive(false);
        SetDirectionIndicatorActive(true);

        _directionIndicator.color = _outOfRangeColor;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _directionRect.localRotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private void SetIconActive(bool active)
    {
        _normalIcon.gameObject.SetActive(active);
    }

    private void SetDirectionIndicatorActive(bool active)
    {
        _directionIndicator.gameObject.SetActive(active);
    }
}
