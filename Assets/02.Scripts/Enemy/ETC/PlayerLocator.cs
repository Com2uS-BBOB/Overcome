using UnityEngine;

public class PlayerLocator : MonoBehaviour
{
    public static Transform Player { get; private set; }

    private void Awake()
    {
        Player = transform;
    }
}
