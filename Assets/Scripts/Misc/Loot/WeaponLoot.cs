using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class WeaponLoot : BaseLoot
{
    [SerializeField] private EquippableItemSO weaponData; // 무기 데이터
    protected override void Awake()
    {
        base.Awake();
        lootingType = LootType.Weapon;
    }
    /// <summary>
    /// * 아이템 획득 가능 여부 검사
    /// 무기 데이터가 유효하고, canLoot 플래그가 true일 때만 획득 가능
    /// </summary>
    public override bool CanPickup()
    {
        return canLoot && weaponData != null;
    }

    /// <summary>
    /// * 아이템 획득 처리
    /// </summary>
    public override void Looting()
    {
        // 무기 획득 로직
        if(WeaponManager.Instance != null)
        {
            // 무기 장착 매서드 호출
            WeaponManager.Instance.EquipWeapon(weaponData);
            Debug.Log($"Weapon looted: {weaponData.name}");
        }
        else
        {
            Debug.LogWarning($"[WeaponLoot] WeaponManager missing when picking up {weaponData.name}");
        }
        canLoot = false;    // 픽업 불가로 설정
        DestroyItem();      // 아이템 제거 애니메이션 및 오브젝트 제거
    }
    public override string GetItemType()
    {
        if(weaponData != null) return weaponData.name;
        else return base.GetItemType();
    }
}
