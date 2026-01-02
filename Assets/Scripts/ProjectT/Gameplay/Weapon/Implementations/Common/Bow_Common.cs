using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Combat;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Weapon.Contracts;
using ProjectT.Gameplay.Weapon.Projectiles;
using ProjectT.Gameplay.Weapon.VFX;
using UnityEngine;

/// <summary>
/// 기본 활 무기 클래스
/// * 기본 공격, 차징 공격 구현
/// * 홀딩 공격, 콤보 공격 미구현
/// </summary>
namespace ProjectT.Gameplay.Weapon.Implementations.Common
{
    public class Bow_Common : BaseWeapon, ICharging
    {
        [Header("VFX Setting")]
        [SerializeField] private GameObject arrowPrefab;       // 화살 프리팹
        [SerializeField] private Transform arrowSpawnPoint;    // 화살 생성 위치

        [Header("Weapon Setting")]
        private Animator anim;                                  // 애니메이터
        private Flash flash;                                    // 피격시 깜빡임 스크립트
        private static readonly int HASH_ATTACK = Animator.StringToHash("Attack");      // 기본 공격 트리거
        private BaseVFX currentVFX;                               // 현재 재생 중인 VFX
        private void Awake()
        {
            anim = GetComponent<Animator>();
            flash = GetComponent<Flash>();
            currentVFX = GetComponentInChildren<BaseVFX>();
        }
        private void OnEnable()
        {
            if (ChargingManager.Instance != null)
            {
                ChargingManager.Instance.OnChargingProgress += OnChargingProgress;
                ChargingManager.Instance.OnChargingCompleted += OnChargingCompleted;
                ChargingManager.Instance.OnChargingCanceled += OnChargingCanceled;
            }
        }
        protected override void OnDisable()
        {
            if (ChargingManager.Instance != null)
            {
                ChargingManager.Instance.OnChargingProgress -= OnChargingProgress;
                ChargingManager.Instance.OnChargingCompleted -= OnChargingCompleted;
                ChargingManager.Instance.OnChargingCanceled -= OnChargingCanceled;
            }
            base.OnDisable();
        }
        private void Update()
        {
        
        }

        protected override void OnAttack()
        {
            anim.SetTrigger(HASH_ATTACK);

            SpawnArrow();
        }

        protected override void OnAttack_Charged()
        {
            Debug.Log($"[Bow]: OnAttack_Charged");
        }
        #region Charging Event Callbacks
        // ICharging 인터페이스 구현
        // 차징 공격 관련 매서드
        public void OnChargingCanceled(ChargingType type)
        {
            if (ActiveWeapon.Instance == null) return;
            if(type != ChargingType.Attack) return;
        }

        public void OnChargingCompleted(ChargingType type)
        {
            if (ActiveWeapon.Instance == null) return;
            if(type != ChargingType.Attack) return;
            StartCoroutine(flash.FlashRoutine()); // 차징 완료 플래시 효과
        }

        public void OnChargingProgress(ChargingType type, float elapsed, float duration)
        {
            if (ActiveWeapon.Instance == null) return;
            if(type != ChargingType.Attack) return;
            // [TODO] 차징 진행도에 따른 효과 구현
        }
        #endregion
        private void SpawnArrow()
        {
            GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, ActiveWeapon.Instance.transform.rotation);

            newArrow.GetComponent<Projectile>().UpdateProjectileRange(weaponInfo.weaponRange);
            newArrow.GetComponent<Projectile>().Initialize(weaponInfo.weaponDamage); // Initialize로 데미지 설정
        }
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // weaponInfo == NULL인 경우 아직 무기 장착 이전이므로 PASS
            if (weaponInfo == null) return;

            Vector3 pos = transform.position;
            // 공격 범위: 반투명 빨간
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(pos, weaponInfo.weaponRange);
        }
        #endif
    }
}
