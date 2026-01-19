# Repository Guidelines

본 문서는 **Project-T** 레포지토리에 기여하는 모든 사람 및 자동화 에이전트(Codex 포함)를 위한 공통 가이드라인이다.  
본 프로젝트는 구조 안정성과 장기 유지보수를 최우선 가치로 둔다.

## Non-Negotiable Rules (Highest Priority)

The following rules override all other guidelines in this repository. Codex and all contributors MUST follow them.

- Codex MUST respond in Korean.
- FSMs MUST handle state decisions and transitions only.
- FSMs MUST NOT execute gameplay logic (movement, attack, damage, animation, VFX).
- All gameplay execution MUST happen in Runtime systems.
- FSM → Binder → Runtime flow MUST be preserved at all times.
- Binder MUST NOT contain decision logic.
- ScriptableObjects MUST NOT contain runtime logic, state mutation, Update, or Coroutines.
- Each system has a Single Source of Truth (SSOT). Other systems MUST NOT directly mutate another system’s state.
- Core and FSM architecture MUST NOT be modified without explicit approval.
- Codex MUST NOT introduce new architectural patterns without asking first.
- Codex MUST NOT automatically run git commit/push or create PRs. Suggest them, but ask for confirmation.

---

## Project Structure & Module Organization

Project-T는 Unity 기반 2D 게임 프로젝트이다.

### 주요 구조
```

Assets/
└ Scripts/
└ ProjectT/
├ Core/        # FSM, Binder, 공용 추상화 (프레임워크 레벨)
├ Gameplay/    # Player, Weapon, Combat, Inventory 등 런타임 로직
├ Systems/     # 시스템 단위 조합 로직
├ Data/        # 순수 데이터 정의 (ScriptableObject)
└ UI/          # UI Controller / View

```

- `Assets/ScriptableObjects/` : ScriptableObject 에셋만 위치
- `ProjectSettings/`, `Packages/` : Unity 설정
- `Library/`, `Temp/`, `Logs/`, `UserSettings/` 는 **수정 금지**

### 구조 규칙
- ScriptableObject에는 **런타임 로직, 상태 변경, Update/Coroutine을 포함하지 않는다**
- 모든 시스템은 **명확한 책임과 단일 진입점(SSOT)** 을 가진다
- Core ↔ Gameplay ↔ UI 간 **순환 참조 금지**

---

## Architecture Overview (중요)

Project-T는 **병렬 FSM 아키텍처**를 사용한다.

### FSM 구성
- **Locomotion FSM**: Idle / Move / Dodge / Hit / Dead
- **Combat FSM**: None / Charging / Holding / Attack

### 책임 분리 원칙
- **FSM**
  - 상태 전이와 판단만 담당
  - 이동, 공격, 데미지, 애니메이션 실행하지 않는다
- **Binder**
  - FSM 상태 변화를 Runtime 이벤트로 변환
  - 판단 로직을 포함하지 않는다
- **Runtime 시스템**
  - 실제 실행 (Movement, Combat, Animation, VFX, Damage)
  - Runtime 시스템은 FSM이나 Binder를 직접 참조하지 않는다.

> FSM → Binder → Runtime 흐름을 반드시 유지한다.

---

## Single Source of Truth (SSOT)

Project-T의 모든 상태는 단일 소유자를 가진다.

- 무기 상태: `AgentWeapon`
- 인벤토리 상태: `InventoryController`
- FSM 상태: 각 FSM 인스턴스

### 규칙
- 다른 시스템은 **직접 수정하지 않는다**
- 이벤트 또는 조회(Read-only) 방식으로만 접근한다

---

## Coding Style & Naming Conventions

- 언어: C#
- 들여쓰기: 공백 4칸
- 네이밍
  - 클래스 / public 멤버: `PascalCase`
  - private 필드 / 지역 변수: `camelCase`
  - 상수: `UPPER_SNAKE_CASE`
- 네임스페이스는 폴더 구조를 따른다  
  예: `ProjectT.Gameplay.Player`

### 추가 규칙
- 의미 없는 `Manager` 남용 금지
- 클래스 이름은 **역할을 드러내야 한다**
  - 예: `PlayerCombatFsmBinder`, `AgentWeapon`

---

## Testing & Validation Guidelines

- 자동 테스트 커버리지 목표는 정의하지 않는다
- 모든 변경은 **실제 플레이 검증**을 전제로 한다

### FSM 변경 시 필수 검증
- 상태 전이 로그 확인
- 인게임 재현 시나리오 최소 1회 이상 확인
- 디버그 로그는 PR 전에 제거한다

---

## Commit & Pull Request Guidelines

### Commit
- 작고 명확한 단위로 커밋
- 형식:
```

[System] Short description

```
예: `[FSM] Stabilize combat transitions`

### Pull Request
- 반드시 포함:
- 변경 내용 요약
- 변경 이유
- 위험 요소 또는 영향 범위
- 리팩토링과 신규 기능을 한 PR에 섞지 않는다

