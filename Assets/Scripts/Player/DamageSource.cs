using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private float damageAmount = 1f;
    private Coroutine continuousDamageCoroutine;

    public float DamageAmount
    {
        get => damageAmount;
        set => damageAmount = value;
    }

    // 초기화 메서드
    public void DamageInitialize(float damage)
    {
        SetDamage(damage);
    }

    // 외부에서 데미지 설정하는 메서드들
    public void SetDamage(float damage)
    {
        damageAmount = damage;
    }

    // 즉시 데미지 (특정 대상)
    public void DealInstantDamage(float damage, EnemyHealth target)
    {
        target?.TakeDamage(damage);
    }

    // 지속 데미지 (단일 대상)
    public void StartContinuousDamage(float damagePerTick, float interval, float duration, EnemyHealth target)
    {
        StartCoroutine(ContinuousDamageCoroutine(damagePerTick, interval, duration, target));
    }

    // 범위 지속 데미지
    public void StartAreaContinuousDamage(float damagePerTick, float interval, float duration, float radius, LayerMask targetLayers)
    {
        StopContinuousDamage();
        continuousDamageCoroutine = StartCoroutine(AreaContinuousDamageCoroutine(damagePerTick, interval, duration, radius, targetLayers));
    }

    // 지속 데미지 중지
    public void StopContinuousDamage()
    {
        if (continuousDamageCoroutine != null)
        {
            StopCoroutine(continuousDamageCoroutine);
            continuousDamageCoroutine = null;
        }
    }

    // 범위 데미지 (AoE)
    public void DealAreaDamage(float damage, float radius, Vector3? center = null)
    {
        Vector3 damageCenter = center ?? transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(damageCenter, radius);

        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                DealInstantDamage(damage, enemy);
            }
        }
    }

    // 단일 대상 지속 데미지 코루틴
    private IEnumerator ContinuousDamageCoroutine(float damagePerTick, float interval, float duration, EnemyHealth target)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && target != null)
        {
            DealInstantDamage(damagePerTick, target);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    // 범위 지속 데미지 코루틴
    private IEnumerator AreaContinuousDamageCoroutine(float damagePerTick, float interval, float duration, float radius, LayerMask targetLayers)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetLayers);
            
            foreach (var hit in hits)
            {
                EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    DealInstantDamage(damagePerTick, enemy);
                }
            }
            
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        
        continuousDamageCoroutine = null;
    }
}
