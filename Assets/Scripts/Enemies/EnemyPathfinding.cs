using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Knockback knockback;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        knockback = GetComponent<Knockback>();
    }
    private void FixedUpdate()
    {
        if (knockback.GettingKnockback) { return; }

        rb.MovePosition(rb.position + moveDir * (moveSpeed * Time.fixedDeltaTime));
        if (moveDir.x < 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveDir.x > 0)
        {
            spriteRenderer.flipX = true;
        }
    }
    public void MoveTo(Vector2 targetPosition)
    {
        Vector2 dir = (targetPosition - rb.position).normalized;
        moveDir = dir;
    }
    public void StopMoving()
    {
        moveDir = Vector3.zero;
    }
}