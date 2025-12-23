using System;
using System.Collections;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.StatsModifiers;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.Items
{
    /// <summary>
    /// 아이템의 기본 속성들을 정의하는 추상 클래스
    /// 사용 아이템, 장착 아이템 유형의 기반
    /// </summary>
    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField] public bool IsStackable { get; set; } // 아이템 개수 스택
        public int ID => GetInstanceID();
        [field: SerializeField] public int MaxStackSize { get; set; } = 1; // 최대 개수
        [field: SerializeField] public string Name { get; set; } // 이름
        [field: SerializeField] [field : TextArea] public string Description { get; set; } // 설명
        [field: SerializeField] public Sprite ItemImage { get; set; } //이미지
        [field: SerializeField] public List<ItemParameter> DefaultParametersList { get; set; } // 아이템 파라미터
    }
    [Serializable] 
    public struct ItemParameter : IEquatable<ItemParameter>
    {
        public ItemParameterSO itemParameter;
        public float value;
        public bool Equals(ItemParameter other)
        {
            return itemParameter == other.itemParameter;
        }
    }
}

