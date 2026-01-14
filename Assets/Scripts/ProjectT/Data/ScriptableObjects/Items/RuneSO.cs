using System;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.StatsModifiers;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.Items.Runes
{
    /// <summary>
    /// RuneSO (스켈레톤)
    /// - 고유 ID 필드를 포함 (중복 장착 방지 기준)
    /// - TODO: 플레이어 영향(스탯/효과)은 이 SO에 선언적으로 담고, 적용은 별도 시스템에서 처리하는 것을 권장
    /// </summary>
    /// [TODO]: 룬의 종류가 세분화 된다면, abstract 상속 구조로 변경 고려
    [CreateAssetMenu(menuName = "New Rune")]
    public class RuneSO : ScriptableObject
    {
        [field: SerializeField] public int ID { get; private set; }

        [Header("UI")]
        [field: SerializeField] private string runeName;    // 이름
        [field: SerializeField, TextArea] private string description;   // 설명
        [field: SerializeField] private Sprite icon;
        [Header("Rune Effects")]
        [field: SerializeField] private List<RuneModifierEntry> modifiers = new();  // 룬이 제공하는 파라미터들

        //Parameters
        public string RuneName => runeName;
        public string Description => description;
        public Sprite Icon => icon;
        public IReadOnlyList<RuneModifierEntry> Modifiers => modifiers;
    
        // TODO: List<StatModifier> Modifiers; 같은 형태로 확장 가능
        [Serializable] public struct RuneModifierEntry
        {
            public CharacterStatModifierSO modifier;
            public float value;
        }
    }
}