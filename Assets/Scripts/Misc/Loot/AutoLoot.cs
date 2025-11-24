using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLoot : MonoBehaviour
{
    private enum AutoLootType
    {
        GoldCoin,
        HealthGlobe,
        StaminaGlobe,
    }

    [SerializeField] private AutoLootType lootType;         // 자동 루팅 아이템 타입
    [SerializeField] private float lootDistance = 5f;       // 플레이어가 아이템을 감지하는 거리
    [SerializeField] private float accelerationRate = .2f;  // 플레이어에게 다가갈 때 가속도
    [SerializeField] private float moveSpeed = 3f;          // 아이템 이동 속도
    [SerializeField] private float maxMoveSpeed = 30f;      // 아이템 최대 이동 속도
    [SerializeField] private AnimationCurve animCurve;      // 아이템이 튀어오르는 애니메이션 커브
    [SerializeField] private float heightY = 1.5f;          // 아이템이 튀어오르는 높이
    [SerializeField] private float popDuration = 1f;        // 아이템이 튀어오르는 애니메이션 지속 시간 
    
    private Vector3 moveDir;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        // 초기 위치에서 위로 튕겨오르는 애니메이션
        StartCoroutine(AnimCurveSpawnRoutine());
    }
    private void Update()
    {
        Vector3 playerPos = PlayerController.Instance.transform.position;
        if (Vector3.Distance(transform.position, playerPos) < lootDistance)
        {
            moveDir = (playerPos - transform.position).normalized;
            moveSpeed = Mathf.Min(moveSpeed + accelerationRate * Time.deltaTime, maxMoveSpeed);
        }
        else
        {
            moveDir = Vector3.zero;
            moveSpeed = 0;
        }
    }
    private void FixedUpdate()
    {
        rb.velocity = moveDir * moveSpeed;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            DetectPickupType();
            Destroy(gameObject);
        }
    }
    private IEnumerator AnimCurveSpawnRoutine()
    {
        Vector2 startPos = transform.position;
        float randomX = transform.position.x + Random.Range(-2f, 2f);
        float randomY = transform.position.y + Random.Range(-1f, 1f);

        Vector2 endPoint = new Vector2(randomX, randomY);
        float timePassed = 0f;

        while (timePassed < popDuration)
        {
            timePassed += Time.deltaTime;
            float linearT = timePassed / popDuration;
            float heightT = animCurve.Evaluate(linearT);
            float height = Mathf.Lerp(0f, heightY, heightT);

            transform.position = Vector2.Lerp(startPos, endPoint, linearT) + new Vector2(0f, height);
            yield return null;
        }
    }

    private void DetectPickupType()
    {
        switch (lootType)
        {
            case AutoLootType.GoldCoin:
                Debug.Log("coin picked up");
                break;
            case AutoLootType.HealthGlobe:
                PlayerHealth.Instance.HealPlayer();
                Debug.Log("health picked up");
                break;
            case AutoLootType.StaminaGlobe:
                Debug.Log("stamina picked up");
                break;
            default:
                Debug.LogWarning("Unknown pickup type");
                break;
        }
    }

    // 씬 뷰에서 lootDistance를 시각화 (항상 보임)
    private void OnDrawGizmos()
    {
        if (lootDistance <= 0f) return;
        Gizmos.color = new Color(1f, 0.85f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, lootDistance);
    }
}