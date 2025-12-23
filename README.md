# Project_T
## 🎮 Project Name

> Project-T

## 📌 프로젝트 개요

- 게임 장르
    - 로그라이크 / 액션 RPG

- 주요 특징
    - 하나의 무기를 기반으로 다양한 강화·변형이 가능한 전투 시스템
    - 플레이 진행에 따라 무기 성능과 플레이 스타일이 변화
     - 액션 중심의 전투와 반복 플레이를 고려한 로그라이크 구조

- 개발 목적
    - Unity 엔진을 활용한 중·대형 게임 구조 설계 경험
    - FSM, 이벤트 기반 입력, 데이터 중심 설계 등 게임 아키텍처 학습 및 실험

## 🛠 개발 환경
- Engine: Unity 2022.3.61f1 (LTS)
- Language: C#
- IDE: VS Code
- Platform: PC (Windows)

## 👥 기여자 (Contributors)

- 한채훈 – 총괄 프로그래밍, 시스템 아키텍처 설계
- 최현규 – 기획, UI 제작

## 📂 폴더 구조

본 프로젝트는 책임 중심 + 도메인 기반 구조를 따릅니다.

```text
Assets/Scripts/ProjectT
├─ Contracts                # 인터페이스 및 추상 의존성 정의
│
├─ Core                     # 게임 전반에서 공용으로 사용되는 핵심 로직
│  └─ FSM                   # 범용 FSM 프레임워크
│
├─ Data
│  └─ ScriptableObjects     # 순수 데이터 정의 영역
│     ├─ Inventory
│     ├─ Items
│     ├─ NPC
│     ├─ Skills
│     ├─ StatsModifiers
│     └─ Weapons
│
├─ Gameplay                 # 실제 게임 플레이 로직
│  ├─ Player
│  │  ├─ Input              # 입력 수집 및 이벤트 브릿지
│  │  ├─ Controller         # 입력 ↔ FSM ↔ 정책 조율
│  │  └─ FSM
│  │     ├─ Locomotion
│  │     │  └─ States
│  │     └─ Combat
│  │        └─ States
│  │
│  ├─ Combat
│  │  ├─ Aiming
│  │  ├─ Damage
│  │  └─ World
│  │
│  ├─ Items
│  │  ├─ Inventory
│  │  ├─ Loot
│  │  └─ Execution
│  │
│  ├─ Weapon
│  │  ├─ Contracts
│  │  ├─ Implementations.Common
│  │  ├─ Projectiles
│  │  └─ VFX
│  │
│  ├─ Skills
│  │  ├─ Contracts
│  │  └─ Common
│  │     ├─ Melee
│  │     └─ Range
│  │
│  ├─ Enemies
│  │  └─ Attribute
│  │
│  └─ NPC
│
└─ Systems                  # 게임을 보조하는 시스템
   ├─ UI
   ├─ Camera
   └─ Scene
```

### 구조 설계 원칙
- Core / Gameplay / Systems / Data 레이어 분리
- Gameplay는 Core와 Data에만 의존
- Contracts를 통해 구현과 의존성 분리
- Player는 Input → Controller → FSM 흐름을 명확히 유지

## 🎮 현재 개발 상태

-[ ] 플레이어 이동 / 전투 입력 정상 동작
-[ ] 병렬 FSM(이동 / 전투) 구조 안정화
-[ ] InputManager 이벤트 기반 입력 구조 확정
-[ ] 대규모 네임스페이스 및 폴더 구조 리팩토링 완료

## 💾 설치 및 실행 방법

추후 제공 예정

## 🔀 Git 브랜치 전략

- `main`
    - 배포용 브랜치
    - 직접 push 금지 (PR을 통해서만 병합)
- `feature/`
    - 기능 단위 개발 브랜치
- `refactor/`
    - 리팩토링 단위 브랜치
- `docs/`
    - 문서 작업 전용 브랜치
- `fix/`
    - 버그 수정 전용 브랜치

## 📏 Commit 규칙
**형식**
```
[타입] 설명
```

### 타입 예시

| 타입 | 설명 |
|------|------|
| `feat` | 새로운 기능 추가 |
| `fix` | 버그 수정 |
| `docs` | 문서 관련 작업 |
| `style` | 코드 스타일 수정 (로직 변경 없음) |
| `refactor` | 코드 리팩토링 |
| `test` | 테스트 코드 추가/수정 |
| `chore` | 빌드, 설정, 패키지 관리 등 |
| `add` | 리소스/파일 추가 (이미지, 재질 등) |
| `working` | 사소한 작업 수정 (환경 이동, 주석 수정 등) |


## 📄 라이선스

본 프로젝트는 현재 라이선스가 지정되어 있지 않습니다.
(Proprietary / Unlicensed)
