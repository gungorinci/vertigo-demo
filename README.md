# Wheel of Fortune

A risk-and-reward wheel game made for the Vertigo Games developer demo.
The player spins a wheel zone after zone, collecting rewards. A bomb on the
wheel takes everything collected so far; after any spin the player can leave
and keep it all.

- **Unity:** 6000.3.23f1
- **Platform:** Android (APK in [Releases](https://github.com/gungorinci/vertigo-demo/releases))
- **Scene:** `Assets/Scenes/Wheel.unity`

## Screenshots

| | 20:9 (2400×1080) | 16:9 (1920×1080) | 4:3 (1600×1200) |
|---|---|---|---|
| Bronze spin | ![](Screenshots/bronze_20x9.png) | ![](Screenshots/bronze_16x9.png) | ![](Screenshots/bronze_4x3.png) |
| Silver spin (safe zone) | ![](Screenshots/silver_20x9.png) | ![](Screenshots/silver_16x9.png) | ![](Screenshots/silver_4x3.png) |
| Rewards collected | ![](Screenshots/collected_20x9.png) | ![](Screenshots/collected_16x9.png) | ![](Screenshots/collected_4x3.png) |
| Bomb | ![](Screenshots/bomb_20x9.png) | ![](Screenshots/bomb_16x9.png) | ![](Screenshots/bomb_4x3.png) |

## Rules

| Zone | Wheel | Bomb |
|---|---|---|
| Normal | Bronze | Yes |
| Every 5th (safe) | Silver | No |
| Every 30th (super) | Gold, special rewards | No |

- Reward amounts grow 8% per zone (`RewardScaling`, set per wheel).
- A normal wheel has exactly one bomb slice, safe and super wheels have none;
  `WheelConfig.OnValidate` logs an error otherwise. Empty slices are never picked.
- Hitting the bomb loses all collected rewards; the game can be restarted.
- The player can leave whenever the wheel is not spinning and at least one
  reward has been collected (`ZoneRules.CanLeave`, the rule lives in one place).
- Leaving moves the collected rewards into the wallet, which is saved to
  `save.json` and loaded on the next start.
- Zones are endless; the wheel type is decided only by the zone number.

## Project structure

```
Assets/
  Art/          Atlases, Backgrounds, Rewards, UI, VFX, Wheel (sprites)
  Audio/        Wheel tick sound
  Data/         ScriptableObject assets: Rewards/, Wheels/, Zones/, RandomSettings
  Prefabs/      Reusable UI prefabs (reward box, wallet box)
  Scenes/       Wheel scene
  Scripts/
    Core/       Pure C# game rules, no UnityEngine (own assembly: Wheel.Core)
    Data/       ScriptableObject definitions
    Game/       GameControllerMono, spin flow, presenters, save
    UI/         Views: only display and input, no game rules; UiTexts, UiEffects
  Tests/
    EditMode/   NUnit tests for the Core rules
```

## Architecture

Three layers, so the rules can be tested without Unity:

1. **Core** (`Wheel.Core` assembly, *No Engine References*)
   - `ZoneSchedule<TZoneType>`: which zone type a zone is. The type with the
     largest interval that divides the zone wins (30 beats 5 beats 1).
   - `ZoneRules`: the leave rule. `RewardScaling`: the amount growth per zone.
   - `EventBus`: typed publish/subscribe, one event struct per change.
   - `GameSession<TReward>`: zone, collected rewards and state
     (`Idle / Spinning / Lost / Left`), publishes its changes on the bus.
   - `Inventory<TItem>`: the wallet the rewards go into on leave, saved
     through `ISaveStore`.
   - `WeightedPicker` + `IRandomSource`: weighted slice choice with an
     injectable random source (deterministic in tests).
2. **Data** (ScriptableObjects, editable in the Inspector)
   - `RewardData`: one reward type (name, icon, is bomb).
   - `ZoneTypeData`: a zone type's interval, bomb rule, wheel and HUD texts.
     A new zone type is a new asset in the GameControllerMono's list, no code change.
   - `RandomSettings`: creates the game's random source. A seed other than 0
     replays the same spins, which helps when reproducing a bug.
   - `WheelConfig`: a wheel's sprites and its 8 slices. Each `WheelSlice` has
     a reward pool, an amount range and a weight. `RolledSlice` is one slice
     after its reward and amount are rolled.
3. **Views / Game**
   - `GameControllerMono` is the thin composition root: it creates the bus,
     the session and the presenters and hands them the views.
   - `SpinFlow` runs a spin: roll, animate, apply the result. Rejected spin
     and leave requests are logged, a second spin while spinning throws.
   - `EconomyPresenter` (inventory, save, wallet) and `ZonePresenter`
     (zone setup, intro, countdown) listen to the bus.
   - Views (`WheelViewMono`, `WheelSpinAnimatorMono`, `HudViewMono`, `RewardHudViewMono`,
     `ResultPanelViewMono`, effects) only show what they are given.
   - Dependencies point one way: Game uses UI, Data and Core; UI uses Data
     and Core and never Game.

**Spin flow:** the result is chosen first by `WeightedPicker`, then
`WheelSpinAnimatorMono` rotates the wheel so that slice stops under the pointer
(angles come from the slice count and the pointer's position, nothing is
hard-coded to 45°). The animation never decides the result.

## Requirements checklist

- **ScriptableObjects:** rewards and the three wheels are assets under `Assets/Data`;
  slice contents, amounts and weights are changed from the Inspector.
- **DOTween:** wheel spin, win glow, reward flight to the HUD, reward box pop
  and the result popup.
- **Sprite Atlas:** `Assets/Art/Atlases/ui_atlas_main` packs the UI, wheel and
  reward sprites (V2, no rotation / tight packing for UI). The whole game
  screen draws in 8 batches.
- **UI rules:**
  - Canvas Scaler: Scale With Screen Size 1920×1080, Expand; TextMeshPro only.
  - Runtime-changing elements end with `_value`; names go general to
    specific (`ui_image_spin_wheel_value`).
  - Raycast Target / Maskable are off on everything that is not clicked.
  - Animated parts (`ui_animator_wheel`, `ui_animator_popup_result`) are separate
    transforms, not roots.
  - Buttons and child references are found in `OnValidate` by name and stored
    in hidden serialized fields; clicks are bound in code with `AddListener`.
    No OnClick or event wiring in the Inspector.
  - Buttons and frames use Sliced sprites; images keep their aspect ratio.
- **Pooling:** reward boxes come from an `ObjectPool` and are released on
  when the rewards are cleared instead of destroyed.
- **Code standard:** `Mono` suffix on MonoBehaviours, `_camelCase` fields,
  `SCREAMING_SNAKE_CASE` constants, `Is/Has/Can` bools, block bodies, regions,
  `if (obj)` for Unity objects. Texts and effect values live in `UiTexts` and
  `UiEffects`.
- **Aspect ratios:** checked at 20:9 (2400×1080), 16:9 (1920×1080) and 4:3 (1600×1200).

## Tests

`Window > General > Test Runner > EditMode > Run All`

`ZoneProgressionTests` plays winning spins from code up to zones 1, 2, 5, 6,
30, 60 and 65 and checks the wheel, the bomb and the leave rule at each one,
plus reward totals, bomb/restart and leaving on a normal zone.
`EventBusTests`, `InventoryTests` and `RewardScalingTests` cover the bus, the
wallet and the amount growth.
