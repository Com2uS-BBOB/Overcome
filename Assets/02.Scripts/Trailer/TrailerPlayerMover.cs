using UnityEngine;

namespace _02.Scripts.Trailer
{
    /// <summary>
    /// 트레일러용 플레이어 자동 이동
    /// 플레이어 오브젝트에 붙이면 앞으로 걸어감
    /// </summary>
    public class TrailerPlayerMover : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _speed = 2f;
        [SerializeField] private Vector3 _direction = Vector3.forward;

        [Header("Options")]
        [SerializeField] private bool _moveOnStart = true;

        private bool _isMoving;

        private void Start()
        {
            if (_moveOnStart)
                _isMoving = true;
        }

        private void Update()
        {
            if (!_isMoving) return;

            Vector3 move = transform.TransformDirection(_direction.normalized) * _speed * Time.deltaTime;
            transform.position += move;
        }

        [ContextMenu("Start Moving")]
        public void StartMove() => _isMoving = true;

        [ContextMenu("Stop Moving")]
        public void StopMove() => _isMoving = false;
    }
}