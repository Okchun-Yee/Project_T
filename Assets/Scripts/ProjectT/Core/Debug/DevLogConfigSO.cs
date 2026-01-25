using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectT.Core.Debug
{
    [Serializable]
    public struct DevLogChannelToggle
    {
        public string Channel;
        public bool Enabled;
    }

    [CreateAssetMenu(fileName = "DevLogConfig", menuName = "ProjectT/Debug/DevLogConfig")]
    public sealed class DevLogConfigSO : ScriptableObject
    {
        public bool EnableGlobal = false;
        public List<DevLogChannelToggle> ChannelToggles = new List<DevLogChannelToggle>();
    }
}
