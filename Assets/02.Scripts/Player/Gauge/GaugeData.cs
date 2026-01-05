using System;

namespace _02.Scripts.Player.Gauge
{
    // 게이지 데이터 구조체
    [Serializable]
    public class GaugeData
    {
        public float Current;
        public float Max;

        public float Ratio => Max > 0 ? Current / Max : 0f;
        public bool IsFull => Current >= Max;
        public bool IsEmpty => Current <= 0;

        public GaugeData(float max)
        {
            Max = max;
            Current = 0f;
        }

        // 게이지 충전
        public void Add(float amount) => Current = Math.Min(Current + amount, Max);

        // 게이지 소모
        public void Consume(float amount) => Current = Math.Max(Current - amount, 0f);

        // 게이지 최대로 채우기
        public void Fill() => Current = Max;

        // 게이지 초기화
        public void Reset() => Current = 0f;
    }
}