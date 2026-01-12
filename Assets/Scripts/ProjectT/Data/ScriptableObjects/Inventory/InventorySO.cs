using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectT.Data.ScriptableObjects.Items;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.Inventory
{
    [CreateAssetMenu(menuName = "New Inventory")]
    public class InventorySO : ScriptableObject
    {
        [SerializeField] private List<InventoryItemObj> inventoryItems;     // 인벤토리 컨트롤러가 이 리스트를 액세스하여 새 값을 수정할 수 있음
        [field: SerializeField] public int Size { get; private set; } = 10; // 인벤토리  사이즈
                    
        public event Action<Dictionary<int, InventoryItemObj>> OnInventoryUpdated;

        private void OnEnable()
        {
            // 에디터에서 Play 모드 시작 시 인벤토리 초기화
            #if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            #endif
            
            inventoryItems = new List<InventoryItemObj>();
            for (int i = 0; i < Size; i++)
            {
                inventoryItems.Add(InventoryItemObj.GetEmptyItem());
            }
        }

        public void Initialize()
        {
            // 런타임 중 명시적 초기화 (OnEnable이 이미 실행되었거나 아직 실행 안 될 수도 있으므로 항상 체크)
            if (inventoryItems == null || inventoryItems.Count != Size)
            {
                inventoryItems = new List<InventoryItemObj>();
                for (int i = 0; i < Size; i++)
                {
                    inventoryItems.Add(InventoryItemObj.GetEmptyItem());
                }
            }
        }

        public Dictionary<int, InventoryItemObj> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItemObj> returnValue = new Dictionary<int, InventoryItemObj>();
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InventoryItemObj GetItemAt(int itemIndex)
        {
            return inventoryItems[itemIndex];
        }
        public void RemoveItem(int itemIndex, int amount)
        {
            if (inventoryItems.Count > itemIndex)
            {
                if (inventoryItems[itemIndex].IsEmpty)
                {
                    Debug.LogWarning($"[InventorySO] Attempting to remove item at index {itemIndex}, but it's already empty.");
                    return;
                }
                int reminder = inventoryItems[itemIndex].quantity - amount;

                if (reminder <= 0)
                {
                    inventoryItems[itemIndex] = InventoryItemObj.GetEmptyItem();
                }
                else
                {
                    inventoryItems[itemIndex] = inventoryItems[itemIndex].ChangeQuantity(reminder);
                }
                InformAboutChange();
            }
        }
        public void AddItem(InventoryItemObj item)
        {
            AddItem(item.item, item.quantity);
        }
        public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            if (item.IsStackable == false)
            {
                while (quantity > 0 && !IsInventoryFull())
                {
                    quantity -= AddItemToFirstFreeSlot(item, 1, itemState);
                }
                InformAboutChange();
                return quantity;
            }
            quantity = AddStackableItem(item, quantity);
            InformAboutChange();
            return quantity;
        }

        private int AddItemToFirstFreeSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            InventoryItemObj newItem = new InventoryItemObj
            {
                item = item,
                quantity = quantity,
                itemState = new List<ItemParameter>(itemState == null ? item.DefaultParametersList : itemState)
            };

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                {
                    inventoryItems[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }

        private bool IsInventoryFull()
        => inventoryItems.Where(item => item.IsEmpty).Any() == false; // 인벤토리가 가득 찼는지 확인

        private int AddStackableItem(ItemSO item, int quantity)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].IsEmpty)
                    continue;
                if (inventoryItems[i].item.ID == item.ID)
                {
                    //만약 아이템을 가지고 있다면 최대 98개를 먹을 수 있기 때문에 현재 최대 사이즈 - 현재 아이템 개수
                    int amountPossibleToTake = inventoryItems[i].item.MaxStackSize - inventoryItems[i].quantity;

                    if (quantity > amountPossibleToTake)
                    {
                        inventoryItems[i] = inventoryItems[i]
                            .ChangeQuantity(inventoryItems[i].item.MaxStackSize); // 만약 최대개수를 넘어갔다면 아이템 획득 후
                        quantity -= amountPossibleToTake; // 획득 가능한 개수만큼 뺀 값으로 재정의 ex) 아이템을 28개 가지고있고 90개를 먹었다면 90 - 71 = 19개를 남겨놓음
                    }
                    else
                    {
                        inventoryItems[i] = inventoryItems[i]
                            .ChangeQuantity(inventoryItems[i].quantity + quantity);
                        InformAboutChange();
                        return 0;
                    }
                }
            }
            while (quantity > 0 && !IsInventoryFull())
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.MaxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(item, newQuantity);
            }
            return quantity;
        }

        public void SwapItems(int itemIndex_1, int itemIndex_2)
        {
            InventoryItemObj item1 = inventoryItems[itemIndex_1];
            inventoryItems[itemIndex_1] = inventoryItems[itemIndex_2];
            inventoryItems[itemIndex_2] = item1;
            InformAboutChange();
        }

        private void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }
    }

    /* 클래스 대신 구조체를 사용하는 이유는 인벤토리 아이템을 struct(구조체)로 만들면, 
    데이터가 '값'으로 복사되어 전달되므로, 의도치 않은 곳에서 원본 데이터가 수정되는 것을 막아
    버그를 줄이고 데이터를 안전하게 관리할 수 있기 때문. 이를 이용하면 값이 Null이 될 수 없지만,
    GetEmptyItem으로 빈항목을 반환해 줄 수 있음*/
    [Serializable]
    public struct InventoryItemObj
    {
        public int quantity;
        public ItemSO item;
        public List<ItemParameter> itemState;
        public bool IsEmpty => item == null;

        public InventoryItemObj ChangeQuantity(int newQuantity) // 값을 변경하기 위한 또 하나의 구조체
        {
            return new InventoryItemObj
            {
                item = this.item,
                quantity = newQuantity,
                itemState = new List<ItemParameter>(this.itemState)
            };
        }
        public static InventoryItemObj GetEmptyItem() => new InventoryItemObj
        {
            item = null,
            quantity = 0,
            itemState = new List<ItemParameter>()
        };
    }
}

