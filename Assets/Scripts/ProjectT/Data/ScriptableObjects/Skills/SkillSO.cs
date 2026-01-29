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
    [CreateAssetMenu(menuName = "New Skill")]
    public class SkillSO : ScriptableObject
    {
        [Header("Common Settings")]
        public Sprite icon;                     // UI용 아이콘
        public SkillCategory skillCategory;     // 스킬 카테고리
        public float skillCooldown;             // 스킬 쿨타임
        public float chargingTime;              // 스킬 시전 시간
        public float skillDamage;               // 스킬 데미지
        [Header("Buff Settings")]
        public bool isBuff = false;             // 버프 여부
        public float buffDuration = 0f;         // 버프 지속 시간
        public float buffDamageMultiplier = 1f; // 버프 수치: 데미지 배율
        public float buffRangeMultiplier = 1f;  // 버프 수치: 사거리 배율
        public float buffSpeedMultiplier = 1f;  // 버프 수치: 이동속도 배율
        public float buffRampDuration = 0f;     // 버프 수치: 점진적 증가 시간. (0이면 즉시 적용)
        [Tooltip("Ramp Curve. Empty means linear.")]
        public AnimationCurve buffRampCurve;    // 버프 수치: 점진적 증가 곡선

        [Header("VFX Settings")]
        public bool hasSpinVfx = false;          // Spin VFX 사용 여부

    }
}
