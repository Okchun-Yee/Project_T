using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ProjectT.Core;

/// <summary>
/// 홀딩 매니저 클래스
/// * 기본 공격 & 스킬 홀딩 관리
/// * 싱글톤 패턴 적용, 인터페이스로 홀딩 매서드 구현
/// * 홀딩 시작, 진행, 완료, 취소 이벤트 제공
/// * 기본 공격 or 스킬 구분
/// </summary>
namespace ProjectT.Gameplay.Weapon
{
    public enum HoldingType
    {
        Skill,
        Attack
    }
    public class HoldingManager : Singleton<HoldingManager>
    {
        private bool isHolding = false; // 홀딩 상태
        private float holdDuration = 0f; // 홀딩 지속 시간

        // 홀딩 이벤트
        public event Action<HoldingType, float> OnHoldingStarted;
        public event Action<HoldingType> OnHoldingEnded;
        public event Action<HoldingType, float, float> OnHoldingProgress; // (elapsed, duration)
        public event Action<HoldingType> OnHoldingCanceled; // 추가: 홀딩 취소 이벤트

        // 프로퍼티
        public bool IsHolding => isHolding;
        public float HoldDuration => holdDuration;

        private Coroutine holdingSkill; // 추가: 코루틴 참조
        private float holdingMaxTime; // 추가: 최대 홀딩 시간
        private float holdingTimeElapsed; // 추가: 경과 시간
        private HoldingType holdingType; // 추가: 현재 홀딩 타입

        public void StartHolding(HoldingType type, float maxDuration)
        {
            if (isHolding) return; // 이미 홀딩 중이면 무시

            holdingType = type;
            isHolding = true;
            holdDuration = 0f;

            holdingMaxTime = maxDuration;
            holdingTimeElapsed = 0f;

            OnHoldingStarted?.Invoke(holdingType, maxDuration);
            holdingSkill = StartCoroutine(HoldingRoutine());
        }

        public void EndHolding()
        {
            if (!isHolding) return; // 홀딩 중이 아니면 무시

            isHolding = false;

            OnHoldingEnded?.Invoke(holdingType);
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
                OnHoldingProgress?.Invoke(holdingType, holdingTimeElapsed, holdingMaxTime);
                yield return null; // 다음 프레임 대기
            }

            // 최대 시간 도달 시 자동으로 종료 (Cancel)
            holdingSkill = null;
            OnHoldingCanceled?.Invoke(holdingType); // 또는 OnHoldingEnded
        }
        // 외부 클래스에서 즉시 타입 설정 매서드
        public void SetType(HoldingType type)
        {
            holdingType = type;
        }
    }
}
