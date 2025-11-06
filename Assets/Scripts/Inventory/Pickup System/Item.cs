using System.Collections;
using System.Collections.Generic;
using Inventory.Model;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [field: SerializeField] public ItemSO InventoryItem { get; private set; }
    [field: SerializeField] public int Quantity { get; set; } = 1;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float duration = 0.3f;
    
}
