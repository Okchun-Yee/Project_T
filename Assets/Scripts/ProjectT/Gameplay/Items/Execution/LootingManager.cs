using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProjectT.Core;
using ProjectT.Gameplay.Player.Input;
using ProjectT.Gameplay.Player.Controller;
using ProjectT.Gameplay.Items.Loot.Contracts;

/// <summary>
/// 플레이어 주변의 픽업 아이템을 관리하고, 입력에 따라 아이템을 픽업하는 매니저 클래스
/// </summary>

namespace ProjectT.Gameplay.Items.Execution
{
    public class LootingManager : Singleton<LootingManager>
    {
        [Header("Looting Settings")]
        [SerializeField] private float lootingRange = 2.0f;     // 픽업 범위
        [SerializeField] private Transform playerTransform;     // 플레이어 트랜스폼
        [SerializeField] private LayerMask lootingLayer = -1;   // 픽업 가능 레이어
        // 픽업 이벤트
        public event Action OnWeaponLoot;       // WeaponData 호출 이벤트 
        public event Action OnItemLoot;         // ItemData 호출 이벤트
        public event Action<ILooting> OnLoot;             // 공통 호출 이벤트

        // 현재 범위 내 픽업 가능한 아이템들
        private readonly List<ILooting> nearItem = new List<ILooting>();
        private ILooting closestLoot;

        // 외부 접근용 프로퍼티
        public Transform PlayerTransform => playerTransform;
        public float LootingRange => lootingRange;
        protected override void Awake()
        {
            base.Awake();
            // 플레이어 트랜스폼이 할당되지 않은 경우 자동으로 할당 (fallback)
            if(playerTransform == null && PlayerLegacyController.Instance != null)
            {
                playerTransform = PlayerLegacyController.Instance.transform;
            }
        }

        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLootingInput += HandleLootInput;
            }
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLootingInput -= HandleLootInput;
            }
        }

        private void Update()
        {
            if (PlayerLegacyController.Instance != null)
            {
                UpdateNearbyLoots();
                UpdateClosestLoot();
            }
        }

        // 픽업 입력 처리
        private void HandleLootInput()
        {
            if (closestLoot != null && closestLoot.CanPickup())
            {
                LootItem(closestLoot);
            }
        }

        // 아이템 픽업 실행
        private void LootItem(ILooting item)
        {
            if (item == null) return;

            switch(item.GetLootingType())
            {
                case LootType.Weapon:
                    OnWeaponLoot?.Invoke();
                    break;
                case LootType.Item:
                case LootType.Consumable:
                    OnItemLoot?.Invoke();
                    break;
            }
            OnLoot?.Invoke(item);   // 공통 이벤트 호출
            item.Looting();         // 파생 클래스의 Looting 메서드 호출
            closestLoot = null;     // 가장 가까운 아이템 초기화
        }

        // 플레이어 주변 픽업 아이템 스캔
        private void UpdateNearbyLoots()
        {
            nearItem.Clear();   // 이전 근처 아이템 리스트 초기화

            Vector3 playerPos = PlayerLegacyController.Instance.transform.position;
            Collider2D[] lootingColliders = Physics2D.OverlapCircleAll(playerPos, lootingRange, lootingLayer);

            foreach (Collider2D collider in lootingColliders)
            {
                ILooting pickupable = collider.GetComponent<ILooting>();
                if (pickupable != null && pickupable.CanPickup())
                {
                    nearItem.Add(pickupable);
                }
            }
        }

        // 가장 가까운 픽업 아이템 선택
        private void UpdateClosestLoot()
        {
            if (nearItem.Count == 0)
            {
                closestLoot = null;
                return;
            }

            Vector3 playerPos = PlayerLegacyController.Instance.transform.position;
            float closestDistance = float.MaxValue;
            ILooting closest = null;

            foreach (ILooting loot in nearItem)
            {
                float distance = Vector3.Distance(playerPos, loot.GetTransform().position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = loot;
                }
            }

            closestLoot = closest;
        }

        // UI용 정보 제공
        public ILooting GetClosestLoot() => closestLoot;
        public bool HasNearbyLoots() => nearItem.Count > 0;
        public int GetNearbyLootCount() => nearItem.Count;
        public float GetLootingRange() => lootingRange;

        // 디버그용 시각화
        private void OnDrawGizmosSelected()
        {
            if (PlayerLegacyController.Instance != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(PlayerLegacyController.Instance.transform.position, lootingRange);
            }
        }
    }
}
