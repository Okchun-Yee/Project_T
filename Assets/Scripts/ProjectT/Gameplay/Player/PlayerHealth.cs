using UnityEngine;
using System.Collections;
using ProjectT.Core;
using ProjectT.Gameplay.Combat;
using ProjectT.Systems.Camera;
using ProjectT.Systems.UI;
using ProjectT.Gameplay.Enemies;
using ProjectT.Gameplay.Weapon;
using ProjectT.Systems.GameMode;
using ProjectT.Systems.Scene;


namespace ProjectT.Gameplay.Player
{
    /// <summary>
    /// Player Health
    /// - 피격/사망 시 PlayerController.ForceHit/ForceDead 호출 (Step 7)
    /// - FSM 상태 변경은 PlayerController가 담당
    /// </summary>
    public class PlayerHealth : Singleton<PlayerHealth>
    {
        public bool isDead { get; private set; }

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 10;
        [Header("Hit Settings")]
        [SerializeField] private float knockBackThrustAmount = 1f;
        [SerializeField] private float damageRecoveryTime = 1f;

        private int currentHealth;
        private bool canTakeDamage = true;
        
        private Knockback _knockback;
        private Flash _flash;
        private Invincibility _invincibility;
        private PlayerController _playerController;

        const string TOWN_TEXT = "GameScene1";
        readonly int DEATH_HASH = Animator.StringToHash("Death");
        protected override void Awake()
        {
            base.Awake();
            _knockback = GetComponent<Knockback>();
            _flash = GetComponent<Flash>();
            _invincibility = GetComponent<Invincibility>();
            currentHealth = maxHealth;
        }
        private void Start()
        {
            isDead = false;
            _playerController = GetComponent<PlayerController>();
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy)
            {
                TakeDamage(1, collision.transform);
            }
        }

        public void HealPlayer(int healAmount=0)
        {
            if (healAmount <= 0)
            {
                if (currentHealth < maxHealth)
                {
                    currentHealth++;
                }
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
            }
            UIManager.Instance.UpdateHealthSlider();
        }
        public void TakeDamage(int damageAmount, Transform hitTransform)
        {
            if (!canTakeDamage) { return; }
            if (_invincibility != null && _invincibility.IsInvincible) { return; }
            Debug.Log($"[TakeDamage] dmg={damageAmount}, from={hitTransform.name}, layer={LayerMask.LayerToName(hitTransform.gameObject.layer)}, root={hitTransform.transform.root.name}");
            ScreenShakeManager.Instance.ShakeScreen();
            _knockback.GetKnockedBack(hitTransform, knockBackThrustAmount);
            StartCoroutine(_flash.FlashRoutine());

            currentHealth -= damageAmount;
            UIManager.Instance.UpdateHealthSlider();
            DamageRecoveryTime();

            // Step 7: FSM 강제 전이는 PlayerController 단일 경로
            if (currentHealth <= 0)
            {
                CheckIfPlayerDeath();
            }
            else
            {
                _playerController?.ForceHit();
            }
        }
        public void DamageRecoveryTime()
        {
            canTakeDamage = false;
            StartCoroutine(DamageRecoveryRoutine());
        }

        private void CheckIfPlayerDeath()
        {
            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                
                // Step 7: FSM 강제 전이는 PlayerController 단일 경로
                _playerController?.ForceDead();
                
                // 무기 해제는 무기 시스템 경유 (SSOT 유지)
                WeaponManager.Instance?.UnequipWeapon();

                currentHealth = 0;
                GetComponent<Animator>().SetTrigger(DEATH_HASH);
                StartCoroutine(DeathLoadSceneRoutine());
            }
        }
        private IEnumerator DeathLoadSceneRoutine()
        {
            yield return new WaitForSeconds(2f);
            Debug.Log("[PlayerHealth] Player has died. Loading town scene.");
            Destroy(gameObject);
            SceneTransitionExecution.Instance?.Request(
                new SceneTransitionRequest(TOWN_TEXT, targetGameMode: GameMode.Town));
        }
        private IEnumerator DamageRecoveryRoutine()
        {
            yield return new WaitForSeconds(damageRecoveryTime);
            canTakeDamage = true;
        }
        public int maxHealthGetter() => maxHealth;
        public int currentHealthGetter() => currentHealth;
    }
}
