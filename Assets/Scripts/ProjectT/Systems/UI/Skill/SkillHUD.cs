using System;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Items;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Skills;
using ProjectT.Gameplay.Weapon;
using UnityEngine;

namespace ProjectT.Systems.UI.Skill
{
    public class SkillHUD : MonoBehaviour
    {
        [SerializeField] private AgentWeapon agentWeapon;
        [SerializeField] private SkillSlotUI[] skillSlots;      // 스킬 슬롯 UI 배열 size = 3
        private BaseSkill[] runtimeSkills = new BaseSkill[3];   // 현재 활성화된 스킬들 (BaseSkill 캐싱용)
        private ActiveWeapon _activeWeapon;    // ActiveWeapon 캐싱
        private void OnEnable()
        {
            agentWeapon.OnWeaponChanged += HandleWeaponChanged; // 무기 변경 이벤트 구독
            TryBindActiveWeapon(); // OnEnable 시점에 없을 수도 있으니 시도만
        }
        private void OnDisable()
        {
            agentWeapon.OnWeaponChanged -= HandleWeaponChanged; // 무기 변경 이벤트 구독 해제
            UnbindActiveWeapon();
        }
        private void TryBindActiveWeapon()
        {
            if(_activeWeapon != null) return;
            var aw = ActiveWeapon.Instance;
            if(aw != null)
            {
                _activeWeapon = aw;
                _activeWeapon.OnRuntimeWeaponChanged += HandleRuntimeWeaponChanged; // 런타임 무기 변경 이벤트 구독
                _activeWeapon.OnRuntimeWeaponCleared += HandleRuntimeWeaponCleared; // 런타임 무기 제거 이벤트 구독
            }
        }
        private void UnbindActiveWeapon()
        {
            if(_activeWeapon == null) return;
            
            _activeWeapon.OnRuntimeWeaponChanged -= HandleRuntimeWeaponChanged; // 런타임 무기 변경 이벤트 구독 해제
            _activeWeapon.OnRuntimeWeaponCleared -= HandleRuntimeWeaponCleared; // 런타임 무기 제거 이벤트 구독 해제
            _activeWeapon = null;
        }

        /// <summary>
        /// Data SSOT
        /// </summary>
        private void HandleWeaponChanged(EquippableItemSO weaponData, List<ItemParameter> list)
        {
            for (int i = 0; i < skillSlots.Length; ++i)
            {
                SkillSlotUI view = skillSlots[i];
                if (view == null) continue;

                if (weaponData.skillInfos == null || i >= weaponData.skillInfos.Length)
                {
                    // 스킬 정보가 없으면 슬롯 비활성화
                    view.SetEnabled(false);
                    runtimeSkills[i] = null;
                    continue;
                }
                SkillSO skillInfo = weaponData.skillInfos[i];
                view.SetEnabled(skillInfo != null);
                view.SetIcon(skillInfo != null ? skillInfo.icon : null);
            }
        }
        /// <summary>
        /// Runtime SSOT
        /// </summary>
        private void HandleRuntimeWeaponChanged(BaseWeapon weapon)
        {
            HandleRuntimeWeaponCleared();
            var skills = weapon.GetSkills();
            for (int i =0; i <runtimeSkills.Length; ++i)
            {
                runtimeSkills[i] = skills[i] as BaseSkill;
            }
        }
        private void HandleRuntimeWeaponCleared()
        {
            for (int i = 0; i < runtimeSkills.Length; ++i)
            {
                runtimeSkills[i] = null;
            }
        }
        private void Update()
        {
            TryBindActiveWeapon(); // 1회 바인딩 시도

            // 각 스킬 슬롯의 쿨다운 오버레이 업데이트
            for (int i = 0; i < skillSlots.Length; ++i)
            {
                SkillSlotUI slotUI = skillSlots[i];
                BaseSkill skill = runtimeSkills[i];
                if(skill == null)
                {
                    slotUI.SetCooldownOverlay(0f);
                    continue;
                }

                if (!skill.IsOnCooldown || skill.CooldownDuration <= 0f)
                {
                    slotUI.SetCooldownOverlay(0f);
                    continue;
                }
                float t = skill.CooldownRemaining / skill.CooldownDuration;
                slotUI.SetCooldownOverlay(t);
            }
        }
    }
}
