using System.Collections;
using ProjectT.Gameplay.Combat;
using ProjectT.Gameplay.Player.Controller;
using UnityEngine;

namespace ProjectT.Gameplay.Enemies
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 1;
        [SerializeField] private GameObject deathVFXPrefab;
        [SerializeField] private float knockBackThrust = 15f;

        // 피격 이펙트
        private Knockback knockback;                            // 피격 시 밀림
        private Flash flash;                                    // 피격 시 깜빡임

        public float currentHealth { get; private set; }        // 현재 체력

        private void Awake()
        {
            flash = GetComponent<Flash>();
            knockback = GetComponent<Knockback>();
        }
        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            // 1) 현재 체력 감소
            currentHealth -= damage;
            Debug.Log($"[EnemyHealth] Took {damage} damage");       // 무기 데미지 체크

            // 2) 피격 시  이벤트 호출 (넉백, 색 변경..)
            knockback.GetKnockedBack(PlayerMovementExecution.Instance.transform, knockBackThrust);
            StartCoroutine(flash.FlashRoutine());

            // 3) 사망 처리 검사
            StartCoroutine(CheckDetectDeathRoutine());
        }
        private IEnumerator CheckDetectDeathRoutine()
        {
            yield return new WaitForSeconds(flash.GetRestoreMatTime());
            DetectDeath();
        }
        public void DetectDeath()
        {
            // 1) 현재 체력이 0 이하인 경우 사망
            if (currentHealth <= 0)
            {
                // 2) 사망 이펙트 인스턴스 생성
                if(deathVFXPrefab != null)
                {
                    Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
                }

                // 3) 아이템 드랍 및 사후 처리

                // 4) 오브젝트 파괴
                Destroy(gameObject);
            }
        } 
    }
}
