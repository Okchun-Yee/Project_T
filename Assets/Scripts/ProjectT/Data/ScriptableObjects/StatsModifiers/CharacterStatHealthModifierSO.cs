using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Player;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.StatsModifiers
{
    [CreateAssetMenu(menuName = "New HealthItem")]
    public class CharacterStatHealthModifierSO : CharacterStatModifierSO
    {
        public override void AffectCharacter(GameObject character, float value)
        {
            PlayerHealth playerHealth = character.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.HealPlayer((int)value);
            }
        }
    }
}
