using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectT.Systems.UI.Skill
{
    public class SkillSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;           // 스킬 아이콘 이미지 컴포넌트
        [SerializeField] private Image cooldownOverlay;     // 쿨다운 오버레이 이미지 컴포넌트
        [SerializeField] private GameObject disabledMask;   // 비활성화 마스크 게임 오브젝트

        public void SetEnabled(bool isEnabled)
        {
            gameObject.SetActive(true);
            if(disabledMask != null)
            {
                disabledMask.SetActive(!isEnabled);
            }
        }
        public void SetIcon(Sprite icon)
        {
            if(iconImage == null) return;
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
        /// <summary>
        /// 쿨다운 오버레이 업데이트 매서드
        /// </summary>
        public void SetCooldownOverlay(float t)
        {
            if(cooldownOverlay == null) return;
            cooldownOverlay.fillAmount = Mathf.Clamp01(t);
        }
    }
}
