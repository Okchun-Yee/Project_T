using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 기본 검 무기 클래스
/// * BaseWeapon 상속
/// * 콤보 공격 기능, 차징 공격 기능 포함
/// * 홀딩 공격 기능 미 포함
/// </summary>
public class Sword_Common : BaseWeapon, ICharging
{
    [Header("VFX Setting")]
    [SerializeField] private GameObject[] slashAnimPrefab;      // 콤보 슬래시 애니메이션 프리팹
    private Transform slashAnimSpawnPoint;                      // 슬래시 애니메이션 생성 위치

    [Header("Weapon Setting")]

    [SerializeField] private Combo comboController; // 콤보 컨트롤러
    private Transform weaponColliders;              // 콤보 무기 콜라이더
    private Animator anim;                          // 애니메이션
    private GameObject slashAnim;                   // 현재 활성화된 슬래시 애니메이션 인스턴스
    private InGame_MouseFollow mouseFollow;         // 마우스 추적 스크립트
    private Flash flash;                            // 피격시 깜빡임 스크립트

    private static readonly int HASH_INDEX = Animator.StringToHash("AttackIndex");  // 현재 콤보 인덱스
    private static readonly int HASH_ATTACK = Animator.StringToHash("Attack");      // 다음 콤보 트리거

    // 현재 콤보 인덱스 프로퍼티
    // get: 애니메이터에서 현재 콤보 인덱스 읽기
    // set: 애니메이터에 현재 콤보 인덱스 설정
    public int AttackIndex
    {
        get => anim.GetInteger(HASH_INDEX);
        set => anim.SetInteger(HASH_INDEX, value);
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        mouseFollow = GetComponent<InGame_MouseFollow>();
        flash = GetComponent<Flash>();
        if (anim == null)
        {
            // Null 방어 코드
            Debug.LogWarning($"[Sword_Common] Animator component not found on {name}. Animator-dependent features will be disabled.");
        }
        if (comboController != null)
        {
            comboController.OnComboAdvanced += OnComboAdvanced;
            comboController.OnComboReset += OnComboReset;
        }
    }

    private void Start()
    {
        slashAnimSpawnPoint = GameObject.Find("Slash SpawnPoint")?.transform;
        if (slashAnimSpawnPoint == null)
        {
            // Null 방어 코드
            Debug.LogWarning("[Sword_Common] 'Slash SpawnPoint' not found in scene. Slash VFX will not be spawned.");
        }
        weaponColliders = GameObject.Find("Sword_Colliders")?.transform;
        if (weaponColliders == null)
        {
            // Null 방어 코드
            Debug.LogWarning($"[Sword_Common] WeaponColliders child not found on {name}. Combo colliders will not function.");
        }

        // 안전하게 SetActive 호출
        if (weaponColliders != null)
            weaponColliders.gameObject.SetActive(false); // 시작 시 모든 콜라이더 비활성화
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
    private void Update()
    {
        mouseFollow.MeleeWeaponMouseFollow();
    }
    protected override void OnDisable()
    {
        if (comboController != null)
        {
            comboController.OnComboAdvanced -= OnComboAdvanced;
            comboController.OnComboReset -= OnComboReset;
        }
        if (ChargingManager.Instance != null)
        {
            ChargingManager.Instance.OnChargingProgress -= OnChargingProgress;
            ChargingManager.Instance.OnChargingCompleted -= OnChargingCompleted;
            ChargingManager.Instance.OnChargingCanceled -= OnChargingCanceled;
        }
        base.OnDisable(); // 기본 비활성화 로직 호출
    }
    // 무기 공격 매서드
    protected override void OnAttack()
    {
        Debug.Log($"[Sword]: OnAttack");

        // Advance combo on the Combo component if present; otherwise just trigger the attack animation
        if (comboController != null)
        {
            comboController.Advance();
        }
        else
        {
            anim.SetTrigger(HASH_ATTACK);
        }
    }
    // 무기 공격 매서드 (차징용)
    protected override void OnAttack_Charged()
    {
        Debug.Log($"Sword OnAttack_Charged");
    }

    // Combo state and timing delegated to Combo component; local timer/index removed.
    // 콤보별 해당하는 콜라이더 활성화
    private void ActivateCollider(int index)
    {
        if (weaponColliders == null) return;

        weaponColliders.gameObject.SetActive(index >= 0);
        // PlayerController의 FacingLeft를 읽어 콜라이더 좌우 반전 적용 (로컬 스케일 사용)
        bool facingLeft = PlayerController.Instance != null && PlayerController.Instance.FacingLeft;
        // 추가: 앞/뒤 판별
        bool facingBack = PlayerController.Instance != null && PlayerController.Instance.FacingBack;

        if (facingLeft)
            weaponColliders.transform.rotation = Quaternion.Euler(0, 180, 0);
        else
            weaponColliders.transform.rotation = Quaternion.Euler(0, 0, 0);
        if(facingBack)
            weaponColliders.transform.rotation *= Quaternion.Euler(-180, 0, 0);

        DamageSource damageSource = weaponColliders.GetComponent<DamageSource>();
        damageSource?.SetDamage(weaponInfo.weaponDamage);
    }
    // Combo component events
    // 콤보가 진행될 때마다 호출
    private void OnComboAdvanced(int idx)
    {
        AttackIndex = idx;
        anim.SetTrigger(HASH_ATTACK);
    }

    private void OnComboReset()
    {
        AttackIndex = -1;
        if (weaponColliders != null)
            weaponColliders.gameObject.SetActive(false);
    }

    // 애니메이션 이벤트에서 호출
    // 애니메이션에서 콤보 허용/공격 프레임에 이 메서드로 콜라이더 활성화하세요.
    public void ComboEnableCollider()
    {
        // 현재 AttackIndex 기준으로 콜라이더 활성화
        int idx = AttackIndex;
        if (weaponColliders != null && idx >= 0)
        {
            ActivateCollider(idx);
        }
    }
    public void ComboDisableCollider()
    {
        // 모든 콜라이더 끄기
        if (weaponColliders != null)
        {
            weaponColliders.gameObject.SetActive(false);
        }
    }
    // 슬래시 애니메이션 활성화
    public void SlashAnimEnable()
    {
        // Out of range 방어
        if (AttackIndex < 0)
            return;
        // 기존 애니메이션이 있으면 제거
        if (slashAnim != null)
            Destroy(slashAnim);
        // 새 애니메이션 생성
        if (slashAnimPrefab != null && AttackIndex < slashAnimPrefab.Length && slashAnimSpawnPoint != null)
        {
            slashAnim = Instantiate(slashAnimPrefab[AttackIndex], slashAnimSpawnPoint.position, Quaternion.identity);
            //slashAnim.transform.parent = this.transform.parent;
        }
    }
    // 슬래시 방향 회전
    public void SwingUp_Flip()
    {
        if (slashAnim == null) return;
        slashAnim.transform.rotation = Quaternion.Euler(-180, 0, 0);
        if (PlayerController.Instance.FacingLeft)
        {
            SpriteRenderer sr = slashAnim.GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = true;
        }
    }
    public void SwingDown_Flip()
    {
        if (slashAnim == null) return;
        slashAnim.transform.rotation = Quaternion.Euler(0, 0, 0);
        if (PlayerController.Instance.FacingLeft)
        {
            SpriteRenderer sr = slashAnim.GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = true;
        }
    }

    // 차징 이벤트 콜백 매서드 모음
    public void OnChargingCanceled(ChargingType type)
    {
        if (ActiveWeapon.Instance == null) return;
        Debug.Log("Charging Canceled");
        // 기본 공격 & 스킬 구분
        if (type == ChargingType.Attack)
        {
            // 차징 취소 시 공격 실행
            _Attack();
        }
    }

    public void OnChargingCompleted(ChargingType type)
    {
        if (ActiveWeapon.Instance == null) return;
        Debug.Log("Charging Completed");
        // 기본 공격 & 스킬 구분
        if (type == ChargingType.Attack)
        {
            StartCoroutine(flash.FlashRoutine()); // 피격시 깜빡임 효과
            _AttackCharged();
        }
        else if (type == ChargingType.Skill)
        {
            Debug.Log("Skill Charging Completed");
        }
    }

    public void OnChargingProgress(ChargingType type, float elapsed, float duration)
    {
        if (ActiveWeapon.Instance == null) return;
    }
}
