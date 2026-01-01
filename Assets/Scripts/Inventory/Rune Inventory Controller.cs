using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Model;
using Inventory.UI;
using UnityEngine;
using UnityEngine.AI;

namespace Inventory
{
    public class RuneInventoryController : MonoBehaviour
    {
        
        [SerializeField] private InventorySO runeInventoryData;
        [SerializeField] private RuneInventoryManager runeInventoryUI;

        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnRuneInventoryInput += RuneInventoryInput;
            }
            PrepareRuneUI();            
            PrepareRuneInventoryData();
        }

        private void OnDisable() 
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnRuneInventoryInput -= RuneInventoryInput;
            }
        }
        private void PrepareRuneUI()
        {
            runeInventoryUI.InitializeInventoryUI(runeInventoryData.Size);
            // 룬 인벤도 드래그/설명 등 쓰려면 이벤트도 연결해야 함(필요 시)
            // runeInventoryUI.OnDescriptionRequested += HandleRuneDescriptionRequest; ...
        }

        private void PrepareRuneInventoryData()
        {
            runeInventoryData.Initialize();
            runeInventoryData.OnInventoryUpdated += UpdateRuneInventoryUI;
        }
        private void RuneInventoryInput() // 인벤토리 인풋
        {
            if (!runeInventoryUI.isActiveAndEnabled)
            {
                runeInventoryUI.Show();
                // foreach (var item in inventoryData.GetCurrentInventoryState()) // 모든 아이템 탐색
                // {
                //     runeInventoryUI.UpdateData(item.Key, item.Value.item.Itemimage, item.Value.quantity); // key는 인덱스

                // }
            }
            else
            {
                runeInventoryUI.Hide();
            }
        }

        private void UpdateRuneInventoryUI(Dictionary<int, InventoryItemObj> inventoryState) // 아이템의 위치(인덱스), 퀀티티 등 세부 정보 업데이트
        {
            runeInventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                runeInventoryUI.UpdateData(item.Key, item.Value.item.Itemimage);
            }
        }
    }
}
