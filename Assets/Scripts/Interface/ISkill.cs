public interface ISkill
{
    public void Skill_Initialized(SkillSO skillInfo);     // 스킬 초기화 매서드
    public void ActivateSkill(int index = -1);      // 스킬 활성화 매서드
    public void SubscribeSkillEvents();             // 스킬 이벤트 구독
    public void UnsubscribeSkillEvents();           // 스킬 이벤트 구독 해제
    public SkillSO GetSkillInfo();                  // 스킬 정보 접근용 프로퍼티
}
