using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectT.Systems.UI
{
    /// <summary>
    /// 모든 슬롯 시스템(Rune, Item, Skill 등)이 공유하는 Tooltip View
    /// - 순수 UI 표현만 담당 (Show/Hide, 위치 조정, 텍스트 업데이트)
    /// - SSOT/도메인 로직을 포함하지 않음
    /// - 재사용 가능한 공용 컴포넌트
    /// </summary>
    public class TooltipUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text additionalInfoText;   // 부가 정보 (효과, 파라미터, 쿨타임 등)

        [Header("Panel Settings")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform tooltipRect;

        [Header("Position Settings")]
        [SerializeField] private Vector2 positionOffset = new Vector2(10, -10);
        [SerializeField] private float screenPadding = 10f;

        private Canvas _rootCanvas;

        private void OnEnable()
        {
            _rootCanvas = GetComponentInParent<Canvas>();
            Hide();
        }

        /// <summary>
        /// 툴팁 표시
        /// </summary>
        public void Show(TooltipData data, Vector2 screenPos)
        {
            // UI 업데이트
            if (iconImage != null)
                iconImage.sprite = data.Icon;

            if (titleText != null)
                titleText.text = data.Title;

            if (descriptionText != null)
                descriptionText.text = data.Description;

            if (additionalInfoText != null)
                additionalInfoText.text = data.AdditionalInfo;

            // 위치 설정 및 클램핑
            SetPosition(screenPos);

            // 표시
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// 툴팁 숨김
        /// </summary>
        public void Hide()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 스크린 좌표에 맞춰 위치 설정 (화면 밖 방지)
        /// </summary>
        private void SetPosition(Vector2 screenPos)
        {
            if (tooltipRect == null || _rootCanvas == null)
                return;

            // 스크린 좌표 → 캔버스 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.transform as RectTransform,
                screenPos + positionOffset,
                _rootCanvas.worldCamera,
                out Vector2 localPos
            );

            tooltipRect.anchoredPosition = localPos;

            // 화면 밖 방지 (클램핑)
            ClampToScreen();
        }

        /// <summary>
        /// 툴팁이 화면 밖으로 나가지 않도록 조정
        /// </summary>
        private void ClampToScreen()
        {
            if (tooltipRect == null || _rootCanvas == null)
                return;

            //[TODO] 툴팁 업데이트 빈도를 줄이기(스로틀링)하거나, 레이아웃 계산을 더 효율적인 방식으로 처리하는 접근을 고려
            LayoutRebuilder.MarkLayoutForRebuild(tooltipRect);

            Vector3[] corners = new Vector3[4];
            tooltipRect.GetWorldCorners(corners);

            Canvas canvas = _rootCanvas;
            RectTransform canvasRect = canvas.transform as RectTransform;

            // 캔버스 범위
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            float minX = canvasCorners[0].x + screenPadding;
            float maxX = canvasCorners[2].x - screenPadding;
            float minY = canvasCorners[0].y + screenPadding;
            float maxY = canvasCorners[2].y - screenPadding;

            // 오른쪽 경계 초과 체크
            if (corners[2].x > maxX)
            {
                tooltipRect.anchoredPosition += Vector2.left * (corners[2].x - maxX);
            }

            // 왼쪽 경계 초과 체크
            if (corners[0].x < minX)
            {
                tooltipRect.anchoredPosition += Vector2.right * (minX - corners[0].x);
            }

            // 위쪽 경계 초과 체크
            if (corners[1].y > maxY)
            {
                tooltipRect.anchoredPosition += Vector2.down * (corners[1].y - maxY);
            }

            // 아래쪽 경계 초과 체크
            if (corners[0].y < minY)
            {
                tooltipRect.anchoredPosition += Vector2.up * (minY - corners[0].y);
            }
        }

        /// <summary>
        /// 위치 오프셋 설정 (마우스로부터의 거리)
        /// </summary>
        public void SetPositionOffset(Vector2 offset)
        {
            positionOffset = offset;
        }

        /// <summary>
        /// 화면 패딩 설정
        /// </summary>
        public void SetScreenPadding(float padding)
        {
            screenPadding = padding;
        }
    }
}
