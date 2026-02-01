using UnityEngine;

namespace ProjectT.Systems.Camera
{
    public class CameraConfinerProvider : MonoBehaviour
    {
        [SerializeField] private Collider2D confinerShape;

        public Collider2D GetConfiner()
        {
            if (confinerShape != null) return confinerShape;
            confinerShape = GetComponent<Collider2D>();
            return confinerShape;
        }
    }
}
