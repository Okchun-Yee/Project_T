using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using ProjectT.Core;
using ProjectT.Gameplay.Combat;
using ProjectT.Systems.Camera;
using ProjectT.Systems.UI;
using ProjectT.Gameplay.Enemies;
using ProjectT.Gameplay.Player.Controller;


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
        private Knockback knockback;
        private Flash flash;
        private PlayerController _playerController;

        const string TOWN_TEXT = "GameScene1";
        readonly int DEATH_HASH = Animator.StringToHash("Death");
        protected override void Awake()
        {
            base.Awake();
            knockback = GetComponent<Knockback>();
            flash = GetComponent<Flash>();
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

            ScreenShakeManager.Instance.ShakeScreen();
            knockback.GetKnockedBack(hitTransform, knockBackThrustAmount);
            StartCoroutine(flash.FlashRoutine());

            Debug.Log("[PlayerHealth] Player took damage: " + damageAmount);
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
                
                Destroy(ActiveWeapon.Instance.gameObject);

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
            SceneManager.LoadScene(TOWN_TEXT);
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
