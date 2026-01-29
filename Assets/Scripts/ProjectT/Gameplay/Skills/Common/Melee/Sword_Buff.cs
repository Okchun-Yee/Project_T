using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Skills;
using ProjectT.Gameplay.VFX;
using UnityEngine;
namespace ProjectT.Gameplay.Skills.Common.Melee
{
    /// <summary>
    /// 버프형 스킬 (Performed Skill)
    /// </summary>
    public class Sword_Buff : BaseSkill
    {
        [SerializeField] private SpinVfxActor buffVfxActor;    // 버프 VFX 액터
        [SerializeField] private SpinVfxConfig _cfg;           // 버프 설정
        protected override void OnSkillActivated()
        {
            if(_buffs == null) _buffs = GetComponentInParent<PlayerBuffs>();
            if(_buffs == null)
            {
                Debug.LogError("[Sword_Buff] PlayerBuffs component not found in parent!");
                return;
            }

            _buffs.ApplyFromSkill(skillInfo);
            _cfg.duration = skillInfo.buffDuration;
            if(buffVfxActor != null)
            {
                for(int i = 0; i < 5;++i)
                {
                    buffVfxActor.AddSlot();
                }
                buffVfxActor.Play(transform, _cfg);
            }
        }
    }
}
