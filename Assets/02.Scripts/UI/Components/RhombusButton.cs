using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(RectTransform))]
public class RhombusButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, ICanvasRaycastFilter
{
    private Image _image;
    private RectTransform _rectTransform;

    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _pressedSprite;
    
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _pressedColor = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private UnityEvent _onButtonClicked = new UnityEvent();

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.sprite = _normalSprite;
        _image.color = _normalColor;
        _image.raycastTarget = true;

        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _image.sprite = _pressedSprite;
        _image.color = _pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _image.sprite = _normalSprite;
        _image.color = _normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onButtonClicked?.Invoke();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform,
            screenPoint,
            eventCamera,
            out Vector2 localPoint
        );

        Rect rect = _rectTransform.rect;
        float halfWidth = rect.width / 2f;
        float halfHeight = rect.height / 2f;

        float distance = Mathf.Abs(localPoint.x / halfWidth) + 
                         Mathf.Abs(localPoint.y / halfHeight);

        return distance <= 1f;
    }
}