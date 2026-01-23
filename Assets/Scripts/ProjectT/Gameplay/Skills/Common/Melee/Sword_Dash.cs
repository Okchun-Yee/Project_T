using System.Collections;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Combat;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    public class Sword_Dash : BaseSkill
    {
        [SerializeField] private float dashForce = 10f;
        private Rigidbody2D rb;
        private Dash dash;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody2D>();
            dash = GetComponent<Dash>();
        }

        protected override void OnSkillActivated()
        {
            float damage = GetSkillDamage();
            Debug.Log($"[Sword]: Dash Skill Activated, Damage {damage}");
        }
    }

} 