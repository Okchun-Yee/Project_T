---
name: PR Template
about: PR 탬플릿 작성
title: "[PR]"
labels: enhancement
assignees: Okchun-Yee

---
<!--이형식은 주석입니다.-->

## 📥 PR 제목
<!--제목을 작성해주세요-->
> [VFX] Spin VFX 런타임 추가 및 스킬 연동 + 데미지 적용 통일

---

## ✨ 변경 사항
<!--변경 사항을 각 클래스별로 작성해주세요-->
### 런타임/VFX
- `Assets/Scripts/ProjectT/Gameplay/Weapon/VFX/SpinVfxActor.cs`: Spin VFX 런타임 액터 추가(공전/자전, 슬롯 구성, 재생/종료 관리)
- `Assets/Scripts/ProjectT/Gameplay/Weapon/VFX/SpinVfxConfig.cs`: Spin VFX 설정 구조체 및 기본값 추가
- `Assets/Scripts/ProjectT/Gameplay/VFX/Rendering/Utilities/OrbitSlot.cs`: 슬롯 데이터 구조 추가

### 스킬 연동
- `Assets/Scripts/ProjectT/Gameplay/Skills/SkillExecutionContext.cs`: 스킬 실행 컨텍스트 추가
- `Assets/Scripts/ProjectT/Gameplay/Skills/BaseSkill.cs`: Spin VFX 실행 연동 및 Execute 추상 메서드 추가
- `Assets/Scripts/ProjectT/Gameplay/Player/Controller/PlayerController.cs`: 스킬 실행 진입점 및 SpinHub 참조 추가
- `Assets/Scripts/ProjectT/Data/ScriptableObjects/Skills/SkillSO.cs`: `hasSpinVfx` 플래그 추가
- `Assets/Scripts/ProjectT/Gameplay/Skills/Common/Melee/Sword_Buff.cs`: Spin VFX 생성/슬롯 구성/재생 로직 추가

### 데미지 적용 통일
- `Assets/Scripts/ProjectT/Gameplay/Weapon/Projectiles/Projectile.cs`: DamageSource 기반 데미지 적용 통일
- `Assets/Scripts/ProjectT/Gameplay/Weapon/VFX/LandingAOE.cs`: DamageSource 우선 적용
- `Assets/Scripts/ProjectT/Gameplay/Skills/Common/Melee/Sword_Slam.cs`: DamageSource 사용 및 SetDamage 통일

---

## 🔗 관련 이슈
<!--해당 PR에서 작업한 이슈를 close 해주세요-->
<!--close #이슈번호 형식으로 이슈를 닫을 수 있습니다.-->
- [ ] 

---

## 📸 Screen Shot
<!--동작 영상을 첨부하여 주세요.-->
> (Spin VFX 테스트 영상/스크린샷 첨부)

---

## ✅ Check List
- [ ] 1. main 브랜치와 충돌이 발생 하나요?
- [ ] 2. 작업 내용에 대해 설명이 충분한가요?
- [ ] 3. 이슈 해결이 완료되었나요?

---

## 🐞 BUG
<!--bug 와 예상되는 원인을 작성해주세요.-->
- [ ] 
> 원인:
- `BaseSkill.Execute()` 추가로 일부 스킬이 `NotImplementedException` 상태.
  - 해당 스킬에서 `hasSpinVfx`가 켜져 있으면 실행 시 크래시 가능.
  - 대응: Execute 구현 또는 플래그 비활성 필요.
- `PlayerController`의 `_spinHubRoot` 레퍼런스 미할당 시 SpinVFX 실행 실패 가능.
- `SpinVfxActor` 내 `Debug.Log` 존재(PR 전 제거 필요).
