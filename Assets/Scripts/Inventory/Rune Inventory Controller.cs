using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Model;
using Inventory.UI;
using UnityEngine;
using UnityEngine.AI;


namespace Inventory
{
    public class RuneInventoryController : MonoBehaviour
    {
         [SerializeField] private InventoryManager runeInventoryUI;

        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnRuneInventoryInput += RuneInventoryInput;
            }
        }

        private void OnDisable() 
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnRuneInventoryInput -= RuneInventoryInput;
            }
        }
        private void RuneInventoryInput() // 인벤토리 인풋
        {
            if (!runeInventoryUI.isActiveAndEnabled)
            {
                runeInventoryUI.Show();
                // foreach (var item in inventoryData.GetCurrentInventoryState()) // 모든 아이템 탐색
                // {
                //     runeInventoryUI.UpdateData(item.Key, item.Value.item.Itemimage, item.Value.quantity); // key는 인덱스

                // }
            }
            else
            {
                runeInventoryUI.Hide();
            }
        }
    }
}
