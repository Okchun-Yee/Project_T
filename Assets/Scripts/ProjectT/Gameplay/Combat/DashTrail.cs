using System.Collections;
using System.Collections.Generic;
using ProjectT.Gameplay.Combat.Damage;
using ProjectT.Gameplay.Enemies;
using UnityEngine;

namespace ProjectT.Gameplay.Combat
{
    /// <summary>
    /// Dash 트레일 효과 관리 컴포넌트
    /// Ghost 효과와 동일한 패턴으로 duration 기반 자동 ON/OFF
    /// isTrigger=true로 설정하여 통과하면서 데미지를 줌
    /// </summary>
    public class DashTrail : MonoBehaviour
    {
        private TrailRenderer[] _trailRenderers;
        private Collider2D _collider;
        private DamageSource _damageSource;
        private HashSet<EnemyHealth> _damagedEnemies = new HashSet<EnemyHealth>();  // 중복 데미지 방지

        private void Awake()
        {
            _trailRenderers = GetComponentsInChildren<TrailRenderer>();
            // 초기 상태: Trail OFF
            if (_trailRenderers != null)
            {
                foreach (var tr in _trailRenderers)
                {
                    tr.emitting = false;
                }
            }

            _collider = GetComponent<Collider2D>();
            _damageSource = GetComponent<DamageSource>();
        }

        /// <summary>
        /// Trail 효과를 특정 시간동안만 실행하는 메서드
        /// </summary>
        public void StartTrailEffect(float duration, float damage = 0)
        {
            if (_trailRenderers == null || _trailRenderers.Length == 0)
            {
                Debug.LogWarning("[DashTrail] No TrailRenderer found in children");
                return;
            }
            
            // Collider isTrigger 확인
            if (_collider != null && !_collider.isTrigger)
            {
                Debug.LogWarning("[DashTrail] Collider isTrigger must be true for dash trail damage");
            }
            
            if (_damageSource != null && damage > 0)
            {
                _damageSource.SetDamage(damage);
                _collider.enabled = true;
            }
            
            _damagedEnemies.Clear();  // 데미지 기록 초기화
            StartCoroutine(TrailEffectRoutine(duration));
        }

        private IEnumerator TrailEffectRoutine(float duration)
        {
            // Trail ON
            foreach (var tr in _trailRenderers)
            {
                tr.emitting = true;
            }
            
            yield return new WaitForSeconds(duration);
            
            // Trail OFF
            foreach (var tr in _trailRenderers)
            {
                tr.emitting = false;
            }

            if (_damageSource != null)
            {
                _collider.enabled = false;
            }
        }

        /// <summary>
        /// Trigger 충돌 시 적에게 데미지 (isTrigger=true여야 함)
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_damageSource == null) return;
            
            // Enemy Layer 확인
            if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (collision.TryGetComponent<EnemyHealth>(out var enemyHealth))
                {
                    // 중복 데미지 방지
                    if (!_damagedEnemies.Contains(enemyHealth))
                    {
                        _damageSource.InstantDamage(_damageSource.DamageAmount, enemyHealth);
                        _damagedEnemies.Add(enemyHealth);
                        Debug.Log($"[DashTrail] Hit {collision.name} for {_damageSource.DamageAmount} damage");
                    }
                }
            }
        }
    }
}
