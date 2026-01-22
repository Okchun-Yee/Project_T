using ProjectT.Data.ScriptableObjects.Skills;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Range
{
    public class Bow_Homing : BaseSkill
    {
        protected override void OnSkillActivated()
        {
            float damage = GetSkillDamage();
            Debug.Log($"[Bow_Homing] Skill Activated, Damage {damage}");
        }
    }
}
