using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;
using Inventory.UI;

namespace RuneInventory
{
    public class RuneInventory : MonoBehaviour
    {  
        [SerializeField] private InventoryManager runeInventoryUI;
        [SerializeField] private InventorySO runeInventoryData;
        private void RuneInventoryInput() // 인벤토리 인풋
        {
            if (!runeInventoryUI.isActiveAndEnabled)
            {
                runeInventoryUI.Show();
                // foreach (var item in runeInventoryData.GetCurrentInventoryState()) // 모든 아이템 탐색
                // {
                //     runeInventoryUI.UpdateData(item.Key, item.Value.item.Itemimage, item.Value.quantity); // key는 인덱스

                // }
            }
            else
            {
                runeInventoryUI.Hide();
            }
        }
        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventoryInput += RuneInventoryInput;
            }
            // PrepareUI();
            // PrepareInventoryData();
        }

        private void OnDisable() 
        {
            if (runeInventoryData != null)
            {
                runeInventoryData.OnInventoryUpdated -= UpdateRuneInventoryUI;
            }
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventoryInput -= RuneInventoryInput;
            }
        }
        private void UpdateRuneInventoryUI(Dictionary<int, InventoryItemObj> inventoryState) // 아이템의 위치(인덱스), 퀀티티 등 세부 정보 업데이트
        {
            runeInventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                runeInventoryUI.UpdateData(item.Key, item.Value.item.Itemimage, item.Value.quantity);
            }
        }
    }

}

