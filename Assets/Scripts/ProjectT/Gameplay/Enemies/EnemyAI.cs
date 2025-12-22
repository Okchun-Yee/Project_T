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
    [SerializeField] private float roamChangeDirFloat = 2f;             // 방향 전환 주기(초)
    [SerializeField] private float trackingRange = 8f;                  // 추적 시작 범위

    [Header("Enemy AI Attack Settings")]
    [SerializeField] private float attackRange = 0f;                    // 공격 범위 (0이면 근접 공격)
    [SerializeField] private float attackCooldown = 2f;                 // 공격 쿨타임
    [SerializeField] private bool stopMovingWhileAttacking = false;     // 공격 시 이동 멈춤 여부

    [Header("State Tuning")]
    [SerializeField] private float trackingHysteresis = 0.5f;           // 상태 변환 경계(변환 완화)
    [SerializeField] private float stateChangeCooldown = 0.15f;         // 상태 변경 디바운스(초)

    [Header("Enemy Type")]
    [SerializeField] private MonoBehaviour enemyType;                   // IEnemy 구현 스크립트 (공격 호출)

    private bool canAttack = true;              // 공격 가능 여부
    private float timeRoaming = 0f;             // (디버그용) 누적 시간
    private Vector2 roamPosition;               // 현재 랜덤 목표 방향 (로밍)
    private State state;                        // 현재 상태

    // 중앙화 이동 제어용
    private Vector2 currentMoveTarget;          // 이번 프레임 실제 MoveTo에 넘길 타겟
    private Vector2 previousMoveTarget;         // 이전 프레임에 넘긴 타겟(중복 호출 방지)
    private bool shouldStopMoving = false;      // 이번 프레임 StopMoving 여부

    private float nextRoamChangeTime = 0f;      // 다음 로밍 변경 시점(Time.time)
    private float lastStateChangeTime = -10f;   // 상태 변경 마지막 시점(Time.time)

    private EnemyPathfinding enemyPathfinding;  // EnemyPathfinding 컴포넌트 참조

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        state = State.Roaming;
    }

    private void Start()
    {
        roamPosition = GetRoamingPosition();        // 시작 시 Roaming 방향 설정
        currentMoveTarget = roamPosition;           // 현재 이동 목표 = 설정한 Raoming 위치
        previousMoveTarget = currentMoveTarget;     // 다음 이동 목표 = 현재 이동 목표
        nextRoamChangeTime = Time.time + Mathf.Max(0.1f, roamChangeDirFloat);   // Roaming 방향 전환 시간 계산
    }

    private void Update()
    {
        MovementStateControl();
        ApplyMovement();        // 중앙에서 한 번만 실제 이동 API 호출
    }

    // Enemy 상태 설정 매서드
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

    // 상태 변경을 중앙화: 디바운스, 진입 처리
    private void SetState(State newState)
    {
        if (state == newState) return;
        if (Time.time - lastStateChangeTime < stateChangeCooldown) return;
        lastStateChangeTime = Time.time;
        state = newState;

        if (state == State.Roaming)
        {
            // 로밍 재초기화
            nextRoamChangeTime = Time.time + Mathf.Max(0.1f, roamChangeDirFloat);
            roamPosition = GetRoamingPosition();
        }
    }

    private void Roaming()
    {
        timeRoaming += Time.deltaTime;
        shouldStopMoving = false;
        currentMoveTarget = roamPosition;

        // 플레이어 존재 확인
        if (PlayerLegacyController.Instance != null)
        {
            float dist = Vector2.Distance(transform.position, PlayerLegacyController.Instance.transform.position);
            if (dist < trackingRange) SetState(State.Tracking);
        }

        // Time.time 기반 정확한 간격으로 새 목표 생성
        if (Time.time >= nextRoamChangeTime)
        {
            roamPosition = GetRoamingPosition();
            nextRoamChangeTime = Time.time + Mathf.Max(0.1f, roamChangeDirFloat);
        }
    }

    private void Attacking()
    {
        // 플레이어 존재 확인
        if (PlayerLegacyController.Instance == null)
        {
            SetState(State.Roaming);
            return;
        }

        float dist = Vector2.Distance(transform.position, PlayerLegacyController.Instance.transform.position);
        // 공격에서 이탈 기준에 히스테리시스 적용
        float attackExit = attackRange + trackingHysteresis;
        if (attackRange > 0f && dist > attackExit)
        {
            SetState(State.Tracking);
            return;
        }

        // 공격 조건: 근접(attackRange == 0) 이거나 범위 내
        if (canAttack && (attackRange == 0f || dist <= attackRange))
        {
            canAttack = false;
            (enemyType as IEnemy)?.Attack();

            shouldStopMoving = stopMovingWhileAttacking;
            currentMoveTarget = roamPosition; // 공격 후 이동 목표(정해두기)
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private void Tracking()
    {
        // 플레이어 존재 확인
        if (PlayerLegacyController.Instance == null)
        {
            SetState(State.Roaming);
            return;
        }

        float dist = Vector2.Distance(transform.position, PlayerLegacyController.Instance.transform.position);

        // 추적 탈출(히스테리시스 적용)
        float trackingExit = trackingRange + trackingHysteresis;
        if (dist > trackingExit)
        {
            SetState(State.Roaming);
            return;
        }

        // 공격 진입(attackRange > 0 인 경우만)
        if (attackRange > 0f && dist < attackRange)
        {
            SetState(State.Attacking);
            return;
        }

        // 플레이어를 따라 이동
        currentMoveTarget = PlayerLegacyController.Instance.transform.position;
        shouldStopMoving = false;
    }

    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private Vector2 GetRoamingPosition()
    {
        timeRoaming = 0f;
        // 랜덤 방향 반환 (정규화)
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    // 중앙화된 이동 적용: 한 프레임에 Stop 또는 MoveTo 한 번만 호출
    private void ApplyMovement()
    {
        if (enemyPathfinding == null) return;

        // StopMoving 우선 처리
        if (shouldStopMoving)
        {
            enemyPathfinding.StopMoving();
            shouldStopMoving = false;
            previousMoveTarget = currentMoveTarget;
            return;
        }

        // 중복 호출 방지: 이전과 목표가 거의 같으면 호출 생략
        if (Vector2.Distance(previousMoveTarget, currentMoveTarget) <= 0.01f) return;

        enemyPathfinding.MoveTo(currentMoveTarget);
        previousMoveTarget = currentMoveTarget;
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