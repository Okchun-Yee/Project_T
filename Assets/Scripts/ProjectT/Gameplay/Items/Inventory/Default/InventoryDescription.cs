using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectT.Gameplay.Items.Inventory
{
    public class InventoryDescription : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text description;
        [SerializeField] private TMP_Text effects;  // 옵션, null 가능 (Rune 설명용)

        public void Awake()
        {
            ResetDescription();
        }

        public void ResetDescription()
        {
            this.itemImage.gameObject.SetActive(false);
            this.title.text = "";
            this.description.text = "";
            if (effects != null)
            {
                effects.gameObject.SetActive(false);
                effects.text = "";
            }
        }

        public void SetDescription(Sprite sprite, string itemName, string itemDescription)
        {
            SetDescription(sprite, itemName, itemDescription, null);
        }

        public void SetDescription(Sprite sprite, string itemName, string itemDescription, string effectsText)
        {
            this.itemImage.gameObject.SetActive(true);
            this.itemImage.sprite = sprite;
            this.title.text = itemName;
            this.description.text = itemDescription;
            
            if (effects != null)
            {
                effects.gameObject.SetActive(!string.IsNullOrEmpty(effectsText));
                if (!string.IsNullOrEmpty(effectsText))
                    effects.text = effectsText;
            }
        }
    }

}
