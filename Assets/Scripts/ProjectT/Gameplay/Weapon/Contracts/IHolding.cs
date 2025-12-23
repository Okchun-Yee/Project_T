namespace ProjectT.Gameplay.Weapon.Contracts
{
    public interface IHolding
    {
        // <홀딩용>
        public void OnHoldingStarted(float maxDuration);                        // 홀딩 시작 시 호출
        public void OnHoldingEnded();                                           // 홀딩 종료 시 호출
        public void OnHoldingProgress(float elapsed, float duration);           // 홀딩 중
        public void OnHoldingCanceled();                                        // 홀딩 시간이 최대치에 도달했을 때 호출
    }
}
