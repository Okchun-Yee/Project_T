using System;
using ProjectT.Core.FSM;
using ProjectT.Gameplay.Player.FSM.Combat;
using ProjectT.Gameplay.Weapon;
using UnityEngine;

namespace ProjectT.Gameplay.Player
{
    /// <summary>
    /// Combat FSM (Decision) → Execution 연결자
    /// - FSM.OnStateChanged 콜백에서 즉시 이벤트 발행 (Update 폴링 대신)
    /// - 무기 실행 명령 전달
    /// 
    /// Step 2 보완 (A/B/C):
    /// - A) Update 폴링 → OnStateChanged 콜백으로 전환 (누락/중복 방지)
    /// - B) AttackStarted는 next==Attack이면 항상 발행, isCharged는 prev==Holding일 때만 true
    /// - C) AttackVariant 필드로 향후 콤보/다타 확장 대비
    /// </summary>
    public sealed class PlayerCombatFsmBinder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _decision;
        [SerializeField] private ActiveWeapon _activeWeapon;

        // ============================================================
        // Combat 이벤트 (Binder가 소유, 전이 시점에만 발행)
        // ============================================================
        public event Action<ChargeStartedEvent> ChargeStarted;
        public event Action<ChargeReachedMaxEvent> ChargeReachedMax;
        public event Action<ChargeCanceledEvent> ChargeCanceled;
        public event Action<AttackStartedEvent> AttackStarted;
        public event Action<AttackEndedEvent> AttackEnded;

        private bool _isSubscribed = false;

        private void Awake()
        {
            if (_decision == null) _decision = GetComponent<PlayerController>();
            if (_activeWeapon == null) _activeWeapon = ActiveWeapon.Instance;
        }

        private void OnEnable()
        {
            SubscribeFsmCallback();
        }

        private void OnDisable()
        {
            UnsubscribeFsmCallback();
        }

        private void Update()
        {
            if (_decision == null) return;

            // 무기 설정 동기화만 Update에서 처리
            // 상태 전이 감지는 OnStateChanged 콜백에서 처리
            UpdateWeaponConfig();
        }

        private void SubscribeFsmCallback()
        {
            if (_isSubscribed || _decision == null) return;
            
            // CombatFsm이 아직 생성 안됐으면 Start에서 재시도
            if (_decision.CombatFsm == null) return;

            _decision.CombatFsm.OnStateChanged += OnCombatStateChanged;
            _isSubscribed = true;
        }

        private void UnsubscribeFsmCallback()
        {
            if (!_isSubscribed || _decision == null || _decision.CombatFsm == null) return;

            _decision.CombatFsm.OnStateChanged -= OnCombatStateChanged;
            _isSubscribed = false;
        }

        private void Start()
        {
            // Awake에서 FSM이 아직 생성 안됐을 수 있으므로 Start에서 재시도
            SubscribeFsmCallback();
        }

        private void UpdateWeaponConfig()
        {
            if (_activeWeapon == null) return;

            BaseWeapon bw = _activeWeapon.currentWeapon as BaseWeapon;
            var info = bw != null ? bw.GetWeaponInfo() : null;

            _decision.CanChargeAttack = (info != null && info.chargeDuration > 0f);
        }

        /// <summary>
        /// FSM.OnStateChanged 콜백 핸들러
        /// ChangeState() 직후 동기적으로 호출되어 누락/중복 없이 전이를 감지
        /// </summary>
        private void OnCombatStateChanged(StateChangedEventArgs<PlayerCombatStateId> args)
        {
            var prev = args.PrevStateId;
            var next = args.NextStateId;

            // ChargeStarted: prev != Charging && next == Charging
            if (prev != PlayerCombatStateId.Charging && next == PlayerCombatStateId.Charging)
            {
                EmitChargeStarted();
            }

            // ChargeReachedMax: prev == Charging && next == Holding
            if (prev == PlayerCombatStateId.Charging && next == PlayerCombatStateId.Holding)
            {
                EmitChargeReachedMax();
            }

            // ChargeCanceled: prev in {Charging, Holding} && next not in {Attack, prev}
            // 즉, Charging/Holding에서 Attack이 아닌 다른 상태로 전이 시
            if ((prev == PlayerCombatStateId.Charging || prev == PlayerCombatStateId.Holding) &&
                next != PlayerCombatStateId.Attack && next != prev)
            {
                EmitChargeCanceled(prev, next);
            }

            // AttackStarted: next == Attack (어떤 prev에서든)
            // 보완점 B: "next == Attack이면 항상 발행, isCharged는 prev == Holding일 때만 true"
            if (next == PlayerCombatStateId.Attack)
            {
                EmitAttackStarted(prev);
            }

            // AttackEnded: prev == Attack && next != Attack
            if (prev == PlayerCombatStateId.Attack && next != PlayerCombatStateId.Attack)
            {
                EmitAttackEnded(next);
            }
        }

        #region Event Emission
        private void EmitChargeStarted()
        {
            var evt = new ChargeStartedEvent(0f);
            ChargeStarted?.Invoke(evt);

            // 실행: ChargingManager 시작
            StartChargingExecution();
        }

        private void EmitChargeReachedMax()
        {
            var evt = new ChargeReachedMaxEvent(1f);
            ChargeReachedMax?.Invoke(evt);
        }

        private void EmitChargeCanceled(PlayerCombatStateId prev, PlayerCombatStateId next)
        {
            // reason 결정
            ChargeCancelReason reason = DetermineChargeCancelReason(next);
            float snapshot = ChargingManager.Instance != null ? ChargingManager.Instance.ChargeNormalized : 0f;

            var evt = new ChargeCanceledEvent(reason, snapshot);
            ChargeCanceled?.Invoke(evt);

            // 실행: ChargingManager 종료
            _activeWeapon?.Fsm_CancelAction();
        }

        private void EmitAttackStarted(PlayerCombatStateId prevState)
        {
            // 보완점 B: isCharged는 prev == Holding일 때만 true
            bool isCharged = (prevState == PlayerCombatStateId.Holding);
            
            // 보완점 C: AttackVariant로 공격 종류 구분 (향후 콤보 확장 대비)
            AttackVariant variant = isCharged ? AttackVariant.Charged : AttackVariant.Normal;

            var evt = new AttackStartedEvent(isCharged, variant);
            AttackStarted?.Invoke(evt);

            // 실행: 무기 공격
            _activeWeapon?.Fsm_AttackExecute(isCharged);
        }

        private void EmitAttackEnded(PlayerCombatStateId nextState)
        {
            // endReason 결정
            AttackEndReason reason = DetermineAttackEndReason(nextState);

            var evt = new AttackEndedEvent(reason);
            AttackEnded?.Invoke(evt);
        }
        #endregion

        #region Execution Helpers
        private void StartChargingExecution()
        {
            var bw = _activeWeapon != null ? _activeWeapon.currentWeapon as BaseWeapon : null;
            var info = bw != null ? bw.GetWeaponInfo() : null;

            if (info == null || info.chargeDuration <= 0f) return;

            ChargingManager.Instance?.StartCharging(ChargingType.Attack, info.chargeDuration);
        }

        private ChargeCancelReason DetermineChargeCancelReason(PlayerCombatStateId nextState)
        {
            // TODO: Hit/Dodge/Pause/Dead 판별을 위해 Locomotion 상태 참조 필요
            // 현재는 단순 분류
            return nextState == PlayerCombatStateId.None ? ChargeCancelReason.Other : ChargeCancelReason.Other;
        }

        private AttackEndReason DetermineAttackEndReason(PlayerCombatStateId nextState)
        {
            // None으로 전이 = 정상 종료
            // 다른 상태 = 인터럽트
            return nextState == PlayerCombatStateId.None ? AttackEndReason.Finished : AttackEndReason.Interrupted;
        }
        #endregion
    }
}
