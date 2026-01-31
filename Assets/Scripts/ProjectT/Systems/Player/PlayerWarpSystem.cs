using ProjectT.Core;
using ProjectT.Gameplay.Player.Controller;
using UnityEngine;

namespace ProjectT.Systems.Player
{
    public class PlayerWarpSystem : Singleton<PlayerWarpSystem>
    {
        public void WarpTo(Transform entry)
        {
            if (entry == null) return;
            WarpTo(entry.position);
        }

        public void WarpTo(Vector3 position)
        {
            if (PlayerMovementExecution.Instance == null) return;

            var playerTransform = PlayerMovementExecution.Instance.transform;
            var rb = PlayerMovementExecution.Instance.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.position = position;
            }
            playerTransform.position = position;
        }
    }
}
