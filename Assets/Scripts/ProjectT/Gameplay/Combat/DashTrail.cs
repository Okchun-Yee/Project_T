using System.Collections;
using UnityEngine;

namespace ProjectT.Gameplay.Combat
{
    /// <summary>
    /// Dash 트레일 효과 관리 컴포넌트
    /// Ghost 효과와 동일한 패턴으로 duration 기반 자동 ON/OFF
    /// </summary>
    public class DashTrail : MonoBehaviour
    {
        private TrailRenderer[] _trailRenderers;

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
        }

        /// <summary>
        /// Trail 효과를 특정 시간동안만 실행하는 메서드
        /// </summary>
        public void StartTrailEffect(float duration)
        {
            if (_trailRenderers == null || _trailRenderers.Length == 0)
            {
                Debug.LogWarning("[DashTrail] No TrailRenderer found in children");
                return;
            }
            
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
        }
    }
}
