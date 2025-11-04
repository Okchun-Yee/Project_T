using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class InventorySO : ScriptableObject
    {
        [SerializeField] private List<InventoryItemObj> inventoryItems; // 인벤토리 컨트롤러가 이 리스트를 액세스하여 새 값을 수정할 수 있음
        [field: SerializeField] public int Size { get; private set; } = 10; // 인벤토리  사이즈
        public event Action<Dictionary<int, InventoryItemObj>> OnInventoryUpdated;
         
        public void Initailize()
        {
            inventoryItems = new List<InventoryItemObj>();
            for (int i = 0; i < Size; i++) // 인벤토리 사이즈만큼 빈 항목 생성
            {
                inventoryItems.Add(InventoryItemObj.GetEmptyItem());
            }
        }
        
        public Dictionary<int, InventoryItemObj> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItemObj> returnValue = new Dictionary<int, InventoryItemObj>();
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].isEmpty)
                    continue;
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InventoryItemObj GetItemAt(int itemIndex)
        {
            return inventoryItems[itemIndex];
        }

        public void  AddItem(InventoryItemObj item)
        {
            AddItem(item.item, item.quantity);
        }
        public void AddItem(ItemSO item, int quantity)
        {
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].isEmpty)
                {
                    inventoryItems[i] = new InventoryItemObj
                    {
                        item = item,
                        quantity = quantity
                    };
                    return;
                }
            }
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
        public bool isEmpty => item == null;

        public InventoryItemObj  ChangeQuantity(int newQuantity) // 값을 변경하기 위한 또 하나의 구조체
        {
            return new InventoryItemObj
            {
                item = this.item,
                quantity = newQuantity,
            };
        }
        public static InventoryItemObj GetEmptyItem() => new InventoryItemObj
        {
            item = null,
            quantity = 0,
        };
    }
}

