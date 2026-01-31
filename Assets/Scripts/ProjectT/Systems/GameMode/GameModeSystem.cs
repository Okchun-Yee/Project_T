using System;
using ProjectT.Core;
using UnityEngine;

namespace ProjectT.Systems.GameMode
{
    public enum GameModeList
    {
        Gameplay = 0,
        Town = 1,
        Menu = 2
    }

    public class GameModeSystem : Singleton<GameModeSystem>
    {
        public event Action<GameModeList> OnModeChanged;
        [SerializeField] private GameModeList currentMode = GameModeList.Gameplay;

        public GameModeList CurrentMode => currentMode;
        public bool IsGameplay => currentMode == GameModeList.Gameplay;

        public void SetMode(GameModeList mode)
        {
            if (currentMode == mode) return;
            currentMode = mode;
            OnModeChanged?.Invoke(currentMode);
        }
    }
}
