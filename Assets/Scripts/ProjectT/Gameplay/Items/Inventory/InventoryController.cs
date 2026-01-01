using System.Collections.Generic;
using System.Text;
using ProjectT.Data.ScriptableObjects.Inventory;
using UnityEngine;
using ProjectT.Core;
using ProjectT.Gameplay.Player.Input;
using ProjectT.Data.ScriptableObjects.Items;
using ProjectT.Gameplay.Items.Execution;

namespace ProjectT.Gameplay.Items.Inventory
{
    public class InventoryController : Singleton<InventoryController>
    {
        [SerializeField] private InventoryManager inventoryUI;
        [SerializeField] private InventorySO inventoryData;
        public List<InventoryItemObj> initialItems = new List<InventoryItemObj>();
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioSource audioSource;


        protected override void Awake()
        {
            base.Awake();
        }
        private void OnEnable()
        {
            // InputManager Ready 이벤트 구독
            InputManager.Ready += OnInputManagerReady;
            
            // 이미 InputManager가 준비된 경우 바로 바인딩
            if (InputManager.Instance != null)
            {
                BindInputEvents();
            }
            PrepareUI();
            PrepareInventoryData();
        }

        private void OnDisable()
        {
            // InputManager Ready 이벤트 구독 해제
            InputManager.Ready -= OnInputManagerReady;
            
            if (inventoryData != null)
            {
                inventoryData.OnInventoryUpdated -= UpdateInventoryUI;
            }
            
            // ✅ 추가: inventoryUI 이벤트 해제
            if (inventoryUI != null)
            {
                inventoryUI.OnDescriptionRequested -= HandleDescriptionRequest;
                inventoryUI.OnSwapItem -= HandleSwapItems;
                inventoryUI.OnStartDragging -= HandleDragging;
                inventoryUI.OnItemActionRequested -= HandleItemActionRequest;
            }
            
            UnbindInputEvents();
        }
        
        /// <summary>
        /// InputManager 준비 완료 시 호출되는 콜백
        /// </summary>
        private void OnInputManagerReady()
        {
            BindInputEvents();
        }
        
        /// <summary>
        /// InputManager 입력 이벤트 바인딩
        /// </summary>
        private void BindInputEvents()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventoryInput += InventoryInput;
            }
        }
        
        /// <summary>
        /// InputManager 입력 이벤트 바인딩 해제
        /// </summary>
        private void UnbindInputEvents()
        {
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
                    inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity); // key는 인덱스

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
                if (item.IsEmpty)
                    continue;
                inventoryData.AddItem(item);
            }
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItemObj> inventoryState)
        {
            inventoryUI.ResetAllItems();
            foreach (var item in inventoryState)
            {
                inventoryUI.UpdateData(item.Key, item.Value.item.ItemImage, item.Value.quantity);
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
                if(inventoryData.GetItemAt(itemIndex).IsEmpty)
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
            if (inventoryItem.IsEmpty) // 빈 슬롯 선택시 선택 초기화
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
            for(int i = 0; i < inventoryItem.itemState.Count; ++i){
                sb.AppendLine($"{inventoryItem.itemState[i].itemParameter.parameterName} "
                + $": {inventoryItem.itemState[i].value} / "
                + $"{inventoryItem.item.DefaultParametersList[i].value}");
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

}
