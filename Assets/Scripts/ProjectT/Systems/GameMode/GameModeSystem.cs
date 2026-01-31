using System;
using ProjectT.Core;
using UnityEngine;

namespace ProjectT.Systems.GameMode
{
    public enum GameMode
    {
        Gameplay = 0,
        Town = 1,
        Menu = 2
    }

    public class GameModeSystem : Singleton<GameModeSystem>
    {
        public event Action<GameMode> OnModeChanged;
        [SerializeField] private GameMode currentMode = GameMode.Gameplay;

        public GameMode CurrentMode => currentMode;
        public bool IsGameplay => currentMode == GameMode.Gameplay;

        public void SetMode(GameMode mode)
        {
            if (currentMode == mode) return;
            currentMode = mode;
            OnModeChanged?.Invoke(currentMode);
        }
    }
}
