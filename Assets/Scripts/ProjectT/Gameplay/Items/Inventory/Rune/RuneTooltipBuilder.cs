using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Items.Runes;
using ProjectT.Systems.UI;
using UnityEngine;

namespace ProjectT.Gameplay.Items.Inventory.Rune
{
    /// <summary>
    /// RuneSO → TooltipData 변환 전담 (Rune 시스템 전용)
    /// - Decision 계층: "룬의 어떤 정보를 툴팁에 표시할지" 결정
    /// - TooltipData만 반환 (SO는 외부로 노출 안 함)
    /// - 각 시스템(Item, Skill 등)은 자신의 Builder 클래스를 가짐
    /// </summary>
    public static class RuneTooltipBuilder
    {
        /// <summary>
        /// RuneSO를 TooltipData로 변환
        /// </summary>
        /// <param name="rune">변환할 룬 데이터</param>
        /// <returns>툴팁에 표시할 데이터 (null인 경우 Empty 반환)</returns>
        public static TooltipData Build(RuneSO rune)
        {
            if (rune == null)
                return TooltipData.Empty;

            string additionalInfo = FormatModifiers(rune);

            return new TooltipData(
                rune.Icon,
                rune.RuneName,
                rune.Description,
                additionalInfo
            );
        }

        /// <summary>
        /// 룬의 수정자(Modifier)를 문자열로 포맷팅
        /// - 상위 2개 수정자만 표시 (툴팁의 공간 제약)
        /// - 형식: "수정자이름 +값" (줄바꿈으로 구분)
        /// </summary>
        private static string FormatModifiers(RuneSO rune)
        {
            if (rune.Modifiers == null || rune.Modifiers.Count == 0)
                return "";

            var lines = new List<string>();

            // 상위 2개만 표시 (그 이상은 Description 패널에서 확인)
            int displayCount = Mathf.Min(2, rune.Modifiers.Count);

            for (int i = 0; i < displayCount; i++)
            {
                var entry = rune.Modifiers[i];
                if (entry.modifier != null)
                {
                    lines.Add($"{entry.modifier.name} +{entry.value}");
                }
            }

            // 3개 이상이면 "등"으로 표시
            if (rune.Modifiers.Count > 2)
            {
                lines.Add($"... 외 {rune.Modifiers.Count - 2}개");
            }

            return string.Join("\n", lines);
        }
    }
}
