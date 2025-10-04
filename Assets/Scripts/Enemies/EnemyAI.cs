using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // 적 AI 상태
    private enum State
    {
        Roaming,
        Attacking,
        Tracking
    }
    [Header("Enemy AI Move Settings")]
    [SerializeField] private float roamChangeDirFloat = 2f;             // 방향 전환 주기
    [SerializeField] private float trackingRange = 8f;                  // 추적 시작 범위
    [Header("Enemy AI Attack Settings")]
    [SerializeField] private float attackRange = 0f;                    // 공격 범위 (0이면 근접 공격)
    [SerializeField] private float attackCooldown = 2f;                 // 공격 쿨타임
    [SerializeField] private bool stopMovingWhileAttacking = false;     // 공격 시 이동 멈춤 여부
    [Header("Enemy Type")]
    [SerializeField] private MonoBehaviour enemyType;                   // EnemyBase 상속받은 스크립트 (공격 속성)

    private bool canAttack = true;              // 공격 가능 여부
    private float timeRoaming = 0f;             // 방향 전환 타이머
    private Vector2 roamPosition;               // 현재 이동 방향
    private State state;                        // 현재 상태
    private EnemyPathfinding enemyPathfinding;       // EnemyPathfinding 컴포넌트 참조
    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        state = State.Roaming;
    }
    private void Start()
    {
        roamPosition = GetRoamingPosition();
    }
    private void Update()
    {
        MovementStateControl();
    }
    private void MovementStateControl()
    {
        switch (state)
        {
            default:
            case State.Roaming:
                Roaming();
                break;
            case State.Attacking:
                Attacking();
                break;
            case State.Tracking:
                Tracking();
                break;
        }
    }
    private void Roaming()
    {
        timeRoaming += Time.deltaTime;
        enemyPathfinding.MoveTo(roamPosition);
        // 플레이어가 추적 범위 안에 들어오면 추적 상태로 전환
        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < trackingRange)
        {
            state = State.Tracking;
        }
        // 일정 시간마다 새로운 위치로 이동
        if (timeRoaming > roamChangeDirFloat)
        {
            roamPosition = GetRoamingPosition();
        }
    }
    private void Attacking()
    {
        // 플레이어가 공격 범위 밖으로 나가면 추적 상태로 전환
        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) > attackRange)
        {
            state = State.Tracking;
        }

        if (attackRange != 0 && canAttack)
        {
            canAttack = false;
            (enemyType as IEnemy).Attack();

            if (stopMovingWhileAttacking)
            {
                enemyPathfinding.StopMoving();
            }
            else
            {
                enemyPathfinding.MoveTo(roamPosition);
            }
            StartCoroutine(AttackCooldownRoutine());
        }
    }
    private void Tracking()
    {
        // 플레이어가 추적 범위 밖으로 나가면 다시 Roaming
        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) > trackingRange)
        {
            state = State.Roaming;
            return;
        }
        // 플레이어가 공격 범위 안에 들어오면 공격
        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < attackRange)
        {
            state = State.Attacking;
            return;
        }
        // 플레이어를 따라 이동
        enemyPathfinding.MoveTo(PlayerController.Instance.transform.position);
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    private Vector2 GetRoamingPosition()
    {
        timeRoaming = 0f;
        // Generate a random direction and normalize it to get a unit vector
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;

        // 추적 범위: 반투명 노란(녹색 계열 유지)
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(pos, trackingRange);

        // 공격 범위: 반투명 빨간
        if (attackRange > 0f)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(pos, attackRange);
        }
        else
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(pos, 0.25f);
        }
    }
}
