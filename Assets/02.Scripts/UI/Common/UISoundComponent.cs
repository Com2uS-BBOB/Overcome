using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundComponent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySfx("SFX_Click");
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySfx("SFX_Hover");
    }
}
