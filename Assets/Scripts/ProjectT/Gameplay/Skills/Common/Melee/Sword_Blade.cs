using ProjectT.Data.ScriptableObjects.Skills;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    public class Sword_Blade : BaseSkill
    {
        protected override void OnSkillActivated()
        {
            float damage = GetSkillDamage();
            Debug.Log($"[Sword]: Blade Skill Activated, Damage {damage} ");
        }
    }
}
