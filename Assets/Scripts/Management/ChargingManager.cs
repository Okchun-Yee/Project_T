using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChargingManager : Singleton<ChargingManager>
{
    private IEnumerator chargingSkill; // 현재 충전 중인 스킬 코루틴
    private float chargeTime = 0f; // 충전 시간
    private float chargeTimeElapsed = 0f;


    //차징 완료 or 취소 이벤트
    public event Action OnChargingCompleted;
    public event Action OnChargingCanceled;
    public event Action<float, float> OnChargingProgress; // (elapsed, duration)

    public void StartCharging(float skillChargeTime)
    {
        if (chargingSkill != null)
            EndCharging(); // 중복 차징 방지

        chargeTime = skillChargeTime;
        chargeTimeElapsed = 0f; // 차징 시작 시 초기화

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
        OnChargingCanceled?.Invoke();
    }
    private IEnumerator ChargingRoutine()
    {
        chargeTimeElapsed = 0f;

        while (chargeTimeElapsed < chargeTime)
        {
            chargeTimeElapsed += Time.deltaTime;
            OnChargingProgress?.Invoke(Mathf.Clamp(chargeTimeElapsed, 0, chargeTime), chargeTime);
            yield return null; // Wait for the next frame
        }
        chargingSkill = null;
        chargeTimeElapsed = 0f; // 차징 완료 시 초기화

        OnChargingCompleted?.Invoke();
    }
}
