using UnityEngine;

namespace ProjectT.Gameplay.Weapon
{
    /// <summary>
    /// ActiveWeapon 회전 Z축 잠금 SSOT
    /// - 락 상태면 매 프레임 Z를 강제로 고정
    /// </summary>
    public class WeaponRotationLockController : MonoBehaviour
    {
        [SerializeField] private bool _isLocked;
        [SerializeField] private float _lockedZ = 0f;

        public bool IsLocked => _isLocked;

        public void LockZ(float z = 0f)
        {
            _isLocked = true;
            _lockedZ = z;
            ApplyLock();
        }

        public void UnlockZ()
        {
            _isLocked = false;
        }

        private void LateUpdate()
        {
            if (_isLocked)
            {
                ApplyLock();
            }
        }

        private void ApplyLock()
        {
            Vector3 euler = transform.localEulerAngles;
            if (!Mathf.Approximately(euler.z, _lockedZ))
            {
                euler.z = _lockedZ;
                transform.localEulerAngles = euler;
            }
        }
    }
}
