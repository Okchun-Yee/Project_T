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
        private void InventoryInput()
        {
            if (!inventoryUI.isActiveAndEnabled)
            {
                inventoryUI.Show();
                foreach (var item in inventoryData.GetCurrentInventoryState()) // 
                {
                    inventoryUI.UpdateData(item.Key, item.Value.item.Itemimage, item.Value.quantity); // key는 인덱스

                }
            }
            else
            {
                inventoryUI.Hide();
            }
        }
        private void PrepareInventoryData()
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

        private void UpdateInventoryUI(Dictionary<int, InventoryItemObj> inventoryState)
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.Itemimage, item.Value.quantity);
            }
        }

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

        private void HandleDragging(int itemIndex)
        {
            InventoryItemObj inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.isEmpty)
                return;
            inventoryUI.CreateDraggedItem(inventoryItem.item.Itemimage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
        {
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex)
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
