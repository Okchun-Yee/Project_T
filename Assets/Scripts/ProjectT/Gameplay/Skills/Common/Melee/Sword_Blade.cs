using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Skills.Runtime;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    public class Sword_Blade : BaseSkill
    {
        public override void Execute(in SkillExecutionContext ctx)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnSkillActivated()
        {
            float damage = GetSkillDamage();
            Debug.Log($"[Sword]: Blade Skill Activated, Damage {damage} ");
        }
    }
}
