using UnityEngine;
using UnityEngine.UI;

public class StarItem : MonoBehaviour
{
    [SerializeField] private Image _starImage;

    public void ActiveStar()
    {
        _starImage.color = Color.yellowNice;
    }
    public void DeactiveColor()
    {
        _starImage.color = Color.white;
    }
}
