using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParabolaProjectile : BaseVFX
{
    [Header("Parabola Projectile Settings")]
    [SerializeField] private float duration = 1f;                   // 투사체가 포물선을 그리며 이동하는 시간
    [SerializeField] private AnimationCurve animCurve;              // 포물선 곡선을 정의하는 애니메이션 커브
    [SerializeField] private float heightY = 3f;                    // 포물선의 최고 높이
    [Header("VFX Settings")]
    [SerializeField] private GameObject shadowPrefab;   // 투사체 그림자 프리팹
    [SerializeField] private GameObject splatterPrefab;             // 투사체가 도착했을 때 생성되는 이펙트 프리팹
    protected override void Start()
    {
        base.Start();
        GameObject projectileShadow =
        Instantiate(shadowPrefab, transform.position + new Vector3(0, -0.3f, 0), Quaternion.identity);

        Vector3 playerPos = PlayerController.Instance.transform.position;
        Vector3 shadowStartPos = projectileShadow.transform.position;

        StartCoroutine(ProjectileCurveRoutine(transform.position, playerPos));
        StartCoroutine(MoveShadowRoutine(projectileShadow, shadowStartPos, playerPos));
    }
    protected override void OnVFXInitialized()
    {
        // 추가 초기화 로직이 필요하면 여기에 작성
        Debug.Log($"ParabolaProjectile [{gameObject.name}]: Initialized with damage {assignedDamage}");
    }
    private IEnumerator ProjectileCurveRoutine(Vector3 startPos, Vector3 endPos)
    {
        float timePassed = 0f;
        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            float heightT = animCurve.Evaluate(linearT);
            float height = Mathf.Lerp(0, heightY, heightT);

            transform.position = Vector2.Lerp(startPos, endPos, linearT) + new Vector2(0, height);
            yield return null;
        }

        Instantiate(splatterPrefab, transform.position, Quaternion.identity);   // 도착 지점에 이펙트 생성
        splatterPrefab.GetComponent<DamageSource>().SetDamage(assignedDamage);  // 도착 시 데미지 설정
        Destroy(gameObject);    // 투사체 파괴
    }
    private IEnumerator MoveShadowRoutine(GameObject shadow, Vector3 startPos, Vector3 endPos)
    {
        float timePassed = 0f;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / duration;
            shadow.transform.position = Vector2.Lerp(startPos, endPos, linearT);
            yield return null;
        }
        Destroy(shadow);
    }
}
