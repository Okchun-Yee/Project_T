using UnityEngine;

[System.Serializable]
public class OrbitSlot
{
    public Transform _pivot;      // Slot_*
    public Transform _visual;     // Visual_*
    [HideInInspector] public float _phaseDeg;
}