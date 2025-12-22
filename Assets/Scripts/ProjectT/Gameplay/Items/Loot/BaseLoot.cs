using System.Collections;
using ProjectT.Gameplay.Items.Loot.Contracts;
using UnityEngine;

namespace ProjectT.Gameplay.Items.Loot
{
    public abstract class BaseLoot : MonoBehaviour, ILooting
    {
        [SerializeField] protected LootType lootingType = LootType.Item;   // 기본 아이템 타입
        [SerializeField] protected bool canLoot = true;                     // 픽업 가능 여부
        [SerializeField] protected float lootAnimDuration = 0.3f;           // 아이템 픽업 애니메이션 지속 시간
        protected Collider2D col;
        protected virtual void Awake()
        {
            col = GetComponent<Collider2D>();
        }

        protected void DestroyItem()
        {
            if(col != null)
            {
                col.enabled = false; // 충돌 비활성화
            }
            StartCoroutine(DestroyAnimation());
        }
        protected virtual IEnumerator DestroyAnimation()
        {
            // 아이템 축소 애니메이션
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.zero;
            float t = 0;

            while (t < lootAnimDuration)
            {
                t += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, t / lootAnimDuration);
                yield return null;
            }
            transform.localScale = endScale;

            // 오브젝트 제거
            Destroy(gameObject);
        }
        #region ILooting Implementation
        public virtual bool CanPickup() => canLoot;
        public abstract void Looting();
        public virtual string GetItemType() => gameObject.name;
        public Transform GetTransform() => transform;
        public virtual LootType GetLootingType() => lootingType;
        #endregion
    }
}
