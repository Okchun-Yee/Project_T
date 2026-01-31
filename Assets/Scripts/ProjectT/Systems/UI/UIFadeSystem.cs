using System.Collections;
using ProjectT.Core;
using UnityEngine;

namespace ProjectT.Systems.UI
{
    public class UIFadeSystem : Singleton<UIFadeSystem>
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.3f;

        protected override void Awake()
        {
            base.Awake();
            if (canvasGroup == null)
            {
                canvasGroup = GetComponentInChildren<CanvasGroup>(true);
            }
        }

        public IEnumerator FadeOut()
        {
            if (canvasGroup == null) yield break;
            yield return Fade(1f);
        }

        public IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;
            yield return Fade(0f);
        }

        private IEnumerator Fade(float targetAlpha)
        {
            float startAlpha = canvasGroup.alpha;
            if (Mathf.Approximately(startAlpha, targetAlpha))
            {
                canvasGroup.alpha = targetAlpha;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            canvasGroup.alpha = targetAlpha;
        }
    }
}
