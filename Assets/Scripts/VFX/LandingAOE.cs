using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 포물선형 투사체가 착지한 후 생성되는 범위 지속 데미지 장판
/// - Player / Enemy 공용
/// - DamageSource를 통해 ADoT(AreaContinuousDamage) 호출
/// - SpriteFade, Collider2D를 이용한 연출 및 수명 제어
/// </summary>
public class LandingAOE : BaseVFX
{
    [Header("Damage Settings")]
    [SerializeField] private float damagePerTick = 1f;   // 틱당 피해량
    [SerializeField] private float tickInterval = 1f;    // 데미지 간격(초)
    [SerializeField] private float totalDuration = 5f;   // 전체 지속시간(초)
    [SerializeField] private float radius = 1f;          // 범위 반경
    [SerializeField] private LayerMask targetLayers;     // 타격 대상 레이어 (Enemy / Player)

    [Header("Timing & Lifetime")]
    [SerializeField] private float startDelay = 0f;               // 착지 후 데미지 시작 지연
    [SerializeField] private bool colLifeTime = true; // 지속 시간 동안만 콜라이더 활성
    [SerializeField] private bool destroyOnEnd = true;            // 종료 후 오브젝트 파괴 여부


    private SpriteFade spriteFader;
    private Collider2D col;
    private Coroutine damageCoroutine;

    protected override void Awake()
    {
        base.Awake();
        spriteFader = GetComponent<SpriteFade>();
        col = GetComponent<CapsuleCollider2D>();
    }

    protected override void Start()
    {
        base.Start();

        // 시각적 효과 시작
        if (spriteFader != null)
        {
            StartCoroutine(spriteFader.SlowFadeRoutine());
        }
        if(col != null && colLifeTime)
        {
            col.enabled = false;
        }
    }

    protected override void OnVFXInitialized()
    {
        // === 지속 데미지 시작 ===
        if (assignedDamage > 0f)
        {
            damagePerTick = assignedDamage;     // BaseVFX에서 설정한 데미지로 덮어쓰기
        }
        damageCoroutine = StartCoroutine(AoESequence());
    }
    private IEnumerator AoESequence()
    {
        // 시작 지연
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        // 콜라이더 활성화
        if (col != null && colLifeTime)
        {
            col.enabled = true;
        }

        // 지속 데미지 적용
        DamageSource damageSource = GetComponent<DamageSource>();
        if (damageSource != null)
        {
            damageSource.ADoTDamage(damagePerTick, tickInterval, totalDuration, radius, targetLayers);
        }
        // 전체 지속 시간 대기
        yield return new WaitForSeconds(totalDuration);
        // 콜라이더 비활성화
        if (col != null && colLifeTime)
        {
            col.enabled = false;
        }
        // 종료 후 처리
        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        // 오브젝트 파괴 시 지속 데미지 정리
        if (damageSource != null)
        {
            damageSource.StopContinuousDamage();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 데미지 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
