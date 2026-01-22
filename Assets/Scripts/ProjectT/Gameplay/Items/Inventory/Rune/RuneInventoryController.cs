using UnityEngine;
using System.Linq;
using ProjectT.Core;
using ProjectT.Data.ScriptableObjects.Items.Runes;
using ProjectT.Gameplay.Items.Inventory.UI;
using ProjectT.Gameplay.Items.Inventory;
using ProjectT.Data.ScriptableObjects.Inventory.Rune;
using ProjectT.Gameplay.Items.Execution;
using ProjectT.Systems.UI;
using System.Collections.Generic;

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
        [SerializeField] private RuneInventoryManager ui;
        [SerializeField] private InventoryRootController root;
        [SerializeField] private RuneInventorySO runeInventory;
        [SerializeField] private List<RuneSO> allRunes; // 전체 룬 풀 (에디터에서 할당)
        [SerializeField] private RuneSelectionPanel runeSelectionPanel;

        private Dictionary<int, int> recentAppearCount = new();
        private bool _isVisible = false;

        protected override void Awake()
        {
            base.Awake();
            ResetRuneStats();   // 룬 상태 초기화
            ValidateRunePool(); // 룬 풀 중복 검사
        }

        private void ResetRuneStats()
        {
            recentAppearCount.Clear();
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
        /// <summary>
        /// Rune 장착 진입점
        /// - 장착 실패 시 로그 출력
        /// </summary>
        public void TryEquip(int slotIndex, RuneSO rune)
        {
            if (runeInventory == null) return;

            bool ok = runeInventory.TryEquip(slotIndex, rune, out var reason);
            if (!ok)
            {
                Debug.LogWarning($"[Rune] Equip failed: {reason}");
            }
        }
        public void TryEquipAuto(RuneSO rune)
        {
            if (runeInventory == null) return;

            for (int i = 0; i < RuneInventorySO.MAX_SLOTS; ++i)
            {
                if (runeInventory.GetRuneAt(i) == null)
                {
                    bool ok = runeInventory.TryEquip(i, rune, out var reason);
                    if (!ok)
                    {
                        Debug.LogWarning($"[Rune] Equip failed: {reason}");
                    }
                    return;
                }
            }
            Debug.LogWarning("[Rune] All slots are full!");
        }

        #region Event Handlers
        // Root 통합 시: "룬 탭"을 enum에 추가했다는 전제로 처리해야 함
        private void HandleTabChanged(InventoryRootController.Tab tab)
        {
            if (tab == InventoryRootController.Tab.Rune)
            {
                _isVisible = true;
                RefreshUI();
            }
            else
            {
                _isVisible = false;
                // 탭 전환 시 툴팁 숨김
                if (ui != null)
                    ui.HideTooltip();
            }
        }

        private void HandleVisibilityChanged(bool isOpen)
        {
            _isVisible = isOpen;
            // UI 갱신은 HandleTabChanged에서만 수행 (중복 방지)
            
            // 인벤토리가 닫힐 때 툴팁 숨김
            if (!isOpen && ui != null)
                ui.HideTooltip();
        }

        private void HandleEquippedChanged()
        {
            if (!_isVisible) return;
            RefreshUI();
        }
        private void HandleSlotClicked(int slotIndex)
        {
            if (ui != null) ui.SelectSlot(slotIndex);

            // 선택된 룬 정보 표시
            var rune = runeInventory.GetRuneAt(slotIndex);
            
            if (rune == null)
            {
                if (ui != null)
                    ui.ResetRuneDescription();
            }
            else
            {
                if (ui != null)
                {
                    // RuneTooltipBuilder 공용 메서드 사용 (전체 Modifier 표시)
                    string effectsText = RuneTooltipBuilder.FormatModifiers(rune);
                    ui.SetRuneDescription(rune.Icon, rune.RuneName, rune.Description, effectsText);
                }
            }
        }

        private void HandleSlotHoverEnter(int slotIndex, Vector2 screenPos)
        {
            // SSOT 접근: 현재 슬롯의 룬 확인
            var rune = runeInventory.GetRuneAt(slotIndex);

            if (rune == null)
            {
                // 빈 슬롯 → 툴팁 숨김
                if (ui != null)
                    ui.HideTooltip();
            }
            else
            {
                // Decision: RuneSO → TooltipData 변환
                var tooltipData = RuneTooltipBuilder.Build(rune);
                
                // Execution: Manager가 UI 표시
                if (ui != null)
                    ui.ShowTooltip(tooltipData, screenPos);
            }
        }

        private void HandleSlotHoverExit(int slotIndex)
        {
            // 슬롯을 떠나면 툴팁 숨김
            if (ui != null)
                ui.HideTooltip();
        }
        #endregion

        #region Rune Selection Helpers
        private float GetWeight(RuneSO rune)
        {
            if (rune == null) return 0f;

            int count = recentAppearCount.TryGetValue(rune.ID, out var c) ? c : 0;
            return 1f / (1 + count); // 등장할수록 확률 감소
        }

        private List<RuneSO> SelectWithWeight(List<RuneSO> candidates, int count)
        {
            var result = new List<RuneSO>();
            var pool = new List<RuneSO>(candidates);

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                float totalWeight = pool.Sum(r => GetWeight(r));
                if (totalWeight <= 0) break;

                float pick = Random.value * totalWeight;
                float accumulated = 0f;

                RuneSO selected = null;
                foreach (var rune in pool)
                {
                    accumulated += GetWeight(rune);
                    if (pick <= accumulated)
                    {
                        selected = rune;
                        break;
                    }
                }

                if (selected != null)
                {
                    result.Add(selected);
                    pool.Remove(selected);

                    // 선택된 룬의 카운트 증가
                    recentAppearCount[selected.ID] =
                        (recentAppearCount.TryGetValue(selected.ID, out var c) ? c : 0) + 1;
                }
            }

            return result;
        }
        /// <summary>
        /// 룬 풀의 중복 ID 검사 및 경고 출력 (초가화 시점에 호출)
        /// </summary>
        private void ValidateRunePool()
        {
            if (allRunes == null) return;
            var duplicates = allRunes
                .Where(r => r != null)
                .GroupBy(r => r.ID)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
            {
                Debug.LogWarning(
            $"[Rune] Duplicate Rune IDs detected in allRunes: " +
            $"{string.Join(", ", duplicates)}\n" +
            "Rune IDs must be unique. This may cause unexpected selection behavior."
        );
            }
        }

        private List<RuneSO> GenerateRuneChoices()
        {
            // Null 체크
            if (runeInventory == null)
            {
                Debug.LogError("[Rune] RuneInventorySO not assigned!");
                return new List<RuneSO>();
            }

            if (allRunes == null || allRunes.Count == 0)
            {
                Debug.LogError("[Rune] allRunes not initialized or empty!");
                return new List<RuneSO>();
            }

            // 1. 현재 장착된 룬 ID 수집
            var equipped = runeInventory.GetEquippedSnapshot();
            var equippedIds = new HashSet<int>(
                equipped.Where(r => r != null).Select(r => r.ID)
            );

            // 2. 선택 가능한 후보 필터링
            var candidates = allRunes
                .Where(r => r != null && !equippedIds.Contains(r.ID))
                .ToList();

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[Rune] No available rune candidates " +
                    "(all slots equipped or no runes)");
                return new List<RuneSO>();
            }

            // 3. 가중치 기반 랜덤 선택 (최대 3개)
            var choices = SelectWithWeight(candidates, 3);

            Debug.Log($"[Rune] Generated {choices.Count} choices from {candidates.Count} candidates");
            return choices;
        }

        public void ShowRuneSelection()
        {
            if (runeSelectionPanel == null)
            {
                Debug.LogError("[Rune] RuneSelectionPanel not assigned!");
                return;
            }

            List<RuneSO> choices = GenerateRuneChoices();

            if (choices.Count == 0)
            {
                Debug.LogWarning("[Rune] No choices available to show");
                return;
            }

            runeSelectionPanel.Open(choices);
        }

        public void ResetRuneDescription()
        {
            if (ui != null)
                ui.ResetRuneDescription();
        }
        #endregion

#if UNITY_EDITOR
        [ContextMenu("Test: Show Rune Selection")]
        // 랜덤 선택지 생성 및 SelectionPanel 열기 테스트
        private void DebugShowRuneSelection()
        {
            ShowRuneSelection();
        }

        [ContextMenu("Test: Clear Recent Appear Count")]
        // 최근 등장 횟수 초기화 테스트
        private void DebugClearStats()
        {
            ResetRuneStats();
            Debug.Log("[Rune] Recent appear count cleared");
        }

        [ContextMenu("Test: Log Rune Pool")]
        // 현재 룬 풀 상태 출력 테스트
        private void DebugLogRunePool()
        {
            if (allRunes == null || allRunes.Count == 0)
            {
                Debug.LogWarning("[Rune] No runes in pool");
                return;
            }

            Debug.Log($"[Rune] Total runes in pool: {allRunes.Count}");
            foreach (var rune in allRunes)
            {
                if (rune != null)
                {
                    bool equipped = runeInventory.IsEquipped(rune.ID);
                    int appearCount = recentAppearCount.TryGetValue(rune.ID, out var c) ? c : 0;
                    float weight = GetWeight(rune);
                    Debug.Log($"  - {rune.name} (ID: {rune.ID}) | Equipped: {equipped} | Appear: {appearCount} | Weight: {weight:F3}");
                }
            }
        }
#endif
    }
}
