// File: SpinVfxActor.cs
// Purpose: 회전 VFX 전용 Runtime Actor (1단계: 연출/궤적/수명만 관리)
// Notes:
// - Root는 위치/수명만 담당하고, 실제 회전은 visualRoot(또는 ringPivot)에만 적용한다.
// - followOwner=true면 owner를 매 프레임 추적한다.
// - radius는 pivot의 localPosition을 (radius,0,0)로 두고 회전으로 원궤적을 만든다.

using UnityEngine;

namespace ProjectT.Gameplay.VFX
{
    public enum OrbitMode
    {
        None,
        Circle,
        Ellipse
    }
    [System.Serializable]
    public struct SpinVfxConfig
    {
        public float duration;          // VFX 지속 시간 (초)
        public float selfSpeedDeg;      // 초당 회전 각도 (360 = 1초에 한 바퀴)
        [Header("Follow")]
        public bool followOwner;        // 소유자 추적 여부
        public Vector2 localOffset;     // 소유자 기준 로컬 오프셋
        public bool useUnscaledTime;    // Time.timeScale 무시 여부
        public bool clockwise;          // 시계 방향 회전 여부
        public OrbitMode orbitMode;     // 궤도 모드 (None, Circle, Ellipse)
        public float orbitSpeedDeg;     // 궤도 회전 각속도 (orbitMode가 None이 아닐 때 유효)

        public float radius;            // 회전 반경 (중심에서 VFX까지 거리)
        public Vector2 ellipseRadii;    // 타원 궤도 반경 (orbitMode가 Ellipse일 때 유효)
        public static SpinVfxConfig Default => new SpinVfxConfig
        {
            duration = 0.6f,
            selfSpeedDeg = 720f,
            followOwner = true,
            localOffset = Vector2.zero,
            useUnscaledTime = false,
            clockwise = true, 

            // Orbit
            orbitMode = OrbitMode.Circle,
            orbitSpeedDeg = 360f,
            radius = 0.6f,
            ellipseRadii = new Vector2(0.8f, 0.4f),
        };
    }
}
