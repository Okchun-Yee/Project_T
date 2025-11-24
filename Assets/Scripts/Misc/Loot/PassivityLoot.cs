using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class PassivityLoot : BaseLoot
{
    [Header("Item Data")]
    [SerializeField] private ItemSO inventoryData;          // Item 데이터
    [SerializeField] private int quantity = 1;              // 아이템 개수
    [SerializeField] private InventorySO targetInventory;   // 아이템을 추가할 대상 인벤토리

    [Header("Loot VFX")]
    [SerializeField] private AudioSource lootSound;          // 아이템 획득 사운드
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lootingType = LootType.Item;
    }
    private void Start()
    {
        // 아이템 스프라이트 설정
        if (inventoryData != null && spriteRenderer != null && inventoryData.Itemimage != null)
        {
            spriteRenderer.sprite = inventoryData.Itemimage;
        }
    }
    public override bool CanPickup()
    {
        // 인벤토리가 존재하고, 아이템 데이터가 유효하며, 인벤토리에 아이템을 추가할 수 있는지 확인
        return canLoot 
            && inventoryData != null
            && quantity > 0
            && targetInventory != null;
    }
    public override void Looting()
    {
        if(!CanPickup()) return;

        // 1) 인벤토리에 아이템 추가
        int remainder = targetInventory.AddItem(inventoryData, quantity);
        // 2) 획득 사운드 재생
        if(lootSound != null)
        {
            lootSound.Play();
        }
        if(remainder <= 0)
        {
            canLoot = false; // 더 이상 획득 불가로 설정
            DestroyItem();  // 아이템 제거 애니메이션 및 오브젝트 제거
        }
        else
        {
            quantity = remainder; // 남은 아이템 개수 갱신
        }
    }
    // UI/디버그용 이름
    public override string GetItemType()
    {
        return inventoryData != null ? inventoryData.name : base.GetItemType();
    }
    private void OnDrawGizmos()
    {
        if(LootingManager.Instance == null) return;
        float gizmoRadius = LootingManager.Instance.GetLootingRange();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
}