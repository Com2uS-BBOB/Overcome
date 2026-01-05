using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _02.Scripts.Player.Interfaces;
namespace _02.Scripts.Player.Gauge
{
    // 크레센트 + 오버드라이브 게이지 관리 중앙화
    public class GaugeManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GaugeSettings _settings;

        [Header("Affected Skills")]
        [SerializeField] private List<MonoBehaviour> _affectedSkills;
        
        private GaugeData _crescentGauge;
        private GaugeData _overDriveGauge;
        private Coroutine _overDriveCoroutine;
        
        // Properties
        public GaugeSettings Settings => _settings;
        public float CrescentCurrent => _crescentGauge.Current;
        public float CrescentMax => _crescentGauge.Max;
        public float CrescentRatio => _crescentGauge.Ratio;
        public float OverDriveCurrent => _overDriveGauge.Current;
        public float OverDriveMax => _overDriveGauge.Max;
        public float OverDriveRatio => _overDriveGauge.Ratio;
        public bool CanUseCrescent => IsOverDriveActive || _crescentGauge.Current >= _settings.CrescentCostPerShot;
        public bool CanActivateOverDrive => _overDriveGauge.IsFull && !IsOverDriveActive;
        public bool IsOverDriveActive { get; private set; }
        
        // Events
        public event Action<float, float> OnCrescentGaugeChanged;
        public event Action<float, float> OnOverDriveGaugeChanged;
        public event Action OnOverDriveActivated;
        public event Action OnOverDriveDeactivated;
        public event Action<float> OnOverDriveDurationTick;
        
        private void Awake()
        {
            _crescentGauge = new GaugeData(_settings.CrescentMaxGauge);
            _overDriveGauge = new GaugeData(_settings.OverDriveMaxGauge);
            _crescentGauge.Fill();
            _overDriveGauge.Fill();
        }

        private void Update()
        {
            if (!IsOverDriveActive && !_crescentGauge.IsFull)
            {
                float previousValue = _crescentGauge.Current;
                _crescentGauge.Add(_settings.CrescentRegenPerSecond * Time.deltaTime);

                if (_crescentGauge.Current != previousValue)
                    OnCrescentGaugeChanged?.Invoke(_crescentGauge.Current, _crescentGauge.Max);
            }
        }
        
        public void ConsumeCrescent()
        {
            if (IsOverDriveActive) return;

            _crescentGauge.Consume(_settings.CrescentCostPerShot);
            OnCrescentGaugeChanged?.Invoke(_crescentGauge.Current, _crescentGauge.Max);
        }
        
        // 오버드라이브 게이지 충전
        public void ChargeOverDrive(float amount)
        {
            if (IsOverDriveActive) return;

            float previousValue = _overDriveGauge.Current;
            _overDriveGauge.Add(amount);

            if (_overDriveGauge.Current != previousValue)
                OnOverDriveGaugeChanged?.Invoke(_overDriveGauge.Current, _overDriveGauge.Max);
        }

        // 적 적중 시 오버드라이브 충전
        public void ChargeOverDriveOnHit() => ChargeOverDrive(_settings.OverDriveChargePerHit);

        // 오버드라이브 발동 시도
        public void TryActivateOverDrive()
        {
            if (!CanActivateOverDrive) return;

            IsOverDriveActive = true;
            _overDriveGauge.Reset();
            OnOverDriveGaugeChanged?.Invoke(_overDriveGauge.Current, _overDriveGauge.Max);

            SetAffectedSkillsOverDriveState(true);
            OnOverDriveActivated?.Invoke();

            _overDriveCoroutine = StartCoroutine(OverDriveCoroutine());
        }

        // 오버드라이브 지속 시간 관리
        private IEnumerator OverDriveCoroutine()
        {
            float remainingTime = _settings.OverDriveDuration;

            while (remainingTime > 0f)
            {
                OnOverDriveDurationTick?.Invoke(remainingTime);
                yield return null;
                remainingTime -= Time.deltaTime;
            }

            Deactivate();
        }

        // 오버드라이브 비활성화
        private void Deactivate()
        {
            IsOverDriveActive = false;
            SetAffectedSkillsOverDriveState(false);
            OnOverDriveDeactivated?.Invoke();
        }

        // 영향받는 스킬들에 오버드라이브 상태 전달
        private void SetAffectedSkillsOverDriveState(bool isActive)
        {
            if (_affectedSkills == null) return;

            foreach (var skill in _affectedSkills)
            {
                if (skill is IOverDriveAffected affected)
                    affected.IsOverDriveActive = isActive;
            }
        }

        private void OnDestroy()
        {
            if (_overDriveCoroutine != null)
                StopCoroutine(_overDriveCoroutine);
        }
    }
}