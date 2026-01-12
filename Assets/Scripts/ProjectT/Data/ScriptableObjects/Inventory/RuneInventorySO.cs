using System;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Items.Runes;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.Inventory.Rune
{
    /// <summary>
    /// RuneInventorySO
    /// - 전역 3슬롯 룬 "장착 상태" 저장소(SSOT)
    /// - 룬은 비스택(슬롯당 1개)
    /// - 슬롯은 총 3개 고정
    /// - 동일 RuneID 중복 장착 불가
    /// - 교체 허용: 기존 룬 제거 후 새 룬 장착
    /// - 런타임 제거 불가 (단, 에디터 테스트용 제거 API 제공)
    /// </summary>
    public enum EquipFailReason
    {
        None = 0,
        InvalidSlot = 1,
        NullRune = 2,
        DuplicateRuneAlreadyEquipped = 3
    }
    [CreateAssetMenu(menuName = "New Rune Inventory")]
    public sealed class RuneInventorySO : ScriptableObject
    {
        public const int MAX_SLOTS = 3;

        [SerializeField] private List<RuneSO> equippedRunes = new List<RuneSO>(MAX_SLOTS); // 슬롯 3개 고정
        public event Action OnEquippedChanged;  // 룬 장착 상태가 변경되었을 때 통지. UI가 필요하면 구독.

        private void OnEnable()
        {
            // 에디터에서 Play 모드 시작 시 인벤토리 초기화
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;
#endif

            Initialize();
#if UNITY_EDITOR
            for (int i = 0; i < MAX_SLOTS; ++i)
            {
                equippedRunes[i] = null;
            }
#endif
        }
        /// <summary>
        /// 런타임 안전 초기화: 슬롯 3개를 보장하고, 비어있으면 null로 채움
        /// </summary>
        public void Initialize()
        {
            if (equippedRunes == null) equippedRunes = new List<RuneSO>(MAX_SLOTS);
            if (equippedRunes.Count != MAX_SLOTS)
            {
                equippedRunes.Clear();
                for (int i = 0; i < MAX_SLOTS; ++i)
                {
                    equippedRunes.Add(null);
                }
            }
        }
        public int SlotCount => MAX_SLOTS;
        public RuneSO GetRuneAt(int slotIndex)
        {
            if (!IsValidSlot(slotIndex)) return null;
            return equippedRunes[slotIndex];
        }
        /// <summary>
        /// 현재 장착 스냅샷(복사본). 외부에서 리스트 변경으로 내부 상태를 건드리지 않게 방어.
        /// </summary>
        public IReadOnlyList<RuneSO> GetEquippedSnapshot()
        {
            // List 복사 후 IReadOnlyList로 반환
            return new List<RuneSO>(equippedRunes);
        }

        public bool IsEquipped(int runeId)
        {
            for (int i = 0; i < MAX_SLOTS; ++i)
            {
                var r = equippedRunes[i];
                if (r != null && r.ID == runeId)
                    return true;
            }
            return false;
        }
        public int FindSlotOf(int runeId)
        {
            for (int i = 0; i < MAX_SLOTS; ++i)
            {
                var r = equippedRunes[i];
                if (r != null && r.ID == runeId)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 룬 장착(교체 허용)
        /// - slotIndex에 기존 룬이 있으면 제거 후 새 룬 장착
        /// - 동일 ID 룬이 다른 슬롯에 이미 있으면 실패
        /// </summary>
        public bool TryEquip(int slotIndex, RuneSO rune, out EquipFailReason reason)
        {
            Initialize();

            if (!IsValidSlot(slotIndex))
            {
                reason = EquipFailReason.InvalidSlot;
                return false;
            }

            if (rune == null)
            {
                reason = EquipFailReason.NullRune;
                return false;
            }

            // 같은 룬 ID가 다른 슬롯에 있으면 중복 장착 불가
            int existingSlot = FindSlotOf(rune.ID);
            if (existingSlot != -1 && existingSlot != slotIndex)
            {
                reason = EquipFailReason.DuplicateRuneAlreadyEquipped;
                return false;
            }

            // 교체 허용: 기존 룬이 있으면 덮어쓰기(= 제거 후 장착과 동일)
            equippedRunes[slotIndex] = rune;

            reason = EquipFailReason.None;
            OnEquippedChanged?.Invoke();
            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터/테스트용 임시 제거 API (빌드에서는 컴파일되지 않음)
        /// </summary>
        public bool EditorTryUnequip(int slotIndex)
        {
            Initialize();

            if (!IsValidSlot(slotIndex))
                return false;

            if (equippedRunes[slotIndex] == null)
                return false;

            equippedRunes[slotIndex] = null;
            OnEquippedChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 에디터/테스트용: 전체 초기화
        /// </summary>
        public void EditorClearAll()
        {
            Initialize();
            for (int i = 0; i < MAX_SLOTS; ++i)
                equippedRunes[i] = null;
            OnEquippedChanged?.Invoke();
        }
#endif
        private static bool IsValidSlot(int slotIndex)
            => slotIndex >= 0 && slotIndex < MAX_SLOTS;
    }
}
