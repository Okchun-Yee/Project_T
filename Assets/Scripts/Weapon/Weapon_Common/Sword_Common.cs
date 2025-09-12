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
    private Transform[] weaponColliders;                        // 콤보 무기 콜라이더

    [Header("VFX Setting")]
    [SerializeField] private GameObject[] slashAnimPrefab;      // 콤보 슬래시 애니메이션 프리팹
    private Transform slashAnimSpawnPoint;                      // 슬래시 애니메이션 생성 위치

    private int activeColliderIndex = -1;       // 현재 활성 콜라이더 추적
    private int currentComboIndex = 0;          // 0~4 인덱스
    private Animator anim;                      // 애니메이션
    private Coroutine comboResetCoroutine;      // 콤보 초기화 코루틴 참조
    private GameObject slashAnim;               // 현재 활성화된 슬래시 애니메이션 인스턴스

    private static readonly string[] comboTriggers = new[] { "Attack1", "Attack2", "Attack3" };

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        ResetCombo();
        slashAnimSpawnPoint = GameObject.Find("Slash SpawnPoint").transform;
    }
    // 무기 공격 매서드
    protected override void OnAttack()
    {
        if (false) return;                                              // 공격 중에는 입력 무시
        Debug.Log("Sword Attacking");

        // 1) 콤보 인덱스 계산
        int idx = currentComboIndex % comboTriggers.Length;             // 총 길이 % 현재 인덱스로 최종 인덱스 계산

        // 2) 해당 하는 콜라이더 활성화
        ActivateCollider(idx);

        // 3) 콤보별 해당하는 프리펩 생성
        if (slashAnimPrefab != null && idx < slashAnimPrefab.Length && slashAnimSpawnPoint != null)
        {
            slashAnim = Instantiate(slashAnimPrefab[idx], slashAnimSpawnPoint.position, Quaternion.identity);
            slashAnim.transform.parent = this.transform.parent;         // 생성 후 플레이어 오브젝트 자식 오브젝트로 변경
        }

        // 4) 콤보 리셋 코루틴 활성화 (콤보 제한 시간)
        if (comboResetCoroutine != null)
            StopCoroutine(comboResetCoroutine);                         // 기존 코루틴 중지
         comboResetCoroutine = StartCoroutine(ComboResetTimer());       // 콤보 중지 코루틴 실행
        currentComboIndex++;                                            // 콤보 인덱스 증가
    }
    // 콤보별 해당하는 콜라이더 활성화
    private void ActivateCollider(int index)
    {
        // 1) 이전 콜라이더 비활성화
        if (activeColliderIndex >= 0)
        {
            weaponColliders[activeColliderIndex].gameObject.SetActive(false);
        }
        // 2) 해당 하는 콜라이더 활성화
        weaponColliders[index].gameObject.SetActive(true);
        // 3) 해당 콜라이더에 데미지 설정
        DamageSource damageSource = weaponColliders[index].GetComponent<DamageSource>();
        damageSource?.SetDamage(weaponInfo.weaponDamage); // 데미지 설정
        // 4) 이전 콜라이더 추적
        activeColliderIndex = index;
    }
    // 콤보 중지 코루틴
    private IEnumerator ComboResetTimer()
    {
        float timer = 0f;
        while (timer < comboDelay)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        ResetCombo();
    }
    private void ResetCombo()
    {
        // 인덱스 초기화
        currentComboIndex = 0;
        // 모든 콜라이더 비활성화
        foreach (var col in weaponColliders)
            col.gameObject.SetActive(false);
    }

    void ICharging.OnChargingCanceled()
    {
        throw new System.NotImplementedException();
    }

    void ICharging.OnChargingCompleted()
    {
        throw new System.NotImplementedException();
    }

    void ICharging.OnChargingProgress(float elapsed, float duration)
    {
        throw new System.NotImplementedException();
    }
}
