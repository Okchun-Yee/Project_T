using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using ProjectT.Core;
using ProjectT.Gameplay.Player.Controller;

namespace ProjectT.Systems.Camera
{
    public class CameraController : Singleton<CameraController>
    {
        private CinemachineVirtualCamera cinemachineVirtualCamera;

        private void Start()
        {
            SetPlayerCameraFollow();
        }
        public void SetPlayerCameraFollow()
        {
            cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            cinemachineVirtualCamera.Follow = PlayerLegacyController.Instance.transform;
        }
    }
}