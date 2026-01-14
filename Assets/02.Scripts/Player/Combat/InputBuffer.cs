using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 입력 버퍼 시스템
    /// 캔슬 불가 구간에서 입력을 저장하고, 캔슬 가능해지면 실행
    /// </summary>
    public class InputBuffer : MonoBehaviour
    {
        [SerializeField] private float _bufferDuration = 0.15f;

        public struct BufferedInput
        {
            public ActionType Action;
            public float Timestamp;
            public Vector2 Direction;

            public BufferedInput(ActionType action, Vector2 direction = default)
            {
                Action = action;
                Timestamp = Time.time;
                Direction = direction;
            }
        }

        private readonly Queue<BufferedInput> _buffer = new();

        /// <summary>
        /// 입력을 버퍼에 저장
        /// </summary>
        public void Buffer(ActionType action, Vector2 direction = default)
        {
            _buffer.Enqueue(new BufferedInput(action, direction));
        }

        /// <summary>
        /// 버퍼에서 유효한 입력을 꺼냄 (만료된 입력은 자동 제거)
        /// </summary>
        public bool TryConsume(out BufferedInput input)
        {
            while (_buffer.Count > 0)
            {
                input = _buffer.Peek();

                // 만료된 입력 제거
                if (Time.time - input.Timestamp > _bufferDuration)
                {
                    _buffer.Dequeue();
                    continue;
                }

                _buffer.Dequeue();
                return true;
            }

            input = default;
            return false;
        }

        /// <summary>
        /// 버퍼에 유효한 입력이 있는지 확인 (제거하지 않음)
        /// </summary>
        public bool HasValidInput()
        {
            CleanExpiredInputs();
            return _buffer.Count > 0;
        }

        /// <summary>
        /// 버퍼 초기화
        /// </summary>
        public void Clear()
        {
            _buffer.Clear();
        }

        private void CleanExpiredInputs()
        {
            while (_buffer.Count > 0)
            {
                var input = _buffer.Peek();
                if (Time.time - input.Timestamp > _bufferDuration)
                {
                    _buffer.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }
    }
}