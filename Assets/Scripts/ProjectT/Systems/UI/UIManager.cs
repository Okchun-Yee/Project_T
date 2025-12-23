using ProjectT.Core;
using ProjectT.Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace ProjectT.Systems.UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private Slider healthSlider;
        public TextMeshProUGUI hpText; //hp 표시 텍스트
        private int maxHealth;
        const string HEALTH_SLIDER_TEXT = "Health Bar";

        protected override void Awake()
        {
            base.Awake();
            if (healthSlider == null)
            {
                healthSlider = GameObject.Find(HEALTH_SLIDER_TEXT).GetComponent<Slider>();
            }
        }

         private void Start()
        {   
            maxHealth = PlayerHealth.Instance.maxHealthGetter();
            healthSlider.maxValue = maxHealth;

            UpdateHealthSlider();
        }
        public void UpdateHealthSlider()
        {
            healthSlider.value = PlayerHealth.Instance.currentHealthGetter();
            hpText.text = PlayerHealth.Instance.currentHealthGetter() + "/" + maxHealth;
        }
    }
}
