using UnityEngine;

namespace ProjectT.Gameplay.Weapon
{
    /// <summary>
    /// 애니메이션 이벤트용 릴레이 (Sword 등 Animator 오브젝트에 부착)
    /// </summary>
    public class WeaponRotationLockRelay : MonoBehaviour
    {
        private WeaponRotationLockController _controller;

        private void Awake()
        {
            _controller = GetComponentInParent<WeaponRotationLockController>();
            if (_controller == null)
            {
                Debug.LogWarning($"[WeaponRotationLockRelay] Controller not found in parents of {name}");
            }
        }

        // Animation Event
        public void AnimEvent_LockWeaponZ0()
        {
            _controller?.LockZ(0f);
        }

        // Animation Event
        public void AnimEvent_UnlockWeaponZ()
        {
            _controller?.UnlockZ();
        }
    }
}
