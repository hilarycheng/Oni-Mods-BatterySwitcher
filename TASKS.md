# BatterySwitcher Tasks

Complete phases in order. A successful build does not replace an in-game gate.
Run `dotnet build` after every code change.

## Current state

- [x] `netstandard2.1` project and `KMod.UserMod2` entry point exist.
- [x] Baseline build succeeds against the installed ONI assemblies.
- [ ] Investigate the current `System.IO.Compression` and `System.Net.Http`
      assembly-version warnings; suppress them only if confirmed harmless.
- [x] Check `mod.yaml` and `mod_info.yaml` into the repository. They currently
      exist only in the local mod directory.
- [x] Choose and freeze the building ID before creating any test save.

## Phase 1 — inert building

- [x] Confirm the current PLib Core/Buildings package IDs and compatible pinned
      versions.
- [x] Configure PLib and ILRepack so PLib is merged into
      `BatterySwitcher.dll`.
- [x] Ensure the build output does not include Harmony, ONI, Unity, or .NET
      runtime assemblies.
- [x] Add `BatterySwitcherConfig.cs` with a temporary base-game animation,
      fixed footprint, construction recipe, and no functional power ports.
- [x] Register the building in the Power build menu through PLib.
- [x] Add the minimum name, description, and effect strings.
- [x] Build and deploy the phase-1 package to the local mod directory.

### Gate

- [x] The game loads with no BatterySwitcher exception in `Player.log`.
- [x] The building appears in the Power menu and can be placed.

## Phase 2 — basic building lifecycle

- [x] Construct and select the building.
- [x] Save and reload with the building present.
- [x] Deconstruct it and verify normal material recovery.
- [x] Confirm repeated load/deconstruct tests add no exceptions to `Player.log`.

### Gate

- [x] Construct, select, save, load, and deconstruct all pass in game.

## Phase 3 — isolated power connections

- [ ] Inspect the installed ONI assemblies for the current public power-port,
      circuit-registration, and simulation APIs.
- [ ] State the selected API assumptions before implementation.
- [ ] Add fixed input and output port offsets without changing the verified
      footprint.
- [ ] Register each port to its own external circuit; do not patch power
      simulation internals.
- [ ] Fail locally with one clear log message if a required public API is
      unavailable.

### Gate

- [ ] Input and output wires report different circuits in game.
- [ ] Connecting or disconnecting either side does not alter the other side.
- [ ] Save/load and deconstruction cleanly restore/remove both connections.

## Phase 4 — one numeric buffer

- [ ] Confirm the initial per-buffer capacity and input/output wattage before
      implementing energy transfer.
- [ ] Add `BatterySwitcherController.cs` using a simulation callback found in
      the installed assemblies, never `Update()` or wall-clock time.
- [ ] Serialize the first buffer with its permanent field name.
- [ ] Verify input-to-buffer and buffer-to-output accounting independently.
- [ ] Clamp energy to `[0, capacity]` and account from actual transferred
      energy.
- [ ] Verify a delayed large simulation update produces the same energy result
      as equivalent smaller updates.

### Gate

- [ ] One-buffer charge, discharge, save/load, pause/resume, and all game
      speeds conserve energy and remain in bounds.

## Phase 5 — deterministic two-buffer switching

Before coding, define the unresolved switching policy:

- [ ] Decide whether a switch is triggered by supplier-empty,
      charger-full, or both.
- [ ] Define deterministic behavior for both-empty, both-full, simultaneous
      boundaries, and insufficient input.
- [ ] Define how remaining transferable energy is processed after a mid-update
      switch.

Then implement:

- [ ] Add the second serialized energy field and serialized charging-role
      field using permanent names.
- [ ] Repair invalid loaded values and role state deterministically.
- [ ] Enforce exactly one charger and one supplier; neither buffer may hold
      both roles.
- [ ] Process boundary crossings without exact-float equality or zero-progress
      switch loops.
- [ ] Preserve energy and bounds across every switch.

### Gate

- [ ] Both empty, one full, both full, insufficient input, and excess output
      demand are safe.
- [ ] Save/load succeeds with A charging, B charging, and at each boundary.
- [ ] Pause/resume, all speeds, rapid speed changes, and delayed updates never
      leave output permanently disabled.
- [ ] No repeated BatterySwitcher exceptions appear in `Player.log`.

## Phase 6 — heat and final balance

- [ ] Confirm final capacities, wattages, construction cost, footprint, and
      switching policy.
- [ ] Read the current Smart Battery active heat from stable game data where
      practical.
- [ ] Set active heat to twice that value plus one named switching-margin
      constant equal initially to 10% of one Smart Battery's active heat.
- [ ] Add no leakage, efficiency loss, automation behavior, or capacity change.
- [ ] Repeat the phase-5 manual gate with final values.

### Gate

- [ ] Final electrical behavior, heat, and balance values pass in game.
- [ ] Footprint and both port offsets are frozen.

## Phase 7 — custom KAnim

- [ ] Produce and approve concept art only after the phase-6 footprint gate.
- [ ] Convert the design into clean layered sprites with two battery sections,
      a central controller, and clear non-text A/B indicators.
- [ ] Build at least build, idle/off, working, and broken states where the
      building behavior supports them.
- [ ] Keep pivots, footprint, ports, building ID, and serialized fields
      unchanged.
- [ ] Keep source art outside the runtime package.

### Gate

- [ ] Every animation state aligns with the building and ports in game.
- [ ] Save/load remains compatible after replacing the temporary animation.

## Phase 8 — release packaging

- [ ] Produce a clean runtime package containing only
      `BatterySwitcher.dll`, `mod.yaml`, `mod_info.yaml`, and required
      `anim/` files.
- [ ] Inspect the final DLL and package for accidentally shipped development,
      framework, Harmony, ONI, Unity, or decompiler files.
- [ ] Test local installation from the clean package.
- [ ] Test Workshop upload metadata, preview image if added, and archived
      version loading.
- [ ] Run the complete manual test list from `AGENTS.md` on the release build.

### Gate

- [ ] Clean release package passes a new-game test and an archived-save test
      without repeated exceptions.
