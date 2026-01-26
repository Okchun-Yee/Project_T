using System;
using ProjectT.Core;
using ProjectT.Gameplay.Player.Input;
using ProjectT.Gameplay.Skills.Contracts;
using ProjectT.Gameplay.Weapon;
using ProjectT.Gameplay.Weapon.Contracts;
using UnityEngine;


namespace ProjectT.Gameplay.Player
{
    public class ActiveWeapon : Singleton<ActiveWeapon>
    {
        private int? currentSkillIndex = 0; // 현재 스킬 인덱스

        public IWeapon currentWeapon; // 현재 활성화된 무기

        protected override void Awake()
        {
            base.Awake();
        }
        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnSkillInput += OnSkillStarted;                 // 스킬 입력 이벤트 구독
                InputManager.Instance.OnSkillCanceled += OnSkillCanceled;                // 스킬 입력 취소 이벤트 구독
            }
        }
        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnSkillInput -= OnSkillStarted;                 // 스킬 입력 이벤트 구독 해제
                InputManager.Instance.OnSkillCanceled -= OnSkillCanceled;                // 스킬 입력 취소 이벤트 구독 해제
            }
        }

        // 무기 상태 초기화 매서드
        public void NewWeapon(IWeapon weapon)
        {
            // 이전 무기의 모든 스킬 구독 해제
            if (currentWeapon is BaseWeapon oldWeapon && currentSkillIndex.HasValue)
            {
                UnsubscribeSkill(currentSkillIndex.Value);
            }
            // 새로운 무기 설정
            currentWeapon = weapon;     // 현재 무기 설정
            currentSkillIndex = null;   // 현재 스킬 인덱스 초기화 (NULLABLE)
        }
        // 무기 상태 초기화 매서드
        public void ClearWeapon()
        {
            currentWeapon = null;
        }
        /// <summary>
        /// FSM에서 공격 진입점
        /// </summary>
        public void Fsm_AttackExecute(bool charged)
        {
            if (currentWeapon == null) return;

            if (currentWeapon is not BaseWeapon bw)
            {
                // 비 BaseWeapon인 경우 경고 로그 출력
                Debug.LogWarning($"[ActiveWeapon] Fsm_AttackExecute called but currentWeapon is not BaseWeapon: {currentWeapon}");
                return;
            }

            bw.ExecuteAttackFromFsm(charged);
        }
        public void Fsm_CancelAction()
        {
            // 기존 private ActionCancel 내용 그대로 호출하게 만들면 됨
            ChargingManager.Instance?.EndCharging();
            HoldingManager.Instance?.EndHolding();
        }

        // 스킬 시전 매서드
        private void OnSkillStarted(int skillIndex)
        {
            if (currentWeapon == null) return;

            // 이전 활성 스킬 구독 해제
            if (currentSkillIndex.HasValue)
            {
                UnsubscribeSkill(currentSkillIndex.Value);
            }

            // 새 스킬 구독
            SubscribeSkill(skillIndex);
            currentSkillIndex = skillIndex;

            // 스킬 사용
            currentWeapon.Skill(skillIndex);
        }

        // 스킬 시전 취소 매서드
        private void OnSkillCanceled(int skillIndex)
        {
            if (currentWeapon == null) return;
            UnsubscribeSkill(skillIndex);           // 해당 스킬 구독 해제

            if (currentSkillIndex == skillIndex)
            {
                currentSkillIndex = null;           // 현재 스킬 인덱스 초기화 (NULLABLE)
            }
            Fsm_CancelAction();                     // 차징 or 홀딩 종료
        }
        // 스킬 구독 매서드
        private void SubscribeSkill(int skillIndex)
        {
            ISkill[] skills = (currentWeapon as BaseWeapon)?.GetSkills();
            if (skills == null || skillIndex < 0 || skillIndex >= skills.Length)
            {
                return;
            }
            skills[skillIndex]?.SubscribeSkillEvents();
        }
        // 스킬 구독 해제 매서드
        private void UnsubscribeSkill(int skillIndex)
        {
            ISkill[] skills = (currentWeapon as BaseWeapon)?.GetSkills();
            if (skills == null || skillIndex < 0 || skillIndex >= skills.Length)
            {
                return;
            }
            skills[skillIndex]?.UnsubscribeSkillEvents();
        }
    }
}
