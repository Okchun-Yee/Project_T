using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 차징 매니저 클래스
/// * 스킬, 기본 공격 등 모든 차징 관리
/// * 싱글톤 패턴 적용, 인터페이스로 차징 매서드 구현
/// * 차징 시작, 진행, 완료, 취소 이벤트 제공
/// * Enum 으로 스킬 or 기본 공격 구분
/// </summary>

public enum ChargingType
{
    Skill,
    Attack
}
public class ChargingManager : Singleton<ChargingManager>
{
    private IEnumerator chargingSkill; // 현재 충전 중인 스킬 코루틴
    private float chargeTime = 0f; // 충전 시간
    private float chargeTimeElapsed = 0f;
    private bool isChargingComplete = false;


    //차징 완료 or 취소 이벤트
    public event Action<ChargingType> OnChargingCompleted;              // 차징 완료 이벤트
    public event Action<ChargingType> OnChargingCanceled;               // 차징 취소 이벤트
    public event Action<ChargingType, float, float> OnChargingProgress; // 차징 중 이벤트 (chargingType, elapsed, duration)

    private ChargingType chargingType; // 현재 충전 중인 타입 (스킬 or 기본 공격)
    public void StartCharging(ChargingType type, float skillChargeTime)
    {
        if (chargingSkill != null)
            EndCharging(); // 중복 차징 방지

        chargingType = type;            // 현재 차징 타입 설정
        chargeTime = skillChargeTime;
        chargeTimeElapsed = 0f;         // 차징 시작 시 초기화
        isChargingComplete = false;     // 차징 시작 시 초기화
        Debug.Log("[Exec] StartCharging called");


        // Start charging logic
        chargingSkill = ChargingRoutine();
        StartCoroutine(chargingSkill);
    }

    public void EndCharging()
    {
        // Stop charging logic
        if (chargingSkill != null)
        {
            StopCoroutine(chargingSkill);
            chargingSkill = null;
        }
        chargeTimeElapsed = 0f; // 차징 취소 시 초기화
        if (!isChargingComplete)
            OnChargingCanceled?.Invoke(chargingType);
    }
    private IEnumerator ChargingRoutine()
    {
        chargeTimeElapsed = 0f;

        while (chargeTimeElapsed < chargeTime)
        {
            chargeTimeElapsed += Time.deltaTime;
            OnChargingProgress?.Invoke(chargingType, Mathf.Clamp(chargeTimeElapsed, 0, chargeTime), chargeTime);
            yield return null; // Wait for the next frame
        }
        chargingSkill = null;
        chargeTimeElapsed = 0f;     // 차징 완료 시 초기화
        isChargingComplete = true;  // 차징 완료 상태 업데이트

        OnChargingCompleted?.Invoke(chargingType);
    }

    public void SetType(ChargingType type)
    {
        chargingType = type;
    }
}
