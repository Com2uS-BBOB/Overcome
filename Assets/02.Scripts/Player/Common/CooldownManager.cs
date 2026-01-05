using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Player.Common
{
    // 쿨타임 중앙 관리
    public class CooldownManager
    {
        private readonly Dictionary<string, float> _lastUseTimes = new();

        public bool IsReady(string key, float cooldown)
        {
            if (!_lastUseTimes.TryGetValue(key, out float lastTime)) return true;
            return Time.time >= lastTime + cooldown;
        }

        public void Use(string key) => _lastUseTimes[key] = Time.time;

        public void Reset(string key) => _lastUseTimes.Remove(key);

        public float GetRemainingTime(string key, float cooldown)
        {
            if (!_lastUseTimes.TryGetValue(key, out float lastTime)) return 0f;
            return Mathf.Max(0f, (lastTime + cooldown) - Time.time);
        }

        public void ResetAll() => _lastUseTimes.Clear();
    }
}