using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using ProjectT.Core;
using ProjectT.Gameplay.Combat;
using ProjectT.Systems.Camera;
using ProjectT.Systems.UI;


namespace ProjectT.Gameplay.Player
{
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

            CheckIfPlayerDeath();
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
