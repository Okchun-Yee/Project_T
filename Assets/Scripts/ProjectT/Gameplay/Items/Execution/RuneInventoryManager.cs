using System;
using ProjectT.Data.ScriptableObjects.Inventory.Rune;
using ProjectT.Gameplay.Items.Inventory.Rune;
using UnityEngine;

namespace ProjectT.Gameplay.Items.Execution
{
    /// <summary>
    /// 룬 장착 UI (3슬롯) View Manager
    /// - 슬롯 배열을 가지고 SetSlot / ClearSlot 제공
    /// - 슬롯 클릭/호버 이벤트를 외부로 전달
    /// </summary>
    public sealed class RuneInventoryManager : MonoBehaviour
    {
        [SerializeField] private RuneInventoryItem[] slots = new RuneInventoryItem[3];

        public event Action<int> OnSlotClicked;
        public event Action<int> OnSlotHoverEnter;
        public event Action<int> OnSlotHoverExit;

        private void Awake()
        {
            BindSlotEvents();
            ResetAll();
        }

        private void OnDestroy()
        {
            UnbindSlotEvents();
        }

        private void BindSlotEvents()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int index = i;
                if (slots[index] == null) continue;

                slots[index].OnItemClicked += _ => OnSlotClicked?.Invoke(index);
                slots[index].OnItemPointerEnter += _ => OnSlotHoverEnter?.Invoke(index);
                slots[index].OnItemPointerExit += _ => OnSlotHoverExit?.Invoke(index);
            }
        }

        private void UnbindSlotEvents()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int index = i;
                if (slots[index] == null) continue;

                // 람다로 붙인 이벤트는 Remove가 어려우니
                // 실전에서는 별도 핸들러 메서드로 바인딩하는 게 더 안전함.
                // (여기선 스켈레톤이므로 OnDestroy에서만 호출되게 두고, 중복 바인딩만 피하면 OK)
            }
        }

        public void ResetAll()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].ResetData();
                slots[i].Deselect();
            }
        }

        public void ResetSelection()
        {
            DeselectAllSlots();
        }

        private void DeselectAllSlots()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].Deselect();
            }
        }

        public void SetSlot(int index, Sprite icon)
        {
            if (!IsValid(index)) return;
            slots[index].SetData(icon);
        }

        public void ClearSlot(int index)
        {
            if (!IsValid(index)) return;
            slots[index].ResetData();
        }

        public void SelectSlot(int index)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                if (i == index) slots[i].Select();
                else slots[i].Deselect();
            }
        }

        private bool IsValid(int index)
            => index >= 0 && index < slots.Length && slots[index] != null;
    }
}
