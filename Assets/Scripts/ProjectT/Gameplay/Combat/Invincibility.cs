using System.Collections;
using UnityEngine;

namespace ProjectT.Gameplay.Combat
{
    /// <summary>
    /// 플레이어 무적 관리 (Layer 기반)
    /// - 적 공격은 무시
    /// - 벽/구조물 충돌은 유지
    /// </summary>
    public class Invincibility : MonoBehaviour
    {
        private int _originalLayer;
        private int _invincibleLayer;
        private bool _isInvincible = false;
        private Coroutine _invincibilityCoroutine;

        public bool IsInvincible => _isInvincible;

        private void Awake()
        {
            _originalLayer = gameObject.layer;
            _invincibleLayer = LayerMask.NameToLayer("PlayerInvincible");
            
            if (_invincibleLayer == -1)
            {
                Debug.LogError("[Invincibility] 'PlayerInvincible' layer not found! Please add it in Tags & Layers.");
            }
        }

        /// <summary>
        /// 무적 효과를 특정 시간동안만 실행하는 메서드
        /// </summary>
        public void StartInvincibility(float duration)
        {
            if( _invincibilityCoroutine != null)
            {
                StopCoroutine(_invincibilityCoroutine);
            }
            if (_invincibleLayer == -1)
            {
                Debug.LogWarning("[Invincibility] Cannot start invincibility - layer not configured");
                return;
            }
            
            _invincibilityCoroutine = StartCoroutine(InvincibilityRoutine(duration));
        }

        private IEnumerator InvincibilityRoutine(float duration)
        {
            // Layer 전환 (적 공격 무시, 벽 충돌 유지)
            _isInvincible = true;
            gameObject.layer = _invincibleLayer;
            Debug.Log($"[Invincibility] ON - Layer changed to {_invincibleLayer} for {duration}s");
            
            yield return new WaitForSeconds(duration);
            
            // Layer 복구
            _isInvincible = false;
            gameObject.layer = _originalLayer;
            _invincibilityCoroutine = null;
            Debug.Log($"[Invincibility] OFF - Layer restored to {_originalLayer}");
        }
    }
}
