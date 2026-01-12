using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using TMPro;

namespace ProjectT.Gameplay.Items.Inventory.Rune
{
    /// <summary>
    /// RuneItemView
    /// - 룬 인벤토리 전용 슬롯 View 컴포넌트
    /// - 입력 이벤트(Click / Hover)만 외부로 위임한다.
    /// - 룬 데이터/로직은 소유하지 않는다. (향후 RuneInventoryController/Manager가 담당)
    /// </summary>
    public class RuneInventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, 
        IPointerExitHandler
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private TMP_Text quantityText;

        public event Action<RuneInventoryItem> OnItemClicked, OnItemPointerEnter, OnItemPointerExit;

        private bool _isEmpty = true;
        private void Awake()
        {
            ResetData();
            Deselect();
        }
        public bool IsEmpty => _isEmpty;
        public void ResetData()
        {
            if(iconImage != null) iconImage.gameObject.SetActive(false);
            if(quantityText != null) quantityText.text = "";
            _isEmpty = true;
        }
        public void SetData(Sprite icon, string text = null)
        {
            if(iconImage != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = icon;
            }
            if(quantityText != null)
            {
                bool hasText = !string.IsNullOrEmpty(text);
                quantityText.gameObject.SetActive(hasText);
                if (hasText) quantityText.text = text;
            }
            _isEmpty = false;
        }
        public void Select()
        {
            if(borderImage != null)
                borderImage.enabled = true;
        }
        public void Deselect()
        {
            if(borderImage != null)
                borderImage.enabled = false;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if(_isEmpty) return;
            // [TODO] 우클릭/좌클릭 구분 추가
            OnItemClicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(_isEmpty) return;
            OnItemPointerEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(_isEmpty) return;
            OnItemPointerExit?.Invoke(this);
        }
    }
}
