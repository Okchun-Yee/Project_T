using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveWeapon : Singleton<ActiveWeapon>
{
    private bool attackButtonDown = false; // 공격 버튼 상태
    private int? currentSkillIndex = 0; // 현재 스킬 인덱스 (Nullable int)

    public IWeapon currentWeapon; // 현재 활성화된 무기

    protected override void Awake()
    {
        base.Awake();
    }
    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSkillInput += OnSkillStarted;                 // 스킬 입력 이벤트 구독
            InputManager.Instance.OnSkillCanceled += OnSkillCanceled;                // 스킬 입력 취소 이벤트 구독
        }
    }
    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSkillInput -= OnSkillStarted;                 // 스킬 입력 이벤트 구독 해제
            InputManager.Instance.OnSkillCanceled -= OnSkillCanceled;                // 스킬 입력 취소 이벤트 구독 해제
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
    // 무기 상태 초기화 매서드
    public void ClearWeapon()
    {
        currentWeapon = null;
        attackButtonDown = false;
    }
    // 공격 버튼 입력 상태 관리 매서드
    private void OnAttackStarted()
    {
        if (currentWeapon == null) return;

        attackButtonDown = true;
        currentWeapon.Attack();     // 공격 메서드 호출
    }
    // 공격 버튼 입력 상태 관리 매서드
    private void OnAttackCanceled()
    {
        if (currentWeapon == null) return;

        attackButtonDown = false;
        ActionCancel();             // 차징 or 홀딩 종료
    }
    // 차징 or 홀딩 종료 매서드
    private void ActionCancel()
    {
        ChargingManager.Instance?.EndCharging();        // 차징 종료
        HoldingManager.Instance?.EndHolding();          // 홀딩 종료
    }
    /// <summary>
    /// FSM에서 공격 진입점
    /// </summary>
    public void Fsm_AttackExecute(bool charged)
    {
        if (currentWeapon == null) return;

        if (currentWeapon is BaseWeapon bw)
        {
            bw.ExecuteAttackFromFsm(charged);
        }
        else
        {
            // 인터페이스만 구현된 특수 무기면 fallback
            currentWeapon.Attack();
        }
    }
    public void Fsm_CancelAction()
    {
        // 기존 private ActionCancel 내용 그대로 호출하게 만들면 됨
        ChargingManager.Instance?.EndCharging();
        HoldingManager.Instance?.EndHolding();
    }

    // 스킬 시전 매서드
    private void OnSkillStarted(int skillIndex)
    {
        if (currentWeapon == null) return;

        // 이전 활성 스킬 구독 해제
        if (currentSkillIndex.HasValue)
        {
            UnsubscribeSkill(currentSkillIndex.Value);
        }

        // 새 스킬 구독
        SubscribeSkill(skillIndex);
        currentSkillIndex = skillIndex;

        // 스킬 사용
        currentWeapon.Skill(skillIndex);
    }

    // 스킬 시전 취소 매서드
    private void OnSkillCanceled(int skillIndex)
    {
        if (currentWeapon == null) return;
        UnsubscribeSkill(skillIndex);           // 해당 스킬 구독 해제

        if (currentSkillIndex == skillIndex)
        {
            currentSkillIndex = null;           // 현재 스킬 인덱스 초기화 (NULLABLE)
        }
        ActionCancel();                         // 차징 or 홀딩 종료
    }
    // 스킬 구독 매서드
    private void SubscribeSkill(int skillIndex)
    {
        ISkill[] skills = (currentWeapon as BaseWeapon)?.GetSkills();
        skills?[skillIndex]?.SubscribeSkillEvents();
    }
    // 스킬 구독 해제 매서드
    private void UnsubscribeSkill(int skillIndex)
    {
        ISkill[] skills = (currentWeapon as BaseWeapon)?.GetSkills();
        skills?[skillIndex]?.UnsubscribeSkillEvents();
    }
}
