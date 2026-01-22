using ProjectT.Data.ScriptableObjects.Skills;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Range
{
    public class Bow_Landing : BaseSkill
    {
        private float damage;

        public override void Skill_Initialize(SkillSO info)
        {
            base.Skill_Initialize(info);
            damage = GetSkillDamage();
        }

        protected override void OnSkillActivated()
        {
            // Implement the skill activation logic here
            Debug.Log("[Bow_Landing] Skill Activated");
        }
    }
}
