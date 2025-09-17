# Copilot / AI agent instructions — Project_T

This Unity C# project follows a small, explicit gameplay architecture. The instructions below focus on patterns and concrete examples an AI coding agent should know to be productive immediately.

**Project Overview**
- **Type:** Unity project (C#) — main gameplay code under `Assets/Scripts`.
- **Data:** ScriptableObjects hold configuration: see `Assets/Scripts/ScriptableObjects/WeaponSO.cs` and `SkillSO.cs`.
- **Entry points:** Player Input flows via `Assets/Scripts/Management/InputManager.cs` and `Assets/Scripts/Player/ActiveWeapon.cs`.

**Big-Picture Architecture**
- **Managers:** Singletons under `Assets/Scripts/Management` (pattern: `Singleton<T>`). Examples: `InputManager`, `WeaponManager`, `ChargingManager`.
- **Gameplay objects:** Player code under `Assets/Scripts/Player`; weapons under `Assets/Scripts/Weapon` (concrete weapons in `Weapon_Common`).
- **Data-driven:** Weapons and skills are configured with `WeaponSO` and `SkillSO`. `WeaponManager.EquipWeapon(WeaponSO)` instantiates the prefab and calls `BaseWeapon.Weapon_Initialize(info)`.
- **Event-driven input:** `InputManager` exposes events (`OnMoveInput`, `OnAttackInput`, `OnSkillInput`, ...). Components subscribe/unsubscribe in `OnEnable`/`OnDisable`.

**Key Code Patterns (do these the same way)**
- **Singleton managers:** Use `class Foo : Singleton<Foo>` and put subscribe/unsubscribe pairs in `OnEnable` / `OnDisable`.
- **Input -> Weapon flow:** `InputManager.OnAttackInput` -> `ActiveWeapon.OnAttackStarted` (sets a flag) -> `ActiveWeapon.Update()` calls `currentWeapon.Attack()` while the attack input is held. See `Assets/Scripts/Player/ActiveWeapon.cs`.
- **Weapon initialization:** After `Instantiate(info.weaponPrefab)`, call `GetComponent<BaseWeapon>()` and `Weapon_Initialize(info)`. `BaseWeapon` will call `GetComponents<ISkill>()` and call `Skill_Initialize` for each.
- **Damage components:** Weapons expect `DamageSource` components on weapon collider children; `BaseWeapon` sets their damage from `WeaponSO.weaponDamage`.
- **Animation hooks:** Weapons use Animator triggers and bools (example: trigger names `Attack1`, `Attack2`, `Attack3` and bool `isAttack` in `Sword_Common`). Use `Animator.StringToHash` where appropriate.

**ScriptableObjects / Editor notes**
- `WeaponSO` CreateAssetMenu name: `New Weapon` — create via Assets → Create → New Weapon.
- `SkillSO` CreateAssetMenu name: `New Skill` — create via Assets → Create → New Skill.
- `PlayerControls.inputactions` is the Unity Input System asset used by `InputManager` (do not replace without updating `PlayerControls` generated class).

**Important Conventions & Gotchas**
- Always unsubscribe events in `OnDisable` to avoid null-reference or duplicated handlers.
- `ActiveWeapon` expects that skills implement `ISkill` and that `BaseSkill` sets `skillIndex` when initialized.
- Some code searches scene objects by name (example: `GameObject.Find("Slash SpawnPoint")` in `Sword_Common`). Ensure the scene contains that GameObject when testing.
- `BaseWeapon` warns when no `DamageSource` is present — weapon prefabs must include colliders with `DamageSource`.
- Continuous attack is deliberate: holding the attack input repeatedly calls `Attack()`; weapon implementations must enforce cooldowns (`BaseWeapon` provides an example).

**Files to read first (priority)**
- `Assets/Scripts/Management/InputManager.cs` — input events and `PlayerControls` hookup.
- `Assets/Scripts/Player/ActiveWeapon.cs` — maps input to weapon/skill lifecycle.
- `Assets/Scripts/Weapon/BaseWeapon.cs` — weapon lifecycle, initialization, skill wiring.
- `Assets/Scripts/ScriptableObjects/WeaponSO.cs` and `SkillSO.cs` — data fields agents may modify.
- `Assets/Scripts/Weapon/Weapon_Common/Sword_Common.cs` — concrete weapon example (combo, VFX spawn, animator usage).
- `Assets/Scripts/Management/WeaponManager.cs` — equip/unequip and UI icon event `onCategoryIconChanged`.

**Examples (copyable snippets)**
- Subscribe to move input:
  ```csharp
  InputManager.Instance.OnMoveInput += Move; // in OnEnable
  InputManager.Instance.OnMoveInput -= Move; // in OnDisable
  ```
- Equip flow (weapon manager):
  ```csharp
  var go = Instantiate(info.weaponPrefab, mount.position, Quaternion.identity, mount);
  var bw = go.GetComponent<BaseWeapon>();
  bw.Weapon_Initialize(info);
  ActiveWeapon.Instance.NewWeapon(bw);
  ```

**What NOT to change without checking**
- Do not rename input action names in `PlayerControls.inputactions` or the generated `PlayerControls` class without updating `InputManager` subscriptions.
- Avoid changing `GameObject.Find("Slash SpawnPoint")` usage without changing the scene — it's a hard dependency.

If anything here is unclear or you want more details (e.g., list of animator parameters, weapon prefab locations, or how skills subscribe to events), tell me which area to expand and I will update this file.
