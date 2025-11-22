using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 플레이어 주변의 픽업 아이템을 관리하고, 입력에 따라 아이템을 픽업하는 매니저 클래스
/// </summary>

public class LootingManager : Singleton<LootingManager>
{
    [Header("Looting Settings")]
    [SerializeField] private float LootingRange = 2f;
    [SerializeField] private LayerMask lootingLayer = -1;

    // 픽업 이벤트
    public event Action OnWeaponLoot;       // WeaponData 호출 이벤트 
    public event Action OnItemLoot;         // ItemData 호출 이벤트

    // 현재 범위 내 픽업 가능한 아이템들
    private List<ILooting> nearbyLoots = new List<ILooting>();
    private ILooting closestLoot;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLootingInput += HandleLootInput;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnLootingInput -= HandleLootInput;
        }
    }

    private void Update()
    {
        if (PlayerController.Instance != null)
        {
            UpdateNearbyLoots();
            UpdateClosestLoot();
        }
    }

    // 픽업 입력 처리
    private void HandleLootInput()
    {
        if (closestLoot != null && closestLoot.CanPickup())
        {
            LootItem(closestLoot);
        }
    }

    // 아이템 픽업 실행
    private void LootItem(ILooting item)
    {
        // 타입별 처리
        if (item is WeaponLoot weaponPickup)
        {
            OnWeaponLoot?.Invoke();
        }
        else if (item is AutoLoot itemPickup)
        {
            // 향후 인벤토리 시스템 연동
            OnItemLoot?.Invoke(); // 임시
        }

        // 픽업 실행
        item.Pickup();

        // 리스트에서 제거
        nearbyLoots.Remove(item);
    }

    // 플레이어 주변 픽업 아이템 스캔
    private void UpdateNearbyLoots()
    {
        nearbyLoots.Clear();

        Vector3 playerPos = PlayerController.Instance.transform.position;
        Collider2D[] lootingColliders = Physics2D.OverlapCircleAll(playerPos, LootingRange, lootingLayer);

        foreach (Collider2D collider in lootingColliders)
        {
            ILooting pickupable = collider.GetComponent<ILooting>();
            if (pickupable != null && pickupable.CanPickup())
            {
                nearbyLoots.Add(pickupable);
            }
        }
    }

    // 가장 가까운 픽업 아이템 선택
    private void UpdateClosestLoot()
    {
        if (nearbyLoots.Count == 0)
        {
            closestLoot = null;
            return;
        }

        Vector3 playerPos = PlayerController.Instance.transform.position;
        float closestDistance = float.MaxValue;
        ILooting closest = null;

        foreach (ILooting loot in nearbyLoots)
        {
            float distance = Vector3.Distance(playerPos, loot.GetTransform().position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = loot;
            }
        }

        closestLoot = closest;
    }

    // UI용 정보 제공
    public ILooting GetClosestLoot() => closestLoot;
    public bool HasNearbyLoots() => nearbyLoots.Count > 0;
    public int GetNearbyLootCount() => nearbyLoots.Count;

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        if (PlayerController.Instance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(PlayerController.Instance.transform.position, LootingRange);
        }
    }
}
