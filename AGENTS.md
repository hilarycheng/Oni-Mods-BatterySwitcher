# AGENTS.md

## Goal

Build a small Oxygen Not Included mod named `BatterySwitcher`.

It adds one new standalone power building. It does not modify Smart Battery, Power Shutoff, Power Transformer, or any other existing building.

The final building contains two isolated internal energy buffers:

- one buffer charges from the input circuit;
- the other buffer supplies the output circuit;
- the buffers exchange roles automatically;
- switching must remain correct through speed changes, delayed simulation updates, pause/resume, and save/load.

## Environment

- Linux
- fish shell
- Project: repository root
- ONI assemblies: `~/.local/share/Steam/steamapps/common/OxygenNotIncluded/OxygenNotIncluded_Data/Managed`
- Local mod: `~/.config/unity3d/Klei/Oxygen Not Included/mods/Local/BatterySwitcher`
- Framework: `netstandard2.1`
- Build: `dotnet build`

Override a non-default ONI installation with `dotnet build -p:OniManagedDir=/path/to/Managed`.
Inspect the installed ONI assemblies before using power APIs. Do not guess API names from old tutorials.

## Keep it minimal

Prefer only:

```text
BatterySwitcher.csproj
Mod.cs
BatterySwitcherConfig.cs
BatterySwitcherController.cs
mod.yaml
mod_info.yaml
```

Do not add service layers, factories, generic frameworks, options screens, UI frameworks, test scaffolding, or unrelated helper libraries.

## Dependencies

- Use `KMod.UserMod2` as the entry point.
- Use the Harmony supplied by ONI at runtime.
- Never ship `0Harmony.dll`.
- Use PLib Core/Buildings for new-building registration.
- Pin PLib and merge it into the final mod DLL with ILRepack.
- Never ship ONI, Unity, Harmony, or .NET runtime assemblies.
- Do not use PLib UI or Options unless explicitly requested later.

## Development stages

Do not combine unverified stages.

1. Register an inert, placeable building using a base-game animation.
2. Verify construct, select, deconstruct, save, and load.
3. Add separate input and output power connections.
4. Implement one internal energy buffer and verify accounting.
5. Extend to two buffers with deterministic switching.
6. Add heat and final balance values.
7. Replace the temporary animation with the custom `kanim`.
8. Test Workshop packaging and archived versions.

The checked-in phase-1 code intentionally has no working power ports. Do not pretend that it already implements the final electrical behavior.

## Internal batteries

Prefer two serialized numeric energy buffers controlled by `BatterySwitcherController`.

Do not add two normal ONI `Battery` components unless current assembly inspection proves that they can remain isolated from both external circuits.

Persist:

- battery A energy;
- battery B energy;
- which battery is charging;
- any extra state required for deterministic recovery.

Do not rename the building ID or serialized fields after public release without save migration.

## Simulation correctness

Electrical switching must be simulation-driven, never frame-driven.

- Use a current ONI simulation callback discovered from the installed assemblies.
- Do not use Unity `Update()`, coroutines, animation events, frame counters, or wall-clock time.
- Never rely on one exact tick, one exact threshold equality, or an automation pulse.
- Use elapsed simulation time or actual energy transferred by the game API.
- Delayed large updates must reach the same valid state as several smaller updates.
- If one update crosses a switching boundary, process remaining transferable energy after switching where practical.

Always preserve:

- exactly one charging buffer;
- exactly one supplying buffer;
- no buffer performs both roles;
- energy never goes below zero;
- energy never exceeds capacity;
- invalid loaded state is repaired deterministically;
- energy is conserved except for an explicitly defined loss.

Do not reproduce the original two-Smart-Battery automation circuit. Implement the intended state machine directly.

## Heat

Final active heat:

```text
2 × current Smart Battery active heat + switching margin
```

Read the current Smart Battery value from stable game data when practical. Keep the margin as one named constant. Initial margin target: 10% of one Smart Battery's active heat.

Do not add unrequested leakage, efficiency loss, automation behavior, or altered capacities.

## Compatibility

- Prefer public ONI APIs.
- Avoid private fields and private methods.
- Avoid reflection when a public API exists.
- Avoid Harmony transpilers.
- Avoid patching power simulation internals.
- Use Harmony only when PLib or a public extension path cannot perform the task.
- A compatibility failure should disable only this building or feature and emit one clear log message.
- Never intentionally crash ONI.

No mod can guarantee permanent compatibility. Minimize contact with game internals so ordinary updates are less likely to break it.

## Visual assets

The temporary base-game animation is only for functional development.

The final Workshop release must have a custom ONI-style `kanim` for Battery Switcher.

- AI may generate concept art and visual references.
- Do not ship a raw flat AI image as the final animation.
- Convert the approved design into clean layered sprites suitable for the ONI KAnim pipeline.
- Show two visibly separate internal battery sections and a central switching controller.
- Provide clear A/B state indicators without depending on tiny text.
- Required states should include at least build, idle/off, working, and broken where supported by the building behavior.
- Keep footprint, pivots, and power-port locations consistent with the code.
- Replacing the temporary animation must not change the building ID or save fields.
- Keep source art outside the runtime package; ship only the generated game animation assets and required atlas files.

Do not start final asset production until the footprint and port offsets are verified in game.

## Manual tests

At minimum verify:

- appears in the Power build menu;
- can be built, selected, deconstructed, saved, and loaded;
- input and output circuits remain separate;
- all buffer states recover correctly after save/load;
- all game speeds work;
- rapid speed changes never leave output permanently disabled;
- pause/resume is safe;
- both empty, one full, insufficient input, and excess output demand are safe;
- no repeated exceptions appear in `Player.log`.

## Packaging

Final runtime package should normally contain:

```text
BatterySwitcher.dll
mod.yaml
mod_info.yaml
anim/...
```

A Workshop preview image and translations may be added later.

Never ship development assemblies, `bin/`, `obj/`, decompiler files, or framework DLLs.

## Codex rules

- Keep replies short and precise.
- Explain a material ONI API assumption before implementing it.
- Do not redesign the project without a confirmed technical reason.
- Do not add features beyond this file.
- Run `dotnet build` after each code change.
- Report relevant errors and warnings only.
- Prefer a small verified implementation over a generalized design.
- Use Conventional Commits for commit messages.
