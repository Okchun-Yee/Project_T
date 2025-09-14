using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combo : ICombo
{
    private int currentComboIndex = 0;                  // 현재 콤보 인덱스
    private readonly int comboLength;                   // 콤보의 전체 길이
    private readonly Transform[] weaponColliders;       // 무기 콜라이더 배열
    private MonoBehaviour coroutineOwner;               // 코루틴 실행을 위한 MonoBehaviour 참조 > 의존성 주입
    private Coroutine comboResetCoroutine;              // 콤보 리셋 코루틴 참조, 중복 실행 방지용
    private readonly float comboDelay;                  // 콤보 입력 허용 시간

    public bool isComboActive = false;                  // 콤보 상태 관리용

    public Combo(Transform[] weaponColliders, int comboLength, float comboDelay, MonoBehaviour coroutineOwner)
    {
        this.weaponColliders = weaponColliders;
        this.comboLength = comboLength;
        this.comboDelay = comboDelay;
        this.coroutineOwner = coroutineOwner;
    }
    // 콤보 시작 매서드
    // 콤보 종료 코루틴 관리
    public void StartCombo()
    {
        // 콤보 리셋 코루틴 관리
        if (comboResetCoroutine != null)
            coroutineOwner.StopCoroutine(comboResetCoroutine);
        comboResetCoroutine = coroutineOwner.StartCoroutine(ComboResetTimer());
        currentComboIndex++;
    }
    // 다음 콤보 진행 매서드
    // 현재 콤보 인덱스 반환
    public int NextCombo()
    {
        return currentComboIndex % comboLength;
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

    // 애니메이션 이벤트 호출 
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
