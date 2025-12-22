using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.StatsModifiers
{
    [CreateAssetMenu(menuName = "New ItemParameter")]
    public class ItemParameterSO : ScriptableObject
    {
        [field: SerializeField] public string parameterName {get; private set;}
    }
}
