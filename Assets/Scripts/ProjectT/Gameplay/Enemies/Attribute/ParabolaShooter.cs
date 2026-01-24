using ProjectT.Gameplay.Enemies.Contracts;
using ProjectT.Gameplay.Player.Controller;
using ProjectT.Gameplay.Weapon.Projectiles;
using UnityEngine;

namespace ProjectT.Gameplay.Enemies.Attribute
{
    public class ParabolaShooter : MonoBehaviour, IEnemy
    {
        [SerializeField] private GameObject parabolaProjectilePrefab;       // 투사체 프리팹
        [SerializeField] private float bulletDamage = 2f;                   // 투사체 피해량

        private Animator anim;
        private SpriteRenderer spriteRenderer;
        readonly int ATTACK_HASH = Animator.StringToHash("Attack");

        private void Awake()
        {
            anim = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        public void Attack()
        {
            anim.SetTrigger(ATTACK_HASH);
            if (transform.position.x - PlayerMovementExecution.Instance.transform.position.x < 0)
            {
                spriteRenderer.flipX = false;
            }
            else
            {
                spriteRenderer.flipX = true;
            }
        }
        // 애니메이션 키프레임 이벤트에서 호출
        public void SpawnProjectileAnimEvent()
        {
            // 투사체 생성
            GameObject pj = Instantiate(parabolaProjectilePrefab, transform.position, Quaternion.identity);
            // parabolaProjectile 자체에는 데미지가 존재 X => 이후 LandingEffect에서 데미지 적용을 위한 전달 용도
            pj.GetComponent<ParabolaProjectile>().Initialize(bulletDamage); // 데미지 설정 
        }
    }
}
