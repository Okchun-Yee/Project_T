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
        [SerializeField] private float dashDuration = 0.2f;

        private PlayerController _controller;

        private void Awake()
        {
            _controller = GetComponentInParent<PlayerController>();
            
            if (_controller == null)
            {
                Debug.LogError("[Sword_Dash] PlayerController not found in parent!");
            }
        }

        protected override void OnSkillActivated()
        {
            if (_controller == null) return;
            
            // 마우스 방향 계산 (Execution 헬퍼 사용)
            Vector2 mouseDirection = PlayerMovementExecution.Instance.GetMouseDirection();
            float damage = GetSkillDamage();
            Debug.Log($"[Sword_Dash]: Dash Skill Activated, Damage {damage}");
            
            // DashContext 생성 및 요청
            _controller.RequestDash(DashContext.CreateForSkill(
                direction: mouseDirection,
                force: dashForce,
                duration: dashDuration, 
                damage: damage
            ));
        }
    }
}