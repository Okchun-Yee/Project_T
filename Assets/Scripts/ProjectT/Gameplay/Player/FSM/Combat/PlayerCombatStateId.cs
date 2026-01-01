namespace ProjectT.Gameplay.Player.FSM.Combat
{
    /// <summary>
    /// Player Combat State Identifiers
    /// 
    /// 【isCharged 판정】
    /// - Holding → Attack: true (차징 공격)
    /// - 그 외 → Attack: false (일반 공격)
    /// 
    /// 【이벤트 발행 시점】
    /// - ChargeReachedMax: Charging → Holding 전이 시 1회
    /// - ChargeCanceled: Charging/Holding → None 전이 시 (reason: Hit/Dodge/Pause/Dead)
    /// </summary>
    public enum PlayerCombatStateId
    {
        None,
        Attack,
        Charging,
        Holding
    }
}
