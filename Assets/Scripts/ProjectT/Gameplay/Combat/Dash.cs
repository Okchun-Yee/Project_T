using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Dash : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDashing = false; // 대시 상태 추가
    public bool IsDashing { get { return isDashing; } } // 대시 상태 접근용 프로퍼티

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void Dash_(Vector2 direction, float force, float duration)
    {
        if (isDashing) return; // 중복 대시 방지
        StartCoroutine(DashRoutine(direction, force, duration));
    }

    private IEnumerator DashRoutine(Vector2 direction, float force, float duration)
    {
        isDashing = true;

        rb.velocity = Vector2.zero; // 초기 속도 초기화
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse); // 대시 방향으로 힘 적용
        yield return new WaitForSeconds(duration);

        rb.velocity = Vector2.zero; // 대시 후 속도 초기화

         yield return new WaitForFixedUpdate();
        isDashing = false;
    }
}
