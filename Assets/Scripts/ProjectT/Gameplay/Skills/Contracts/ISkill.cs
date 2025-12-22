using ProjectT.Data.ScriptableObjects.Skills;

namespace ProjectT.Gameplay.Skills.Contracts
{
    public interface ISkill
    {
        public void Skill_Initialize(SkillSO skillInfo);   // 스킬 초기화 매서드
        public void ActivateSkill(int index = -1);          // 스킬 활성화 매서드
        public void SubscribeSkillEvents();                 // 스킬 이벤트 구독
        public void UnsubscribeSkillEvents();               // 스킬 이벤트 구독 해제
        public SkillSO GetSkillInfo();                      // 스킬 정보 접근용 프로퍼티
        public float GetWeaponDamage();                     // 무기 공격력 접근용 매서드
        public float GetSkillDamage();                      // 스킬 공격력 계산 & 접근용 매서드
    }
}
