using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectT.Gameplay.Items.Inventory
{
    public class MouseFollower : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private InventoryItem item;

        public void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            item = GetComponentInChildren<InventoryItem>();
        }

        public void SetData(Sprite sprite, int quantity)
        {
            item.SetData(sprite, quantity);
        }
        void Update()
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform,
                Input.mousePosition,
                canvas.worldCamera,
                out position);
            transform.position = canvas.transform.TransformPoint(position);
        }
        public void Toggle(bool val)
        {
            gameObject.SetActive(val);
        }
    }
}
