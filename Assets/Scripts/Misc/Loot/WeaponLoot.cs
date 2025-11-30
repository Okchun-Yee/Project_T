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
        // 방어 코드: 획득 불가 또는 weaponData null 인 경우
        if (!canLoot || weaponData == null)
        {
            Debug.LogWarning($"[WeaponLoot] Cannot loot weapon. canLoot={canLoot}, weaponData={(weaponData == null ? "null" : weaponData.name)}");
            return;
        }

        // 1) LootingManager에서 플레이어 트랜스폼 가져오기
        Transform playerTf = LootingManager.Instance != null ? LootingManager.Instance.PlayerTransform : null;
        // 방어 코드: 플레이어 트랜스폼 null 방지
        if (playerTf == null)
        {
            Debug.LogWarning($"[WeaponLoot] PlayerTransform not found when picking up {weaponData.name}");
            return;
        }
        
        // 2) performAction 호출하여 무기 장착 시도
        GameObject player = playerTf.gameObject;
        bool equipped = weaponData.PerformAction(player);
        if (equipped)
        {
            Debug.Log($"[WeaponLoot] Weapon looted & equipped via PerformAction: {weaponData.name}");
            // 획득 후 아이템 제거
            canLoot = false;
            DestroyItem();
        }
        else
        {
            Debug.LogWarning($"[WeaponLoot] PerformAction failed for {weaponData.name}");
        }

    }
    public override string GetItemType()
    {
        if (weaponData != null) return weaponData.name;
        else return base.GetItemType();
    }
}
