using System.Collections;
using System.Collections.Generic;
using Inventory.Model;
using UnityEngine;
namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "New EquippableItem")]
    public class EquippableItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Equip";

        public AudioClip actionSFX {get; private set;}

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
