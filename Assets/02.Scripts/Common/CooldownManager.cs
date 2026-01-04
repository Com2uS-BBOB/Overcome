using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Common
{
    /// <summary>
    /// 쿨타임 중앙 관리 클래스
    /// 스킬, 대시 등의 쿨타임을 통합 관리
    /// </summary>
    public class CooldownManager
    {
        private readonly Dictionary<string, float> _lastUseTimes = new();

        /// <summary>
        /// 쿨타임 준비 여부
        /// </summary>
        public bool IsReady(string key, float cooldown)
        {
            if (!_lastUseTimes.TryGetValue(key, out float lastTime))
            {
                return true;
            }
            return Time.time >= lastTime + cooldown;
        }

        /// <summary>
        /// 스킬 사용 기록
        /// </summary>
        public void Use(string key)
        {
            _lastUseTimes[key] = Time.time;
        }

        /// <summary>
        /// 쿨타임 초기화
        /// </summary>
        public void Reset(string key)
        {
            _lastUseTimes.Remove(key);
        }

        /// <summary>
        /// 남은 쿨타임 시간
        /// </summary>
        public float GetRemainingTime(string key, float cooldown)
        {
            if (!_lastUseTimes.TryGetValue(key, out float lastTime))
            {
                return 0f;
            }
            return Mathf.Max(0f, (lastTime + cooldown) - Time.time);
        }

        /// <summary>
        /// 모든 쿨타임 초기화
        /// </summary>
        public void ResetAll()
        {
            _lastUseTimes.Clear();
        }
    }
}