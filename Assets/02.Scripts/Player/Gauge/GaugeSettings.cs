using UnityEngine;
namespace _02.Scripts.Player.Gauge
{
    [CreateAssetMenu(fileName = "GaugeSettings", menuName = "Overcome/Gauge Settings")]
    public class GaugeSettings : ScriptableObject
    {
        [Header("크레센트 게이지")] 
        [Tooltip("최대 게이지량")]
        public float CrescentMaxGauge = 100f;

        [Tooltip("초당 자동 회복량")] 
        public float CrescentRegenPerSecond = 5f;
        
        [Tooltip("발사당 소모량")]
        public float CrescentCostPerShot = 25f;

        [Header("오버드라이브 게이지")]
        [Tooltip("최대 게이지량")]
        public float OverDriveMaxGauge = 100f;

        [Tooltip("공격 적중당 충전량")]
        public float OverDriveChargePerHit = 4f;

        [Tooltip("지속 시간 (초)")]
        public float OverDriveDuration = 10f;
    }
}