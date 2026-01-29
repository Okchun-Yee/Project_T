using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Skills.Runtime;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Range
{
    public class Bow_Homing : BaseSkill
    {
        public override void Execute(in SkillExecutionContext ctx)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnSkillActivated()
        {
            float damage = GetSkillDamage();
            Debug.Log($"[Bow_Homing] Skill Activated, Damage {damage}");
        }
    }
}
