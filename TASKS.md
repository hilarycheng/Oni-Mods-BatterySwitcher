# BatterySwitcher Tasks

Complete phases in order. A successful build does not replace an in-game gate.
Run `dotnet build` after every code change.

## Current state

- [x] `netstandard2.1` project and `KMod.UserMod2` entry point exist.
- [x] Baseline build succeeds against the installed ONI assemblies.
- [x] Investigate the current `System.IO.Compression` and `System.Net.Http`
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

PLib's pinned `PowerRequirement` establishes the distinct input and output
building offsets. Its generated input `EnergyConsumer` is removed at runtime so
the native transfer `Battery` is the only input-circuit load. Its fuel-oriented
`EnergyGenerator` is replaced with ONI's public base `Generator`, because U59's
`EnergyGenerator` requires a non-empty fuel formula. The battery and generator
own their `CircuitManager` spawn/cleanup registration.

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
as transformer input and is the sole runtime input consumer, so ONI accounts
its 1 kW charging load without competing building demand. The verified input
port offset is `(-1, 0)`; the output remains `(1, 0)`.

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
switching thresholds, 400 kg refined-metal cost, 3×2 footprint, input `(-1, 0)`,
and output `(1, 0)`. U59's `BatterySmartConfig` sets active self-heat directly
to 0.5 kDTU/s, so Battery Switcher uses 1.0 kDTU/s for its two battery sections
plus the named 0.05 kDTU/s switching margin. Active means that output energy
moved during the latest simulation update; charging alone produces no heat.

### Gate

- [x] Final electrical behavior, heat, and balance values pass in game.
- [x] Footprint and both port offsets are frozen.

## Phase 7 — custom KAnim

- [x] Produce and approve concept art only after the phase-6 footprint gate.
- [x] Convert the design into clean layered sprites with two battery sections,
      a central controller, and clear non-text A/B indicators.
- [x] Build at least build, idle/off, working, and broken states where the
      building behavior supports them.
- [x] Keep pivots, footprint, ports, building ID, and serialized fields
      unchanged.
- [x] Keep source art outside the runtime package.

The final world art is 290×215 px inside the frozen 3×2 footprint. The
dedicated 116×86 px `ui` sprite prevents the Power-menu icon from rendering at
building scale. The layered SVG and SCML sources remain under `art/`; the
runtime package receives only the compiled atlas, build bytes, and animation
bytes.

### Gate

- [x] Every animation state aligns with the building and ports in game.
- [x] Save/load remains compatible after replacing the temporary animation.

## Phase 8 — runtime per-buffer thresholds

Before coding:

- [x] Use independent low/high thresholds for battery A and battery B.
- [x] Store thresholds per building as serialized integer percentages, defaulting
      to 30%/80%.
- [x] Enforce `0 <= low < high <= 100` with a minimum 1% gap.
- [x] Keep threshold changes simulation-driven: UI callbacks change only the
      configured values; the next simulation update handles stopping or
      switching.
- [x] Use one custom PLib UI side screen because U59's public
      `IActivationRangeTarget` side screen supports only one threshold pair per
      selected object.

Then implement:

- [x] Add permanent serialized A/B low/high fields and deterministic repair for
      missing, invalid, or out-of-range loaded values.
- [x] Apply each physical buffer's high threshold while charging and low
      threshold while supplying, regardless of its current role.
- [x] Add four whole-percentage runtime controls without a global Options
      screen or another dependency.
- [x] Show both configured ranges in the selected-building status.
- [x] If side-screen registration fails, retain the default thresholds and emit
      one clear log message without disabling the building.

### Gate

- [x] Each threshold can be changed independently while running or paused.
- [x] Existing saves load with 30%/80% defaults; new values survive save/load.
- [x] Boundary edits, both-empty, both-full, insufficient input, excess demand,
      pause/resume, all speeds, and rapid speed changes preserve valid roles and
      energy bounds.
- [x] No repeated BatterySwitcher exceptions appear in `Player.log`.

## Phase 9 — release packaging

### Pre-release corrections

- [x] Make the missing-power-API fallback leave the building inert without
      dereferencing an absent `EnergyConsumer`.
- [x] Investigate the `System.Net.Http` and `System.IO.Compression` build
      warnings. Suppress them only after confirming they are harmless.
- [x] Replace deprecated `supportedContent` metadata with the current minimal
      unrestricted `mod_info.yaml`, using the build actually tested:

  ```yaml
  minimumSupportedBuild: 740622
  version: 0.1.0
  APIVersion: 2
  ```

  Update `minimumSupportedBuild` again if the final test uses a newer game
  build. Add `requiredDlcIds` or `forbiddenDlcIds` only if a real restriction
  is introduced.
- [ ] Keep the project, `mod_info.yaml`, Workshop listing, and release tag on
      the same version.
- [x] Preserve the MIT notices for Battery Switcher and the merged PLib,
      including `Copyright 2025 Peter Han`, in the release package.

Current metadata guidance:
https://forums.kleientertainment.com/forums/topic/158363-setting-up-mod_infoyaml-and-archived_versions/

### Build and package

- [x] Run `dotnet build -c Release` against the final checked-in source. Stop
      and resolve any error before packaging; report relevant warnings only.
- [x] Confirm `bin/Release/netstandard2.1/BatterySwitcher.dll` has the expected
      release timestamp and that the Release merge removed the standalone
      `PLib.dll`.
- [x] Assemble a fresh upload directory containing only:

  ```text
  BatterySwitcher.dll
  LICENSE
  mod.yaml
  mod_info.yaml
  preview.png
  anim/assets/batteryswitcher/batteryswitcher_0.png
  anim/assets/batteryswitcher/batteryswitcher_anim.bytes
  anim/assets/batteryswitcher/batteryswitcher_build.bytes
  ```

- [x] Use a square, legible `preview.png`, preferably an in-game screenshot,
      and keep it under 1 MB.
- [x] Confirm the package contains no `PDB`, `.deps.json`, source art, `bin/`,
      `obj/`, decompiler files, development assemblies, Harmony, ONI, Unity,
      PLib, or .NET runtime DLLs.
- [x] Confirm PLib is merged into `BatterySwitcher.dll` and the only shipped
      DLL is `BatterySwitcher.dll`.
- [x] Record the final DLL/package hash.
- [x] Deploy and test these exact bytes;
      do not substitute a later rebuild without repeating the release gate.

#### Exact-byte release procedure

- [x] Create the clean upload directory from the completed Release build and
      package it as a ZIP without adding a parent directory to the archive.
- [x] Record SHA-256 hashes for the runtime DLL and the ZIP in this file,
      replacing the provisional values below.
- [x] Copy that exact clean package to the local mod directory. If the DLL or
      package is rebuilt after this point, recreate the package, hashes, and
      all release-gate tests.

Current locally tested DLL:

- DLL SHA-256: `eebbeac5b197e1ecae7023c825915314f45c959bc8186c87777080a901b3fff6`
- ZIP SHA-256: `1776ff266d7dffd243b530f33527671cae4454ebbcc7e3444007d8ab84390599`

### Local release test

- [x] Install only the clean package in the local mod directory.
- [x] Disable unrelated mods so Battery Switcher is the only enabled local mod.
- [x] In base-game content, start a new game and verify construction,
      selection, deconstruction, save/load, and independently wired input and
      output circuits.
- [x] In base-game content, test both empty, one full, both full, insufficient
      input, excess output demand, every threshold boundary, pause/resume,
      every game speed, and rapid speed changes.
- [x] In base-game content, change all four thresholds while running and
      paused; save and load to confirm new values persist. Load an earlier
      tested save to confirm absent threshold fields default to 30%/80%.
- [x] Repeat the new-game and earlier-save smoke tests in Spaced Out content.
- [x] Exit normally after each content-mode test and confirm `Player.log`
      contains no repeated BatterySwitcher exception or error.

### Workshop dry run

- [ ] Install **Oxygen Not Included Uploader** from Steam Library → Tools.
- [ ] Prepare the Workshop title, description, change note, appropriate tags,
      tested game build, feature summary, source-repository link, and preview.
- [ ] Upload with hidden visibility first and accept the Steam Workshop legal
      agreement before making the item public.
- [ ] Subscribe to the hidden Workshop item, then remove or disable the local
      copy so two mods with `hilary.BatterySwitcher` cannot mask each other.
- [ ] Confirm Steam downloads the expected version and exact runtime files.
- [ ] Verify the Workshop-downloaded DLL and package hashes match the recorded
      release hashes, where Steam's installed layout permits direct comparison.
- [ ] Repeat the base-game and Spaced Out new-game and archived-save smoke
      tests using only the Workshop-downloaded copy.
- [ ] Check the Workshop page, preview, description, tags, visibility, and
      subscriber installation before publishing publicly.

Steam upload guidance:
https://partner.steamgames.com/doc/features/workshop/implementation

### Archived versions and publication

- [ ] Perform one local `archived_versions` selection test using a copy of the
      clean package and its own `mod_info.yaml`; confirm ONI chooses the
      intended package for the simulated build condition.
- [ ] Do not ship a redundant archived copy in the first `0.1.0` release.
      Add an archive only before a later incompatible game-build, DLC, or
      public-testing update.
- [ ] Make the Workshop item public only after the subscribed-copy test passes.
- [ ] Confirm `<Version>` in `BatterySwitcher.csproj`, `version` in
      `mod_info.yaml`, the Workshop listing, and the intended Git tag all read
      `0.1.0` before release.
- [ ] Commit final metadata or documentation changes with a Conventional
      Commit, tag that commit `v0.1.0`, and record the Workshop URL below.

### Gate

- [ ] The exact clean release package passes local and Workshop-subscribed
      new-game and archived-save tests without repeated exceptions.
- [ ] The public Workshop download contains only the approved release files
      and loads with the documented version and compatibility metadata.
