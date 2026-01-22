using ProjectT.Data.ScriptableObjects.Skills;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    public class Sword_Blade : BaseSkill
    {
        private float damage;

        public override void Skill_Initialize(SkillSO info)
        {
            base.Skill_Initialize(info);
            damage = GetSkillDamage();
        }

        protected override void OnSkillActivated()
        {
            Debug.Log($"[Sword]: Blade Skill Activated, Damage {damage} ");
        }
    }
}
