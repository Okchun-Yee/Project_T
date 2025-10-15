using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCam;
    [SerializeField] private InventoryItem item;

    public void Awake()
    {
        canvas = transform.root.GetComponent<Canvas>();
        mainCam = Camera.main;
        item = GetComponentInChildren<InventoryItem>();
    }

    public void SetData(Sprite sprite, int quantity)
    {
        item.SetData(sprite, quantity);
    }
}
