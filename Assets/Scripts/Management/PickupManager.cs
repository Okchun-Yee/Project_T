using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 플레이어 주변의 픽업 아이템을 관리하고, 입력에 따라 아이템을 픽업하는 매니저 클래스
/// </summary>

public class PickupManager : Singleton<PickupManager>
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask pickupLayer = -1;

    // 픽업 이벤트
    public event Action OnWeaponPickedUp;       // WeaponData 호출 이벤트 
    public event Action OnItemPickedUp;         // ItemData 호출 이벤트

    // 현재 범위 내 픽업 가능한 아이템들
    private List<IPickupable> nearbyPickups = new List<IPickupable>();
    private IPickupable closestPickup;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPickupInput += HandlePickupInput;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPickupInput -= HandlePickupInput;
        }
    }

    private void Update()
    {
        if (PlayerController.Instance != null)
        {
            UpdateNearbyPickups();
            UpdateClosestPickup();
        }
    }

    // 픽업 입력 처리
    private void HandlePickupInput()
    {
        if (closestPickup != null && closestPickup.CanPickup())
        {
            PickupItem(closestPickup);
        }
    }

    // 아이템 픽업 실행
    private void PickupItem(IPickupable item)
    {
        // 타입별 처리
        if (item is WeaponPickup weaponPickup)
        {
            OnWeaponPickedUp?.Invoke();
        }
        else if (item is ItemPickup itemPickup)
        {
            // 향후 인벤토리 시스템 연동
            OnItemPickedUp?.Invoke(); // 임시
        }

        // 픽업 실행
        item.Pickup();

        // 리스트에서 제거
        nearbyPickups.Remove(item);
    }

    // 플레이어 주변 픽업 아이템 스캔
    private void UpdateNearbyPickups()
    {
        nearbyPickups.Clear();

        Vector3 playerPos = PlayerController.Instance.transform.position;
        Collider2D[] pickupColliders = Physics2D.OverlapCircleAll(playerPos, pickupRange, pickupLayer);

        foreach (Collider2D collider in pickupColliders)
        {
            IPickupable pickupable = collider.GetComponent<IPickupable>();
            if (pickupable != null && pickupable.CanPickup())
            {
                nearbyPickups.Add(pickupable);
            }
        }
    }

    // 가장 가까운 픽업 아이템 선택
    private void UpdateClosestPickup()
    {
        if (nearbyPickups.Count == 0)
        {
            closestPickup = null;
            return;
        }

        Vector3 playerPos = PlayerController.Instance.transform.position;
        float closestDistance = float.MaxValue;
        IPickupable closest = null;

        foreach (IPickupable pickup in nearbyPickups)
        {
            float distance = Vector3.Distance(playerPos, pickup.GetTransform().position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = pickup;
            }
        }

        closestPickup = closest;
    }

    // UI용 정보 제공
    public IPickupable GetClosestPickup() => closestPickup;
    public bool HasNearbyPickups() => nearbyPickups.Count > 0;
    public int GetNearbyPickupCount() => nearbyPickups.Count;

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        if (PlayerController.Instance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(PlayerController.Instance.transform.position, pickupRange);
        }
    }
}
