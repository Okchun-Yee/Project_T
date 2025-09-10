public interface IWeapon
{
    public void Initialized();          // 무기 초기화 메서드
    public void Attack();               // 무기 공격 메서드
    public void Skill(int skillIndex);  // 무기 스킬 메서드
    public WeaponSO GetWeaponInfo();    // 무기 정보 반환 메서드
}