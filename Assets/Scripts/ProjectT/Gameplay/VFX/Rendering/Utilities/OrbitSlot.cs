using UnityEngine;

[System.Serializable]
public class OrbitSlot
{
    public Transform pivot;      // Slot_*
    public Transform visual;     // Visual_*
    [HideInInspector] public float phaseDeg;
}