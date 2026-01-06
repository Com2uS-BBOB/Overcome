using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private GameObject _targetPlayer;

    private void FixedUpdate()
    {
        transform.position = _targetPlayer.transform.position + new Vector3(0, 20, 0);
    }
}
