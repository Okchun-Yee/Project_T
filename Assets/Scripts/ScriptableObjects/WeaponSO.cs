using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 무기 카테고리 열거형
public enum WeaponCategory
{
    Melee,
    Range,
    Staff
}

[CreateAssetMenu(menuName = "New Weapon")]
public class weaponSO : ScriptableObject
{
    [Header("Weapon Prefab")]
    public GameObject weaponPrefab;             // 무기 프리팹
    [Header("Common Settings")]
    public WeaponCategory weaponCategory;       // 무기 카테고리
    public skillSO[] Skills;                    // 스킬 정보 배열
    public float weaponCooldown;                // 무기 쿨타임 (공격 속도)
    public int weaponDamage;                    // 무기 데미지
    public float weaponRange;                   // 무기 사거리  (무기 지속 범위 = 근접 무기의 경우 0)
}
