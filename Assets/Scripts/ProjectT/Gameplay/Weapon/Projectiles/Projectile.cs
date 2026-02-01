using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectT.Gameplay.Combat.World;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Weapon.VFX;
using ProjectT.Gameplay.Enemies;

/// <summary>
/// 투사체 VFX 클래스
/// * BaseVFX를 상속받아 투사체의 특수한 동작 구현
/// * 발사, 충돌, 궤적 등 투사체 관련 로직 구현
/// </summary>
namespace ProjectT.Gameplay.Weapon.Projectiles
{
    public class Projectile : BaseVFX
    {
        [Header("Projectile Settings")]
        [SerializeField] private float moveSpeed = 22f;                 // 투사체 이동 속도
        [SerializeField] private GameObject particleOnHitPrefabVFX;     // 충돌 시 생성할 파티클 VFX 프리팹
        [SerializeField] private bool isEnemyProjectile = false;        // 적 투사체 여부 (플레이어 공격 판정용)
        [SerializeField] private float projectileRange = 10f;           // 투사체 최대 사거리
        private Vector3 startPosition;                                  // 투사체 시작 위치 (사거리 계산용)
        public Action<Vector3, EnemyHealth> OnEnemyHit;                 // 적 충돌 시 이벤트 (충돌 위치, 적 체력 컴포넌트)
        public Action<Vector3> OnEnvironmentHit;                        // 환경 충돌 시 이벤트 (충돌 위치)

        // 프로퍼티: 적 투사체 여부
        public bool IsEnemyProjectile => isEnemyProjectile;
        
        protected override void Start()
        {
            base.Start();
            startPosition = transform.position;
        }
        private void Update()
        {
            MoveProjectile();
            DetectFireDistance();
        }
        // * 투사체 거리 제한 매서드
        private void DetectFireDistance()
        {
            // 시작 위치에서 일정 거리 이상 이동 시 투사체 파괴
            if (Vector3.Distance(transform.position, startPosition) > projectileRange)
            {
                Destroy(gameObject);
            }
        }
        // * 투사체 이동 매서드
        private void MoveProjectile()
        {
            // 투사체를 오른쪽 방향으로 이동 (로컬 기준)
            transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        }
        // 외부에서 투사체 사거리 업데이트
        public void UpdateProjectileRange(float projectileRange)
        {
            this.projectileRange = projectileRange;
        }
        // 외부에서 투사체 속도 업데이트
        public void UpdateMoveSpeed(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }

        protected override void OnVFXInitialized()
        {
            // 투사체의 초기화 로직 구현
            // Debug.Log($"Projectile [{gameObject.name}]: Initialized with damage {assignedDamage}");
        }

        // 투사체 충돌 처리 매서드
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null || collision.isTrigger) return;

            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            Indestructible indestructible = collision.GetComponent<Indestructible>();
            PlayerHealth player = collision.GetComponent<PlayerHealth>();

            // 1) 플레이어와 충돌 (적 투사체일 때)
            if (player != null && isEnemyProjectile)
            {
                PlayerHit(player);
                return;
            }

            if (enemyHealth != null && !isEnemyProjectile)
            {
                Debug.Log($"Projectile hit enemy: {enemyHealth.gameObject.name}");
                EnemyHit(enemyHealth);
                return;
            }

            if (indestructible != null)
            {
                IndestructibleHit();
                return;
            }
        }

        // 투사체가 자연 오브젝트와 충돌 시 호출
        private void IndestructibleHit()
        {
            OnEnvironmentHit?.Invoke(transform.position);
            DestroyProjectile();
        }
        // 투사체가 적과 충돌 시 호출
        private void EnemyHit(EnemyHealth enemyHealth)
        {
            if (enemyHealth == null)
            {
                DestroyProjectile();
                return;
            }
            // 적 체력 컴포넌트에 데미지 적용
            if (damageSource != null)
            {
                damageSource.InstantDamage(damageSource.DamageAmount, enemyHealth);   // DamageSource가 있으면 해당 데미지 적용
            }
            else
            {
                Debug.LogWarning($"Projectile [{gameObject.name}]: No DamageSource component found!");
            }
            OnEnemyHit?.Invoke(transform.position, enemyHealth);
            DestroyProjectile();
        }
        // 투사체가 플레이어와 충돌 시 호출 (적 투사체일 때)
        private void PlayerHit(PlayerHealth player)
        {
            if (damageSource == null)
            {
                Debug.LogWarning($"Projectile [{gameObject.name}]: No DamageSource component found!");
                return;
            }
            int dmg = (int)damageSource.DamageAmount;
            damageSource.InstantDamageToPlayer(dmg, player, transform);    // DamageSource가 있으면 해당 데미지 적용
            DestroyProjectile();
        }
        // 투사체 충돌 시 파괴 VFX 생성 및 오브젝트 제거
        public void DestroyProjectile()
        {
            if (particleOnHitPrefabVFX != null)
                Instantiate(particleOnHitPrefabVFX, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
