using System;
using System.Collections.Generic;
using ProjectT.Core.Debug;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Player.Input;
using TMPro;
using UnityEngine;

namespace ProjectT.Systems.UI
{
    public class MenuRootController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject menuTitleText;
        [SerializeField] private GameObject optionsPanel;

        [Header("Startup")]
        [SerializeField] private bool hideOnAwake = true;

        public bool IsOpen { get; private set; } = false;

        private bool _isBound = false;
        private Stack<GameObject> _panelStack = new Stack<GameObject>();

        private void Start()
        {
            if (hideOnAwake)
            {
                ForceHideWithoutPause();
            }
        }

        private void OnEnable()
        {
            InputManager.Ready += TryBindInput;
            if (InputManager.Instance != null)
            {
                TryBindInput();
            }
        }

        private void OnDisable()
        {
            InputManager.Ready -= TryBindInput;
            UnbindInputEvents();
        }

        private void TryBindInput()
        {
            if (_isBound) return;
            if (InputManager.Instance == null)
            {
                Debug.LogError("[MenuRootController] InputManager.Instance is null");
                return;
            }

            BindInputEvents();
        }

        private void BindInputEvents()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMenuInput += HandleMenu;
                _isBound = true;
            }
        }

        private void UnbindInputEvents()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMenuInput -= HandleMenu;
                _isBound = false;
            }
        }

        private void HandleMenu()
        {
            if (!IsOpen)
            {
                Open();
            }
            else if (_panelStack.Count > 1)
            {
                Back();
            }
            else
            {
                Close();
            }
        }

        private void ForceHideWithoutPause()
        {
            IsOpen = false;
            CloseAllPanels();
            _panelStack.Clear();
        }

        private void CloseAllPanels()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);
            if (menuTitleText != null)
                menuTitleText.SetActive(false);
            if (optionsPanel != null)
                optionsPanel.SetActive(false);
        }

        public void Open()
        {
            IsOpen = true;
            CloseAllPanels();
            _panelStack.Clear();

            _panelStack.Push(mainMenuPanel);
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
            if (menuTitleText != null)
                menuTitleText.SetActive(true);

            SetPaused(true);
        }

        public void OpenOptionsMenu()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(false);

            _panelStack.Push(optionsPanel);
            if (optionsPanel != null)
                optionsPanel.SetActive(true);
        }

        public void Back()
        {
            if (_panelStack.Count > 0)
                _panelStack.Pop().SetActive(false);

            if (_panelStack.Count > 0)
            {
                _panelStack.Peek().SetActive(true);
            }
            else
            {
                Close();
            }
        }

        public void Close()
        {
            IsOpen = false;
            CloseAllPanels();
            _panelStack.Clear();
            SetPaused(false);
        }

        private void SetPaused(bool paused)
        {
            PlayerController pc = PlayerController.Instance;
            if (pc != null)
            {
                pc.SetPaused(paused);
                DevLog.Log(DevLogChannels.PlayerRuntime, 
                    paused ? "[MenuRootController] Game Paused" : "[MenuRootController] Game Resumed");
            }
        }
    }
}
