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

            string additionalInfo = FormatModifiers(rune, maxCount: 2, showEllipsis: true);

            return new TooltipData(
                rune.Icon,
                rune.RuneName,
                rune.Description,
                additionalInfo
            );
        }

        /// <summary>
        /// 룬의 수정자(Modifier)를 문자열로 포맷팅 (공용 메서드)
        /// - 형식: "수정자이름 +값" (줄바꿈으로 구분)
        /// </summary>
        /// <param name="rune">룬 데이터</param>
        /// <param name="maxCount">최대 표시 개수 (0 이하면 전체 표시)</param>
        /// <param name="showEllipsis">maxCount 초과 시 "... 외 N개" 표시 여부</param>
        /// <returns>포맷된 문자열</returns>
        public static string FormatModifiers(RuneSO rune, int maxCount = 0, bool showEllipsis = false)
        {
            if (rune == null || rune.Modifiers == null || rune.Modifiers.Count == 0)
                return "";

            var lines = new List<string>();

            // maxCount가 0 이하면 전체 표시
            int displayCount = (maxCount > 0) 
                ? Mathf.Min(maxCount, rune.Modifiers.Count) 
                : rune.Modifiers.Count;

            for (int i = 0; i < displayCount; i++)
            {
                var entry = rune.Modifiers[i];
                if (entry.modifier != null)
                {
                    lines.Add($"{entry.modifier.name} +{entry.value}");
                }
            }

            // maxCount 초과 시 ellipsis 표시
            if (showEllipsis && maxCount > 0 && rune.Modifiers.Count > maxCount)
            {
                lines.Add($"... 외 {rune.Modifiers.Count - maxCount}개");
            }

            return string.Join("\n", lines);
        }
    }
}
