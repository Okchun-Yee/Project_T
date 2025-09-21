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
    private void Start()
    {   
        Debug.Log("테스트 시작");
        currentHealth = maxHealth;
        UpdateHealthSlider();
        
        // 1초마다 TakeDamage() 실행
        InvokeRepeating("TakeDamage", 2, 2);
    }


    private void TakeDamage()
    {
        currentHealth -= 10;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 0 이하로는 안 떨어지게
        Debug.Log("Hp감소");
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
