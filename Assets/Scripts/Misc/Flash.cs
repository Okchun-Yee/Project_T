using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 피격을 가지는 모든 오브젝트 공통 클래스. 
/// 플레이어, 적 공통으로 사용되는 클래스입니다.
/// 피격시 변경되는 색상은 지정가능합니다.
/// </summary>
public class Flash : MonoBehaviour
{
    [SerializeField] private Material whiteFlashMat;
    [SerializeField] private float restoreDefaultMatTime = .2f;

    private Material defaultMat;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMat = spriteRenderer.material;
    }
    public float GetRestoreMatTime()
    {
        return restoreDefaultMatTime;
    }
    public IEnumerator FlashRoutine()
    {
        spriteRenderer.material = whiteFlashMat;
        yield return new WaitForSeconds(restoreDefaultMatTime);
        spriteRenderer.material = defaultMat;
    }
}
