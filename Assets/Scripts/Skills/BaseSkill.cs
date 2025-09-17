using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSkill : MonoBehaviour, ISkill
{
    private bool isSkillActive = false;             // 스킬 활성화 상태
    private bool isOnCooldown = false;              // 스킬 쿨타임 상태
    public SkillSO skillInfo { get; private set; }  // 스킬 정보

    [HideInInspector] public int skillIndex;        // 스킬 인덱스 (무기에서 자동 설정)

    public virtual void Skill_Initialize(SkillSO info)
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
    // 스킬 데이터 반환 매서드
    public SkillSO GetSkillInfo() => skillInfo;

    // 추상 매서드 정리
    // 파생 클래스에서 구체화할 스킬 매서드
    protected abstract void OnSkillActivated();
}
