using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인게임 마우스 추적 스크립트
/// * 근접 무기인 경우 무기 프리팹의 컴포넌트로 사용
/// * 원거리 무기인 경우 플레이어 오브젝트의 컴포넌트로 사용 (프리팹에 포함 X)
/// </summary>
public class InGame_MouseFollow : MonoBehaviour
{
    private void Update()
    {
        // 1) 현재 활성화된 무기가 없는 경우 추적 X
        if (ActiveWeapon.Instance.currentWeapon == null) return;
        // 2) 플레이어가 조작 불가능한 상태인 경우 추적 X (예: 대화 중, 스킬 시전 중, UI 조작 중 등)
        // 조작 불가능 상태 체크 코드 추가

        // 3) 현재 활성화된 무기의 종류에 따라 마우스 추적 매서드 호출
        if (ActiveWeapon.Instance.currentWeapon.GetWeaponInfo().weaponCategory == WeaponCategory.Range)
        {
            RangeWeaponMouseFollow();
        }
        else if( ActiveWeapon.Instance.currentWeapon.GetWeaponInfo().weaponCategory == WeaponCategory.Magical)
        {
            RangeWeaponMouseFollow();
        }
    }
    // 원거리 무기 마우스 추적
    public void MeleeWeaponMouseFollow()
    {
        // 마우스 추적 종료 조건 (1. 공격 중, 2. 스킬 시전 중 ...)
        // if ( ) { return; }

        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.Instance.transform.position);


        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreenPoint.x)
        {
            transform.parent.rotation = Quaternion.Euler(0, -180, angle);
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, -180, angle);
            // 여기에 무기 콜라이더를 어떻게 가져오지
        }
        else
        {
            transform.parent.rotation = Quaternion.Euler(0, 0, angle);
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    public void RangeWeaponMouseFollow()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 direction = transform.position - mousePosition;
        transform.right = -direction;
    }
}
