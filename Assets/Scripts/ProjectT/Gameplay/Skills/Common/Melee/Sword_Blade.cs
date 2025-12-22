using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    public class Sword_Blade : BaseSkill
    {
        private float damage;
        private void Start()
        {
            if (skillInfo != null)
            {
                damage = GetSkillDamage();
            }
            else
            {
                Debug.LogError("[Sword_Blade] SkillInfo is not set.");
            }
        }
        protected override void OnSkillActivated()
        {
            Debug.Log($"[Sword]: Blade Skill Activated, Damage {damage} ");
        }
    }
}
