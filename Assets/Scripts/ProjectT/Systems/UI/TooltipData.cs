using UnityEngine;

namespace ProjectT.Systems.UI
{
    /// <summary>
    /// 모든 슬롯 시스템(Rune, Item, Skill 등)이 공유하는 Tooltip 데이터
    /// - DTO(Data Transfer Object) 패턴
    /// - 시스템별 SO → TooltipData 변환은 각 Builder가 담당
    /// - View는 이 데이터만 받아 표시
    /// </summary>
    public readonly struct TooltipData
    {
        public Sprite Icon { get; }
        public string Title { get; }
        public string Description { get; }
        public string AdditionalInfo { get; }   // 부가 정보 (효과, 파라미터, 쿨타임 등)

        public TooltipData(Sprite icon, string title, string description, string additionalInfo = "")
        {
            Icon = icon;
            Title = title;
            Description = description;
            AdditionalInfo = additionalInfo;
        }

        /// <summary>
        /// 기본값 (모든 필드가 null/empty)
        /// </summary>
        public static TooltipData Empty => new TooltipData(null, "", "", "");
    }
}
