using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Combat.Damage;
using ProjectT.Gameplay.Enemies;
using ProjectT.Gameplay.Weapon.Projectiles;
using UnityEngine;
namespace ProjectT.Gameplay.Combat
{
    public class MeleeCollider : MonoBehaviour
    {
        private enum HitMode
        {
            SinglePerEnable = 0,
            CooldownPerTarget = 1,
        }

        [SerializeField] private bool isProjectileDestroyer = false;
        [SerializeField] private HitMode hitMode = HitMode.SinglePerEnable;
        [SerializeField] private float targetHitCooldown = 0.2f;
        private DamageSource damageSource;
        private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>(); // 중복 방지 (SinglePerEnable)
        private Dictionary<EnemyHealth, float> nextHitTimeByTarget = new Dictionary<EnemyHealth, float>(); // CooldownPerTarget

        private void Awake()
        {
            damageSource = GetComponent<DamageSource>();
        }

        private void OnEnable()
        {
            // 콜라이더 활성화 시 히트 목록 초기화
            hitEnemies.Clear();
            nextHitTimeByTarget.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            Projectile obj = collision.GetComponent<Projectile>();
            // 모두 NULL인 경우 return
            if (enemyHealth == null && obj == null) return;

            if (enemyHealth != null)
            {
                TryHitEnemy(enemyHealth);
                return;
            }

            if (obj != null && isProjectileDestroyer)
            {
                obj.DestroyProjectile();
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (hitMode != HitMode.CooldownPerTarget) return;

            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth == null) return;

            TryHitEnemy(enemyHealth);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (hitMode != HitMode.CooldownPerTarget) return;

            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth == null) return;

            nextHitTimeByTarget.Remove(enemyHealth);
        }

        private void TryHitEnemy(EnemyHealth enemyHealth)
        {
            if (damageSource == null || enemyHealth == null) return;

            switch (hitMode)
            {
                case HitMode.SinglePerEnable:
                    if (hitEnemies.Contains(enemyHealth)) return;
                    hitEnemies.Add(enemyHealth);
                    damageSource.InstantDamage(damageSource.DamageAmount, enemyHealth);
                    break;

                case HitMode.CooldownPerTarget:
                    float now = Time.time;
                    if (nextHitTimeByTarget.TryGetValue(enemyHealth, out float nextTime) && now < nextTime)
                        return;

                    nextHitTimeByTarget[enemyHealth] = now + Mathf.Max(0f, targetHitCooldown);
                    damageSource.InstantDamage(damageSource.DamageAmount, enemyHealth);
                    break;
            }
        }
    }
}
