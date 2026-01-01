namespace ProjectT.Gameplay.Player
{
    /// <summary>
    /// 차징 취소 이유
    /// ChargeCanceled 이벤트 발행 시 취소 원인을 명시
    /// </summary>
    public enum ChargeCancelReason { Hit, Dodge, Pause, Dead, Other }

    /// <summary>
    /// 공격 종료 이유
    /// AttackEnded 이벤트 발행 시 종료 원인을 명시
    /// - Finished: 정상 종료 (Attack → None)
    /// - Interrupted: 인터럽트 (Hit/Dodge 등으로 중단)
    /// - Forced: 강제 종료 (Dead/Pause 등)
    /// </summary>
    public enum AttackEndReason { Finished, Interrupted, Forced }

    /// <summary>
    /// 공격 종류
    /// 향후 콤보/다타/재진입 확장을 위한 Attack 변형 구분
    /// - Normal: 일반 공격 (차징 안함)
    /// - Charged: 차징 공격 (Holding에서 Release)
    /// - Combo1/2/3: 콤보 공격 (향후 확장용)
    /// </summary>
    public enum AttackVariant
    {
        Normal = 0,
        Charged = 1,
        // 향후 콤보 확장용
        Combo1 = 10,
        Combo2 = 11,
        Combo3 = 12,
    }

    /// <summary>
    /// 차징 시작 이벤트 데이터
    /// 발행 조건: prev != Charging && next == Charging
    /// chargeNormalized: 항상 0 (시작 시점)
    /// </summary>
    public readonly struct ChargeStartedEvent 
    {
        public readonly float chargeNormalized;
        
        public ChargeStartedEvent(float normalized = 0f)
        {
            chargeNormalized = normalized;
        }
    }

    /// <summary>
    /// 차징 완료(Max 도달) 이벤트 데이터
    /// 발행 조건: prev == Charging && next == Holding
    /// 일회성 이벤트 - Charging→Holding 전이 시 정확히 1회만 발행
    /// chargeNormalized: 항상 1 (완료 시점)
    /// </summary>
    public readonly struct ChargeReachedMaxEvent 
    {
        public readonly float chargeNormalized;
        
        public ChargeReachedMaxEvent(float normalized = 1f)
        {
            chargeNormalized = normalized;
        }
    }

    /// <summary>
    /// 차징 취소 이벤트 데이터
    /// 발행 조건: prev in {Charging, Holding} && next ∉ {Attack, prev}
    /// reason: 취소 원인 (Hit/Dodge/Pause/Dead/Other)
    /// chargeNormalizedSnapshot: 취소 시점의 차징 진행률 스냅샷
    /// </summary>
    public readonly struct ChargeCanceledEvent 
    {
        public readonly ChargeCancelReason reason;
        public readonly float chargeNormalizedSnapshot;
        
        public ChargeCanceledEvent(ChargeCancelReason reason, float snapshot)
        {
            this.reason = reason;
            this.chargeNormalizedSnapshot = snapshot;
        }
    }

    /// <summary>
    /// 공격 시작 이벤트 데이터
    /// 발행 조건: next == Attack (어떤 prev에서든)
    /// - Normal/Charged는 variant 필드로 구분
    /// - Attack 진입 1회당 정확히 1회만 발행됨을 보장
    /// </summary>
    public readonly struct AttackStartedEvent 
    {
        /// <summary>
        /// 차징 공격 여부 (하위 호환용, variant로도 판별 가능)
        /// </summary>
        public readonly bool isCharged;
        
        /// <summary>
        /// 공격 종류 (Normal/Charged/Combo 등)
        /// 향후 콤보 시스템 확장 시 이 필드로 구분
        /// </summary>
        public readonly AttackVariant variant;
        
        public AttackStartedEvent(bool charged, AttackVariant variant = AttackVariant.Normal)
        {
            isCharged = charged;
            this.variant = variant;
        }
        
        /// <summary>
        /// 하위 호환 생성자 (isCharged만으로 생성)
        /// </summary>
        public AttackStartedEvent(bool charged) 
            : this(charged, charged ? AttackVariant.Charged : AttackVariant.Normal)
        {
        }
    }

    /// <summary>
    /// 공격 종료 이벤트 데이터
    /// 발행 조건: prev == Attack && next != Attack
    /// endReason: 종료 원인 (Finished/Interrupted/Forced)
    /// </summary>
    public readonly struct AttackEndedEvent 
    {
        public readonly AttackEndReason endReason;
        
        public AttackEndedEvent(AttackEndReason reason)
        {
            endReason = reason;
        }
    }
}
