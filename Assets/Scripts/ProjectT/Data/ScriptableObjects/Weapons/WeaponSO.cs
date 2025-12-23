using System.Collections;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Skills;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.Weapons
{
    // 무기 카테고리 열거형
    public enum WeaponCategory
    {
        Melee,
        Range,
        Magical
    }

    [CreateAssetMenu(menuName = "New Weapon")]
    public class WeaponSO : ScriptableObject
    {
        [Header("Weapon Prefab")]
        public GameObject weaponPrefab;             // 무기 프리팹
        [Header("Common Settings")]
        public WeaponCategory weaponCategory;       // 무기 카테고리
        public SkillSO[] skillInfos;                // 스킬 정보 배열
        public float weaponCooldown;                // 무기 쿨타임 (공격 속도)
        public int weaponDamage;                    // 무기 데미지
        public float weaponRange;                   // 무기 사거리  (무기 지속 범위 = 근접 무기의 경우 0)
        [Header("Charge Settings (optional)")]
        public float chargeDuration = 0f;            // 0 = 차징 없음, >0 = 차징 지원 (초)
        public float chargedDamageMultiplier = 1.5f; // 차징 시 데미지 배수 (옵션)
    }
}
