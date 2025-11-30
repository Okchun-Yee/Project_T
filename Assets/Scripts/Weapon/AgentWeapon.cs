using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Model;
using UnityEngine;

public class AgentWeapon : MonoBehaviour
{
    [Header("Equipped Weapon State")]
    [SerializeField] private EquippableItemSO weaponData;               // 현재 장착 중인 무기 데이터
    [SerializeField] private InventorySO inventoryData;                 // 플레이어 인벤토리 데이터 : 무기 반환용
    [SerializeField] private List<ItemParameter> parametersToModify;    // 무기 장착 시 수정할 파라미터 리스트
    [SerializeField] private List<ItemParameter> itemCurrentState;      // 현재 무기 상태 (강화 수치 등)

    public event Action<EquippableItemSO, List<ItemParameter>> OnWeaponChanged; // 장착된 무기가 변경되었을 때 발생하는 이벤트 (weaponSO, itemState)

    // ---------- 외부 접근용 프로퍼티 ----------
    public EquippableItemSO CurrentWeapon => weaponData;
    public IReadOnlyList<ItemParameter> CurrentParameters => itemCurrentState;
    public InventorySO InventoryData => inventoryData;  // 인벤토리 데이터 접근용
                                                        // -----------------------------------------

    /// <summary>
    /// * 무기를 장착/교체 시 진입점
    /// EquippableItemSO.PerformAction()에서 호출됨.
    /// </summary>
    public void SetWeapon(EquippableItemSO weaponSO, List<ItemParameter> itemState)
    {
        // 방어 코드: null 무기 장착 시도 방지
        if (weaponSO == null)
        {
            Debug.LogWarning("[AgentWeapon] Attempted to set null weapon. Use UnequipWeapon instead.");
            return;
        }
        // 0) 방어 코드: 인벤토리/입력 상태 null 방지
        if (itemCurrentState == null)
        {
            Debug.LogWarning("[AgentWeapon] itemCurrentState was null. Initializing new list to avoid null reference.");
            itemCurrentState = new List<ItemParameter>();
        }
        // 1) 무기 장착/교체 : 기존 장착 무기가 있다면 인벤토리로 반환
        if (weaponData != null)
        {
            // 기존 장착된 무기가 있으면 인벤토리에 반환
            int remaining = inventoryData.AddItem(weaponData, 1, itemCurrentState);

            // fail silently 발생 위험: 인벤토리가 꽉 찼을 경우
            if (remaining > 0)
            {
                Debug.LogWarning("[AgentWeapon] Inventory full, could not return existing weapon upon equipping new one.");
                // TODO: fail silently 예외 처리 로직
                // 인벤토리가 꽉 참
                // 무기를 다시 떨어뜨리거나
                // UI 알림을 띄우거나
                // 플레이어 발밑에 떨어뜨리거나
            }
        }
        // 2) 무기 장착/교체 : 새로운 무기 장착
        this.weaponData = weaponSO;
        // 인자 상태를 그대로 참조하지 않고 복사해서 내부 상태로 유지, null 방지
        this.itemCurrentState = itemState != null ? new List<ItemParameter>(itemState) : new List<ItemParameter>();
        ModifyParameters();
        // 3) 장착 상태 변경 이벤트 발행 -> WeaponManager 구독
        OnWeaponChanged?.Invoke(weaponData, itemCurrentState);
    }
    /// <summary>
    /// * 무기 장착 시 파라미터 수정
    /// ParametersToModify에 정의된 파라미터들을 itemCurrentState에 반영
    /// EX) 공격력 +10, 사거리 +5 등..
    /// </summary>
    private void ModifyParameters()
    {
        // 0) itemCurrentState는 SetWeapon에서 이미 null 체크 및 초기화됨
        if (parametersToModify == null || parametersToModify.Count == 0)
        {
            Debug.LogWarning("[AgentWeapon] parametersToModify is null or empty in ModifyParameters. No parameters to apply.");
            return;
        }
        // 1) 파라미터 수정 로직
        foreach (var parameter in parametersToModify)
        {
            if (parameter.itemParameter == null)
            {
                Debug.LogWarning("[AgentWeapon] Null itemParameter found in parametersToModify. Skipping.");
                continue;
            }
            // 같은 타입의 parameter 값 찾기
            // 기존 객체 비교 -> Parameter 타입 비교
            int index = itemCurrentState.FindIndex(p => p.itemParameter == parameter.itemParameter);
            if (index >= 0)
            {
                float newValue = itemCurrentState[index].value + parameter.value;
                itemCurrentState[index] = new ItemParameter
                {
                    itemParameter = parameter.itemParameter,
                    value = newValue
                };
            }
            else
            {
                // 파라미터 없을 시 새로 추가
                itemCurrentState.Add(new ItemParameter
                {
                    itemParameter = parameter.itemParameter,
                    value = parameter.value
                });
            }
        }
    }
}
