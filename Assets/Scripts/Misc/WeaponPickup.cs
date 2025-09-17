using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour, IPickupable
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponSO weaponData;
    private string displayName;
    private bool canPickup = true;
    
    private void OnEnable()
    {
        // PickupManager의 무기 픽업 이벤트 구독
        if (PickupManager.Instance != null)
        {
            PickupManager.Instance.OnWeaponPickedUp += Pickup;
        }
    }
    
    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (PickupManager.Instance != null)
        {
            PickupManager.Instance.OnWeaponPickedUp -= Pickup;
        }
    }
    
    private void Start()
    {
        // WeaponSO에서 displayName 설정
        if (weaponData != null && string.IsNullOrEmpty(displayName))
        {
            displayName = weaponData.name;
        }
    }
    
    // 이벤트 구독 핸들러 - 이 WeaponPickup이 픽업되었을 때 호출
    public void Pickup()
    {
        // 현재 가장 가까운 픽업이 자신인지 확인
        if (ReferenceEquals(PickupManager.Instance.GetClosestPickup(), this))
        {
            // WeaponManager에 무기 장착 요청
            if (WeaponManager.Instance != null && weaponData != null)
            {
                WeaponManager.Instance.EquipWeapon(weaponData);
                Debug.Log($"Weapon picked up: {weaponData.name}");
            }
            
            // 픽업 완료 후 오브젝트 제거
            Destroy(gameObject);
        }
    }

    #region IPickupable Implementation
    
    public bool CanPickup()
    {
        return canPickup && weaponData != null;
    }

    public string GetItemType()
    {
        return weaponData != null ? weaponData.name : "Unknown Weapon";
    }

    public PickupType GetPickupType()
    {
        return PickupType.Weapon;
    }

    public Transform GetTransform()
    {
        return transform;
    }
    #endregion
}
