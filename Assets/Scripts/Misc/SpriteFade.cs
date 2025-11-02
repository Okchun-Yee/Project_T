using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFade : MonoBehaviour
{
    [SerializeField] private float fadeTime = .4f;
    private SpriteRenderer spriteRenderer;
    private BaseVFX parentVFX;

    // 외부에서 참조할 수 있는 읽기 전용 프로퍼티
    public float FadeTime => fadeTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        parentVFX = GetComponentInParent<BaseVFX>();
    }

    public IEnumerator SlowFadeRoutine()
    {
        float timePassed = 0f;
        float startValue = spriteRenderer.color.a;

        while (timePassed < fadeTime)
        {
            timePassed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, 0f, timePassed / fadeTime);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);
            yield return null;
        }

        // 페이드 종료: VFX 루트가 있으면 BaseVFX에 정리 위임, 없으면 스프라이트만 제거
        if (parentVFX != null)
        {
            parentVFX.OnVFXDestroyed();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
