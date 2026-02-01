# Scene Transition Implementation Report

## Overview
- 씬 전환을 단일 파이프라인으로 통합하고, 모든 LoadScene 호출이 SceneTransitionExecution을 경유하도록 구성했습니다.
- GameMode를 도입해 Gameplay/Town/Menu 전환 시 인게임 전용 로직이 안전하게 동작하도록 했습니다.
- 카메라 Follow/Confiner 재바인딩을 CameraSystem API로 통일했습니다.

## New Files
- `Assets/Scripts/ProjectT/Systems/Scene/SceneTransitionExecution.cs`
- `Assets/Scripts/ProjectT/Systems/Scene/SceneTransitionRequest.cs`
- `Assets/Scripts/ProjectT/Systems/Scene/SceneReadyEmitter.cs`
- `Assets/Scripts/ProjectT/Systems/Scene/EntryPointMarker.cs`
- `Assets/Scripts/ProjectT/Systems/GameMode/GameModeSystem.cs`
- `Assets/Scripts/ProjectT/Systems/GameMode/IGameModeListener.cs`
- `Assets/Scripts/ProjectT/Systems/Camera/CameraSystem.cs`
- `Assets/Scripts/ProjectT/Systems/Camera/CameraConfinerProvider.cs`
- `Assets/Scripts/ProjectT/Systems/UI/UIInteractionSystem.cs`
- `Assets/Scripts/ProjectT/Systems/UI/UIFadeSystem.cs`
- `Assets/Scripts/ProjectT/Systems/Player/PlayerWarpSystem.cs`

## Modified Files
- `Assets/Scripts/ProjectT/Systems/Scene/MainMenu.cs`
- `Assets/Scripts/ProjectT/Gameplay/Player/PlayerHealth.cs`
- `Assets/Scripts/ProjectT/Gameplay/Combat/Aiming/InGame_MouseFollow.cs`
- `Assets/Scripts/ProjectT/Systems/Camera/CameraController.cs`
- `Assets/Scripts/ProjectT/Gameplay/Player/Controller/PlayerController.cs`
- `Assets/Prefabs/Manager/GameManager.prefab`
- `Assets/Prefabs/Manager/Camera.prefab`

## SceneTransitionExecution Flow (Text Diagram)
1) AcquireTransitionLock(ActionLockFlags)
2) UIInteractionSystem.SetInteractable(false)
3) FadeOut (optional)
4) LoadSceneAsync(Single)
5) Wait SceneReadyEmitter
6) Rebind
   - EntryPointMarker -> PlayerWarpSystem.WarpTo
   - CameraSystem.BindFollow(player)
   - CameraSystem.SetConfiner(scene provider collider)
   - GameModeSystem.SetMode(targetGameMode)
7) FadeIn (optional)
8) UIInteractionSystem.SetInteractable(true)
9) ReleaseTransitionLock(ActionLockFlags)
10) Reset Time.timeScale (optional)

## Known Limitations / Follow-up
- UIRoot에 UIInteractionSystem/UIFadeSystem용 CanvasGroup 설정 필요.
- SceneReadyEmitter/EntryPointMarker/CameraConfinerProvider는 씬마다 배치해야 함.
- 일부 런타임 로직은 여전히 Camera.main을 사용 중 (추가 정리 필요).
- InGame 전용 스크립트는 GameMode 기반 early return으로 동작 (완전 비활성화는 추후 고려).
