using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManger : Singleton<UIManger>
{
    private Slider healthSlider;
    private int currentHealth;
    [SerializeField] private int maxHealth = 100;

    protected override void Awake()
    {
        base.Awake();
        currentHealth = maxHealth;
    }
    

    private void TakeDamage()
    {
        currentHealth -= 10;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 0 이하로는 안 떨어지게
        UpdateHealthSlider();
    }
    private void UpdateHealthSlider()
    {
        const string HEALTH_SLIDER_TEXT = "Health Bar";
        if (healthSlider == null)
        {
            healthSlider = GameObject.Find(HEALTH_SLIDER_TEXT).GetComponent<Slider>();
        }
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
