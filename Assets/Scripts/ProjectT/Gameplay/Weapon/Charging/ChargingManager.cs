using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectT.Core;
using ProjectT.Gameplay.Player;

/// <summary>
/// 차징 매니저 - 값 제공자 역할
/// - 값 제공: ChargeElapsed, ChargeNormalized, ChargeDuration, LastCancelReason
/// - FSM이 ChargeNormalized >= 1 로 Holding 전이 결정 (여기서 ChangeState 호출 금지)
/// </summary>
namespace ProjectT.Gameplay.Weapon
{
    public enum ChargingType
    {
        Skill,
        Attack
    }
    public class ChargingManager : Singleton<ChargingManager>
    {
        // ============================================================
        // 값 기반 상태 (FSM 결정에 직접 사용 금지, UI/실행 레이어용)
        // ============================================================
        private IEnumerator _chargingRoutine;   // 현재 충전 중인 코루틴
        private float _chargeDuration = 0f;     // 충전 목표 시간
        private float _chargeElapsed = 0f;      // 경과 시간
        private ChargingType _chargingType;     // 현재 충전 타입

        /// <summary>
        /// 마지막 취소 사유 (1회성 스냅샷)
        /// - Coordinator(PlayerController)가 EndCharging(reason) 호출 시 설정
        /// - Binder가 이벤트 발행 시 읽음
        /// - EndCharging() 호출마다 갱신되므로 이전 값은 보장되지 않음
        /// </summary>
        public ChargeCancelReason LastCancelReason { get; private set; } = ChargeCancelReason.Other;

        // 읽기 전용 프로퍼티 (값 기반 파생)
        public bool IsCharging => _chargingRoutine != null;
        public float ChargeElapsed => _chargeElapsed;
        public float ChargeDuration => _chargeDuration;
        public float ChargeNormalized => (_chargeDuration > 0f) 
            ? Mathf.Clamp01(_chargeElapsed / _chargeDuration) 
            : 0f;
        // 실행 레이어 이벤트 (Binder가 구독하여 FSM 전이 요청에 사용)
        public event Action<ChargingType> OnChargingCompleted;              // 차징 완료 이벤트
        public event Action<ChargingType> OnChargingCanceled;               // 차징 취소 이벤트
        public event Action<ChargingType, float, float> OnChargingProgress; // 차징 중 이벤트 (chargingType, elapsed, duration)

        public void StartCharging(ChargingType type, float duration)
        {
            if (_chargingRoutine != null)
                EndCharging(); // 중복 차징 방지

            _chargingType = type;
            _chargeDuration = duration;
            _chargeElapsed = 0f;
#if UNITY_EDITOR
            Debug.Log("[ChargingManager] StartCharging called");
#endif
            _chargingRoutine = ChargingRoutine();
            StartCoroutine(_chargingRoutine);
        }

        public void EndCharging()
        {
            EndCharging(ChargeCancelReason.Other);
        }

        /// <summary>
        /// 차징 종료 (사유 포함)
        /// Coordinator(PlayerController)가 정책 결정 후 호출
        /// </summary>
        public void EndCharging(ChargeCancelReason reason)
        {
            LastCancelReason = reason;
            
            if (_chargingRoutine != null)
            {
                StopCoroutine(_chargingRoutine);
                _chargingRoutine = null;
            }
            
            // 값 기반 파생: 완료 전에 취소된 경우에만 Canceled 이벤트
            bool wasComplete = (_chargeElapsed >= _chargeDuration);
            _chargeElapsed = 0f;
            
            if (!wasComplete)
                OnChargingCanceled?.Invoke(_chargingType);
        }

        private IEnumerator ChargingRoutine()
        {
            _chargeElapsed = 0f;

            while (_chargeElapsed < _chargeDuration)
            {
                _chargeElapsed += Time.deltaTime;
                OnChargingProgress?.Invoke(_chargingType, Mathf.Clamp(_chargeElapsed, 0, _chargeDuration), _chargeDuration);
                yield return null;
            }
            
            _chargingRoutine = null;
            // 값 기반: _chargeElapsed >= _chargeDuration 이면 완료
            // elapsed는 유지 (ChargeNormalized >= 1 로 파생 가능)
            
            OnChargingCompleted?.Invoke(_chargingType);
        }

        public void SetType(ChargingType type)
        {
            _chargingType = type;
        }
    }
}
