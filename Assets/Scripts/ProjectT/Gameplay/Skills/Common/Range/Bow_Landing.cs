using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Skills.Runtime;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Range
{
    public class Bow_Landing : BaseSkill
    {
        public override void Execute(in SkillExecutionContext ctx)
        {
            return;
        }

        protected override void OnSkillActivated()
        {
            float damage = GetSkillDamage();
            Debug.Log($"[Bow_Landing] Skill Activated, Damage {damage}");
        }
    }
}
