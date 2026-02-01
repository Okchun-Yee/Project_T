using System.Collections;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Skills.Runtime;
using ProjectT.Gameplay.Weapon.Projectiles;
using UnityEngine;

namespace ProjectT.Gameplay.Skills.Common.Range
{
    public class Bow_Homing : BaseSkill
    {

        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Transform[] arrowSpawnPoints; // 화살 생성 위치
        [SerializeField] private float projectileRange = 12f; // 화살의 사거리
        [SerializeField] private int numberOfArrows = 3;
        private float arrowInterval = 0.2f; // 화살 발사 간격
        private Animator _animator;
        readonly int FIRE_HASH = Animator.StringToHash("Attack");
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        public override void Execute(in SkillExecutionContext ctx)
        {
            return;
        }

        protected override void OnSkillActivated()
        {
            var controller = PlayerController.Instance;
            if (controller != null)
            {
                float lockDuration = Mathf.Max(0f, numberOfArrows * arrowInterval);
                controller.LockActions(ActionLockFlags.Move | ActionLockFlags.BasicAttack | ActionLockFlags.Dash, lockDuration);
            }
            _animator.SetTrigger(FIRE_HASH);
            StartCoroutine(FireArrowsRoutine());
        }
        private IEnumerator FireArrowsRoutine()
        {
            for (int i = 0; i < numberOfArrows; i++)
            {
                foreach (Transform spawnPoint in arrowSpawnPoints)
                {
                    SpawnArrow(spawnPoint);
                }
                yield return new WaitForSeconds(arrowInterval);
            }
        }

        private void SpawnArrow(Transform spawnPoint)
        {
            GameObject newArrow = Instantiate(arrowPrefab, spawnPoint.position, ActiveWeapon.Instance.transform.rotation);

            newArrow.GetComponent<Projectile>().UpdateProjectileRange(projectileRange);
            newArrow.GetComponent<Projectile>().Initialize(GetSkillDamage());
        }
    }
}
