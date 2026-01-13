using UnityEngine;
using ProjectT.Core;
using ProjectT.Data.ScriptableObjects.Items.Runes;
using ProjectT.Gameplay.Items.Inventory.UI;
using ProjectT.Data.ScriptableObjects.Inventory.Rune;
using ProjectT.Gameplay.Items.Execution;

namespace ProjectT.Gameplay.Items.Inventory.Rune
{
    /// <summary>
    /// RuneInventoryController (A안: 장착 슬롯 3개만 표시)
    /// 책임:
    /// - RuneInventorySO(SSOT) 상태를 룬 UI(3슬롯)에 반영
    /// - Root(탭/가시성) 이벤트를 받아 "보일 때만" Refresh
    /// - 슬롯 클릭/호버 이벤트 처리(현재는 임시 테스트 장착 흐름 포함)
    /// </summary>
    public sealed class RuneInventoryController : Singleton<RuneInventoryController>
    {
        [Header("Data")]
        [SerializeField] private RuneInventorySO runeInventory;

        [Header("UI")]
        [SerializeField] private RuneInventoryManager ui;
        [SerializeField] private InventoryRootController root; // 현재 Root와 통합되어 있으면 연결

        [Header("Debug")]
        [SerializeField] private bool enableDebugEquipFlow = true;
        [SerializeField] private RuneSO[] debugRunes; // 인스펙터에 2~3개 넣고 클릭으로 순환 장착

        private bool _isVisible = false;
        private int _debugIndex = 0;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            if (runeInventory != null)
                runeInventory.Initialize();

            if (ui != null)
            {
                ui.OnSlotClicked += HandleSlotClicked;
                ui.OnSlotHoverEnter += HandleSlotHoverEnter;
                ui.OnSlotHoverExit += HandleSlotHoverExit;
            }

            if (runeInventory != null)
                runeInventory.OnEquippedChanged += HandleEquippedChanged;

            // 시작 시점엔 UI가 닫혀 있을 수 있으니 Refresh는 visible에서만
        }

        private void OnEnable()
        {
            SubscribeRootEvents();
        }

        private void OnDisable()
        {
            UnsubscribeRootEvents();
        }

        private void OnDestroy()
        {
            if (ui != null)
            {
                ui.OnSlotClicked -= HandleSlotClicked;
                ui.OnSlotHoverEnter -= HandleSlotHoverEnter;
                ui.OnSlotHoverExit -= HandleSlotHoverExit;
            }

            if (runeInventory != null)
                runeInventory.OnEquippedChanged -= HandleEquippedChanged;
        }

        private void SubscribeRootEvents()
        {
            if (root == null) return;
            root.OnTabChanged += HandleTabChanged;
            root.OnInventoryVisibilityChanged += HandleVisibilityChanged;
        }

        private void UnsubscribeRootEvents()
        {
            if (root == null) return;
            root.OnTabChanged -= HandleTabChanged;
            root.OnInventoryVisibilityChanged -= HandleVisibilityChanged;
        }

        // Root 통합 시: "룬 탭"을 enum에 추가했다는 전제로 처리해야 함
        private void HandleTabChanged(InventoryRootController.Tab tab)
        {
            // TODO: Root의 Tab에 Rune가 추가되면 아래 조건을 tab == Tab.Rune으로 변경
            // 지금은 A안 스켈레톤이므로, 네 프로젝트에서 Rune 탭 enum 확정 후 수정
        }

        private void HandleVisibilityChanged(bool isOpen)
        {
            _isVisible = isOpen;
            if (_isVisible)
                RefreshUI();
        }

        private void HandleEquippedChanged()
        {
            if (!_isVisible) return;
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (ui == null || runeInventory == null) return;

            ui.ResetAll();

            var snapshot = runeInventory.GetEquippedSnapshot(); // 3개
            for (int i = 0; i < snapshot.Count; i++)
            {
                var rune = snapshot[i];
                if (rune == null) continue;

                ui.SetSlot(i, rune.Icon);
            }
        }

        private void HandleSlotClicked(int slotIndex)
        {
            if (ui != null) ui.SelectSlot(slotIndex);

            if (!enableDebugEquipFlow) return;
            if (runeInventory == null) return;
            if (debugRunes == null || debugRunes.Length == 0) return;

            // 임시: 클릭할 때마다 debugRunes를 순환하며 장착 시도
            var rune = debugRunes[_debugIndex % debugRunes.Length];
            _debugIndex++;

            var ok = runeInventory.TryEquip(slotIndex, rune, out var reason);
            if (!ok)
            {
                Debug.LogWarning($"[RuneInventoryController] Equip failed: slot={slotIndex}, rune={rune?.name}, reason={reason}");
            }
        }

        private void HandleSlotHoverEnter(int slotIndex)
        {
            // TODO: 툴팁 연결 시 여기서 rune 정보 표시
        }

        private void HandleSlotHoverExit(int slotIndex)
        {
            // TODO: 툴팁 숨김
        }
    }
}
