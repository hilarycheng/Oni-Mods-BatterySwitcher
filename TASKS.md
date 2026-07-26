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

- [x] Inspect the installed ONI assemblies for the current public power-port,
      circuit-registration, and simulation APIs.
- [x] State the selected API assumptions before implementation.
- [x] Add fixed input and output port offsets without changing the verified
      footprint.
- [x] Register each port to its own external circuit; do not patch power
      simulation internals.
- [x] Fail locally with one clear log message if a required public API is
      unavailable.

PLib's pinned `PowerRequirement` maps the input to ONI's `EnergyConsumer`.
Its fuel-oriented `EnergyGenerator` is replaced with ONI's public base
`Generator`, because U59's `EnergyGenerator` requires a non-empty fuel formula.
The consumer and generator use distinct building offsets and own their
`CircuitManager` spawn/cleanup registration.

### Gate

- [x] Input and output wires report different circuits in game.
- [x] Connecting or disconnecting either side does not alter the other side.
- [x] Save/load and deconstruction cleanly restore/remove both connections.

## Phase 4 — one numeric buffer

- [x] Confirm the initial per-buffer capacity and input/output wattage before
      implementing energy transfer.
- [x] Add `BatterySwitcherController.cs` using a simulation callback found in
      the installed assemblies, never `Update()` or wall-clock time.
- [x] Serialize the first buffer with its permanent field name.
- [x] Verify input-to-buffer and buffer-to-output accounting independently.
- [x] Clamp energy to `[0, capacity]` and account from actual transferred
      energy.
- [x] Verify a delayed large simulation update produces the same energy result
      as equivalent smaller updates.

Initial phase-4 values are 20 kJ per buffer and 1 kW input. Output is
demand-driven and limited by available buffer energy and normal circuit/wire
rules, not an artificial building wattage cap. U59's `PowerTransformer` is the
transfer reference: an input `Battery` records actual circuit energy, while
`Generator.ApplyDeltaJoules` reports actual output consumption. Battery
Switcher's capacity remains the serialized numeric buffer; the native input
battery is only a zero-leak 1 kJ circuit-transfer accumulator. It is registered
as transformer input so ONI includes charging in current wire load; the
zero-draw input consumer preserves the 1 kW potential load. The verified input
port offset is `(0, 0)`; the output remains `(1, 0)`.

### Gate

- [x] One-buffer charge, discharge, save/load, pause/resume, and all game
      speeds conserve energy and remain in bounds.

## Phase 5 — deterministic two-buffer switching

Before coding, define the unresolved switching policy:

- [x] Switch only when the charger reaches its upper threshold and the
      supplier reaches its lower threshold.
- [x] Define deterministic behavior for both-empty, simultaneous
      boundaries, and insufficient input.
- [x] When both buffers are full, stop input consumption; output and the
      current roles otherwise remain unchanged.
- [x] Define how remaining transferable energy is processed after a mid-update
      switch.

Charging stops at 80% and discharging stops at 30%. The roles exchange only
when both thresholds have been reached, so both-empty startup charges one
buffer without supplying output. Remaining input energy and actual output
demand continue after a mid-update exchange where possible.
The output generator offers all currently usable buffer energy. Its public
`GeneratorBaseCapacity` is only a transfer ledger sized for both buffers, so
sequential consumers share the offer while ONI applies normal wire-load rules.

Then implement:

- [x] Add the second serialized energy field and serialized charging-role
      field using permanent names.
- [x] Repair invalid loaded values and role state deterministically.
- [x] Enforce exactly one charger and one supplier; neither buffer may hold
      both roles.
- [x] Process boundary crossings without exact-float equality or zero-progress
      switch loops.
- [x] Preserve energy and bounds across every switch.
- [x] Show each buffer's charge and threshold-aware role on separate lines,
      plus combined stored energy, when the building is selected.

### Gate

- [x] Selected status text tracks both role exchanges and stored-energy
      changes.
- [x] Both empty, one full, both full, insufficient input, and excess output
      demand are safe.
- [x] A 10 W lamp and a 240 W gas pump receive power simultaneously while a
      supplier is above its discharge threshold.
- [x] The input wire reports 1 kW current load while charging and 1 kW
      potential load.
- [x] Save/load succeeds with A charging, B charging, and at each boundary.
- [x] Pause/resume, all speeds, rapid speed changes, and delayed updates never
      leave output permanently disabled.
- [x] No repeated BatterySwitcher exceptions appear in `Player.log`.

## Phase 6 — heat and final balance

- [x] Confirm final capacities, wattages, construction cost, footprint, and
      switching policy.
- [x] Read the current Smart Battery active heat from stable game data where
      practical.
- [x] Set active heat to twice that value plus one named switching-margin
      constant equal initially to 10% of one Smart Battery's active heat.
- [x] Add no leakage, efficiency loss, automation behavior, or capacity change.
- [x] Repeat the phase-5 manual gate with final values.

Final values retain the phase-5-tested 20 kJ per buffer, 1 kW input, 80%/30%
switching thresholds, 400 kg refined-metal cost, 3×2 footprint, input `(0, 0)`,
and output `(1, 0)`. U59's `BatterySmartConfig` sets active self-heat directly
to 0.5 kDTU/s, so Battery Switcher uses 1.0 kDTU/s for its two battery sections
plus the named 0.05 kDTU/s switching margin. Active means that output energy
moved during the latest simulation update; charging alone produces no heat.

### Gate

- [x] Final electrical behavior, heat, and balance values pass in game.
- [x] Footprint and both port offsets are frozen.

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
