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
        [field: SerializeField] public AudioClip actionSFX {get; private set;}

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            AgentWeapon agentWeapon = character.GetComponent<AgentWeapon>();
            if (agentWeapon != null)
            {
                agentWeapon.SetWeapon(this, itemState == null ? 
                    DefaultParametersList : itemState);
                return true;
            }
            return false;
        }
    }
}
