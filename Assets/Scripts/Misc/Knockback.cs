using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 피격을 가지는 모든 오브젝트 공통 클래스. 
/// 만약 보스와 같이 움직임을 가지지 않는 다면 <EnemyHealth> 에서 0으로 설정
/// 플레이어, 적 공통으로 사용되는 클래스입니다.
/// </summary>
public class Knockback : MonoBehaviour
{
    [SerializeField] private float knockBackTime = .2f;
    public bool GettingKnockback { get; private set; }
    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void GetKnockedBack(Transform damageSource, float knockBackThrust)
    {
        GettingKnockback = true;
        Vector2 difference =
        (transform.position - damageSource.position).normalized * knockBackThrust * rb.mass;
        rb.AddForce(difference, ForceMode2D.Impulse);
        StartCoroutine(KnockRoutine());
    }

    private IEnumerator KnockRoutine()
    {
        yield return new WaitForSeconds(knockBackTime);
        rb.velocity = Vector2.zero;
        GettingKnockback = false;
    }
}
