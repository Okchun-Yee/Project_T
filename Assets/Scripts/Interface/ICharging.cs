public interface ICharging
{
    // 스킬 발동 시 이벤트 매서드 (파생 클래스에서 구현)
    //<차징용>
    public void OnChargingCompleted();                                      // 차징 완료 시 호출
    public void OnChargingCanceled();                                       // 차징 취소 시 호출
    public void OnChargingProgress(float elapsed, float duration);          // 차징 중
}
