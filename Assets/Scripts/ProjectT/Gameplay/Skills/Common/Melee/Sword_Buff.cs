using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Skills;
using UnityEngine;
namespace ProjectT.Gameplay.Skills.Common.Melee
{
    /// <summary>
    /// 버프형 스킬 (Performed Skill)
    /// </summary>
    public class Sword_Buff : BaseSkill
    {
        protected override void OnSkillActivated()
        {
            if(_buffs == null) _buffs = GetComponentInParent<PlayerBuffs>();
            if(_buffs == null)
            {
                Debug.LogError("[Sword_Buff] PlayerBuffs component not found in parent!");
                return;
            }

            _buffs.ApplyFromSkill(skillInfo);
        }
    }
}
