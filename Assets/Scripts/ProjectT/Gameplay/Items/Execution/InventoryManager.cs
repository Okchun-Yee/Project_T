using System;
using System.Collections.Generic;
using ProjectT.Gameplay.Items.Inventory;
using UnityEngine;

namespace ProjectT.Gameplay.Items.Execution
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private InventoryItem itemPrefab;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private InventoryDescription itemDescription;

        [SerializeField] private MouseFollower mouseFollower;

        List<InventoryItem> listOfItems = new List<InventoryItem>();


        private int currentlyDraggedItemIndex = -1;

        public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging; // 인덱스를 가져와 적용
        public event Action<int, int> OnSwapItem; // 스왑할 두 개의 아이템 인덱스를 가져와 적용
        [SerializeField] private ItemActionPanel actionPanel;

        private bool _isInitialized = false;  // InitializeInventoryUI 1회만 실행

        private void Awake()
        {
            if (gameObject.activeSelf)
            {
                Hide();
            }
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }
        public void InitializeInventoryUI(int inventorySize) //인벤토리 초기화
        {
            if (_isInitialized) return;  // 중복 초기화 방지
            
            _isInitialized = true;
            for (int i = 0; i < inventorySize; i++) // 인벤토리 사이즈 만큼 복사
            {
                InventoryItem inventoryItemUI = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                inventoryItemUI.transform.SetParent(contentPanel);
                listOfItems.Add(inventoryItemUI);
                inventoryItemUI.OnItemClicked += HandleItemSelection;
                inventoryItemUI.OnItemBeginDrag += HandleBeginDrag;
                inventoryItemUI.OnItemDroppedOn += HandleSwap;
                inventoryItemUI.OnItemEndDrag += HandleEndDrag;
                inventoryItemUI.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }
        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
        {
            if (listOfItems.Count > itemIndex) //해당 항목이 목록에 있는지확인
            {
                listOfItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }

        private void HandleShowItemActions(InventoryItem inventoryItemUI)
        {
            int index = listOfItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnItemActionRequested?.Invoke(index);
        }

        private void HandleEndDrag(InventoryItem inventoryItemUI)
        {
            ResetDraggedItem(); //매 드래그마다 두 번 호출하는 것이 아닌(Swap) 드래그가 끝날 때 한 번만 호출
        }

        private void HandleSwap(InventoryItem inventoryItemUI)
        {
            int index = listOfItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            // 드래그 검사 방어 코드 추가
            if (currentlyDraggedItemIndex == -1 || currentlyDraggedItemIndex == index)
            {
                return;
            }
            OnSwapItem?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
        }

        private void ResetDraggedItem() //현재 드래그한 아이템의 인덱스를 -1로 설정
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        private void HandleBeginDrag(InventoryItem inventoryItemUI)
        {
            int index = listOfItems.IndexOf(inventoryItemUI);
            if (index == -1)
                return;
            currentlyDraggedItemIndex = index;
            HandleItemSelection(inventoryItemUI); // 동일한 아이템이 존재할 경우 혼돈을 방지하기 위함
            OnStartDragging?.Invoke(index); // 인벤토리 자체가 아닌 드래그한 아이템을 생성할지 결정

        }

        public void CreateDraggedItem(Sprite sprite, int quantity)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity); // 아이템 스프라이트와 개수 재정의
        }

        private void HandleItemSelection(InventoryItem inventoryItemUI) // 선택된 아이템 설명 설정
        {
            int index = listOfItems.IndexOf(inventoryItemUI);
            if (index == -1) // 빈 슬롯 클릭시 동작하지 않음
                return;
            OnDescriptionRequested?.Invoke(index);

        }

        public void Show()
        {
            gameObject.SetActive(true);
            ResetSelection(); // 아이템 선택할 때 마다 설명 리셋
        }

        public void ResetSelection()
        {
            itemDescription.ResetDescription();
            DeselectAllItems(); // 테두리 초기화
        }
        public void AddAction(string actionName, Action performAction)
        {
            actionPanel.AddButon(actionName, performAction);
        }
        public void ShowItemAction(int itemIndex)
        {
            actionPanel.Toggle(true);
            actionPanel.transform.position = listOfItems[itemIndex].transform.position;
        }

        private void DeselectAllItems()
        {
            foreach (InventoryItem item in listOfItems)
            {
                item.Deselect(); // 선택 취소
            }
            actionPanel.Toggle(false);
        }

        public void Hide()
        {
            actionPanel.Toggle(false);
            gameObject.SetActive(false);
            ResetDraggedItem();
        }

        internal void UpdateDecription(int itemIndex, Sprite itemimage, string name, string description)
        {
            itemDescription.SetDescription(itemimage, name, description);
            DeselectAllItems(); // 다른 것이 선택되어 있을 수 있으니 선택 취소
            listOfItems[itemIndex].Select();
        }

        internal void ResetAllItems()
        {
            foreach (var item in listOfItems)
            {
                item.ResetData();
                item.Deselect();
            }
        }
    }

}