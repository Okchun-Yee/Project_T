using System.Collections;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Data.ScriptableObjects.Weapons;
using ProjectT.Gameplay.Weapon;
using UnityEngine;
namespace ProjectT.Data.ScriptableObjects.Items
{
    [CreateAssetMenu(menuName = "New EquippableItem")]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        [Header("Weapon Prefab")]
        public GameObject weaponPrefab;             // 무기 프리팹

        [Header("Weapon Common Settings")]
        public WeaponCategory weaponCategory;       // 무기 카테고리 (Melee / Range / Magical 등)
        public SkillSO[] skillInfos;                // 이 무기가 참조하는 스킬 정보 배열
        public float weaponCooldown;                // 무기 쿨타임 (공격 속도)
        public int weaponDamage;                    // 기본 무기 데미지
        public float weaponRange;                   // 무기 사거리 (근접 무기는 0)
        
        [Header("Charge Settings (optional)")]
        public float chargeDuration = 0f;            // 0 = 차징 없음, >0 = 차징 지원 (초)
        public float chargedDamageMultiplier = 1.5f; // 차징 시 데미지 배수 (옵션)


        public string ActionName => "Equip";
        [field: SerializeField] public AudioClip actionSFX {get; private set;}

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            AgentWeapon agentWeapon = character.GetComponent<AgentWeapon>();
            if (agentWeapon != null)
            {
                agentWeapon.SetWeapon(this, itemState == null ? 
                    DefaultParametersList : itemState);
                return true;
            }
            return false;
        }
    }
}
