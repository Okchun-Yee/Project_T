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
        [SerializeField] private bool isProjectileDestroyer = false;
        private DamageSource damageSource;
        private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>(); // 중복 방지

        private void Awake()
        {
            damageSource = GetComponent<DamageSource>();
        }

        private void OnEnable()
        {
            // 콜라이더 활성화 시 히트 목록 초기화
            hitEnemies.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            Projectile obj = collision.GetComponent<Projectile>();
            // 모두 NULL인 경우 return
            if (enemyHealth == null && obj == null) return;
            if (enemyHealth != null){
                // 이미 맞은 적은 다시 맞지 않도록 방지
                if (hitEnemies.Contains(enemyHealth)) return;
                hitEnemies.Add(enemyHealth);

                // 데미지 처리
                if (damageSource != null)
                {
                    damageSource.InstantDamage(damageSource.DamageAmount, enemyHealth);
                }
            }
            else if (obj != null && isProjectileDestroyer)
            {
                // 투사체 파괴
                obj.DestroyProjectile();
            }
        }
    }
}
