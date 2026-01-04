using System.Collections.Generic;
using System.Text;
using ProjectT.Data.ScriptableObjects.Inventory;
using UnityEngine;
using ProjectT.Core;
using ProjectT.Data.ScriptableObjects.Items;
using ProjectT.Gameplay.Items.Execution;
using ProjectT.Gameplay.Items.Inventory.UI;

namespace ProjectT.Gameplay.Items.Inventory
{
    /// <summary>
    /// InventoryController (축소 버전)
    /// 책임: 데이터 관리 + UI 업데이트 (활성 상태일 때만)
    /// 
    /// 입력(InputManager) 및 Pause 제어는 InventoryRootController로 분리됨
    /// 생명주기 이벤트(Open/Close)는 Root에서 전달받음
    /// </summary>
    public class InventoryController : Singleton<InventoryController>
    {
        [SerializeField] private InventoryManager inventoryUI;
        [SerializeField] private InventorySO inventoryData;
        public List<InventoryItemObj> initialItems = new List<InventoryItemObj>();
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioSource audioSource;

        private bool _isVisible = false;  // 현재 인벤토리가 표시되는 상태인지 여부

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            PrepareUI();
            PrepareInventoryData();
            SubscribeToRootEvents();
        }

        private void OnDisable()
        {
            if (inventoryData != null)
            {
                inventoryData.OnInventoryUpdated -= UpdateInventoryUI;
            }

            if (inventoryUI != null)
            {
                inventoryUI.OnDescriptionRequested -= HandleDescriptionRequest;
                inventoryUI.OnSwapItem -= HandleSwapItems;
                inventoryUI.OnStartDragging -= HandleDragging;
                inventoryUI.OnItemActionRequested -= HandleItemActionRequest;
            }

            UnsubscribeFromRootEvents();
        }

        private void SubscribeToRootEvents()
        {
            InventoryRootController root = GetComponentInParent<InventoryRootController>();
            if (root != null)
            {
                root.OnTabChanged += HandleTabChanged;
                root.OnInventoryVisibilityChanged += HandleVisibilityChanged;
            }
        }

        private void UnsubscribeFromRootEvents()
        {
            InventoryRootController root = GetComponentInParent<InventoryRootController>();
            if (root != null)
            {
                root.OnTabChanged -= HandleTabChanged;
                root.OnInventoryVisibilityChanged -= HandleVisibilityChanged;
            }
        }

        private void HandleTabChanged(InventoryRootController.Tab tab)
        {
            Debug.Log($"[InventoryController] HandleTabChanged called with tab: {tab}");
            if (tab == InventoryRootController.Tab.Default)
                OnInventoryOpened();
            else
                OnInventoryClosed();
        }

        private void HandleVisibilityChanged(bool isOpen)
        {
            if (isOpen) OnInventoryOpened();
            else OnInventoryClosed();
        }

        private void PrepareInventoryData()
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
            foreach (InventoryItemObj item in initialItems)
            {
                if (item.IsEmpty)
                    continue;
                inventoryData.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItemObj> inventoryState)
        {
            // 활성 상태일 때만 UI 갱신
            if (!_isVisible) return;

            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
            }
        }

        private void PrepareUI()
        {
            inventoryUI.InitializeInventoryUI(inventoryData.Size);
            inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
            inventoryUI.OnSwapItem += HandleSwapItems;
            inventoryUI.OnStartDragging += HandleDragging;
            inventoryUI.OnItemActionRequested += HandleItemActionRequest;
        }

        /// <summary>
        /// Root에서 호출: UI가 표시될 때 상태 업데이트 및 갱신
        /// </summary>
        public void OnInventoryOpened()
        {
            _isVisible = true;
            RefreshUI();
        }

        /// <summary>
        /// Root에서 호출: UI가 숨겨질 때 상태 업데이트
        /// </summary>
        public void OnInventoryClosed()
        {
            _isVisible = false;
        }

        /// <summary>
        /// UI를 현재 데이터 상태로 갱신 (Open 시에만 호출)
        /// </summary>
        private void RefreshUI()
        {
            var state = inventoryData.GetCurrentInventoryState();
            inventoryUI.ResetAllItems();
            foreach (var item in state)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
            }
        }

        private void HandleItemActionRequest(int itemIndex)
        {
            InventoryItemObj inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                inventoryUI.ShowItemAction(itemIndex);
                inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
            }

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
            }
        }

        private void DropItem(int itemIndex, int quantity)
        {
            inventoryData.RemoveItem(itemIndex, quantity);
            inventoryUI.ResetSelection();
            audioSource.PlayOneShot(dropSound);
        }

        public void PerformAction(int itemIndex)
        {
            InventoryItemObj inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(itemIndex, 1);
            }

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                itemAction.PerformAction(gameObject, inventoryItem.itemState);
                audioSource.PlayOneShot(itemAction.actionSFX);
                if (inventoryData.GetItemAt(itemIndex).IsEmpty)
                {
                    inventoryUI.ResetSelection();
                }
            }
        }

        private void HandleDragging(int itemIndex)
        {
            InventoryItemObj inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
                return;
            inventoryUI.CreateDraggedItem(inventoryItem.item.ItemImage, inventoryItem.quantity);
        }

        private void HandleSwapItems(int itemIndex_1, int itemIndex_2)
        {
            inventoryData.SwapItems(itemIndex_1, itemIndex_2);
        }

        private void HandleDescriptionRequest(int itemIndex)
        {
            InventoryItemObj inventoryItem = inventoryData.GetItemAt(itemIndex);
            if (inventoryItem.IsEmpty)
            {
                inventoryUI.ResetSelection();
                return;
            }

            ItemSO item = inventoryItem.item;
            string description = PrepareDescription(inventoryItem);
            inventoryUI.UpdateDecription(itemIndex, item.ItemImage, item.Name, description);
        }

        private string PrepareDescription(InventoryItemObj inventoryItem)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(inventoryItem.item.Description);
            sb.AppendLine();

            for (int i = 0; i < inventoryItem.itemState.Count; ++i)
            {
                sb.AppendLine($"{inventoryItem.itemState[i].itemParameter.parameterName} "
                    + $": {inventoryItem.itemState[i].value} / "
                    + $"{inventoryItem.item.DefaultParametersList[i].value}");
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

}

