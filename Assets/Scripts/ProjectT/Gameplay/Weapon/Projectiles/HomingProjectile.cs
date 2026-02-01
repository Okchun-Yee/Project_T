using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ProjectT.Gameplay.Weapon.Projectiles
{
    public class HomingProjectile : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float rotationSpeed = 120f;        // 회전 속도
        [SerializeField] private float searchRadius = 5f;           // 적 탐색 반경
        [SerializeField] private float retargetInterval = 0.2f;     // 재탐색 간격
        private float _retargetTimer;

        private Rigidbody2D _rb;
        private Transform _target;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            // 시작 시 가장 가까운 적을 타겟으로 설정
            _target = FindClosestEnemy();
        }
        private void FixedUpdate()
        {
            // 간격동안 계속 타겟 재탐색
            _retargetTimer -= Time.fixedDeltaTime;
            if (_target == null && _retargetTimer <= 0f)
            {
                _target = FindClosestEnemy();
                _retargetTimer = retargetInterval;
            }

            if (_target == null) { if (_rb != null) _rb.angularVelocity = 0f; return; }

            var dir = (_target.position - transform.position).normalized;
            float rotationAmount = Vector3.Cross(dir, transform.right).z;

            if (_rb != null)
                _rb.angularVelocity = -rotationAmount * rotationSpeed;
            else
                transform.Rotate(0f, 0f, -rotationAmount * rotationSpeed * Time.fixedDeltaTime);
        }

        private Transform FindClosestEnemy()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);
            Transform closest = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            foreach (Collider2D enemy in enemies)
            {
                float dist = Vector3.Distance(enemy.transform.position, currentPos);
                if (dist < minDist)
                {
                    closest = enemy.transform;
                    minDist = dist;
                }
            }
            return closest;
        }

        #region  Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
        }
#endif
        #endregion
    }
}