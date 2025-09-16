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
    [Header("Weapon Setting")]
    [SerializeField] private float comboDelay;                  // 다음 콤보 입력 허용 시간
    [SerializeField] private Transform[] weaponColliders;       // 콤보 무기 콜라이더

    [Header("VFX Setting")]
    [SerializeField] private GameObject[] slashAnimPrefab;      // 콤보 슬래시 애니메이션 프리팹
    private Transform slashAnimSpawnPoint;                      // 슬래시 애니메이션 생성 위치


    private int activeColliderIndex = -1;       // 현재 활성 콜라이더 추적
    private int currentComboIndex = 0;          // 0~4 인덱스
    private Animator anim;                      // 애니메이션
    private Coroutine comboResetCoroutine;      // 콤보 초기화 코루틴 참조
    private GameObject slashAnim;               // 현재 활성화된 슬래시 애니메이션 인스턴스
    private Combo combo;                        // 콤보 관리 컴포넌트

    private static readonly int HASH_COMBO = Animator.StringToHash("NextCombo");
    private static readonly int HASH_ATTACK = Animator.StringToHash("Attack");
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        slashAnimSpawnPoint = GameObject.Find("Slash SpawnPoint").transform;
    }
    // 무기 공격 매서드
    protected override void OnAttack()
    {
        Debug.Log("Sword OnAttack");
    }
    // 콤보별 해당하는 콜라이더 활성화
    private void ActivateCollider(int index)
    {
        for (int i = 0; i < weaponColliders.Length; i++)
            weaponColliders[i].gameObject.SetActive(i == index);
        DamageSource damageSource = weaponColliders[index].GetComponent<DamageSource>();
        damageSource?.SetDamage(weaponInfo.weaponDamage);
    }
    // 애니메이션 이벤트 호출

    // 차징 이벤트 콜백 매서드 모음
    public void OnChargingCanceled()
    {
        throw new System.NotImplementedException();
    }

    public void OnChargingCompleted()
    {
        throw new System.NotImplementedException();
    }

    public void OnChargingProgress(float elapsed, float duration)
    {
        throw new System.NotImplementedException();
    }
}
