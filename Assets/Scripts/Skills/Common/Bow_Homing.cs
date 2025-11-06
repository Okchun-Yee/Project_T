using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow_Homing : BaseSkill
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
            Debug.LogError("[Bow_Homing] SkillInfo is not set.");
        }
    }
    protected override void OnSkillActivated()
    {
        // Implement the skill activation logic here
        Debug.Log($"[Bow_Homing] Skill Activated");
    }
}
