public interface ICharging
{
    // 스킬 발동 시 이벤트 매서드 (파생 클래스에서 구현)
    //<차징용>
    public void OnChargingCompleted(ChargingType type);                                      // 차징 완료 시 호출
    public void OnChargingCanceled(ChargingType type);                                       // 차징 취소 시 호출
    public void OnChargingProgress(ChargingType type, float elapsed, float duration);        // 차징 중
}
