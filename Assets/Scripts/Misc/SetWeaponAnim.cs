using System;
using UnityEngine;

/// <summary>
/// PlayerController에서 movement/lastMovement 값을 읽어 4방향 인덱스(0=R,1=U,2=L,3=D)를 계산합니다.
/// Sword_Common 같은 소비자는 CurrentDirection 프로퍼티를 조회하거나 OnDirectionChanged 이벤트를 구독해서 최신 값을 얻을 수 있습니다.
/// </summary>
[DisallowMultipleComponent]
public class SetWeaponAnim : MonoBehaviour
{
    [Tooltip("입력이 이 값 이하이면 lastMovement로 방향을 결정")]
    [SerializeField] private float deadzone = 0.1f;

    // 현재 방향 인덱스 (외부 접근용)
    public int CurrentDirection { get; private set; } = 0;

    // 방향 변경 이벤트: int 인자로 새 방향 인덱스 전달
    public event Action<int> OnDirectionChanged;

    private Vector2 lastReportedMove = Vector2.zero;

    private void Update()
    {
        // 안전성: PlayerController 싱글톤 확인
        if (PlayerController.Instance == null) return;

        // 우선 현재 입력 벡터를 읽음
        Vector2 move = PlayerController.Instance.CurrentMovement;

        // 입력이 충분히 작으면 마지막으로 의미있는 이동값 사용
        if (move.sqrMagnitude < deadzone * deadzone)
            move = PlayerController.Instance.LastMovement;

        // 입력이 거의 없으면 방향을 변경하지 않음
        if (move.sqrMagnitude < deadzone * deadzone)
            return;

        // 변경이 생겼을 때만 계산/발신
        if (Vector2.SqrMagnitude(move - lastReportedMove) < 0.0001f) return;
        lastReportedMove = move;

        int dir = Vector2To4DirIndex(move);
        if (dir == CurrentDirection) return;

        CurrentDirection = dir;
        OnDirectionChanged?.Invoke(CurrentDirection);
    }

    // Sword_Common 등에서 즉시 읽을 수 있는 헬퍼
    public int GetDirectionIndex() => CurrentDirection;

    private int Vector2To4DirIndex(Vector2 v)
    {
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        if (angle >= -45f && angle < 45f) return 0;   // Right
        if (angle >= 45f && angle < 135f) return 1;   // Up
        if (angle >= 135f || angle < -135f) return 2; // Left
        return 3;                                     // Down
    }
}
