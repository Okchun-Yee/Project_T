using UnityEngine;
// 픽업 타입 열거형
public enum LootType
{
    Weapon,      // 무기
    Item,        // 일반 아이템
    Consumable,  // 소모품
}
public interface ILooting
{
    public bool CanPickup();            // 아이템을 픽업할 수 있는지 여부
    public void Pickup();               // 아이템이 픽업될 때 호출되는 매서드
    public string GetItemType();        // 아이템의 이름 반환 매서드
    public Transform GetTransform();    // 아이템의 Transform 반환 매서드
    public LootType GetLootingType();   // 아이템의 타입 반환 매서드
}
