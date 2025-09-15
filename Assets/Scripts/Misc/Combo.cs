using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : MonoBehaviour, ICombo
{
    private int currentComboIndex = 0;                  // 현재 콤보 인덱스
    private int comboHASH;                              // 콤보의 전체 길이
    private float comboDelay;                           // 콤보 입력 허용 시간
    private Animator anim;                              // 애니메이터
    private Transform[] weaponColliders;                // 무기 콜라이더 배열
    private MonoBehaviour coroutineOwner;               // 코루틴 실행을 위한 MonoBehaviour 참조 > 의존성 주입
    private Coroutine comboResetCoroutine;              // 콤보 리셋 코루틴 참조, 중복 실행 방지용
    

    // 콤보 상태 관리용
    public bool isComboActive = false;                  // 콤보 활성 상태
    public bool isComboEnabled = false;                 // 콤보 진행 가능 상태

    internal void Initialize(Transform[] weaponColliders, int hASH_COMBO, float comboDelay, Sword_Common sword_Common)
    {
        this.weaponColliders = weaponColliders;
        this.comboHASH = hASH_COMBO;
        this.comboDelay = comboDelay;
        this.coroutineOwner = sword_Common;
        this.anim = GetComponent<Animator>();
    }

    // 콤보 시작 매서드
    // 콤보 종료 코루틴 관리
    public void StartCombo()
    {
        // 콤보 리셋 코루틴 관리
        if (comboResetCoroutine != null)
            coroutineOwner.StopCoroutine(comboResetCoroutine);
        comboResetCoroutine = coroutineOwner.StartCoroutine(ComboResetTimer());

        if (isComboActive) return;  // 콤보가 활성 상태면 무시
        isComboEnabled = true;      // 콤보 진행 가능 상태로 변경
    }
    // 다음 콤보 진행 매서드
    // 현재 콤보 인덱스 반환
    public void NextCombo()
    {
        if (!isComboEnabled)
            return;                     // 콤보가 진행 가능 상태가 아니면 무시

        isComboEnabled = false;         // 콤보 진행 불가 상태로 변경
        currentComboIndex++;            // 다음 콤보 인덱스
        
        anim.SetTrigger(comboHASH);     // 애니메이션 트리거
    }
    
    // 콤보 종료 매서드
    // 콤보 인덱스 초기화 및 모든 콜라이더 비활성화
    public void ResetCombo()
    {
        currentComboIndex = 0;
        // 모든 콜라이더 비활성화
        foreach (Transform col in weaponColliders)
            col.gameObject.SetActive(false);
    }
    // 콤보 리셋 코루틴
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
    // 다음 콤보 인덱스 반환 매서드
    public int NextComboIndex()
    {
        return currentComboIndex % comboHASH;
    }
    // 콤보 상태 관리 On / Off
    public void OnComboEnable()
    {
        isComboActive = true;
    }
    public void OnComboDisable()
    {
        isComboActive = false;
    }
}
