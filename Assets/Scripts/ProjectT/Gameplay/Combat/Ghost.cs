using System.Collections;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [Header("Ghost Effect Settings")]
    [SerializeField] private GameObject ghostTemplate; // 잔상 프리팹
    [SerializeField] private float ghostDelay = 0.05f;     // 잔상 생성 간격
    [SerializeField] private float ghostLifetime = 0.5f;   // 잔상 지속시간
    [SerializeField] private float ghostAlpha = 0.3f;      // 잔상 투명도
    
    private float ghostDelayTime;
    private bool makeGhost = false;
    private SpriteRenderer spriteRenderer;
    
    
    // 외부에서 잔상 효과 제어용 프로퍼티
    public bool MakeGhost 
    { 
        get { return makeGhost; } 
        set { makeGhost = value; }
    }
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ghostDelayTime = ghostDelay;
    }

    private void FixedUpdate()
    {
        if (makeGhost && spriteRenderer != null)
        {
            if (ghostDelayTime > 0)
            {
                ghostDelayTime -= Time.fixedDeltaTime;
            }
            else
            {
                CreateGhost();
                ghostDelayTime = ghostDelay;
            }
        }
    }
    
    private void CreateGhost()
    {   
        // 잔상 인스턴스 생성
        GameObject ghostInstance = Instantiate(ghostTemplate.gameObject);
        ghostInstance.transform.position = transform.position;
        ghostInstance.transform.rotation = transform.rotation;
        ghostInstance.transform.localScale = transform.localScale;
        
        // SpriteRenderer 설정 (이미 있다고 가정)
        SpriteRenderer ghostRenderer = ghostInstance.GetComponent<SpriteRenderer>();
        ghostRenderer.sprite = spriteRenderer.sprite;
        ghostRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        ghostRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        
        // 반투명 설정
        Color ghostColor = spriteRenderer.color;
        ghostColor.a = ghostAlpha;
        ghostRenderer.color = ghostColor;
        
        // 페이드아웃 시작
        StartCoroutine(FadeOutAndDestroy(ghostInstance, ghostRenderer));
    }
    
    private IEnumerator FadeOutAndDestroy(GameObject ghostObject, SpriteRenderer ghostRenderer)
    {
        float elapsed = 0f;
        Color startColor = ghostRenderer.color;
        
        while (elapsed < ghostLifetime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / ghostLifetime);
            ghostRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        if (ghostObject != null)
        {
            Destroy(ghostObject);
        }
    }
    
    // 잔상 효과를 특정 시간동안만 실행하는 메서드
    public void StartGhostEffect(float duration)
    {
        StartCoroutine(GhostEffectRoutine(duration));
    }
    
    private IEnumerator GhostEffectRoutine(float duration)
    {
        makeGhost = true;
        yield return new WaitForSeconds(duration);
        makeGhost = false;
    }
}
