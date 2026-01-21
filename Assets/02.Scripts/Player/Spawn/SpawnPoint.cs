using UnityEngine;

namespace _02.Scripts.Player.Spawn
{
    /// <summary>
    /// 스폰 위치 마커. 빈 게임오브젝트에 부착하여 리스폰 위치로 사용
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Gizmo Settings")]
        [SerializeField] private Color _gizmoColor = Color.green;
        [SerializeField] private float _gizmoRadius = 0.5f;

        public Vector3 Position => transform.position;

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _gizmoRadius);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.5f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, _gizmoRadius * 0.3f);
        }
    }
}