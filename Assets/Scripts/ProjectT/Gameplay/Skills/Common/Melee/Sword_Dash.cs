using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Combat;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    public class Sword_Dash : BaseSkill
    {
        [SerializeField] private float dashForce = 10f; // dash Power
        private Rigidbody2D rb;
        private Dash dash;
        private float damage;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody2D>();
            dash = GetComponent<Dash>();
        }

        public override void Skill_Initialize(SkillSO info)
        {
            base.Skill_Initialize(info);
            damage = GetSkillDamage();
        }

        protected override void OnSkillActivated()
        {
            Debug.Log($"[Sword]: Dash Skill Activated, Damage {damage}");
        }
    }

} 