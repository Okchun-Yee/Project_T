using Cinemachine;
using ProjectT.Core;
using UnityEngine;

namespace ProjectT.Systems.Camera
{
    public class CameraSystem : Singleton<CameraSystem>
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachineConfiner2D confiner;

        protected override void Awake()
        {
            base.Awake();
            BindReferencesIfNeeded();
        }

        private void BindReferencesIfNeeded()
        {
            if (virtualCamera == null)
            {
                virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);
            }

            if (confiner == null)
            {
                confiner = GetComponentInChildren<CinemachineConfiner2D>(true);
            }
        }

        public void BindFollow(Transform target)
        {
            BindReferencesIfNeeded();
            if (virtualCamera == null) return;
            virtualCamera.Follow = target;
        }

        public void BindLookAt(Transform target)
        {
            BindReferencesIfNeeded();
            if (virtualCamera == null) return;
            virtualCamera.LookAt = target;
        }

        public void SetConfiner(Collider2D shape)
        {
            BindReferencesIfNeeded();
            if (confiner == null) return;
            confiner.m_BoundingShape2D = shape;
            confiner.InvalidateCache();
        }
    }
}
