using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSkill : MonoBehaviour, ISkill
{
    private bool isSkillActive = false;             // 스킬 활성화 상태
    private bool isOnCooldown = false;              // 스킬 쿨타임 상태
    public SkillSO skillInfo { get; private set; }  // 스킬 정보

    [HideInInspector] public int skillIndex;        // 스킬 인덱스 (무기에서 자동 설정)

    public virtual void Skill_Initialized(SkillSO info)
    {
        if (info == null)
        {
            Debug.LogError($"[BaseSkill] SkillInfo is null on {name}");
            return;
        }
        this.skillInfo = info; // 스킬 정보 주입
    }
    public virtual void ActivateSkill(int index = -1)
    {
        if (isOnCooldown) return; // 쿨타임 중이면 무시

        // 1) 인덱스가 전달되면 skillIndex 업데이트
        if (index >= 0)
        {
            skillIndex = index;
        }

        // 2) 스킬 카테고리에 따른 처리 
        if (skillInfo.skillCategory == SkillCategory.Charging)      // 차징 스킬
        {
            SubscribeSkillEvents();
            ChargingManager.Instance.StartCharging(skillInfo.chargingTime);
        }
        else if (skillInfo.skillCategory == SkillCategory.Holding)  // 홀딩 스킬
        {
            SubscribeSkillEvents();
            HoldingManager.Instance.StartHolding(skillInfo.chargingTime);
        }
        else // 즉시 발동 스킬
        {
            OnSkill();
        }
    }
    public void OnSkill()
    {
        if (!isOnCooldown)
        {
            StartCoroutine(ActivateRoutine());
        }
    }

    private IEnumerator ActivateRoutine()
    {
        isOnCooldown = true;
        OnSkillActivated();

        yield return new WaitForSeconds(skillInfo.skillCooldown);
        isOnCooldown = false;
    }
    // 스킬 발동 시 이벤트 매서드 (파생 클래스에서 구현)
    protected abstract void OnSkillActivated();
    //<차징용>
    // 차징 완료 시 호출
    protected virtual void OnChargingCompleted() { }
    // 차징 취소 시 호출
    protected virtual void OnChargingCanceled() { }
    // 차징 중
    protected virtual void OnChargingProgress(float elapsed, float duration) { }

    // <홀딩용>
    // 홀딩 시작 시 호출
    protected virtual void OnHoldingStarted(float maxDuration) { }
    // 홀딩 종료 시 호출
    protected virtual void OnHoldingEnded() { }
    // 홀딩 중
    protected virtual void OnHoldingProgress(float elapsed, float duration) { }
    // 홀딩 시간이 최대치에 도달했을 때 호출
    protected virtual void OnHoldingCanceled() { }

    // 스킬 이벤트 구독
    public virtual void SubscribeSkillEvents()
    {
        // 각 스킬 타입별로 오버라이드에서 구현
    }

    // 스킬 이벤트 해제
    public virtual void UnsubscribeSkillEvents()
    {
        // 각 스킬 타입별로 오버라이드에서 구현
    }

    // 무기 기본 공격력 가져오기
    public float GetWeaponDamage()
    {
        // ActiveWeapon에서 현재 무기의 WeaponInfo 접근
        // weaponDamage 반환
        float weaponDamage = 0f;
        weaponDamage = ActiveWeapon.Instance.currentWeapon.GetWeaponInfo().weaponDamage;

        return weaponDamage;
    }

    // 스킬 데미지 계산 (무기 공격력 × 스킬 계수)
    public float GetSkillDamage()
    {
        float weaponDamage = GetWeaponDamage();
        float skillMultiplier = skillInfo.skillDamage / 100f;

        return weaponDamage * skillMultiplier;
    }

    // VFX & Projectile에 데미지 설정
    public void SetupDamageSource(GameObject target, float damage)
    {
        DamageSource damageSource = target.GetComponent<DamageSource>();
        damageSource?.SetDamage(damage);
    }
    public SkillSO GetSkillInfo() => skillInfo;
}
