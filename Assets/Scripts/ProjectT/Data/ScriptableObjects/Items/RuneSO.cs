using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectT.Data.ScriptableObjects.Items.Runes
{
    /// <summary>
    /// RuneSO (스켈레톤)
    /// - 고유 ID 필드를 포함 (중복 장착 방지 기준)
    /// - TODO: 플레이어 영향(스탯/효과)은 이 SO에 선언적으로 담고, 적용은 별도 시스템에서 처리하는 것을 권장
    /// </summary>
    public abstract class RuneSO : ScriptableObject
    {
        [field: SerializeField] public int ID { get; private set; }

        [Header("UI")]
        [SerializeField] private string runeName;
        [SerializeField] private Sprite icon;

        public string RuneName => runeName;
        public Sprite Icon => icon;

        // TODO: List<StatModifier> Modifiers; 같은 형태로 확장 가능
    }
}