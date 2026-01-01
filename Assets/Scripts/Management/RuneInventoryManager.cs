using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.UI
{
    public class RuneInventoryManager : MonoBehaviour
    {
        [SerializeField] private RuneItem RunePrefab;
        [SerializeField] private RectTransform contentPanel;
        [SerializeField] private InventoryDescription itemDescription;

        [SerializeField] private MouseFollower mouseFollower;

        List<RuneItem> listOfItems = new List<RuneItem>();


        private int currentlyDraggedItemIndex = -1;

        public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging; // 인덱스를 가져와 적용



        private void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }
        public void InitializeInventoryUI(int inventorySize) //인벤토리 초기화
        {
            for (int i = 0; i < inventorySize; i++) // 인벤토리 사이즈 만큼 복사
            {
                RuneItem inventoryItemUI = Instantiate(RunePrefab, Vector3.zero, Quaternion.identity);
                inventoryItemUI.transform.SetParent(contentPanel);
                listOfItems.Add(inventoryItemUI);
                inventoryItemUI.OnItemClicked += HandleItemSelection;
                inventoryItemUI.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }   
        public void UpdateData(int itemIndex, Sprite itemImage)
        {
            if(listOfItems.Count > itemIndex) //해당 항목이 목록에 있는지확인
            {
                listOfItems[itemIndex].SetData(itemImage);
            }
        }

        private void HandleShowItemActions(RuneItem inventoryItemUI)
        {
        }

        private void HandleItemSelection(RuneItem inventoryItemUI) // 선택된 아이템 설명 설정
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

        private void DeselectAllItems() 
        {
            foreach(RuneItem item in listOfItems)
            {
                item.Deselect(); // 선택 취소
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
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