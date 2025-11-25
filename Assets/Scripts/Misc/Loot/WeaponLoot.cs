using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLoot : BaseLoot
{
    [SerializeField] private WeaponSO weaponData; // 무기 데이터
    protected override void Awake()
    {
        base.Awake();
        lootingType = LootType.Weapon;
    }
    public override bool CanPickup()
    {
        return canLoot && weaponData != null;
    }
    public override void Looting()
    {
        // 무기 획득 로직 구현
        if(WeaponManager.Instance != null)
        {
            WeaponManager.Instance.EquipWeapon(weaponData);
            Debug.Log($"Weapon looted: {weaponData.name}");
        }
        else
        {
            Debug.LogWarning($"[WeaponLoot] WeaponManager missing when picking up {weaponData.name}");
        }
        canLoot = false; // 픽업 불가로 설정
        DestroyItem();  // 아이템 제거 애니메이션 및 오브젝트 제거
    }
    public override string GetItemType()
    {
        if(weaponData != null) return weaponData.name;
        else return base.GetItemType();
    }
}
