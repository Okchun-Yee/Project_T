namespace ProjectT.Gameplay.Weapon
{
    /// <summary>
    /// 차징 값 제공 인터페이스 (Step 8)
    /// - Guard/State가 전역 참조 없이 차징 값에 접근
    /// - Context를 통해 주입받아 사용
    /// </summary>
    public interface IChargingProvider
    {
        bool IsCharging { get; }
        float ChargeElapsed { get; }
        float ChargeDuration { get; }
        float ChargeNormalized { get; }
    }
}
