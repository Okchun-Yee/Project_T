using UnityEngine;

using ProjectT.Core;
using ProjectT.Gameplay.Combat;


namespace ProjectT.Gameplay.Player.Controller
{
    public class PlayerLegacyController : Singleton<PlayerLegacyController>
    {
        public bool FacingLeft { get { return _facingLeft; } }
        public bool FacingBack { get { return _facingBack; } }

        [Header("Components")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;

        [Header("Executors")]
        [SerializeField] private Dash _dash;  // Dash 컴포넌트 참조 추가
        [SerializeField] private Ghost _ghostEffect; // Ghost 컴포넌트 참조 추가
        [SerializeField] private Knockback _knockback; // Knockback 컴포넌트 참조 추가

        [Header("Dash Settings")]
        [SerializeField] private float dashForce = 15f;    // 대시 힘
        [SerializeField] private float dashDuration = 0.2f; // 대시 지속시간

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;  // 플레이어 이동 속도
        private Vector2 movement;
        private Vector2 lastMovement;

        [Header("Facing / Visual")]                          // 플레이어 상태 변수 목록
        [SerializeField] private bool _facingLeft = false;    // 플레이어 왼쪽 / 오른쪽 판별
        [SerializeField] private bool _facingBack = false;    // 플레이어 앞 / 뒤 판별

        [Header("Runtime Guards")]
        [SerializeField] private bool _isDead;     // 사망 상태 프로퍼티 (상태 방어막)

        // 무기 애니메이션 방향 결정 프로터피
        public Vector2 CurrentMovement => movement;     // 현재 이동 방향 벡터
        public Vector2 LastMovement => lastMovement;    // 마지막 이동 방향 벡터
        protected override void Awake()
        {
            base.Awake();

            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _sprite = GetComponent<SpriteRenderer>();
            _dash = GetComponent<Dash>();                            // Dash 컴포넌트 참조
            _ghostEffect = GetComponent<Ghost>();                    // Ghost 컴포넌트 참조
            _knockback = GetComponent<Knockback>();                  // Knockback 컴포넌트 참조
        }
        private void Start()
        {

        }
        private void Update()
        {

        }
        private void FixedUpdate()
        {
            PlayerMovement();           // 물리 프레임 당 플레이어 이동 계산
            PlayerDirection();          // 플레이어 방향 계산
        }

        public void SetMoveInput(Vector2 moveInput) // Input Manager 키보드 이벤트 구독용 메서드
        {
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
            if (_dash.IsDashing ||
            _knockback.isKnockback ||
            PlayerHealth.Instance.isDead)
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
        public void TryDodge() // Input Manager 키보드 이벤트 구독용 메서드
        {
            // (스킬 시전 중, 공격 중, 죽음 중) 대시 불가 상태 관리
            if (_dash.IsDashing ||
            _knockback.isKnockback ||
            PlayerHealth.Instance.isDead)
            {
                return;
            }

            if (!_dash.IsDashing)  // Dash 컴포넌트의 대시 상태 확인
            {
                Vector2 dashDirection = GetDashDirection();

                // 잔상 효과 시작 (대시 지속시간 동안)
                if (_ghostEffect != null)
                {
                    _ghostEffect.StartGhostEffect(dashDuration);
                }

                _dash.Dash_(dashDirection, dashForce, dashDuration);
            }
        }
        private Vector2 GetDashDirection()
        {
            if (movement.magnitude > 0.1f)
                return movement;
            else if (lastMovement.magnitude > 0.1f)
                return lastMovement;
            else
                return _facingLeft ? Vector2.left : Vector2.right;
        }

        private void PlayerMovement()
        {
            // Execution 안전장치 (최소)
            if (_dash != null && _dash.IsDashing) return;
            if (_knockback != null && _knockback.isKnockback) return;
            if (_isDead) return; // 또는 PlayerHealth.Instance.isDead
            _rb.MovePosition(_rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
        }
    }
}
