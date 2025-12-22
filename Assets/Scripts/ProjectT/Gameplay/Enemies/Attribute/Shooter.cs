using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour, IEnemy
{
    [Header("Common Shooter Setting")]
    [SerializeField] private GameObject bulletPrefab;           // 투사체 프리펩
    [SerializeField] private float bulletMoveSpeed;             // 투사체 속도 
    [SerializeField] private int burstCount;                    // 투사체 연속 발사
    [SerializeField] private float bulletRange;                 // 투사체 사거리
    [SerializeField] private float bulletDamage;                // 투사체 데미지
    [SerializeField] private int projectilesPerBurst;           // 한번의 공격에서 동시에 발사되는 투사체 수
    [Header("Oscillate Shooter Setting")]
    [SerializeField][Range(0, 359)] private float angleSpread;  // 투사체가 퍼지는 각도 범위
    [SerializeField] private float startingDistance = 0.1f;     // 투사체가 생성될 때 시작 거리 (Offset)
    [SerializeField] private float timeBetweenBursts;           // 투사체 간 발사 간격
    [SerializeField] private float restTime = 1f;               // 다음 투사체 발사 시작까지 시간
    [Tooltip("Oscillate가 제대로 작동하려면 Stagger가 활성화되어야 합니다.")]
    [SerializeField] private bool stagger;                      // 투사체가 하나씩 발사되는지 여부
    [SerializeField] private bool oscillate;                    // 투사체가 좌우로 번갈아가며 발사되는지 여부

    private bool isShooting = false;

    private void OnValidate()
    {
        if (oscillate) { stagger = true; }
        if (!oscillate) { stagger = false; }
        if (projectilesPerBurst < 1) { projectilesPerBurst = 1; }
        if (burstCount < 1) { burstCount = 1; }
        if (timeBetweenBursts < 0.1f) { timeBetweenBursts = 0.1f; }
        if (restTime < 0.1f) { restTime = 0.1f; }
        if (startingDistance < 0.1f) { startingDistance = 0.1f; }
        if (angleSpread == 0) { projectilesPerBurst = 1; }
        if (bulletMoveSpeed <= 0) { bulletMoveSpeed = 0.1f; }
    }
    public void Attack()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootRoutine());
        }
    }
    private IEnumerator ShootRoutine()
    {
        isShooting = true;
        float startAngle, currentAngle, angleStep, endAngle;
        float timeBetweenProjectiles = 0f;


        TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep, out endAngle);

        if (stagger) { timeBetweenProjectiles = timeBetweenBursts / projectilesPerBurst; }

        for (int i = 0; i < burstCount; i++)
        {
            if (!oscillate)
            {
                TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep, out endAngle);
            }
            if (oscillate && i % 2 != 1)
            {
                TargetConeOfInfluence(out startAngle, out currentAngle, out angleStep, out endAngle);
            }
            else if (oscillate)
            {
                currentAngle = endAngle;
                endAngle = startAngle;
                startAngle = currentAngle;
                angleStep *= -1;
            }

            for (int j = 0; j < projectilesPerBurst; j++)
            {
                Vector2 pos = FindBulletSpawnPos(currentAngle);

                GameObject newBullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
                newBullet.transform.right = newBullet.transform.position - transform.position;

                if (newBullet.TryGetComponent(out Projectile projectile))
                {
                    projectile.UpdateMoveSpeed(bulletMoveSpeed);        // 투사체 속도 설정
                    projectile.UpdateProjectileRange(bulletRange);     // 투사체 사거리 설정
                    projectile.Initialize(bulletDamage);               // 투사체 데미지 설정
                }

                currentAngle += angleStep;
                if (stagger) { yield return new WaitForSeconds(timeBetweenProjectiles); }
            }
            currentAngle = startAngle;

            if (!stagger) { yield return new WaitForSeconds(timeBetweenBursts); }

        }

        yield return new WaitForSeconds(restTime);
        isShooting = false;
    }

    private void TargetConeOfInfluence(out float startAngle, out float currentAngle, out float angleStep, out float endAngle)
    {
        Vector2 targetDirection = PlayerLegacyController.Instance.transform.position - transform.position;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

        startAngle = targetAngle;
        endAngle = targetAngle;
        currentAngle = targetAngle;

        float halfAngleSpread = 0f;
        angleStep = 0;

        if (angleSpread != 0)
        {
            angleStep = angleSpread / (projectilesPerBurst - 1);
            halfAngleSpread = angleSpread / 2f;
            startAngle = targetAngle - halfAngleSpread;
            endAngle = targetAngle + halfAngleSpread;
            currentAngle = startAngle;

        }
    }

    private Vector2 FindBulletSpawnPos(float currentAngle)
    {
        float x = transform.position.x + startingDistance * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
        float y = transform.position.y + startingDistance * Mathf.Sin(currentAngle * Mathf.Deg2Rad);

        Vector2 pos = new Vector2(x, y);
        return pos;
    }
}
