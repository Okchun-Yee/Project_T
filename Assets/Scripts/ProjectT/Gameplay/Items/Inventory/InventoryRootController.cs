using System;
using UnityEngine;
using UnityEngine.UI;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Player.Input;
using ProjectT.Gameplay.Items.Execution;

namespace ProjectT.Gameplay.Items.Inventory.UI
{
    public sealed class InventoryRootController : MonoBehaviour
    {
        public enum Tab { Default = 0, New = 1 }

        [Header("Root")]
        [SerializeField] private GameObject rootPanel;

        [Header("Views")]
        [SerializeField] private GameObject defaultInventoryPanel;
        [SerializeField] private GameObject newInventoryPanel;

        [Header("Controllers")]
        [SerializeField] private InventoryController defaultInventoryController;
        [SerializeField] private InventoryManager defaultInventoryManager;
        [SerializeField] private PlayerController playerController;

        [Header("NavBar Buttons")]
        [SerializeField] private Button defaultTabButton;
        [SerializeField] private Button newTabButton;
        [SerializeField] private Button closeButton;

        [Header("Startup")]
        [SerializeField] private bool hideOnAwake = true;
        [SerializeField] private Tab startupTab = Tab.Default;

        public event Action<bool> OnInventoryVisibilityChanged;
        public event Action<Tab> OnTabChanged;

        public bool IsOpen { get; private set; } = false;  // 실제 인벤토리 열기 상태
        public Tab CurrentTab { get; private set; } = Tab.Default;

        private bool _isBound = false;

        private void Awake()
        {
            if (rootPanel == null) rootPanel = gameObject;
        }

        private void Start()
        {
            if (hideOnAwake)
            {
                ForceHideWithoutPause();
            }
        }

        private void OnEnable()
        {
            Debug.Log("[InventoryRootController] OnEnable");
            InputManager.Ready += TryBindInput;
            
            if (InputManager.Instance != null)
            {
                TryBindInput();
            }

            // BindNavBarButtons();     // 주석 처리: 네비게이션 바 버튼 사용 안 함

            if (!hideOnAwake)
            {
                Open(startupTab);
            }
        }

        private void OnDisable()
        {
            InputManager.Ready -= TryBindInput;
            UnbindInputEvents();
            // UnbindNavBarButtons();   // 주석 처리: 네비게이션 바 버튼 사용 안 함
        }

        private void TryBindInput()
        {
            if (_isBound) return;
            if (InputManager.Instance == null)
            {
                Debug.LogError("[InventoryRootController] InputManager.Instance is null");
                return;
            }

            BindInputEvents();
        }

        private void BindInputEvents()
        {
            if (InputManager.Instance != null)
            {
                Debug.Log("[InventoryRootController] Binding OnInventoryInput");
                InputManager.Instance.OnInventoryInput += OnInventoryInput;
                _isBound = true;
            }
        }

        private void UnbindInputEvents()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInventoryInput -= OnInventoryInput;
                _isBound = false;
            }
        }

        /// <summary>
        /// Tab 입력 시 인벤토리 토글 또는 탭 전환 (열려있으면 탭 전환)
        /// </summary>
        private void OnInventoryInput()
        {
            Debug.Log("[InventoryRootController] OnInventoryInput called");
            if (!IsOpen)
            {
                Open(startupTab);
            }
            else
            {
                ToggleTab();  // 이미 열려있으면 탭 전환
            }
        }

        #region  UI Nav btn handlers
        private void BindNavBarButtons()
        {
            if (defaultTabButton != null)
                defaultTabButton.onClick.AddListener(ShowDefaultTab);
            if (newTabButton != null)
                newTabButton.onClick.AddListener(ShowNewTab);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        private void UnbindNavBarButtons()
        {
            if (defaultTabButton != null)
                defaultTabButton.onClick.RemoveListener(ShowDefaultTab);
            if (newTabButton != null)
                newTabButton.onClick.RemoveListener(ShowNewTab);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
        }
        #endregion

        /// <summary>
        /// 초기 숨김 (Pause 건드리지 않음)
        /// </summary>
        private void ForceHideWithoutPause()
        {
            IsOpen = false;
            rootPanel.SetActive(false);
            SetViewActive(defaultInventoryPanel, false);
            SetViewActive(newInventoryPanel, false);
        }

        /// <summary>
        /// 인벤토리 열기: Root 활성화 → Tab 전환 → Pause 진입
        /// </summary>
        public void Open(Tab tab = Tab.Default)
        {
            IsOpen = true;

            if (rootPanel != null)
                rootPanel.SetActive(true);

            SwitchTo(tab);

            if (playerController != null)
            {
                playerController.SetPaused(true);
            }

            OnInventoryVisibilityChanged?.Invoke(true);
        }

        /// <summary>
        /// 인벤토리 닫기: Root 숨김 → Pause 해제
        /// </summary>
        public void Close()
        {
            IsOpen = false;

            rootPanel.SetActive(false);

            if (defaultInventoryManager != null)
                defaultInventoryManager.ResetSelection();

            SetViewActive(defaultInventoryPanel, false);
            SetViewActive(newInventoryPanel, false);

            if (playerController != null)
            {
                playerController.SetPaused(false);
            }

            OnInventoryVisibilityChanged?.Invoke(false);
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open(startupTab);
        }

        public void ToggleTab()
        {
            var next = (CurrentTab == Tab.Default) ? Tab.New : Tab.Default;
            SwitchTo(next);
        }

        public void ShowDefaultTab()
        {
            SwitchTo(Tab.Default);
        }

        public void ShowNewTab()
        {
            SwitchTo(Tab.New);
        }

        /// <summary>
        /// Tab 전환: View 활성화/비활성화 → 이벤트 발행 (각 컨트롤러가 구독)
        /// </summary>
        public void SwitchTo(Tab tab)
        {
            if (!IsOpen)
            {
                Open(tab);
                return;
            }

            CurrentTab = tab;

            SetViewActive(defaultInventoryPanel, tab == Tab.Default);
            SetViewActive(newInventoryPanel, tab == Tab.New);

            if (tab == Tab.Default && defaultInventoryManager != null)
            {
                defaultInventoryManager.ResetSelection();
            }

            OnTabChanged?.Invoke(tab);
        }

        private static void SetViewActive(GameObject go, bool active)
        {
            if (go == null) return;
            if (go.activeSelf == active) return;
            go.SetActive(active);
        }
    }
}
