using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class WeaponManager : Singleton<WeaponManager>
{
    [Header("Weapon Icons")]
    [SerializeField] private Sprite meleeIcon;
    [SerializeField] private Sprite rangedIcon;
    [SerializeField] private Sprite magicIcon;

    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponMountPoint;        // 무기 장착 위치
    [SerializeField] private AgentWeapon playerAgentWeapon;     // 플레이어 에이전트 무기 참조

    public event Action<Sprite> onCategoryIconChanged;          // 무기 카테고리 아이콘 변경 이벤트
    private BaseWeapon currentWeapon;                           // 현재 장착된 무기

    protected override void Awake()
    {
        base.Awake();
    }
    // AgentWeapon에서 무기 변경 이벤트 구독
    private void OnEnable()
    {
        if (playerAgentWeapon != null)
            playerAgentWeapon.OnWeaponChanged += HandleAgentWeaponChanged;
    }

    private void OnDisable()
    {
        if (playerAgentWeapon != null)
            playerAgentWeapon.OnWeaponChanged -= HandleAgentWeaponChanged;
    }
    /// <summary>
    /// * 무기 장착 매서드
    /// info는 null이 아니어야 하며, 무기 해제는 UnequipWeapon() 또는 AgentWeapon.SetWeapon(null)로 처리해야 합니다.
    /// </summary>
    public void EquipWeapon(EquippableItemSO info)
    {
        if (info == null)
        {
            Debug.LogError("[WeaponManager] WeaponInfo is null");
            UnequipWeapon();     // null이 들어오면 해제, 편의용 숏컷
            return;
        }
        // 1) 기존 장착 무기 제거
        if (currentWeapon != null)
        {
           UnequipWeapon();     // 편의용 숏컷 SSOT 구조상 무기 장착/해제는 AgentWeapon가 담당하지만, 여기서도 안전하게 처리
        }

        // 2) 새 무기 인스턴스화 및 마운트
        GameObject gameObj = Instantiate(
            info.weaponPrefab, 
            weaponMountPoint.position, 
            Quaternion.identity, 
            weaponMountPoint
        );

        // 3) BaseWeapon 컴포넌트 가져와 초기화
        BaseWeapon bw = gameObj.GetComponent<BaseWeapon>();
        if (bw == null)
        {
            Debug.LogError("[WeaponManager] WeaponPrefab missing BaseWeapon component");
            Destroy(gameObj);
            return;
        }
        bw.Weapon_Initialize(info); // 무기 정보로 초기화
        currentWeapon = bw;         // 현재 무기 참조 저장

        // 4) ActiveWeapon에 장착 통보
        ActiveWeapon.Instance.NewWeapon(bw);

        // 5) UI 아이콘 갱신 이벤트 발행
        Sprite icon = info.weaponCategory switch
        {
            WeaponCategory.Melee    => meleeIcon,
            WeaponCategory.Range    => rangedIcon,
            WeaponCategory.Magical  => magicIcon,
            _                       => null
        };
        onCategoryIconChanged?.Invoke(icon);    // UI Update 이벤트 발행
    }
    /// <summary>
    /// * 무기 장착 해제 매서드
    /// </summary>
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
    /// <summary>
    /// * AgentWeapon의 무기 변경 이벤트 핸들러
    /// </summary>
    public void HandleAgentWeaponChanged(EquippableItemSO info, List<ItemParameter> state)
    {
        // 일단은 state는 사용하지 않고, 무기 데이터 기준으로만 장착
        // (추후 state를 반영해서 강화/옵션을 Weapon에 반영하는 단계로 확장 가능)
        EquipWeapon(info);
    }
}
