using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : Singleton<PlayerController>
{
    public bool FacingLeft { get { return facingLeft; } }
    [SerializeField] private float moveSpeed = 4f;  // 플레이어 이동 속도
    [SerializeField] private float dashSpeed = 4f;  // 대쉬 추가 이동 속도
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnim;
    private SpriteRenderer mySprite;
    private float startingMoveSpeed;    // 기본 이동 속도 (증가 후 복귀 할 원본 이동 속도)
    private bool facingLeft = false;    // 플레이어 왼쪽 / 오른쪽 판별
    private bool isDashing = false;     // 플레이어 대쉬 판별
    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
        mySprite = GetComponent<SpriteRenderer>();
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
        SetPlayerDirection();   // 물리 프레임 당 플레이어 방향 계산
        PlayerMovement();           // 물리 프레임 당 플레이어 이동 계산
    }

    public void Move(Vector2 moveInput) // Input Manager 키보드 이벤트 구독용 메서드
    {
        movement = moveInput;
        Debug.Log("Move: " + movement);

        myAnim.SetFloat("moveX", movement.x);
        myAnim.SetFloat("moveY", movement.y);
    }
    public void Dodge() // Input Manager 키보드 이벤트 구독용 메서드
    {
        if (!isDashing)
        {
            isDashing = true;
            moveSpeed *= dashSpeed;

            StartCoroutine(EndDodgeRoutine());
        }
    }

    private IEnumerator EndDodgeRoutine()
    {
        float dodgeTime = .2f;
        float dodgeCD = .25f;
        yield return new WaitForSeconds(dodgeTime);
        moveSpeed = startingMoveSpeed;

        yield return new WaitForSeconds(dodgeCD);
        isDashing = false;
    }
    private void PlayerMovement()
    {
        if (false)  // (스킬 시전 중, 공격 중, 죽음 중) 이동 불가
            return;
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }
    private void SetPlayerDirection()
    {
        if (false)  // (스킬 시전 중, 공격 중, 죽음 중) 건너뛰기
            return;

        if (movement != Vector2.zero)
        {
            // 단순한 4방향 처리
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                // 좌우가 더 강한 경우
                facingLeft = movement.x < 0;
                mySprite.flipX = facingLeft;
            }
            // 상하 방향일 때는 현재 방향 유지
        }
    }
}
