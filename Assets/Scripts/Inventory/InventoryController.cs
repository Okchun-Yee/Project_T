using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public int inventorySize = 5;
    void Start()
    {
        inventoryUI.InitializeInventoryUI(inventorySize);
    }
    [SerializeField] private InventoryManager inventoryUI;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryUI.isActiveAndEnabled == false)
            {
                inventoryUI.Show();
            }
            else
            {
                inventoryUI.Hide();
            }
        }
    }
}
