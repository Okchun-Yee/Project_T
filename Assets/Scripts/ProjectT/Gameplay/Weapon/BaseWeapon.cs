using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectT.Data.ScriptableObjects.Inventory;
using System;
using ProjectT.Gameplay.Weapon.Contracts;
using ProjectT.Data.ScriptableObjects.Items;
using ProjectT.Gameplay.Skills.Contracts;
using ProjectT.Gameplay.Combat.Damage;
using ProjectT.Gameplay.Skills;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Core.Debug;

namespace ProjectT.Gameplay.Weapon
{
    public abstract class BaseWeapon : MonoBehaviour, IWeapon
    {
        // SSOT: "공격 중" 여부는 FSM 상태(CombatState == Attack)로 판단
        // isAttacking 플래그 제거됨
        
        private const int SkillSlotCount = 3;
        public EquippableItemSO weaponInfo { get; private set; }

        private Coroutine CooldownCoroutine;
        private ISkill[] skillsBySlot;

        private float weaponCooldown;
        private bool isCooldown = false;
        private DamageSource ds;

        /// <summary>
        /// 초기화 진입점 매서드 (weaponSO & skillSO 주입)
        /// 파생 클래스의 무기 SO, 스킬 SO 초기화 매서드 호출
        /// </summary>
        public virtual void Weapon_Initialize(EquippableItemSO info)
        {
            if (info == null)
            {
                Debug.LogError($"[BaseWeapon] WeaponInfo is null on {name}");
                return;
            }
            WeaponInitialization(info); // 무기 초기화
            SkillInitialization(info);  // 스킬 초기화
        }
        // 무기 초기화 매서드
        private void WeaponInitialization(EquippableItemSO info)
        {
            // 1) 무기 정보 주입
            weaponInfo = info;
            weaponCooldown = info.weaponCooldown; 
            // 장착(초기화) 시점에 이전 코루틴이 남아있을 수 있으므로 쿨다운 초기화
            if (CooldownCoroutine != null)
            {
                try { StopCoroutine(CooldownCoroutine); } 
                catch (System.Exception ex) 
                { 
                #if UNITY_EDITOR
                    Debug.LogWarning($"[BaseWeapon] Failed to stop cooldown coroutine: {ex.Message}");
                #endif
                }
                CooldownCoroutine = null;
            }
            isCooldown = false;
        }
        private void SkillInitialization(EquippableItemSO info)
        {
            skillsBySlot = new ISkill[SkillSlotCount];
            
            ISkill[] skillComponents = GetComponents<ISkill>();
            
            if (skillComponents.Length < SkillSlotCount) {
                Debug.LogWarning($"[BaseWeapon] Expected {SkillSlotCount} skill components, found {skillComponents.Length} on {name}. Missing slots will be inactive.");
            }

            if (info.skillInfos == null || info.skillInfos.Length < SkillSlotCount) {
                int infoLength = info.skillInfos == null ? 0 : info.skillInfos.Length;
                Debug.LogWarning($"[BaseWeapon] Expected {SkillSlotCount} skillInfos in SO, found {infoLength} on {name}. Missing slots will be inactive.");
            }

            for (int slot = 0; slot < SkillSlotCount; slot++) {
                ISkill skillComponent = slot < skillComponents.Length ? skillComponents[slot] : null;
                SkillSO skillSO = (info.skillInfos != null && slot < info.skillInfos.Length) ? info.skillInfos[slot] : null;
                
                if (skillComponent == null) {
                    Debug.LogWarning($"[BaseWeapon] Skill component at slot {slot} is missing on {name}");
                    skillsBySlot[slot] = null;
                    continue;
                }
                
                if (skillSO == null) {
                    Debug.LogWarning($"[BaseWeapon] SkillSO at slot {slot} is null on {name}. Slot will be inactive.");
                    skillsBySlot[slot] = null;
                    continue;
                }
                
                skillComponent.Skill_Initialize(skillSO, info);
                
                skillsBySlot[slot] = skillComponent;
                
                if (skillComponent is BaseSkill baseSkill) {
                    baseSkill.skillIndex = slot;
                }
            }
            
            if (skillComponents.Length > SkillSlotCount) {
                Debug.LogWarning($"[BaseWeapon] Found {skillComponents.Length} skill components on {name}. " +
                    $"Using first {SkillSlotCount}. Check if extra components are attached unintentionally.");
            }
        }

        // 공격 진입점 매서드
        [Obsolete("Do not call Attack() from FSM path. Use ExecuteAttackFromFsm(charged) via binder.", false)]
        public void Attack()
        {
            throw new NotImplementedException("BaseWeapon.Attack() is obsolete. Use ExecuteAttackFromFsm(bool charged) instead.");
        }
        // 새 헬퍼: 무기가 (차징 취소 시) 직접 즉시 공격을 트리거할 때 사용.
        // ForceAttack은 공격 쿨다운 검사를 포함하고 CooldownRoutine을 시작합니다.
        protected void _Attack()
        {
            if (isCooldown) { return; }
            OnAttack();
            CooldownCoroutine = StartCoroutine(CooldownRoutine());
        }
        protected void _AttackCharged()
        {
            if (isCooldown) { return; }
            OnAttack_Charged();
            CooldownCoroutine = StartCoroutine(CooldownRoutine());
        }
        /// <summary>
        /// FSM에서 공격 진입점
        /// </summary>
        public void ExecuteAttackFromFsm(bool charged)
        {
            if (isCooldown)
            {
                DevLog.Log(DevLogChannels.Weapon, "Attack skipped (cooldown)");
                return;
            }
            DevLog.Log(DevLogChannels.Weapon, $"Attack execute start (charged:{charged})");
            if (charged) _AttackCharged();
            else _Attack();
            DevLog.Log(DevLogChannels.Weapon, $"Attack execute end (charged:{charged})");
        }

        // 무기 쿨다운 코루틴
        private IEnumerator CooldownRoutine()
        {
            isCooldown = true;
            yield return new WaitForSeconds(weaponCooldown);
            isCooldown = false;
        }
        // 객체 비활성 시 처리 매서드 (코루틴 정리)
        protected virtual void OnDisable()
        {
            // 쿨다운 코루틴 정리만 남김
            if (CooldownCoroutine != null)
                StopCoroutine(CooldownCoroutine);
        }
        public void Skill(int skillIndex)
        {
            if (skillsBySlot == null || skillIndex < 0 || skillIndex >= skillsBySlot.Length) {
                Debug.LogError($"[BaseWeapon] Invalid skill slot {skillIndex}, must be 0~{SkillSlotCount - 1} on {name}");
                return;
            }
            
            ISkill skill = skillsBySlot[skillIndex];
            if (skill == null) {
                Debug.LogWarning($"[BaseWeapon] Skill slot {skillIndex} is empty on {name}");
                return;
            }
            
            skill.ActivateSkill();
        }
        public ISkill[] GetSkills() => skillsBySlot;
        public EquippableItemSO GetWeaponInfo() => weaponInfo;

        // 추상 매서드
        // 파생 무기 클래스에서 구현할 공격 매서드
        protected abstract void OnAttack();
        // 추상 매서드 (차징 공격용)
        protected abstract void OnAttack_Charged();
    }
}
