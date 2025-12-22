using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Dash : BaseSkill
{
    [SerializeField] private float dashForce = 10f; // dash Power
    private Rigidbody2D rb;
    private Dash dash;                              // Dash 컴포넌트 참조 추가
    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        dash = GetComponent<Dash>(); // Dash 컴포넌트 참조
    }
    private float damage;
    private void Start()
    {
        if (skillInfo != null)
        {
            damage = GetSkillDamage();
        }
        else
        {
            Debug.LogError("[Sword_Dash] SkillInfo is not set.");
        }
    }
    protected override void OnSkillActivated()
    {
        Debug.Log($"[Sword]: Dash Skill Activated, Damage {damage}");
    }
}
 