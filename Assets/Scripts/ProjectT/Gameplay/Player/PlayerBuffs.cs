using System;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Weapon;
using UnityEngine;

namespace ProjectT.Gameplay.Player
{
    /// <summary>
    /// Runtime SSOT: 버프 상태 단일 소스
    /// - 버프 없으면 damageMul/rangeMul은 항상 1f
    /// - Refresh 형식 고정
    /// </summary>
    public class PlayerBuffs : MonoBehaviour
    {
        private ActiveWeapon _activeWeapon;
        private bool _boundActiveWeapon  = false;

        [Header("Runtime Buff States")]
        [SerializeField] private bool _hasBuff = false;
        [SerializeField] private float _remaining = 0f;
        [SerializeField] private float _duration = 0f;

        [SerializeField] private float _damageMul = 1f;
        [SerializeField] private float _rangeMul = 1f;


        [SerializeField] private float _rampDuration = 0f;
        [SerializeField] private float _elapsed = 0f;
        [SerializeField] private AnimationCurve _rampCurve = null;

        // 프로퍼티 목록
        public bool HasBuff => _hasBuff;
        public float Remaining => _remaining;

        // 현재 프레임 기준 데미지 배율(버프 없으면 1)
        public float DamageMultiplier => _hasBuff ? EvaluateMultiplier(_damageMul) : 1f;
        // 현재 프레임 기준 범위 배율(버프 없으면 1)
        public float RangeMultiplier => _hasBuff ? EvaluateMultiplier(_rangeMul) : 1f;
        private void OnEnable()
        {
            TryBindActiveWeapon();
        }
        private void OnDisable()
        {
            UnbindActiveWeapon();
        }
        private void TryBindActiveWeapon()
        {
            if(_boundActiveWeapon) return;
            if(ActiveWeapon.Instance == null) return;

            _activeWeapon = ActiveWeapon.Instance;
            _activeWeapon.OnRuntimeWeaponChanged += HandleRuntimeWeaponChanged;
            _activeWeapon.OnRuntimeWeaponCleared += HandleRuntimeWeaponCleared;
            _boundActiveWeapon = true;
        }

        private void UnbindActiveWeapon()
        {
            if(!_boundActiveWeapon || _activeWeapon == null) return;
            _activeWeapon.OnRuntimeWeaponChanged -= HandleRuntimeWeaponChanged;
            _activeWeapon.OnRuntimeWeaponCleared -= HandleRuntimeWeaponCleared;
            _activeWeapon = null;
            _boundActiveWeapon = false;
        }

        private void HandleRuntimeWeaponChanged(BaseWeapon weapon)
        {
            Clear();
        }

        private void HandleRuntimeWeaponCleared()
        {
            Clear();
        }

        private void Update()
        {
            TryBindActiveWeapon(); // 1회 시도
            
            if (!_hasBuff) return;

            float dt = Time.deltaTime;
            _elapsed += dt;
            _remaining -= dt;

            if(_remaining <= 0f)
            {
                Clear();
            }
        }
        /// <summary>
        /// 버프 적용.
        /// - SkillSO의 버프 파라미터 읽기
        /// - 기존 버프가 있으면 Refresh
        /// - 버프 스킬이 아니거나, duration이 0이하면 무시
        /// </summary>
        public void ApplyFromSkill(SkillSO skill)
        {
            if (skill == null) return;
            if (!skill.isBuff) return;
            if (skill.buffDuration <= 0f) return;

            _hasBuff = true;
            _duration = skill.buffDuration;
            _remaining = skill.buffDuration;
            _elapsed = 0f;

            // 버프 수치 설정
            _damageMul = Mathf.Max(1f, skill.buffDamageMultiplier);
            _rangeMul  = Mathf.Max(1f, skill.buffRangeMultiplier);


            _rampDuration = Mathf.Max(0f, skill.buffRampDuration);
            _rampCurve = skill.buffRampCurve; // null 가능
        }

        public void Clear()
        {
            // 버프 종료
            _hasBuff = false;
            _remaining = 0f;
            _duration = 0f;
            _elapsed = 0f;

            _damageMul = 1f;
            _rangeMul = 1f;
            _rampDuration = 0f;
            _rampCurve = null;
        }
        #region Apply Methods
        // 최종 데미지 계산. 버프 배율 1회만 계산
        public float ApplyDamage(float baseDamage)
        {
            return baseDamage * DamageMultiplier;
        }
        // 최종 사거리 계산. 버프 배율 1회만 계산
        public Vector3 ApplyRange(Vector3 baseRange)
        {
            return baseRange * RangeMultiplier;
        }
        #endregion
        public float EvaluateMultiplier(float targetMul)
        {
            // 0이면 즉시 적용
            if (_rampDuration <= 0f) return targetMul;

            float t = Mathf.Clamp01(_elapsed / _rampDuration);
            float eased = (_rampCurve != null) ? _rampCurve.Evaluate(t) : t; // 선형 보간 기본

            return Mathf.Lerp(1f, targetMul, eased);
        }
    }
}
