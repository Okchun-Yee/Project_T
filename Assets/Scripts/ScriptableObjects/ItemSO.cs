using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class ItemSO : ScriptableObject
    {
        [field: SerializeField] public bool IsStackable { get; set; } // 아이템 개수 스택
        public int ID => GetInstanceID();
        [field: SerializeField] public int MaxStackSize { get; set; } = 1; // 최대 개수
        [field: SerializeField] public string Name { get; set; } // 이름
        [field: SerializeField] [field : TextArea] public string Description { get; set; } // 설명
        [field: SerializeField] public Sprite Itemimage { get; set; } //이미지
    }
}

