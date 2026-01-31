using UnityEngine;

namespace ProjectT.Systems.Scene
{
    public class EntryPointMarker : MonoBehaviour
    {
        [SerializeField] private string entryId;
        public string EntryId => entryId;
    }
}
