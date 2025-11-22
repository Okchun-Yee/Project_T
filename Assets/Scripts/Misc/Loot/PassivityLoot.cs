using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.Model;

public class PassivityLoot : MonoBehaviour
{
    [field: SerializeField] public ItemSO InventoryItem { get; private set; }
    [field: SerializeField] public int Quantity { get; set; } = 1;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float duration = 0.3f;

     private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = InventoryItem.Itemimage;
    }

    public void Item_Pickup(InventorySO inventory)
    {
        // 1) 인벤토리에 아이템 추가 요청
        int reminder = inventory.AddItem(InventoryItem, Quantity);
        if (reminder == 0)
        {
            DestroyItem();
        }
        else
        {
            Quantity = reminder; // 남은 수량 업데이트
        }
    }
    public void DestroyItem()
    {
        GetComponent<Collider2D>().enabled = false; // 충돌 비활성화
        StartCoroutine(AnimateItemPickup());
    }
    private IEnumerator AnimateItemPickup()
    {
        // 1) 획득 효과 재생 (사운드, 파티클)
        //pickUpSound.Play();

        // 아이템 오브젝트를 천천히 사라지게 함
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        
        // 아이템 축소 애니메이션
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        transform.localScale = endScale;

        // 3) 즉시 제거하여 리소스 최소화
        Destroy(gameObject);
    }
}
