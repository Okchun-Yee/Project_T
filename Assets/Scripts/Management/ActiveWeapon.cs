using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWeapon : Singleton<ActiveWeapon>
{
    private IWeapon currentWeapon; // 현재 활성화된 무기
    private bool attackButtonDown = false; // 공격 버튼 상태
    private int? currentSkillIndex = 0; // 현재 스킬 인덱스 (Nullable int)

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (attackButtonDown && currentWeapon != null)
        {
            currentWeapon.Attack();     // 공격 메서드 호출
            attackButtonDown = false;   // 공격 버튼 상태 초기화
        }

    }
    // 무기 상태 초기화 매서드
    public void NewWeapon(IWeapon weapon)
    {
        // 이전 무기의 모든 스킬 구독 해제
        if (currentWeapon is BaseWeapon oldWeapon && currentSkillIndex.HasValue)
        {
            UnsubscribeSkill(currentSkillIndex.Value);
        }
        // 새로운 무기 설정
        currentWeapon = weapon;     // 현재 무기 설정
        currentSkillIndex = null;   // 현재 스킬 인덱스 초기화 (NULLABLE)
        attackButtonDown = false;   // 공격 버튼 상태 초기화
    }

    // 스킬 시전 취소 매서드
    private void OnSkillCanceled(int skillIndex)
    {
        if (currentWeapon == null) return;

        // 해당 스킬 구독 해제
        UnsubscribeSkill(skillIndex);

        if (currentSkillIndex == skillIndex)
        {
            currentSkillIndex = null;   // 현재 스킬 인덱스 초기화 (NULLABLE)
        }

        // 차징 or 홀딩 종료
        ChargingManager.Instance?.EndCharging();
        HoldingManager.Instance?.EndHolding();
    }
    // 스킬 구독 매서드
    private void SubscribeSkill(int skillIndex)
    {
        var skills = (currentWeapon as BaseWeapon)?.GetSkills();
        skills?[skillIndex]?.SubscribeSkillEvents();
    }
    // 스킬 구독 해제 매서드
    private void UnsubscribeSkill(int skillIndex)
    {
        var skills = (currentWeapon as BaseWeapon)?.GetSkills();
        skills?[skillIndex]?.UnsubscribeSkillEvents();
    }
}
