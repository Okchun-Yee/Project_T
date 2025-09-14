using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : Singleton<WeaponManager>
{
    [Header("Weapon Icons")]
    [SerializeField] private Sprite meleeIcon;
    [SerializeField] private Sprite rangedIcon;
    [SerializeField] private Sprite magicIcon;

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponMountPoint;        // 무기 장착 위치
    private BaseWeapon currentWeapon;                           // 현재 장착된 무기
    public event Action<Sprite> onCategoryIconChanged;          // 무기 카테고리 아이콘 변경 이벤트
    protected override void Awake()
    {
        base.Awake();
    }
    // 무기 장착 매서드
    public void EquipWeapon(WeaponSO info)
    {
        if (info == null)
        {
            Debug.LogError("[WeaponManager] WeaponInfo is null");
            return;
        }
        // 1) 기존 장착 무기 제거
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
            currentWeapon = null;
            ActiveWeapon.Instance.ClearWeapon();
        }

        // 2) 새 무기 인스턴스화 및 마운트
        GameObject gameObj = Instantiate(info.weaponPrefab, weaponMountPoint.position, Quaternion.identity, weaponMountPoint);

        // 3) BaseWeapon 컴포넌트 가져와 초기화
        BaseWeapon bw = gameObj.GetComponent<BaseWeapon>();
        if (bw == null)
        {
            Debug.LogError("[WeaponManager] WeaponPrefab missing BaseWeapon component");
            
            Destroy(gameObj);
            return;
        }
        bw.Weapon_Initialize(info);    // 무기 정보로 초기화
        currentWeapon = bw;     // 현재 무기 참조 저장

        // 4) ActiveWeapon에 장착 통보
        ActiveWeapon.Instance.NewWeapon(bw);

        // 5) UI 아이콘 갱신 이벤트 발행
        Sprite icon = info.weaponCategory switch
        {
            WeaponCategory.Melee => meleeIcon,
            WeaponCategory.Range => rangedIcon,
            WeaponCategory.Magic => magicIcon,
            _ => null
        };
        onCategoryIconChanged?.Invoke(icon);
    }

    // 무기 장착 해제 매서드
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
            currentWeapon = null;
            ActiveWeapon.Instance.ClearWeapon();
        }

        onCategoryIconChanged?.Invoke(null);        // UI Update 이벤트 발행

    }
}
