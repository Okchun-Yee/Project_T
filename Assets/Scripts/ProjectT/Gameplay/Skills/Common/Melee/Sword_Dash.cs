using System.Collections;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Combat;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Player.Controller;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    public class Sword_Dash : BaseSkill
    {
        [SerializeField] private float dashForce = 10f;

        private PlayerMovementExecution pc;
        private Rigidbody2D rb;
        private Dash dash;

        private void Awake()
        {
            pc = GetComponentInParent<PlayerMovementExecution>();

            if(pc != null)
            {
                rb = pc.GetComponent<Rigidbody2D>();
                dash = pc.GetComponent<Dash>();
            }
        }

        protected override void OnSkillActivated()
        {
            if(pc == null) return;
            pc._Dash(dashForce, 0.2f);

            float damage = GetSkillDamage();
            Debug.Log($"[Sword_Dash]: Dash Skill Activated, Damage {damage}");
        }
    }
} 