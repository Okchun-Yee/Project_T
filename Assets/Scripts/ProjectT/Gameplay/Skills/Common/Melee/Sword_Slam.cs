using System;
using System.Collections;
using ProjectT.Gameplay.Combat;
using ProjectT.Gameplay.Enemies;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Player.Controller;
using ProjectT.Gameplay.Weapon;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Melee
{
    /// <summary>
    /// 대검 범위 일격 스킬
    /// - 차징 타입 스킬 (skillCategory = Charging)
    /// - 시전 중: 이동/공격/대쉬 금지 + 무적
    /// - chargingTime 후 전방 OverlapBox 1회 타격
    /// </summary>
    public class Sword_Slam : BaseSkill
    {
        [Header("Hit Detection")]
        [SerializeField] private Vector2 hitBoxSize = new Vector2(2.8f, 1.2f);  // 타격 범위 (X, Y)
        [SerializeField] private float hitBoxOffsetX = 1.4f;                    // 플레이어 중심으로부터 오프셋 (X축)
        [SerializeField] private LayerMask enemyLayerMask;                      // Enemy 레이어 마스크
        
        [Header("Timing")]
        [SerializeField] private float recoveryTime = 0.25f;                    // 후딜레이 시간 (캐스팅 후 복구)
        
        private Animator _animator;
        private Invincibility _invincibility;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _animator = GetComponentInParent<Animator>();
            _invincibility = GetComponentInParent<Invincibility>();
            _spriteRenderer = GetComponentInParent<SpriteRenderer>();
            
            if (_animator == null)
            {
                Debug.LogError("[Sword_Slam] Animator not found in parent!");
            }
            
            if (_invincibility == null)
            {
                Debug.LogWarning("[Sword_Slam] Invincibility component not found in parent!");
            }
            
            if (_spriteRenderer == null)
            {
                Debug.LogWarning("[Sword_Slam] SpriteRenderer not found in parent!");
            }
        }

        public override void SubscribeSkillEvents()
        {
            ChargingManager.Instance.OnChargingCompleted += OnChargingCompleted;
            ChargingManager.Instance.OnChargingCanceled += OnChargingCanceled;
        }
        public override void UnsubscribeSkillEvents()
        {
            ChargingManager.Instance.OnChargingCompleted -= OnChargingCompleted;
            ChargingManager.Instance.OnChargingCanceled -= OnChargingCanceled;
        }
        private void OnChargingCompleted(ChargingType type)
        {
            if(type != ChargingType.Skill) return;
            UnsubscribeSkillEvents();
            ExecuteSkill();
        }
        private void OnChargingCanceled(ChargingType type)
        {
            if(type != ChargingType.Skill) return;
            UnsubscribeSkillEvents();
        }


        protected override void OnSkillActivated()
        {
            // 총 시전 시간 = 캐스팅 + 후딜
            float totalTime = skillInfo.chargingTime + recoveryTime;
            
            // 1. 애니메이션 트리거
            _animator?.SetTrigger("SwordSlam");
            
            // 2. 액션 잠금 (이동/공격/대쉬 금지)
            PlayerController.Instance.LockActions(
                ActionLockFlags.Move | ActionLockFlags.BasicAttack | ActionLockFlags.Dash,
                totalTime
            );
            
            // 3. 무적 활성화
            _invincibility?.StartInvincibility(totalTime);
            
            // 4. 타격 타이밍 스케줄 (chargingTime 후 실행)
            StartCoroutine(ExecuteAfterCharging());
        }

        private IEnumerator ExecuteAfterCharging()
        {
            // 캐스팅 시간 대기
            yield return new WaitForSeconds(skillInfo.chargingTime);
            
            // 타격 판정 실행
            ExecuteHitDetection();
        }

        private void ExecuteHitDetection()
        {
            // Facing 방향 결정 (PlayerMovementExecution.FacingLeft 기준)
            bool facingLeft = PlayerMovementExecution.Instance != null && PlayerMovementExecution.Instance.FacingLeft;
            Vector2 facingDir = facingLeft ? Vector2.left : Vector2.right;
            
            // 타격 박스 중심 계산
            Vector2 center = (Vector2)transform.position + (facingDir * hitBoxOffsetX);
            
            // OverlapBox 판정 (1회만)
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, hitBoxSize, 0f, enemyLayerMask);
            
            float damage = GetSkillDamage();
            
            Debug.Log($"[Sword_Slam] Hit Detection: {hits.Length} enemies, Damage: {damage:F1}");
            
            // 모든 적에게 데미지 전달
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        // Gizmo 디버깅 (Scene 뷰에서 타격 범위 시각화)
        private void OnDrawGizmos()
        {
            // 에디터 시간: PlayerMovementExecution 없으면 기본 오른쪽
            // 플레이 시간: 실제 방향 반영
            bool facingLeft = PlayerMovementExecution.Instance != null && PlayerMovementExecution.Instance.FacingLeft;
            Vector2 facingDir = facingLeft ? Vector2.left : Vector2.right;
            Vector2 center = (Vector2)transform.position + (facingDir * hitBoxOffsetX);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, hitBoxSize);
        }
    }
}
