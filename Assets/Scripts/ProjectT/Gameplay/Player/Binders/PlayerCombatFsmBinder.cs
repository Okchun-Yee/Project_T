using ProjectT.Gameplay.Weapon;
using UnityEngine;

namespace ProjectT.Gameplay.Player
{
    public sealed class PlayerCombatFsmBinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _decision; // FSM PlayerController
        [SerializeField] private ActiveWeapon _activeWeapon; // Singleton이라면 null이어도 Awake에서 가져옴

        private bool _charged; // 차징 완료/홀드 여부 (FSM 이벤트로만 갱신)

        private void Awake()
        {
            if (_decision == null) _decision = GetComponent<PlayerController>();
            if (_activeWeapon == null) _activeWeapon = ActiveWeapon.Instance;
        }

        private void OnEnable()
        {
            if (_decision == null) return;

            _decision.AttackStarted += OnAttackStarted;
            _decision.AttackEnded += OnAttackEnded;

            _decision.ChargeStarted += OnChargeStarted;
            _decision.ChargeReachedMax += OnChargeReachedMax;
            _decision.ChargeCanceled += OnChargeCanceled;
            _decision.HoldStarted += OnHoldStarted;
        }

        private void OnDisable()
        {
            if (_decision == null) return;

            _decision.AttackStarted -= OnAttackStarted;
            _decision.AttackEnded -= OnAttackEnded;

            _decision.ChargeStarted -= OnChargeStarted;
            _decision.ChargeReachedMax -= OnChargeReachedMax;
            _decision.ChargeCanceled -= OnChargeCanceled;
            _decision.HoldStarted -= OnHoldStarted;
        }
        private void Update()
        {
            if (_decision == null || _activeWeapon == null) return;

            BaseWeapon bw = _activeWeapon.currentWeapon as BaseWeapon;
            var info = bw != null ? bw.GetWeaponInfo() : null;

            _decision.CanChargeAttack = (info != null && info.chargeDuration > 0f);
        }

        private void OnAttackStarted()
        {
            if (_activeWeapon == null) return;

            _activeWeapon.Fsm_AttackExecute(_charged);
            // 공격이 실행되면 charged는 소비되는 게 자연스러움
            _charged = false;
        }

        private void OnAttackEnded()
        {
            // 지금 단계에서는 Attack 종료 타이밍을 무기/애니 쪽에서 다듬기 전이라
            // 최소 정리만 수행
            _charged = false;
        }

        private void OnChargeStarted()
        {
            _charged = false;

            // 무기 데이터에서 차징 시간 가져오기 (SSOT: weapon)
            var bw = _activeWeapon != null ? _activeWeapon.currentWeapon as BaseWeapon : null;
            var info = bw != null ? bw.GetWeaponInfo() : null;

            if (info == null) return;

            // 공격 차징 시작
            ChargingManager.Instance?.StartCharging(ChargingType.Attack, info.chargeDuration);
        }
        // TODO: subscribe ChargingManager.Completed -> pc.NotifyChargeReachedMax()

        // TODO: set pc.IsChargeMaxReached

        private void OnChargeReachedMax()
        {
            _charged = true;
            // [TODO] 차징 완료 시 효과음 재생 등 피드백 추가
        }

        private void OnHoldStarted()
        {
            _charged = true;
        }

        private void OnChargeCanceled()
        {
            _charged = false;
            _activeWeapon?.Fsm_CancelAction();
        }
    }
}
