using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Skills;
using ProjectT.Gameplay.Skills.Runtime;
using ProjectT.Gameplay.VFX;
using UnityEngine;
namespace ProjectT.Gameplay.Skills.Common.Melee
{
    /// <summary>
    /// 버프형 스킬 (Performed Skill)
    /// </summary>
    public class Sword_Buff : BaseSkill
    {
        [Header("Spin VFX")]
        [SerializeField] private SpinVfxActor spinActorPrefab;
        [SerializeField] private Transform pivotPrefab;
        [SerializeField] private int slotCount = 4;
        [SerializeField] private SpinVfxConfig spinConfig;

        protected override void OnSkillActivated()
        {
            if (_buffs == null) _buffs = GetComponentInParent<PlayerBuffs>();
            if (_buffs == null)
            {
                Debug.LogError("[Sword_Buff] PlayerBuffs component not found in parent!");
                return;
            }

            _buffs.ApplyFromSkill(skillInfo);
        }

        public override void Execute(in SkillExecutionContext ctx)
        {
            if (spinActorPrefab == null || pivotPrefab == null)
            {
                Debug.LogWarning("SwordSpinSkill: SpinVFX prefab or pivot is missing.");
                return;
            }
            // 지속 시간 동기화
            spinConfig.duration = skillInfo.buffDuration;

            // 1. Actor 생성
            var actor = Instantiate(spinActorPrefab, ctx.spinHubRoot);

            // 2. 스킬 전용 Pivot + 슬롯 구성
            actor.ConfigurePivotAndSlots(pivotPrefab, slotCount);

            // 3. 재생
            actor.Play(ctx.owner, spinConfig);
        }
    }
}
