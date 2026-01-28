using UnityEngine;

using ProjectT.Core;
using ProjectT.Gameplay.Combat;
using UnityEngine.InputSystem;
using ProjectT.Core.Debug;
using ProjectT.Gameplay.Player.FSM.Combat;


namespace ProjectT.Gameplay.Player.Controller
{
    #region _DashContext
    /// <summary>
    /// 대시 실행에 필요한 모든 파라미터를 담는 컨텍스트
    /// </summary>
    public struct DashContext
    {
        public Vector2 direction;
        public float force;
        public float duration;
        public float damage;
        public bool useGhostEffect; // Ghost 효과 사용 여부
        public bool useDashTrail;  // Trail 효과 사용 여부
        public int requestedFrame;  // Time.frameCount (오발동 방지용)
        public bool lockMovementDuringDash;  // 대시 지속시간 동안 입력 잠금 여부
        public bool grantInvincibility;    // 대시 동안 무적 부여 여부 (미사용)

        /// <summary>
        /// Dodge용 DashContext 생성 (Ghost 효과 포함, 입력 잠금 없음)
        /// </summary>
        public static DashContext CreateForDodge(Vector2 direction, float force, float duration)
        {
            return new DashContext
            {
                direction = direction,
                force = force,
                duration = duration,
                damage = 0,                         // 회피는 데미지 없음
                useGhostEffect = true,
                useDashTrail = false,
                requestedFrame = Time.frameCount,
                lockMovementDuringDash = false,     // Dodge는 입력 잠금 없음
                grantInvincibility = true           // 회피 동안 무적
            };
        }

        /// <summary>
        /// 스킬용 DashContext 생성 (Ghost 효과 없음, duration 동안 입력 잠금)
        /// </summary>
        public static DashContext CreateForSkill(Vector2 direction, float force, float duration, float damage = 0)
        {
            return new DashContext
            {
                direction = direction,
                force = force,
                duration = duration,
                damage = damage,
                useGhostEffect = false,
                useDashTrail = true,
                requestedFrame = Time.frameCount,
                lockMovementDuringDash = true,      // 스킬은 duration 동안 입력 잠금
                grantInvincibility = true          // 대시 동안 무적 
            };
        }
    }
    #endregion
    public class PlayerMovementExecution : Singleton<PlayerMovementExecution>
    {
        public bool FacingLeft { get { return _facingLeft; } }
        public bool FacingBack { get { return _facingBack; } }

        [Header("Components")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;

        [Header("Executors")]
        [SerializeField] private DashMove _dash;            // Dash 컴포넌트 참조 추가
        [SerializeField] private Ghost _ghostEffect;    // Ghost 컴포넌트 참조 추가
        [SerializeField] private Knockback _knockback;  // Knockback 컴포넌트 참조 추가
        [SerializeField] private DashTrail _dashTrail;  // DashTrail 컴포넌트 참조 추가
        [SerializeField] private Invincibility _invincibility;    // Invincibility 컴포넌트 참조 추가
        
        [Header("Dodge Settings")]
        [SerializeField] private float dodgeForce = 15f;    // 대시 힘
        [SerializeField] private float dodgeDuration = 0.2f; // 대시 지속시간

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;  // 플레이어 이동 속도
        private Vector2 movement;
        private Vector2 lastMovement;

        [Header("Facing / Visual")]                          // 플레이어 상태 변수 목록
        [SerializeField] private bool _facingLeft = false;    // 플레이어 왼쪽 / 오른쪽 판별
        [SerializeField] private bool _facingBack = false;    // 플레이어 앞 / 뒤 판별

        [Header("Runtime Guards")]
        [SerializeField] private bool _isDead;     // 사망 상태 프로퍼티 (상태 방어막)

        private DashContext? _pendingDashContext = null;
        private float _movementLockTime = 0f;  // 이동 입력 잠금 남은 시간(초)

        // 무기 애니메이션 방향 결정 프로퍼티
        public Vector2 CurrentMovement => movement;     // 현재 이동 방향 벡터
        public Vector2 LastMovement => lastMovement;    // 마지막 이동 방향 벡터

        // 외부에서 접근 가능한 설정값
        public float DodgeForce => dodgeForce;
        public float DodgeDuration => dodgeDuration;
        protected override void Awake()
        {
            base.Awake();

            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _sprite = GetComponent<SpriteRenderer>();
            _dash = GetComponent<DashMove>();                        // Dash 컴포넌트 참조
        }
        private void Update()
        {
            // 이동 잠금 타이머 감소 (입력 여부와 무관하게 매 프레임 실행)
            if (_movementLockTime > 0f)
            {
                _movementLockTime -= Time.deltaTime;
            }
        }
        
        private void FixedUpdate()
        {
            PlayerMovement();           // 물리 프레임 당 플레이어 이동 계산
            PlayerDirection();          // 플레이어 방향 계산
        }
        #region Animation Helpers
        public void SetMoveInput(Vector2 moveInput) // Input Manager 키보드 이벤트 구독용 메서드
        {
            // ActionLock 체크 (스킬 시전 중 이동 금지)
            if (PlayerController.Instance.IsActionLocked(ActionLockFlags.Move))
            {
                movement = Vector2.zero;
                _anim.SetFloat("moveX", 0f);
                _anim.SetFloat("moveY", 0f);
                return;
            }
            
            // 기존 대쉬 전용 이동 잠금 체크 (타이머는 Update에서 감소)
            if (_movementLockTime > 0f)
            {
                movement = Vector2.zero;
                _anim.SetFloat("moveX", 0f);
                _anim.SetFloat("moveY", 0f);
                return;
            }
            
            
            movement = moveInput.normalized; // 정규화하여 저장
            // 플레이어 이동 애니메이션 및 방향 설정
            if (movement.magnitude > 0.1f)
            {
                lastMovement = movement.normalized;  // 정규화하여 저장
                _anim.SetFloat("moveX", movement.x);
                _anim.SetFloat("moveY", movement.y);
            }
            else
            {
                // 이동 입력이 없을 때는 0으로 설정
                _anim.SetFloat("moveX", 0f);
                _anim.SetFloat("moveY", 0f);
            }
        }
        private void PlayerDirection()
        {
            // (스킬 시전 중, 공격 중, 죽음 중) 방향 전환 불가 상태 관리
            if (PlayerController.Instance.CombatState == PlayerCombatStateId.Charging) return;
            if (_isDead) return;

            if (PlayerController.Instance.IsActionLocked(ActionLockFlags.Dash)
            || PlayerController.Instance.IsActionLocked(ActionLockFlags.BasicAttack)
            || PlayerController.Instance.IsActionLocked(ActionLockFlags.Move))
            {
                return;
            }

            Vector3 mousePos = UnityEngine.Input.mousePosition;
            Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
            if (mousePos.x < playerScreenPoint.x)
            {
                _sprite.flipX = true;
                _facingLeft = true;
            }
            else
            {
                _sprite.flipX = false;
                _facingLeft = false;
            }
            _facingBack = mousePos.y > playerScreenPoint.y;
            _anim?.SetBool("isBack", _facingBack);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// 마우스 방향 계산 (스킬에서도 사용 가능하도록 public)
        /// </summary>
        public Vector2 GetMouseDirection()
        {
            Vector3 mousePos = UnityEngine.Input.mousePosition;
            Vector3 playerWorldPos = transform.position;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

            Vector2 direction = (mouseWorldPos - playerWorldPos).normalized;
            return direction;
        }

        /// <summary>
        /// 이동 방향 계산 (키보드 입력 기반)
        /// </summary>
        public Vector2 GetDirection()
        {
            if (movement.magnitude > 0.1f)
                return movement;
            else if (lastMovement.magnitude > 0.1f)
                return lastMovement;
            else
                return _facingLeft ? Vector2.left : Vector2.right;
        }
        #endregion
        
        #region Movement Execution
        private void PlayerMovement()
        {
            // Execution 안전장치 (최소)
            if (_dash != null && _dash.IsDashing) return;
            if (_knockback != null && _knockback.isKnockback) return;
            if (_isDead) return; // 또는 PlayerHealth.Instance.isDead
            _rb.MovePosition(_rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
        }
        /// <summary>
        /// PlayerController에서만 호출되는 내부 메서드
        /// Pending 상태 설정
        /// </summary>
        public void SetPendingDash(DashContext context)
        {
            _pendingDashContext = context;
            DevLog.Log(DevLogChannels.PlayerRuntime, $"Dash pending set (dir:{context.direction}, force:{context.force}, duration:{context.duration})");
        }
        
        /// <summary>
        /// PlayerLocomotionFsmBinder에서만 호출되는 실제 대시 실행 메서드
        /// Frame 검증으로 오발동 방지
        /// </summary>
        public void ExecutePendingDash()
        {
            // 1. Pending Context 없으면 조용히 리턴
            if (!_pendingDashContext.HasValue)
            {
                return;
            }
            DevLog.Log(DevLogChannels.PlayerRuntime, "Execute pending dash");
            
            // 2. Context 꺼내기
            DashContext ctx = _pendingDashContext.Value;
            
            // 3. Frame 검증 (3프레임 이상 지났으면 폐기)
            if (Time.frameCount - ctx.requestedFrame > 3)
            {
                Debug.LogWarning($"[PlayerMovementExecution] Stale dash context discarded " +
                    $"(requested: {ctx.requestedFrame}, current: {Time.frameCount})");
                DevLog.Log(DevLogChannels.PlayerRuntime, "Dash discarded (stale context)");
                _pendingDashContext = null;
                return;
            }
            
            // 4. 실행 불가 조건 체크
            if (_dash.IsDashing || _knockback.isKnockback || PlayerHealth.Instance.isDead)
            {
                DevLog.Log(DevLogChannels.PlayerRuntime, "Dash aborted (busy or dead)");
                _pendingDashContext = null;
                return;
            }
            
            // 5. 방향 0벡터 방어 (fallback)
            Vector2 dashDirection = ctx.direction;
            if (dashDirection == Vector2.zero)
            {
                Debug.LogWarning("[PlayerMovementExecution] Dash direction is zero, using fallback");
                DevLog.Log(DevLogChannels.PlayerRuntime, "Dash direction fallback applied");
                dashDirection = _facingLeft ? Vector2.left : Vector2.right;
            }
            
            // 6. 이동 입력 잠금 설정 (duration과 동기화)
            if (ctx.lockMovementDuringDash)
            {
                _movementLockTime = ctx.duration + 0.1f;
                DevLog.Log(DevLogChannels.PlayerRuntime, $"Movement locked for {ctx.duration:0.00}s");
            }
            
            // 7. Ghost 효과 (조건부)
            if (ctx.useGhostEffect && _ghostEffect != null)
            {
                _ghostEffect.StartGhostEffect(ctx.duration);
            }
            
            // 8. Trail 효과 (조건부)
            if (ctx.useDashTrail && _dashTrail != null)
            {
                _dashTrail.StartTrailEffect(ctx.duration, ctx.damage);
            }
            // 9. 무적 효과 (조건부)
            if (ctx.grantInvincibility && _invincibility != null)
            {
                _invincibility.StartInvincibility(ctx.duration);
            }
            // 10. 실제 대시 실행
            _dash.DashMove_(dashDirection, ctx.force, ctx.duration);
            DevLog.Log(DevLogChannels.PlayerRuntime, "Dash executed");
            
            // 11. Context 소비
            _pendingDashContext = null;
        }
        #endregion
    }
}
