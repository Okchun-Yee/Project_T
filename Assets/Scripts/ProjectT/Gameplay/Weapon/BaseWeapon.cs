using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectT.Data.ScriptableObjects.Inventory;
using System;
using ProjectT.Gameplay.Weapon.Contracts;
using ProjectT.Data.ScriptableObjects.Items;
using ProjectT.Gameplay.Skills.Contracts;
using ProjectT.Gameplay.Combat.Damage;
using ProjectT.Gameplay.Skills;

namespace ProjectT.Gameplay.Weapon
{
    public abstract class BaseWeapon : MonoBehaviour, IWeapon
    {
        // SSOT: "공격 중" 여부는 FSM 상태(CombatState == Attack)로 판단
        // isAttacking 플래그 제거됨
        
        public EquippableItemSO weaponInfo { get; private set; }    // 무기 정보

        private Coroutine CooldownCoroutine;                    //무기 공격 쿨다운 코루틴
        protected ISkill[] skills;                              // 무기에 적용된 스킬들

        // SO 에서 주입받는 무기 값 목록
        private float weaponCooldown;                           // SO에서 주입받는 쿨다운 시간
        private bool isCooldown = false;                        //무기 쿨타임 검사 (공격 속도)
        private float[] skillCastingTime;                       // 스킬 시전 시간
        private DamageSource ds;                                // 데미지 소스 컴포넌트

        /// <summary>
        /// 초기화 진입점 매서드 (weaponSO & skillSO 주입)
        /// 파생 클래스의 무기 SO, 스킬 SO 초기화 매서드 호출
        /// </summary>
        public virtual void Weapon_Initialize(EquippableItemSO info)
        {
            if (info == null)
            {
                Debug.LogError($"[BaseWeapon] WeaponInfo is null on {name}");
                return;
            }
            WeaponInitialization(info); // 무기 초기화
            SkillInitialization(info);  // 스킬 초기화
        }
        // 무기 초기화 매서드
        private void WeaponInitialization(EquippableItemSO info)
        {
            // 1) 무기 정보 주입
            weaponInfo = info;
            weaponCooldown = info.weaponCooldown; 
            // 장착(초기화) 시점에 이전 코루틴이 남아있을 수 있으므로 쿨다운 초기화
            if (CooldownCoroutine != null)
            {
                try { StopCoroutine(CooldownCoroutine); } catch { }
                CooldownCoroutine = null;
            }
            isCooldown = false;
        }
        // 스킬 초기화 매서드
        private void SkillInitialization(EquippableItemSO info)
        {
            // 1) 스킬 배열 초기화
            skills = GetComponents<ISkill>();               // ISkill 인터페이스를 구현한 모든 컴포넌트 가져오기
            skillCastingTime = new float[skills.Length];    // 스킬 시전 시간 배열 초기화
            // 스킬 개수 검증
            if (skills.Length != info.skillInfos.Length)
                Debug.LogWarning($"[BaseWeapon] SkillInfo length ({info.skillInfos.Length}) != ISkill components ({skills.Length}) on {name}");

            // 2) 각 스킬에 정보 주입 및 시전 시간 저장
            for (int i = 0; i < skills.Length; i++)
            {
                skills[i].Skill_Initialize(info.skillInfos[i]);             // 스킬 초기화
                skillCastingTime[i] = info.skillInfos[i].chargingTime;      // 캐스팅 시간 설정

                // 스킬 인덱스 자동 설정
                if (skills[i] is BaseSkill baseSkill)
                {
                    baseSkill.skillIndex = i;
                }
            }
            // 3) UI 매니저에 스킬 정보 전달
            //SkillUIManager.Instance.Initialized(info.Skills);
        }

        // 공격 진입점 매서드
        [Obsolete("Do not call Attack() from FSM path. Use ExecuteAttackFromFsm(charged) via binder.", false)]
        public void Attack()
        {
            throw new NotImplementedException("BaseWeapon.Attack() is obsolete. Use ExecuteAttackFromFsm(bool charged) instead.");
        }
        // 새 헬퍼: 무기가 (차징 취소 시) 직접 즉시 공격을 트리거할 때 사용.
        // ForceAttack은 공격 쿨다운 검사를 포함하고 CooldownRoutine을 시작합니다.
        protected void _Attack()
        {
            if (isCooldown) { return; }
            OnAttack();
            CooldownCoroutine = StartCoroutine(CooldownRoutine());
        }
        protected void _AttackCharged()
        {
            if (isCooldown) { return; }
            OnAttack_Charged();
            CooldownCoroutine = StartCoroutine(CooldownRoutine());
        }
        /// <summary>
        /// FSM에서 공격 진입점
        /// </summary>
        public void ExecuteAttackFromFsm(bool charged)
        {
            if (isCooldown)
            {
                return;
            }
            if (charged) _AttackCharged();
            else _Attack();
        }

        // 무기 쿨다운 코루틴
        private IEnumerator CooldownRoutine()
        {
            isCooldown = true;
            yield return new WaitForSeconds(weaponCooldown);
            isCooldown = false;
        }
        // 객체 비활성 시 처리 매서드 (코루틴 정리)
        protected virtual void OnDisable()
        {
            // 쿨다운 코루틴 정리만 남김
            if (CooldownCoroutine != null)
                StopCoroutine(CooldownCoroutine);
        }
        // 스킬 진입점 매서드
        public void Skill(int skillIndex)
        {
            // 1) 유효한 스킬 인덱스 검사
            if (skillIndex < 0 || skillIndex >= skills.Length)
            {
                Debug.LogError($"[BaseWeapon] Invalid skill index {skillIndex} on {name}");
                return;
            }
            // 2) 스킬 시전 매서드 호출
            skills[skillIndex].ActivateSkill();
        }
        // 무기의 스킬 정보 반환 매서드
        public ISkill[] GetSkills() => skills;
        public EquippableItemSO GetWeaponInfo() => weaponInfo;    // 무기 정보 반환 메서드

        // 추상 매서드
        // 파생 무기 클래스에서 구현할 공격 매서드
        protected abstract void OnAttack();
        // 추상 매서드 (차징 공격용)
        protected abstract void OnAttack_Charged();
    }
}
