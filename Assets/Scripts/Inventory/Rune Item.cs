using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace Inventory.UI
{
    public class RuneItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image runeImage;
        [SerializeField] private Image borderImage;

        public event Action<RuneItem> OnItemClicked, OnRightMouseBtnClick;
        private bool empty = true;
        public void Awake()
        {
            ResetData();
            Deselect();
        }
        public void ResetData()
        {
            this.runeImage.gameObject.SetActive(false);     // 아이템 이미지 비활성화
            this.empty = true;
            
        }
        public void Deselect()
        {
            this.borderImage.enabled = false;               // 아이템 슬롯 테두리 비활성화
        }
        public void SetData(Sprite sprite)
        {
            Debug.Log("호출됨");
            this.runeImage.gameObject.SetActive(true);      // 아이템 이미지 활성화
            this.runeImage.sprite = sprite;
            this.empty = false;
        }
        public void Select() // 아이템 선택 시
        {
            borderImage.enabled = true;
        }
        public void OnPointerClick(PointerEventData pointerData)
        {
            if (pointerData.button == PointerEventData.InputButton.Right)
            {
                OnRightMouseBtnClick?.Invoke(this);
            }
            else
            {
                OnItemClicked?.Invoke(this);
            }
        }

    }
    
}
