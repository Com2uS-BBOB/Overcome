using System;
namespace _02.Scripts.Player.Gauge
{
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
            this.Max = max;
            Current = 0f;
        }
        
        public void Add(float amount) => Current = Math.Min(Current + amount, Max);
        public void Consume(float amount) => Current = Math.Max(Current - amount, 0f);
    }
}