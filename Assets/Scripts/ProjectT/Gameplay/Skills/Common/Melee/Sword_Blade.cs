using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Combat.Damage;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Skills.Runtime;
using ProjectT.Gameplay.VFX;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    public class Sword_Blade : BaseSkill
    {
        [Header("Spin VFX")]
        [SerializeField] private SpinVfxActor spinActorPrefab;
        [SerializeField] private Transform pivotPrefab;
        [SerializeField] private int slotCount = 4;
        [SerializeField] private SpinVfxConfig spinConfig;
        private float damage;
        public override void Execute(in SkillExecutionContext ctx)
        {
            if (spinActorPrefab == null || pivotPrefab == null)
            {
                Debug.LogWarning("SwordSpinSkill: SpinVFX prefab or pivot is missing.");
                return;
            }

            // 1. Actor 생성
            var actor = Instantiate(spinActorPrefab, ctx.spinHubRoot);
            // 2. 스킬 전용 Pivot + 슬롯 구성
            actor.ConfigurePivotAndSlots(pivotPrefab, slotCount);

            foreach (var ds in actor.GetComponentsInChildren<DamageSource>(true))
                ds.SetDamage(damage);

            // 3. 재생
            actor.Play(ctx.owner, spinConfig);
        }

        protected override void OnSkillActivated()
        {
            damage = GetSkillDamage();
            PlayerController.Instance.LockActions(ActionLockFlags.Dash, spinConfig.duration);
        }
    }
}
