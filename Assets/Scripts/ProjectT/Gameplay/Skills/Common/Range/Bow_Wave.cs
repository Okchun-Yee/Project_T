using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Combat.Damage;
using ProjectT.Gameplay.Skills.Runtime;
using ProjectT.Gameplay.VFX;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Range
{
    public class Bow_Wave : BaseSkill
    {
        [Header("Spin VFX")]
        [SerializeField] private SpinVfxActor spinActorPrefab;
        [SerializeField] private Transform pivotPrefab;
        [SerializeField] private int slotCount = 4;
        [SerializeField] private SpinVfxConfig spinConfig;
        private float damage;

        public override void Execute(in SkillExecutionContext ctx)
        {
            var actor = Instantiate(spinActorPrefab, ctx.spinHubRoot);
            actor.ConfigurePivotAndSlots(pivotPrefab, slotCount);

            foreach (var ds in actor.GetComponentsInChildren<DamageSource>(true))
                ds.SetDamage(damage);

            actor.Play(ctx.owner, spinConfig);
        }

        protected override void OnSkillActivated()
        {
            damage = GetSkillDamage();
            Debug.Log($"[Bow_Wave] Skill Activated, Damage {damage}");
        }
    }
}
