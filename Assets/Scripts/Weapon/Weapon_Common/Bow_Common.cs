using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기본 활 무기 클래스
/// * 기본 공격, 차징 공격 구현
/// * 홀딩 공격, 콤보 공격 미구현
/// </summary>
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
        Debug.Log($"[Bow]: OnAttack");
        anim.SetTrigger(HASH_ATTACK);

        SpawnArrow();
    }

    protected override void OnAttack_Charged()
    {
        if (ActiveWeapon.Instance == null) return;

        Debug.Log($"[Bow]: OnAttack_Charged");
    }
    // ICharging 인터페이스 구현
    // 차징 공격 관련 매서드
    public void OnChargingCanceled(ChargingType type)
    {
        if (ActiveWeapon.Instance == null) return;

        Debug.Log($"[Bow]: OnChargingCanceled: {type}");
        if (type == ChargingType.Attack)
        {
            _Attack();
        }
    }

    public void OnChargingCompleted(ChargingType type)
    {
        if (ActiveWeapon.Instance == null) return;

        Debug.Log($"[Bow]: OnChargingCompleted");
        if( type == ChargingType.Attack)
        {
            StartCoroutine(flash.FlashRoutine()); // 피격시 깜빡임 효과
            _AttackCharged();
        }
    }

    public void OnChargingProgress(ChargingType type, float elapsed, float duration)
    {
        if (ActiveWeapon.Instance == null) return;
    }
    private void SpawnArrow()
    {
        GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, ActiveWeapon.Instance.transform.rotation);

        newArrow.GetComponent<Projectile>().UpdateProjectileRange(weaponInfo.weaponRange);
        newArrow.GetComponent<Projectile>().Initialize(weaponInfo.weaponDamage); // Initialize로 데미지 설정
    }
}
