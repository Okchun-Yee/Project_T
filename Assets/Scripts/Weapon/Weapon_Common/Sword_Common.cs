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
        combo = gameObject.GetComponent<Combo>();
        combo.Initialize(weaponColliders, HASH_COMBO, comboDelay, this);
    }
    private void Start()
    {
        slashAnimSpawnPoint = GameObject.Find("Slash SpawnPoint").transform;
    }
    // 무기 공격 매서드
    protected override void OnAttack()
    {
        Debug.Log("Sword OnAttack");
        if (isAttacking) return;                        // 공격 중에는 입력 무시
        
        // 1) 콤보 인덱스 계산
        if (!combo.isComboActive)
            combo.StartCombo();                         // (콤보가 시작 상태가 아니면) 콤보 시작
        int idx = combo.NextComboIndex();               // idx = 다음 콤보 인덱스

        // 2) 해당 하는 콜라이더 활성화 & 애니메이션 트리거
        if (idx == 0) anim.SetTrigger(HASH_ATTACK);     // 첫 공격
        ActivateCollider(idx);                          // 콤보별 해당하는 콜라이더 활성화

        // 3) 콤보별 해당하는 프리펩 생성
        if (slashAnimPrefab != null && idx < slashAnimPrefab.Length && slashAnimSpawnPoint != null)
        {
            slashAnim = Instantiate(slashAnimPrefab[idx], slashAnimSpawnPoint.position, Quaternion.identity);
            slashAnim.transform.parent = this.transform.parent;         // 생성 후 플레이어 오브젝트 자식 오브젝트로 변경
        }
 
        Debug.Log($"Sword Attacking {currentComboIndex}");
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
    public void ComboEnable() { combo.OnComboEnable(); }
    public void ComboDisable() { combo.OnComboDisable();}
    public void NextCombo() { combo.NextCombo(); }
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
