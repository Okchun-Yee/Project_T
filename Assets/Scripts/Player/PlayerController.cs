using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : Singleton<PlayerController>
{
    public bool FacingLeft { get { return facingLeft; } }
    [Header("Player Settings")]
    [SerializeField] private float moveSpeed = 4f;  // 플레이어 이동 속도

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 15f;    // 대시 힘
    [SerializeField] private float dashDuration = 0.2f; // 대시 지속시간

    private Vector2 movement;
    private Vector2 lastMovement;
    private Rigidbody2D rb;
    private Animator myAnim;
    private SpriteRenderer mySprite;
    private Dash dash;  // Dash 컴포넌트 참조 추가
    private Ghost ghostEffect; // Ghost 컴포넌트 참조 추가
    private float startingMoveSpeed;    // 기본 이동 속도 (증가 후 복귀 할 원본 이동 속도)

    // 플레이어 상태 변수 목록
    private bool facingLeft = false;    // 플레이어 왼쪽 / 오른쪽 판별

    // 무기 애니메이션 방향 결정 프로터피
    public Vector2 CurrentMovement => movement;     // 현재 이동 방향 벡터
    public Vector2 LastMovement => lastMovement;    // 마지막 이동 방향 벡터

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        mySprite = GetComponent<SpriteRenderer>();
        dash = GetComponent<Dash>(); // Dash 컴포넌트 참조
        ghostEffect = GetComponent<Ghost>(); // Ghost 컴포넌트 참조
    }
    private void Start()
    {
        startingMoveSpeed = moveSpeed;
    }
    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMoveInput += Move;
            InputManager.Instance.OnDodgeInput += Dodge;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMoveInput -= Move;
            InputManager.Instance.OnDodgeInput -= Dodge;
        }
    }
    private void Update()
    {

    }
    private void FixedUpdate()
    {
        PlayerMovement();           // 물리 프레임 당 플레이어 이동 계산
        PlayerDirection();          // 플레이어 방향 계산
    }

    public void Move(Vector2 moveInput) // Input Manager 키보드 이벤트 구독용 메서드
    {
        movement = moveInput.normalized; // 정규화하여 저장
        // 플레이어 이동 애니메이션 및 방향 설정
        if (movement.magnitude > 0.1f)
        {
            lastMovement = movement.normalized;  // 정규화하여 저장
            myAnim.SetFloat("moveX", movement.x);
            myAnim.SetFloat("moveY", movement.y);
        }
        else
        {
            // 이동 입력이 없을 때는 0으로 설정
            myAnim.SetFloat("moveX", 0f);
            myAnim.SetFloat("moveY", 0f);
        }
    }
    private void PlayerDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
        if (mousePos.x < playerScreenPoint.x)
        {
            mySprite.flipX = true;
            facingLeft = true;
        }
        else
        {
            mySprite.flipX = false;
            facingLeft = false;
        }
        bool isBack = mousePos.y > playerScreenPoint.y;
        myAnim?.SetBool("isBack", isBack);
    }
    public void Dodge() // Input Manager 키보드 이벤트 구독용 메서드
    {
        Debug.Log("Dodge called - Simple test");

        if (!dash.IsDashing)  // Dash 컴포넌트의 대시 상태 확인
        {
            Vector2 dashDirection = GetDashDirection();

            // 잔상 효과 시작 (대시 지속시간 동안)
            if (ghostEffect != null)
            {
                ghostEffect.StartGhostEffect(dashDuration);
            }

            dash.Dash_(dashDirection, dashForce, dashDuration);
        }
    }
    private Vector2 GetDashDirection()
    {
        if (movement.magnitude > 0.1f)
            return movement;
        else if (lastMovement.magnitude > 0.1f)
            return lastMovement;
        else
            return facingLeft ? Vector2.left : Vector2.right;
    }

    private void PlayerMovement()
    {
        if (dash.IsDashing)  // (스킬 시전 중, 공격 중, 죽음 중) 이동 불가
            return;
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }
}
