using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    //   Input Action Events 매니저
    public event Action<Vector2> OnMoveInput;
    public event Action OnDodgeInput;
    private PlayerControls playerControls;
    protected override void Awake()
    {
        base.Awake();
        playerControls = new PlayerControls();
    }
    // 이벤트 구독 활성화
    private void OnEnable()
    {
        playerControls.Enable();
        // 플레이어 이동 이벤트 구독
        playerControls.Movement.Move.performed += HandleMove;   // 플레이어 이동 입력 감지
        playerControls.Movement.Move.canceled += HandleMove;    // 이 줄 추가!
        playerControls.Movement.Dodge.performed += HandleDodge; // 플레이어 회피 입력 감지
    }

    // 이벤트 구독 해제
    private void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.Movement.Move.performed -= HandleMove;
            playerControls.Movement.Move.canceled -= HandleMove;    // 이 줄 추가!
            playerControls.Movement.Dodge.performed -= HandleDodge;
            playerControls.Disable();
        }
    }
    // 플레이어 이동 이벤트 매서드
    private void HandleMove(InputAction.CallbackContext context)
    {
        Vector2 moveValue = context.ReadValue<Vector2>();
        Debug.Log("InputManager Move: " + moveValue);
        OnMoveInput?.Invoke(moveValue);
    }
    // 플레이어 회피 이벤트 매서드
    private void HandleDodge(InputAction.CallbackContext context)
    {
        OnDodgeInput?.Invoke();
    }
}
