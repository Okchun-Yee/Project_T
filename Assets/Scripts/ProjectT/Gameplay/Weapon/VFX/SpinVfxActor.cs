using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectT.Gameplay.VFX
{
    public sealed class SpinVfxActor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform orbitRoot;   // 궤도 회전용 루트 (null = self)


        [Header("Orbit Slots")]
        [SerializeField] private OrbitSlot[] slots;

        [SerializeField] private bool autoDistributePhase = true; // ★ 자동 분배

        [Tooltip("재생/정지를 함께 관리할 파티클(선택)")]
        [SerializeField] private ParticleSystem[] particlesToPlay;  // 재생/정지를 함께 관리할 파티클(선택)

        [SerializeField] private SpinVfxConfig defaultConfig = default;

        [Header("Dubug Test")]
        [SerializeField] private bool autoPlayOnStart = false;

        private Transform _owner;
        private SpinVfxConfig _cfg;
        private float _elapsed;
        private float _orbitAngleDeg;       // 타원 공전용 누적 각도
        private bool _isPlaying;

        // 캐시 (회전/오프셋 적용 대상)
        private Transform _visual;
        private Transform _pivot;
        private Transform _orbit;

        public bool IsPlaying => _isPlaying;
        public Transform Owner => _owner;

        private void Awake()
        {
            // defaultConfig가 인스펙터에서 비어있을 수 있으니, duration이 0이면 Default로 보정
            if (defaultConfig.duration <= 0f)
                defaultConfig = SpinVfxConfig.Default;

            _orbit = orbitRoot != null ? orbitRoot : transform;
        }
        private void Start()
        {
            Debug.Log($"slots={slots?.Length ?? 0}");
            for (int i = 0; i < (slots?.Length ?? 0); i++)
            {
                Debug.Log($"[{i}] pivot={slots[i]?.pivot?.name}, visual={slots[i]?.visual?.name}");
            }

            if (autoPlayOnStart)
            {
                Play(transform, defaultConfig);
            }
        }

        /// <summary>
        /// 회전 VFX를 재생한다.
        /// </summary>
        public void Play(Transform owner, in SpinVfxConfig cfg)
        {
            _owner = owner;
            _cfg = Sanitize(cfg);
            _elapsed = 0f;
            _isPlaying = true;

            SetupSlotPhases();  // 위성 배치 각도 자동 분배
            // 초기 배치
            UpdateFollowPosition(force: true);

            // 원형이면 초기 반경 배치
            if (_cfg.orbitMode == OrbitMode.Circle)
                SetCircleRadius();

            // 타원이면 초기 위치 세팅
            if (_cfg.orbitMode == OrbitMode.Ellipse)
                ApplyEllipseOrbit(0f);

            ResetOrbitRotation();
            ResetVisualRotation();

            // 파티클 재생
            if (particlesToPlay != null)
            {
                for (int i = 0; i < particlesToPlay.Length; i++)
                {
                    if (particlesToPlay[i] == null) continue;
                    particlesToPlay[i].Play(true);
                }
            }
        }

        /// <summary>
        /// 기본 설정으로 재생 (인스펙터 defaultConfig)
        /// </summary>
        public void PlayDefault(Transform owner)
        {
            Play(owner, defaultConfig);
        }

        /// <summary>
        /// 즉시 종료(파괴). 필요하면 후에 자연 소멸(파티클 stop) 방식으로 확장 가능.
        /// </summary>
        public void Stop(bool immediate = true)
        {
            _isPlaying = false;

            if (immediate)
            {
                Destroy(gameObject);
                return;
            }

            // 자연 종료가 필요하면 여기서 파티클 Stop 후,
            // 남은 lifetime 체크해서 Destroy 하는 방식으로 확장.
            Destroy(gameObject);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            float dt = _cfg.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _elapsed += dt;

            UpdateFollowPosition(force: false);

            switch (_cfg.orbitMode)
            {
                case OrbitMode.Circle:
                    ApplyCircleOrbit(dt);
                    break;

                case OrbitMode.Ellipse:
                    ApplyEllipseOrbit(dt);
                    break;
            }

            ApplySelfRotation(dt);

            if (_elapsed >= _cfg.duration)
                Stop(immediate: true);
        }

        #region Helpers Methods
        private void UpdateFollowPosition(bool force)
        {
            if (!_cfg.followOwner) return;

            if (_owner == null)
            {
                // owner가 사라졌으면 즉시 종료(유령 VFX 방지)
                Stop(immediate: true);
                return;
            }

            // Root는 위치만 추적
            transform.position = _owner.position + (Vector3)_cfg.localOffset;
        }

        /// <summary>
        /// 원형 공전: orbitRoot(부모)를 Rotate해서 자식이 원 궤적을 그리게 한다.
        /// 전제: pivot(또는 visual)이 orbitRoot 기준으로 radius만큼 떨어져 있어야 함.
        /// </summary>
        private void ApplyCircleOrbit(float dt)
        {
            // 반경은 pivot localPosition으로 유지
            SetCircleRadius();

            float sign = _cfg.clockwise ? -1f : 1f;
            float deltaAngle = sign * _cfg.orbitSpeedDeg * dt;

            _orbit.Rotate(0f, 0f, deltaAngle, Space.Self);
        }

        private void SetCircleRadius()
        {
            int n = slots.Length;
            float step = 360f / n;

            for (int i = 0; i < n; i++)
            {
                var s = slots[i];
                if (s?.pivot == null) continue;

                float phase = step * i;
                float rad = phase * Mathf.Deg2Rad;

                float x = _cfg.radius * Mathf.Cos(rad);
                float y = _cfg.radius * Mathf.Sin(rad);


                s.pivot.localRotation = Quaternion.Euler(0, 0, phase);
                s.pivot.localPosition = new Vector3(x, y, 0);
            }
            
        }


        /// <summary>
        /// 타원 공전: pivot.localPosition을 (a cos t, b sin t)로 매 프레임 갱신.
        /// orbitRoot 자체 회전은 필요 없으며(원하는 경우 추가 가능), 이 방식이 가장 직관적.
        /// </summary>
        private void ApplyEllipseOrbit(float dt)
        {
            float sign = _cfg.clockwise ? -1f : 1f;
            _orbitAngleDeg += sign * _cfg.orbitSpeedDeg * dt;

            float a = _cfg.ellipseRadii.x;
            float b = _cfg.ellipseRadii.y;

            int n = slots.Length;
            float step = 360f / n;

            for (int i = 0; i < n; i++)
            {
                var s = slots[i];
                if (s?.pivot == null) continue;

                float phase = step * i;
                float ang = (_orbitAngleDeg + phase) * Mathf.Deg2Rad;

                s.pivot.localPosition = new Vector3(a * Mathf.Cos(ang), b * Mathf.Sin(ang), 0);
            }
        }



        public void SetEllipseRadii(Vector2 radii)
        {
            _cfg.ellipseRadii = new Vector2(Mathf.Max(0f, radii.x), Mathf.Max(0f, radii.y));
        }

        private void ApplySelfRotation(float dt)
        {
            float sign = _cfg.clockwise ? -1f : 1f;
            float deltaAngle = sign * _cfg.selfSpeedDeg * dt;

            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                if (s == null || s.visual == null) continue;
                s.visual.Rotate(0f, 0f, deltaAngle, Space.Self);
            }
        }
        private void ResetOrbitRotation()
        {
            _orbit.localRotation = Quaternion.identity;
        }
        private void ResetVisualRotation()
        {
            if (slots == null) return;

            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                if (s == null || s.visual == null) continue;

                var e = s.visual.localEulerAngles;
                s.visual.localEulerAngles = new Vector3(e.x, e.y, 0f);
            }
        }

        private void SetupSlotPhases()
        {
            if (!autoDistributePhase) return;
            if (slots == null || slots.Length == 0) return;

            int n = slots.Length;
            float step = 360f / n;

            for (int i = 0; i < n; i++)
            {
                if (slots[i] == null) continue;
                slots[i].phaseDeg = step * i;
            }
        }


        private static SpinVfxConfig Sanitize(SpinVfxConfig cfg)
        {
            if (cfg.duration <= 0f) cfg.duration = 0.1f;
            if (Mathf.Abs(cfg.selfSpeedDeg) < 0.01f) cfg.selfSpeedDeg = 1f;
            if (cfg.radius < 0f) cfg.radius = 0f;
            return cfg;
        }
        #endregion
    }
}
