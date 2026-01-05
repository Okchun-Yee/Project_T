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
        [SerializeField] private GameObject playerCharacter;  // 장착/사용 시 대상 플레이어
        public List<InventoryItemObj> initialItems = new List<InventoryItemObj>();
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioSource audioSource;

        private bool _isVisible = false;  // 현재 인벤토리가 표시되는 상태인지 여부
        private bool _isUIInitialized = false;  // UI 슬롯 초기화 완료 여부

        protected override void Awake()
        {
            base.Awake();
            // 데이터는 Awake에서 초기화 (GameObject 활성화 여부와 무관)
            PrepareInventoryData();
        }

        private void Start()
        {
            // UI 초기화는 Start에서 (InventoryManager가 준비된 후)
            EnsureUIInitialized();
        }

        private void OnEnable()
        {
            // GameObject가 비활성 상태였다가 처음 활성화되는 경우 UI 초기화
            EnsureUIInitialized();
            
            // UI 이벤트 구독
            SubscribeToUIEvents();
            SubscribeToRootEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromUIEvents();
            UnsubscribeFromRootEvents();
        }

        private void SubscribeToUIEvents()
        {
            if (inventoryUI != null)
            {
                inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
                inventoryUI.OnSwapItem += HandleSwapItems;
                inventoryUI.OnStartDragging += HandleDragging;
                inventoryUI.OnItemActionRequested += HandleItemActionRequest;
            }
        }

        private void UnsubscribeFromUIEvents()
        {
            if (inventoryUI != null)
            {
                inventoryUI.OnDescriptionRequested -= HandleDescriptionRequest;
                inventoryUI.OnSwapItem -= HandleSwapItems;
                inventoryUI.OnStartDragging -= HandleDragging;
                inventoryUI.OnItemActionRequested -= HandleItemActionRequest;
            }
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

        private void EnsureUIInitialized()
        {
            if (!_isUIInitialized && inventoryUI != null && inventoryData != null)
            {
                inventoryUI.InitializeInventoryUI(inventoryData.Size);
                _isUIInitialized = true;
            }
        }

        private void HandleTabChanged(InventoryRootController.Tab tab)
        {
            if (tab == InventoryRootController.Tab.Default)
            {
                _isVisible = true;
                RefreshUI();
            }
            else
            {
                _isVisible = false;
            }
        }

        private void HandleVisibilityChanged(bool isOpen)
        {
            _isVisible = isOpen;
            // UI 갱신은 HandleTabChanged에서만 수행 (중복 방지)
        }

        private void PrepareInventoryData()
        {
            if (inventoryData != null)
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
        }

        private void UpdateInventoryUI(Dictionary<int, InventoryItemObj> inventoryState)
        {
            // 활성 상태일 때만 UI 갱신
            if (!_isVisible) return;

            RefreshUI();
        }

        /// <summary>
        /// UI를 현재 데이터 상태로 갱신 (Open 시 또는 데이터 변경 시 호출)
        /// </summary>
        private void RefreshUI()
        {
            if (inventoryUI == null || inventoryData == null) return;

            // UI가 초기화되지 않았으면 먼저 초기화
            EnsureUIInitialized();

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

            IItemAction itemAction = inventoryItem.item as IItemAction;
            if (itemAction != null)
            {
                // playerCharacter가 Inspector에서 할당되어 있어야 함
                if (playerCharacter != null)
                {
                    bool actionSuccess = itemAction.PerformAction(playerCharacter, inventoryItem.itemState);
                    
                    if (actionSuccess && itemAction.actionSFX != null)
                    {
                        audioSource.PlayOneShot(itemAction.actionSFX);
                    }
                }
                else
                {
                    Debug.LogWarning("[InventoryController] playerCharacter is not assigned in Inspector!");
                }
            }

            IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
            if (destroyableItem != null)
            {
                inventoryData.RemoveItem(itemIndex, 1);
            }

            if (inventoryData.GetItemAt(itemIndex).IsEmpty)
            {
                inventoryUI.ResetSelection();
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

