using System;
using System.Collections.Generic;
using ProjectT.Data.ScriptableObjects.Items;
using ProjectT.Data.ScriptableObjects.Skills;
using ProjectT.Gameplay.Combat.Damage;
using ProjectT.Gameplay.Player;
using ProjectT.Gameplay.Skills.Contracts;
using ProjectT.Gameplay.Weapon;
using UnityEngine;

namespace ProjectT.Gameplay.Skills
{
    public abstract class BaseSkill : MonoBehaviour, ISkill
    {
        /// <summary>
        /// Cooldown SSOT (Runtime)
        /// </summary>
        [SerializeField] private bool _isOnCooldown = false;
        [SerializeField] private float _cooldownRemaining = 0f;

        public SkillSO skillInfo { get; private set; }
        private EquippableItemSO weaponData;
        protected PlayerBuffs _buffs;

        [HideInInspector] public int skillIndex;

        /// <summary>
        /// Cooldown 프로퍼티 
        /// </summary>
        public float CooldownDuration => skillInfo != null ? skillInfo.skillCooldown : 0f;  // SkillSO 기반 쿨다운 시간
        public float CooldownRemaining => _cooldownRemaining;   // 남은 쿨다운 시간
        public bool IsOnCooldown => _isOnCooldown;              // 현재 쿨다운 상태
        public Sprite Icon => skillInfo != null ? skillInfo.icon : null;    // SkillSO 기반 스킬 아이콘. UI용

        private void Update()
        {
            TickCooldown(Time.deltaTime);
        }
        public virtual void Skill_Initialize(SkillSO info, EquippableItemSO weapon)
        {
            if (info == null || weapon == null)
            {
                Debug.LogError($"[BaseSkill] SkillInfo or WeaponInfo is null on {name}");
                return;
            }
            this.skillInfo = info;
            this.weaponData = weapon;
            if(_buffs == null) _buffs = GetComponentInParent<PlayerBuffs>();    // BaseSkill에서 PlayerBuffs 캐싱
        }
        public virtual void ActivateSkill(int index = -1)
        {
            if (IsOnCooldown) return; // 쿨타임 중이면 무시

            // 1) 인덱스가 전달되면 skillIndex 업데이트
            if (index >= 0)
            {
                skillIndex = index;
            }

            // 2) 스킬 카테고리에 따른 처리 
            if (skillInfo.skillCategory == SkillCategory.Charging)      // 차징 스킬
            {
                SubscribeSkillEvents();
                ChargingManager.Instance.StartCharging(ChargingType.Skill, skillInfo.chargingTime);
            }
            else if (skillInfo.skillCategory == SkillCategory.Holding)  // 홀딩 스킬
            {
                SubscribeSkillEvents();
                HoldingManager.Instance.StartHolding(HoldingType.Skill, skillInfo.chargingTime);
            }
            else // 즉시 발동 스킬
            {
                ChargingManager.Instance.SetType(ChargingType.Skill);
                HoldingManager.Instance.SetType(HoldingType.Skill);
                OnSkill();
            }
        }
        public void OnSkill()
        {
            if(IsOnCooldown) return; // 쿨타임 중이면 무시

            /// <summary>
            /// 스킬 활성화 진입점
            /// Perform skill은 ActivateSkill()에서 바로 OnSkill()을 호출
            /// Charging/Holding 스킬은 ChargingManager/HoldingManager에서 완료 콜백으로 OnSkill()을 호출
            /// </summary>
            ExecuteSkill();
        }
        /// <summary>
        /// 실제 발동 단일 진입점
        /// * 쿨타임 시작 시점은 "발동 시점" 으로 통일
        /// </summary>
        protected void ExecuteSkill()
        {
            if(IsOnCooldown) return; // 쿨타임 중이면 무시

            StartCooldown();   // 쿨타임 시작
            OnSkillActivated(); // 파생 클래스에서 구체화된 스킬 발동 매

            // 이벤트 누수/중복 방지
            UnsubscribeSkillEvents();
        }

        protected void StartCooldown()
        {
            _isOnCooldown = true;
            _cooldownRemaining = CooldownDuration;
        }

        /// <summary>
        /// 쿨타임 감소 처리(Update 폴링)
        /// </summary>
        protected void TickCooldown(float deltaTime)
        {
            if (!_isOnCooldown) return;

            if(_cooldownRemaining <= 0f)
            {
                _isOnCooldown = false;
                _cooldownRemaining = 0f;
                return;
            }
            _cooldownRemaining -= deltaTime;
            if(_cooldownRemaining <= 0f)
            {
                _isOnCooldown = false;
                _cooldownRemaining = 0f;
            }
        }

        // 스킬 이벤트 구독
        public virtual void SubscribeSkillEvents()
        {
            // 각 스킬 타입별로 오버라이드에서 구현
        }

        // 스킬 이벤트 해제
        public virtual void UnsubscribeSkillEvents()
        {
            // 각 스킬 타입별로 오버라이드에서 구현
        }

        public float GetWeaponDamage()
        {
            if (weaponData == null)
            {
                Debug.LogWarning($"[BaseSkill] WeaponInfo is null on {name}");
                return 0f;
            }
            return weaponData.weaponDamage;
        }

        public float GetSkillDamage()
        {
            float weaponDamage = GetWeaponDamage();
            float rawDamage = weaponDamage * skillInfo.skillDamage;
            
            if(_buffs == null) _buffs = GetComponentInParent<PlayerBuffs>();
            float Damage = (_buffs != null) ? _buffs.ApplyDamage(rawDamage) : rawDamage;

            return Damage;
        }

        // VFX & Projectile에 데미지 설정
        public void SetupDamageSource(GameObject target, float damage)
        {
            DamageSource damageSource = target.GetComponent<DamageSource>();
            damageSource?.SetDamage(damage);
        }
        // 스킬 데이터 반환 매서드
        public SkillSO GetSkillInfo() => skillInfo;

        // 추상 매서드 정리
        // 파생 클래스에서 구체화할 스킬 매서드
        protected abstract void OnSkillActivated();
    }
}
