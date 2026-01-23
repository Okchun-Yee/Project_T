using System.Collections;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Items;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Combat.Damage;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Skills.Contracts;
using ProjectT.Gameplay.Weapon;
using UnityEngine;

namespace ProjectT.Gameplay.Skills
{
    public abstract class BaseSkill : MonoBehaviour, ISkill
    {
        private bool isOnCooldown = false;
        public SkillSO skillInfo { get; private set; }
        private EquippableItemSO weaponData;

        [HideInInspector] public int skillIndex;

        public virtual void Skill_Initialize(SkillSO info, EquippableItemSO weapon)
        {
            if (info == null || weapon == null)
            {
                Debug.LogError($"[BaseSkill] SkillInfo or WeaponInfo is null on {name}");
                return;
            }
            this.skillInfo = info;
            this.weaponData = weapon;
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
                ChargingManager.Instance.StartCharging(ChargingType.Skill, skillInfo.chargingTime);
            }
            else if (skillInfo.skillCategory == SkillCategory.Holding)  // 홀딩 스킬
            {
                SubscribeSkillEvents();
                HoldingManager.Instance.StartHolding(HoldingType.Skill, skillInfo.chargingTime);
            }
            else // 즉시 발동 스킬
            {
                ChargingManager.Instance.SetType(ChargingType.Skill);
                HoldingManager.Instance.SetType(HoldingType.Skill);
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

        public float GetWeaponDamage()
        {
            if (weaponData == null)
            {
                Debug.LogWarning($"[BaseSkill] WeaponInfo is null on {name}");
                return 0f;
            }
            return weaponData.weaponDamage;
        }

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
}
