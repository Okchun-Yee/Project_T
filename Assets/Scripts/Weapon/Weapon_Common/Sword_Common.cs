using System.Collections;
using System.Collections.Generic;
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

    // 클래스 레벨에 추가할 필드
    [SerializeField] private int comboLength = 3;   // 에디터에서 단계 수 설정 (예: 3 -> 인덱스 0,1,2)
    private int comboIndex = -1;                    // 내부 모듈러 인덱스
    private Coroutine comboTimeoutCoroutine;        // 콤보 초기화 코루틴 참조
    private Animator anim;                          // 애니메이션
    private GameObject slashAnim;                   // 현재 활성화된 슬래시 애니메이션 인스턴스

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
    }
    private void Start()
    {
        slashAnimSpawnPoint = GameObject.Find("Slash SpawnPoint").transform;
        foreach (Transform col in weaponColliders)
            col.gameObject.SetActive(false); // 시작 시 모든 콜라이더 비활성화
    }
    // 무기 공격 매서드
    protected override void OnAttack()
    {
        Debug.Log($"Sword OnAttack");

        // 1) 공격 인덱스 증가 및 애니메이터에 반영
        SetAttackIndex();
    }

    // 인덱스 증가(모듈러) 및 타이머 재시작
    private void SetAttackIndex()
    {
        comboIndex = (comboIndex + 1) % Mathf.Max(1, comboLength);
        AttackIndex = comboIndex; // Animator에 int 쓰기

        anim.SetTrigger(HASH_ATTACK);

        // 타임아웃(리셋) 재시작
        RestartTimeout();
    }

    // 즉시 콤보 리셋
    private void ResetCombo()
    {
        comboIndex = 0;
        AttackIndex = comboIndex; // Animator에 0 반영

        if (comboTimeoutCoroutine != null)
        {
            StopCoroutine(comboTimeoutCoroutine);
            comboTimeoutCoroutine = null;
        }

        // 콜라이더 끄기(안전): weaponColliders가 존재하면 모두 끔
        if (weaponColliders != null)
        {
            for (int i = 0; i < weaponColliders.Length; i++)
                weaponColliders[i].gameObject.SetActive(false);
        }
    }

    // 타임아웃 재시작
    private void RestartTimeout()
    {
        if (comboTimeoutCoroutine != null)
        {
            StopCoroutine(comboTimeoutCoroutine);
            comboTimeoutCoroutine = null;
        }
        if (comboDelay > 0f)
            comboTimeoutCoroutine = StartCoroutine(ComboTimeoutCoroutine());
    }

    private IEnumerator ComboTimeoutCoroutine()
    {
        float t = 0f;
        while (t < comboDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }
        ResetCombo();
    }
    // 콤보별 해당하는 콜라이더 활성화
    private void ActivateCollider(int index)
    {
        for (int i = 0; i < weaponColliders.Length; i++)
            weaponColliders[i].gameObject.SetActive(i == index);
        DamageSource damageSource = weaponColliders[index].GetComponent<DamageSource>();
        damageSource?.SetDamage(weaponInfo.weaponDamage);
    }

    // 애니메이션 이벤트에서 호출
    // 애니메이션에서 콤보 허용/공격 프레임에 이 메서드로 콜라이더 활성화하세요.
    public void ComboEnableCollider()
    {
        // 현재 AttackIndex 기준으로 콜라이더 활성화
        int idx = AttackIndex;
        if (weaponColliders != null && idx >= 0 && idx < weaponColliders.Length)
        {
            ActivateCollider(idx);
        }
    }
    public void ComboDisableCollider()
    {
        // 모든 콜라이더 끄기
        if (weaponColliders != null)
        {
            for (int i = 0; i < weaponColliders.Length; i++)
                weaponColliders[i].gameObject.SetActive(false);
        }
    }
    public void SlashAnimEnable()
    {
        // 기존 애니메이션이 있으면 제거
        if (slashAnim != null)
            Destroy(slashAnim);

        if (slashAnimPrefab != null && AttackIndex < slashAnimPrefab.Length && slashAnimSpawnPoint != null)
        {
            slashAnim = Instantiate(slashAnimPrefab[AttackIndex], slashAnimSpawnPoint.position, Quaternion.identity);
            slashAnim.transform.parent = this.transform.parent;
        }
    }
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
