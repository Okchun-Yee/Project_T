using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Model;
using Inventory.UI;
using UnityEngine;
using UnityEngine.AI;

namespace Inventory
{
    public class InventoryController : Singleton<InventoryController>
    {
        [SerializeField] private InventoryManager inventoryUI;
        [SerializeField] private InventorySO inventoryData;
        [SerializeField] private InventoryManager runeInventoryUI;
        [SerializeField] private InventorySO runeInventoryData;
        public List<InventoryItemObj> initialItems = new List<InventoryItemObj>();


        protected override void Awake()
        {
            base.Awake();
        }


        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventoryInput += InventoryInput;
            }
            PrepareUI();
            PrepareInventoryData();
        }

        private void OnDisable() 
        {
            if (inventoryData != null)
            {
                inventoryData.OnInventoryUpdated -= UpdateInventoryUI;
            }
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventoryInput -= InventoryInput;
            }
        }
        private void InventoryInput() // 인벤토리 인풋
        {
            if (!inventoryUI.isActiveAndEnabled)
            {
                inventoryUI.Show();
                foreach (var item in inventoryData.GetCurrentInventoryState()) // 모든 아이템 탐색
                {
                    inventoryUI.UpdateData(item.Key, item.Value.item.Itemimage, item.Value.quantity); // key는 인덱스

                }
            }
            else
            {
                inventoryUI.Hide();
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
        private void PrepareInventoryData() // 아이템 초기화
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            foreach (InventoryItemObj item in initialItems)
            {
                if (item.isEmpty)
                    continue;
                inventoryData.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItemObj> inventoryState) // 아이템의 위치(인덱스), 퀀티티 등 세부 정보 업데이트
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.Itemimage, item.Value.quantity);
            }
        }

        // UI 초기화. 각 핸들러 및 이벤트 초기화
        private void PrepareUI() 
        {
            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            this.inventoryUI.OnSwapItem += HandleSwapItems;
            this.inventoryUI.OnStartDragging += HandleDragging;
            this.inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        private void HandleItemActionRequest(int itemIndex)
        {

        }

        private void HandleDragging(int itemIndex) // 아이템 드래그 확인
        {
            InventoryItemObj inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty)
                return;
            inventoryUI.CreateDraggedItem(inventoryItem.item.Itemimage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2) // 아이템 위치 스왑
        {
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex) // 아이템 선택이 바뀌었을 때 아이템 설명 업데이트
        {
            InventoryItemObj inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty) // 빈 슬롯 선택시 선택 초기화
            {
                inventoryUI.ResetSelection();
                return;
            }
            ItemSO item = inventoryItem.item;
            inventoryUI.UpdateDecription(itemIndex, item.Itemimage, item.Name, item.Description);
        }
    }

}
