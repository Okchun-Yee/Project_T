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

        // 2) 플레이어의 AgentWeapon에서 InventorySO 꺼내기
        AgentWeapon agentWeapon = playerTf.GetComponent<AgentWeapon>();
        // 방어 코드 
        if (agentWeapon == null || agentWeapon.InventoryData == null)
        {
            Debug.LogWarning($"[WeaponLoot] AgentWeapon or InventoryData missing on player when picking up {weaponData.name}");
            return;
        }
        InventorySO inventory = agentWeapon.InventoryData;

        // 3) 인벤토리에 무기 추가
        int quantityToAdd = 1;
        // itemState가 필요하면 여기서 준비해서 넘기면 됨 (예: 기본 파라미터 복사)
        // List<ItemParameter> state = new List<ItemParameter>(weaponData.DefaultParametersList);
        List<ItemParameter> state = null;
        int remaining = inventory.AddItem(weaponData, quantityToAdd, state);
        if (remaining == 0)
        {
            // 전부 성공적으로 들어감
            Debug.Log($"[WeaponLoot] Weapon added to inventory: {weaponData.name}");
            // 4) 획득 후 처리
            canLoot = false;
            DestroyItem();
        }
        else
        {
            // 인벤토리 가득 차서 못 넣음 (또는 일부만 넣었는데 우리는 1개 기준이라 = 1이면 전부 못 넣은 것)
            Debug.LogWarning($"[WeaponLoot] Failed to add weapon to inventory (maybe full): {weaponData.name}");
            
            // 획득 실패 (인벤토리 Full 피드백 추가)
        }

    }
    public override string GetItemType()
    {
        if (weaponData != null) return weaponData.name;
        else return base.GetItemType();
    }
}
