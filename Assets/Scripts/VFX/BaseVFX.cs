using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 VFX의 기본 클래스
/// * VFX의 데미지, 지속시간, 초기화 등을 관리
/// * 파생 클래스에서 구체적인 VFX 동작 구현
/// </summary>
public abstract class BaseVFX : MonoBehaviour
{
    [Header("Base VFX Settings")]
    [SerializeField] protected float lifetime = 1f;     // VFX 지속 시간
    [SerializeField] protected bool autoDestroy = true; // VFX 종료 시 자동 파괴 여부

    protected DamageSource damageSource;                // VFX의 DamageSource 컴포넌트
    protected float assignedDamage;                     // VFX에 할당된 피해량 = 최종 데미지
    protected bool isInitialized = false;               // VFX 초기화 여부
    public bool IsInitialized => isInitialized;         // 초기화 여부 검사
    public float GetAssignedDamage() => assignedDamage; // 현재 설정한 데미지 반환

    [System.NonSerialized] private float scheduledDestroyAt = -1f;

    protected virtual void Awake()
    {
        damageSource = GetComponent<DamageSource>();    // VFX의 DamageSource 컴포넌트
    }
    protected virtual void Start()
    {
        if (!isInitialized)
        {
            Initialize(1f); // 기본 데미지 1로 초기화
        }
        if (autoDestroy && lifetime > 0f)
        {
            scheduledDestroyAt = Time.time + lifetime;
            Destroy(gameObject, lifetime); // VFX가 자동으로 파괴되도록 설정
        }
    }

    public virtual void Initialize(float damage)
    {
        assignedDamage = damage;
        isInitialized = true;

        damageSource?.SetDamage(assignedDamage); // DamageSource에 데미지 설정 (즉시 데미지 용도)

        // 각 VFX별 초기화 로직 실행
        OnVFXInitialized();
    }
    protected abstract void OnVFXInitialized();
    public virtual void OnVFXDestroyed()
    {
        if (autoDestroy)
        {
            Destroy(gameObject); // VFX 즉시 파괴
        }
        else
        {
            gameObject.SetActive(false); // VFX 비활성화
        }
    }
    public float GetLifetime()
    {
        return lifetime;
    }
}