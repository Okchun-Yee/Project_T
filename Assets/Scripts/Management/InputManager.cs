using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    //   Input Action Events 매니저
    public event Action<Vector2> OnMoveInput;           // 이동 입력 이벤트
    public event Action OnDodgeInput;                   // 회피 입력 이벤트
    public event Action OnAttackInput;                  // 기본 공격 입력 이벤트
    public event Action<int> OnSkillInput;              // 스킬 입력 이벤트 (int Index : 스킬 번호9)
    public event Action OnPickupInput;                  // 아이템 획득 입력 이벤트
    public event Action OnInventoryInput;               // 인벤토리 UI 입력 이벤트
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
        playerControls.Movement.Move.performed += HandleMove;                       // 플레이어 이동 입력 감지
        playerControls.Movement.Move.canceled += HandleMove;
        // 플레이어 회피 이벤트 구독
        playerControls.Movement.Dodge.performed += HandleDodge;                     // 플레이어 회피 입력 감지
        // 플레이어 기본 공격 이벤트 구독
        playerControls.Combat.Attack.performed += HandleAttack_Started;             // 기본 공격 입력 감지
        playerControls.Combat.Attack.canceled += HandleAttack_Canceled;
        // 플레이어 스킬 이벤트 구독
        playerControls.Combat.Skill_01.performed += ctx => HandleSkill_Started(0);  // 스킬 1 입력 감지
        playerControls.Combat.Skill_01.canceled += ctx => HandleSkill_Canceled(0);
        playerControls.Combat.Skill_02.performed += ctx => HandleSkill_Started(1);  // 스킬 2 입력 감지
        playerControls.Combat.Skill_02.canceled += ctx => HandleSkill_Canceled(1);
        // 플레이어 획득 이벤트 구독
        playerControls.System.Pickup.performed += HandlePickup;                     // 아이템 획득 입력 감지
        // 플레이어 인벤토리 이벤트 구독
        playerControls.System.InventoryUI.performed += HandleInventoryUI;           // 인벤토리 UI 입력 감지
        // 플레이어 임시 UI 1 이벤트 구독
        playerControls.System.TempUI1.performed += HandleTempUI1;                   // 임시 UI 1 입력 감지
        // 플레이어 임시 UI 2 이벤트 구독
        playerControls.System.TempUI2.performed += HandleTempUI2;                   // 임시 UI 2 입력 감지
    }

    // 이벤트 구독 해제
    private void OnDisable()
    {
        if (playerControls != null)
        {
            // 플레이어 이동 이벤트 구독 해제
            playerControls.Movement.Move.performed -= HandleMove;
            playerControls.Movement.Move.canceled -= HandleMove;

            // 플레이어 회피 이벤트 구독 해제
            playerControls.Movement.Dodge.performed -= HandleDodge;
            // 플레이어 기본 공격 이벤트 구독 해제
            playerControls.Combat.Attack.performed -= HandleAttack_Started;
            playerControls.Combat.Attack.canceled -= HandleAttack_Canceled;
            // 플레이어 스킬 이벤트 구독 해제
            playerControls.Combat.Skill_01.performed -= ctx => HandleSkill_Started(0);
            playerControls.Combat.Skill_01.canceled -= ctx => HandleSkill_Canceled(0);
            playerControls.Combat.Skill_02.performed -= ctx => HandleSkill_Started(1);
            playerControls.Combat.Skill_02.canceled -= ctx => HandleSkill_Canceled(1);
            // 플레이어 획득 이벤트 구독 해제
            playerControls.System.Pickup.performed -= HandlePickup;
            // 플레이어 인벤토리 이벤트 구독 해제
            playerControls.System.InventoryUI.performed -= HandleInventoryUI;
            playerControls.System.TempUI1.performed -= HandleTempUI1;
            playerControls.System.TempUI2.performed -= HandleTempUI2;

            playerControls.Disable();
        }
    }
    
    private void OnDestroy()
    {
        if (playerControls != null)
        {
            playerControls.Dispose();
        }
    }
    // 플레이어 이동 이벤트 매서드
    private void HandleMove(InputAction.CallbackContext context)
    {
        Vector2 moveValue = context.ReadValue<Vector2>();
        OnMoveInput?.Invoke(moveValue);
    }
    // 플레이어 회피 이벤트 매서드
    private void HandleDodge(InputAction.CallbackContext context)
    {
        Debug.Log("HandleDodge called!"); // 입력 확인
        OnDodgeInput?.Invoke();
    }
    // 플레이어 공격 이벤트 매서드
    private void HandleAttack_Started(InputAction.CallbackContext context)
    {
        OnAttackInput?.Invoke();
    }
    private void HandleAttack_Canceled(InputAction.CallbackContext context)
    {
        // 공격 취소 시 필요한 로직 추가
    }
    // 플레이어 스킬 이벤트 매서드
    private void HandleSkill_Started(int index)
    {
        OnSkillInput?.Invoke(index);
    }
    private void HandleSkill_Canceled(int index)
    {
        // 스킬 취소 시 필요한 로직 추가
    }
    // 플레이어 아이템 획득 이벤트 매서드
    private void HandlePickup(InputAction.CallbackContext context)
    {
        OnPickupInput?.Invoke();
    }
    // 플레이어 인벤토리 UI 이벤트 매서드
    private void HandleInventoryUI(InputAction.CallbackContext context)
    {
        OnInventoryInput?.Invoke();
    }
    // 플레이어 임시 UI 1 이벤트 매서드
    private void HandleTempUI1(InputAction.CallbackContext context)
    {
        // 임시 UI 1 토글 시 필요한 로직 추가
    }
    // 플레이어 임시 UI 2 이벤트 매서드
    private void HandleTempUI2(InputAction.CallbackContext context)
    {
        // 임시 UI 2 토글 시 필요한 로직 추가
    }
}
