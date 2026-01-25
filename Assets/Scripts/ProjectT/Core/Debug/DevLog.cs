using System.Diagnostics;
using UnityEngine;

namespace ProjectT.Core.Debug
{
    public static class DevLogChannels
    {
        public const string PlayerInput = "Player.Input";
        public const string PlayerFsm = "Player.FSM";
        public const string PlayerBinder = "Player.Binder";
        public const string PlayerRuntime = "Player.Runtime";
        public const string Weapon = "Weapon";
    }

    public static class DevLog
    {
        private const string ConfigPath = "ProjectT/Dev/DevLogConfig";

        private static DevLogConfigSO _config;
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string channel, string message, Object context = null)
        {
            if (!IsEnabled(channel)) return;

            string formatted = $"[PT][{channel}][Frame:{Time.frameCount}] {message}";
            if (context != null)
            {
                UnityEngine.Debug.Log(formatted, context);
            }
            else
            {
                UnityEngine.Debug.Log(formatted);
            }
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void RefreshConfig()
        {
            _config = null;
        }

        private static bool IsEnabled(string channel)
        {
            DevLogConfigSO cfg = GetConfig();
            if (cfg == null || !cfg.EnableGlobal) return false;
            if (string.IsNullOrEmpty(channel)) return false;

            if (cfg.ChannelToggles == null)
            {
                return false;
            }

            foreach (DevLogChannelToggle toggle in cfg.ChannelToggles)
            {
                if (!string.IsNullOrEmpty(toggle.Channel) && toggle.Channel == channel)
                {
                    return toggle.Enabled;
                }
            }
            return false;
        }

        private static DevLogConfigSO GetConfig()
        {
            if (_config != null) return _config;
            _config = Resources.Load<DevLogConfigSO>(ConfigPath);
            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<DevLogConfigSO>();
            }
            return _config;
        }
    }
}
