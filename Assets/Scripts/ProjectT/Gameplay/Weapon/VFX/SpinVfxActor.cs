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

        [Header("Runtime Slot Factory (Optional)")]
        [SerializeField] private Transform slotPivot;         // Ring 템플릿 (자식 Visual 포함)

        [SerializeField] private bool autoDistributePhase = true; // ★ 자동 분배

        [Tooltip("재생/정지를 함께 관리할 파티클(선택)")]
        [SerializeField] private ParticleSystem[] particlesToPlay;  // 재생/정지를 함께 관리할 파티클(선택)

        [SerializeField] private SpinVfxConfig defaultConfig = default;

        [Header("Dubug Test")]
        [SerializeField] private bool autoPlayOnStart = false;  
        [SerializeField] private bool autoRotation = true;

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

            if (autoPlayOnStart)
            {
                Play(transform, defaultConfig);
            }
        }
        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
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
        #region Orbit Methods
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
                if (s?._pivot == null) continue;

                float phase = step * i;
                float rad = phase * Mathf.Deg2Rad;

                float x = _cfg.radius * Mathf.Cos(rad);
                float y = _cfg.radius * Mathf.Sin(rad);


                s._pivot.localRotation = Quaternion.Euler(0, 0, phase);
                s._pivot.localPosition = new Vector3(x, y, 0);
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
                if (s?._pivot == null) continue;

                float phase = step * i;
                float ang = (_orbitAngleDeg + phase) * Mathf.Deg2Rad;

                s._pivot.localPosition = new Vector3(a * Mathf.Cos(ang), b * Mathf.Sin(ang), 0);
            }
        }
        public void SetEllipseRadii(Vector2 radii)
        {
            _cfg.ellipseRadii = new Vector2(Mathf.Max(0f, radii.x), Mathf.Max(0f, radii.y));
        }
        #endregion
        #region Self Rotation
        private void ApplySelfRotation(float dt)
        {
            if (!autoRotation) return;

            float sign = _cfg.clockwise ? -1f : 1f;
            float deltaAngle = sign * _cfg.selfSpeedDeg * dt;

            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                if (s == null || s._visual == null) continue;
                s._visual.Rotate(0f, 0f, deltaAngle, Space.Self);
            }
        }
        #endregion
        #region Helpers Methods
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
                if (s == null || s._visual == null) continue;

                var e = s._visual.localEulerAngles;
                s._visual.localEulerAngles = new Vector3(e.x, e.y, 0f);
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
                slots[i]._phaseDeg = step * i;
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

        #region  Runtime Slot Factory
        // SpinVfxActor.cs 안에 추가

        public void AddSlot()
        {
            if (_orbit == null)
                _orbit = orbitRoot != null ? orbitRoot : transform;

            // 기존 배열 -> 리스트
            int cur = slots?.Length ?? 0;
            var list = new List<OrbitSlot>(cur + 1);
            if (slots != null) list.AddRange(slots);

            int index = cur;

            // 1) pivot 생성
            Transform pivot = null;
            if (slotPivot != null)
            {
                pivot = Instantiate(slotPivot, _orbit);
            }
            else
            {
                Debug.LogWarning("[SpinVfxActor] slotPivot이 설정되지 않음. 빈 GameObject로 대체합니다.");
            }

            // 2) visual 생성
            Transform visual = FindVisualUnderPivot(pivot);

            list.Add(new OrbitSlot { _pivot = pivot, _visual = visual, _phaseDeg = 0f });
            slots = list.ToArray();

            // 3) phase 재분배 + 즉시 배치 갱신
            SetupSlotPhases();
            RefreshOrbitLayout();
        }
        private Transform FindVisualUnderPivot(Transform pivot)
        {
            if (pivot == null) return null;

            // 1) 직계 자식에 "Circle"이 있으면 우선
            var child = pivot.Find("Circle");
            if (child != null) return child;

            // 2) SpriteRenderer 가진 첫 Transform
            var sr = pivot.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) return sr.transform;

            // 3) ParticleSystem 가진 첫 Transform
            var ps = pivot.GetComponentInChildren<ParticleSystem>(true);
            if (ps != null) return ps.transform;

            // 4) 없으면 pivot 자체를 visual로 사용(최후)
            return pivot;
        }

        public void RemoveSlot()
        {
            int cur = slots?.Length ?? 0;
            if (cur <= 0) return;

            // 마지막 슬롯 제거
            int last = cur - 1;
            var s = slots[last];

            if (s?._pivot != null)
                Destroy(s._pivot.gameObject); // pivot 밑 visual까지 같이 제거됨

            if (cur == 1)
            {
                slots = System.Array.Empty<OrbitSlot>();
            }
            else
            {
                var next = new OrbitSlot[cur - 1];
                for (int i = 0; i < cur - 1; i++)
                    next[i] = slots[i];
                slots = next;
            }

            SetupSlotPhases();
            RefreshOrbitLayout();
        }

        private void RefreshOrbitLayout()
        {
            // 슬롯이 없으면 아무 것도 안 함
            if (slots == null || slots.Length == 0) return;

            // 재생 중일 때만 즉시 재배치 (원하면 항상 재배치로 바꿔도 됨)
            if (!_isPlaying) return;

            if (_cfg.orbitMode == OrbitMode.Circle)
            {
                SetCircleRadius();       // 너 코드 기준: 슬롯 x,y 배치 포함
            }
            else if (_cfg.orbitMode == OrbitMode.Ellipse)
            {
                ApplyEllipseOrbit(0f);   // 현재 각도 기준으로 즉시 갱신하려면 dt=0
            }
        }
        // 외부 Weapon에서 Pivot Prefab 설정
        public void SetSlotPivotPrefab(Transform newPivotPrefab)
        {
            slotPivot = newPivotPrefab;
        }
        #endregion
    }
}
