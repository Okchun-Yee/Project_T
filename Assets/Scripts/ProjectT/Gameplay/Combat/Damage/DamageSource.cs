using System;
using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Enemies;
using ProjectT.Gameplay.Player;
using UnityEngine;

/// <summary>
/// DamageSource
/// 4가지 데미지 타입을 관리하는 클래스
/// 1) 즉시 데미지
/// 2) DoT = 지속 데미지 (단일 대상)
/// 3) ADoT = 지속 데미지 (범위 대상)   *Player / Enemy 공용
/// 4) 범위 즉발 데미지 (AoE)           *Player / Enemy 공용
/// </summary>
namespace ProjectT.Gameplay.Combat.Damage
{
    public class DamageSource : MonoBehaviour
    {
        [SerializeField] private float damageAmount = 1f;
        private Coroutine singleContinuousCoroutine;        // 단일 대상 지속 데미지 코루틴
        private Coroutine areaContinuousCoroutine;        // 범위 지속 데미지 코루틴

        // 외부 접근용 데미지 프로퍼티
        public float DamageAmount
        {
            get => damageAmount;
            set => damageAmount = value;
        }

        // 외부에서 데미지를 초기화 하는 매서드 > 필수 호출
        public void SetDamage(float damage) => damageAmount = damage;

        // 1) 즉시 데미지 (Enemy 대상)
        public void InstantDamage(float damage, EnemyHealth target)
        {
            target?.TakeDamage(damage);
        }
        // 1) 즉시 데미지 (Player 대상)
        public void InstantDamageToPlayer(int damage, PlayerHealth target, Transform source = null)
        {
            target?.TakeDamage(damage, source);
        }

        // 2) [DoT] 단일 대상 지속 데미지 (Enemy 대상)
        public void DoTDamage(float damagePerTick, float interval, float duration, EnemyHealth target)
        {
            StopSingleContinuous();
            singleContinuousCoroutine = StartCoroutine(DoTCoroutine(damagePerTick, interval, duration, target));
        }
        // 2) [DoT] 단일 대상 지속 데미지 코루틴 (Player 대상)
        public void DoTDamageToPlayer(int damagePerTick, float interval, float duration, PlayerHealth target, Transform source = null)
        {
            StopSingleContinuous();
            singleContinuousCoroutine = StartCoroutine(DoTCoroutineToPlayer(damagePerTick, interval, duration, target, source));
        }

        // 2-1) DoT 코루틴 (Enemy 대상)
        private IEnumerator DoTCoroutine(float damagePerTick, float interval, float duration, EnemyHealth target)
        {
            if (interval <= 0f || duration <= 0f || target == null) yield break;
            float elapsed = 0f;

            while (elapsed < duration && target != null)
            {
                target.TakeDamage(damagePerTick);
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
        }
        // 2-1) DoT 코루틴 (Player 대상)
        private IEnumerator DoTCoroutineToPlayer(float damagePerTick, float interval, float duration, PlayerHealth target, Transform source = null)
        {
            if (interval <= 0f || duration <= 0f || target == null) yield break;
            float elapsed = 0f;

            while (elapsed < duration && target != null)
            {
                target.TakeDamage((int)damagePerTick, source);
                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }
        }

        // 3) [ADoT] 범위 지속 데미지
        public void ADoTDamage(float damagePerTick, float interval, float duration, float radius, LayerMask targetLayers)
        {
            StopContinuousDamage();
            areaContinuousCoroutine = StartCoroutine(ADoTCoroutine(damagePerTick, interval, duration, radius, targetLayers));
        }
        // 3-1) ADoT 코루틴
        private IEnumerator ADoTCoroutine(float damagePerTick, float interval, float duration, float radius, LayerMask targetLayers)
        {
            if (radius <= 0f || duration <= 0f || interval <= 0f) yield break;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetLayers);

                foreach (Collider2D hit in hits)
                {
                    if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                    {
                        enemy.TakeDamage(damagePerTick);
                    }
                    else if (hit.TryGetComponent<PlayerHealth>(out PlayerHealth player))
                    {
                        player.TakeDamage((int)damagePerTick, transform);
                    }
                }

                yield return new WaitForSeconds(interval);
                elapsed += interval;
            }

            areaContinuousCoroutine = null;
        }

        // 4) [AoE] 범위 데미지
        public void AreaDamage(float damage, float radius, LayerMask targetLayers, Vector3? center = null)
        {
            Vector3 damageCenter = center ?? transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(damageCenter, radius, targetLayers);

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                {
                    enemy.TakeDamage(damage);
                }
                else if (hit.TryGetComponent<PlayerHealth>(out PlayerHealth player))
                {
                    player.TakeDamage((int)damage, transform);
                }
            }
        }

        // 코루틴 관리
        public void StopContinuousDamage()
        {
            StopSingleContinuous();
            StopAreaContinuous();
        }
        // 단일 지속 데미지 코루틴 정리
        private void StopSingleContinuous()
        {
            if (singleContinuousCoroutine != null)
            {
                StopCoroutine(singleContinuousCoroutine);
                singleContinuousCoroutine = null;
            }
        }
        // 범위 지속 데미지 코루틴 정리
        private void StopAreaContinuous()
        {
            if (areaContinuousCoroutine != null)
            {
                StopCoroutine(areaContinuousCoroutine);
                areaContinuousCoroutine = null;
            }
        }
        private void OnDisable() => StopContinuousDamage();
        private void OnDestroy() => StopContinuousDamage();
    }
}
