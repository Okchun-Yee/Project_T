using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HoldingManager : Singleton<HoldingManager>
{
    private bool isHolding = false; // 홀딩 상태
    private float holdDuration = 0f; // 홀딩 지속 시간

    // 홀딩 이벤트
    public event Action<float> OnHoldingStarted;
    public event Action OnHoldingEnded;
    public event Action<float, float> OnHoldingProgress; // (elapsed, duration)
    public event Action OnHoldingCanceled; // 추가: 홀딩 취소 이벤트

    // 프로퍼티
    public bool IsHolding => isHolding;
    public float HoldDuration => holdDuration;

    private Coroutine holdingSkill; // 추가: 코루틴 참조
    private float holdingMaxTime; // 추가: 최대 홀딩 시간
    private float holdingTimeElapsed; // 추가: 경과 시간

    public void StartHolding(float maxDuration)
    {
        if (isHolding) return; // 이미 홀딩 중이면 무시

        isHolding = true;
        holdDuration = 0f;
        Debug.Log("[HoldingManager] 홀딩 시작");
        
        holdingMaxTime = maxDuration;
        holdingTimeElapsed = 0f;

        OnHoldingStarted?.Invoke(maxDuration);
        holdingSkill = StartCoroutine(HoldingRoutine());
    }

    public void EndHolding()
    {
        if (!isHolding) return; // 홀딩 중이 아니면 무시

        isHolding = false;
        
        OnHoldingEnded?.Invoke();
        holdDuration = 0f;

        if (holdingSkill != null)
        {
            StopCoroutine(holdingSkill);
            holdingSkill = null;
        }
    }

    private IEnumerator HoldingRoutine()
    {
        while (holdingTimeElapsed < holdingMaxTime)
        {
            holdingTimeElapsed += Time.deltaTime;
            OnHoldingProgress?.Invoke(holdingTimeElapsed, holdingMaxTime);
            yield return null; // 다음 프레임 대기
        }
        
        // 최대 시간 도달 시 자동으로 종료 (Cancel)
        holdingSkill = null;
        OnHoldingCanceled?.Invoke(); // 또는 OnHoldingEnded
    }
}
