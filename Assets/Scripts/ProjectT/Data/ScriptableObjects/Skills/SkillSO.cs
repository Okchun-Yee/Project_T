using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.Skills
{
    public enum SkillCategory
    {
        Charging,
        Performed,
        Holding
    }

    /// <summary>
    /// 스킬 타입 (타입 기반 매칭용)
    /// </summary>
    public enum SkillType
    {
        None = 0,   // 미설정 (검증용)
        Dash,
        Blade,
        Homing,
        Landing
    }

    [CreateAssetMenu(menuName = "New Skill")]
    public class SkillSO : ScriptableObject
    {
        [Header("Common Settings")]
        public Sprite icon;                     // UI용 아이콘
        public SkillCategory skillCategory;     // 스킬 카테고리
        public SkillType skillType;             // 스킬 타입 (매칭용)
        public float skillCooldown;             // 스킬 쿨타임
        public float chargingTime;               // 스킬 시전 시간
        public float skillDamage;               // 스킬 데미지    
    }
}
