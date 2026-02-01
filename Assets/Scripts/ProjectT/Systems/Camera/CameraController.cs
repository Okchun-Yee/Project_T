using UnityEngine;
using ProjectT.Core;
using ProjectT.Gameplay.Player.Controller;

namespace ProjectT.Systems.Camera
{
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField] private Transform followTarget;

        private void Start()
        {
            SetPlayerCameraFollow();
        }
        public void SetPlayerCameraFollow()
        {
            if (followTarget == null && PlayerMovementExecution.Instance != null)
            {
                followTarget = PlayerMovementExecution.Instance.transform;
            }
            
            if (followTarget != null)
            {
                CameraSystem.Instance?.BindFollow(followTarget);
            }
        }
    }
}
