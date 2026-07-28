# Battery Switcher

Battery Switcher is an *Oxygen Not Included* mod that adds a standalone power
building with two isolated internal batteries.

One 20 kJ buffer charges from an isolated 1 kW input while the other supplies
an isolated output circuit. Charging stops at 80% and discharging stops at 30%
by default; each buffer's thresholds can be changed in its side screen. The
buffers exchange roles when both thresholds are reached. Output is
limited by usable stored energy and ONI's normal wire rules, not an artificial
building wattage cap. The input wire reports its 1 kW charging load through
ONI's native circuit accounting. The 3×2 building costs 400 kg of refined
metal and generates 1.05 kDTU/s while supplying output power.

## Graphics

The custom KAnim keeps the frozen 3×2 footprint. Its in-world art is 290×215
pixels, while its dedicated Power-menu icon is 116×86 pixels. Input and output
socket artwork remains aligned with offsets `(-1, 0)` and `(1, 0)`.

`art/batteryswitcher/source.svg` is the canonical layered source. Its 320×220
tiles are ordered as follows:

| Row | Left | Middle | Right |
| --- | --- | --- | --- |
| 1 | body | left off | left working |
| 2 | right off | right working | switch idle |
| 3 | switch working | switch broken | damage |
| 4 | build | menu icon | placement |

The checked-in trimmed sprites and SCML produce the build, idle/off, working,
broken, placement, and menu states. The approved AI concept was reference
only; it is not checked in or shipped.

## Rebuild the KAnim

Install the pinned Linux converter and its image dependency:

```sh
sudo apt install curl unzip libgdiplus
curl -fL https://github.com/skairunner/kanimal-SE/releases/download/1.3.31/Linux.Self.contained.zip \
  -o /tmp/kanimal-se.zip
mkdir -p ~/.local/share/kanimal-se ~/.local/bin
unzip -o /tmp/kanimal-se.zip -d ~/.local/share/kanimal-se
chmod +x ~/.local/share/kanimal-se/kanimal-cli
ln -sf ~/.local/share/kanimal-se/kanimal-cli ~/.local/bin/kanimal-cli
```

From the repository root:

```sh
(cd art/batteryswitcher/sprites &&
  kanimal-cli kanim batteryswitcher.scml -o /tmp/batteryswitcher-kanim)
install -m 0644 /tmp/batteryswitcher-kanim/batteryswitcher.png \
  anim/assets/batteryswitcher/batteryswitcher_0.png
install -m 0644 /tmp/batteryswitcher-kanim/batteryswitcher_build.bytes \
  anim/assets/batteryswitcher/batteryswitcher_build.bytes
install -m 0644 /tmp/batteryswitcher-kanim/batteryswitcher_anim.bytes \
  anim/assets/batteryswitcher/batteryswitcher_anim.bytes
```

Source art stays under `art/`; only the three compiled KAnim files under
`anim/` belong in the runtime package.
