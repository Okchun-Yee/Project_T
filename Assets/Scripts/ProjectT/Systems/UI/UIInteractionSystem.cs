using ProjectT.Core;
using UnityEngine;

namespace ProjectT.Systems.UI
{
    public class UIInteractionSystem : Singleton<UIInteractionSystem>
    {
        [SerializeField] private CanvasGroup canvasGroup;

        protected override void Awake()
        {
            base.Awake();
            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            }
        }

        public void SetInteractable(bool isInteractable)
        {
            if (canvasGroup == null) return;
            canvasGroup.interactable = isInteractable;
            canvasGroup.blocksRaycasts = isInteractable;
        }
    }
}
