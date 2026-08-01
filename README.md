# Battery Switcher

Battery Switcher is an *Oxygen Not Included* mod that adds a standalone 3×2
power building with two isolated internal 20 kJ batteries.

One battery charges from the input circuit while the other supplies a separate
output circuit. Charging stops at 80% and discharging stops at 30% by default;
each battery's thresholds can be changed in its side screen. The buffers
exchange roles when both thresholds are reached. Output is limited by usable
stored energy and ONI's normal wire rules, not an artificial building wattage
cap. The building costs 400 kg of refined metal and generates 1.05 kDTU/s
while supplying output power.

When an output draw reaches a supplier's low threshold, the remaining draw
continues from the newly supplying buffer in the same simulation step.

## Inspiration and source

Battery Switcher is inspired by Saturnus' classic switched-battery concept:

> "switch which battery is connected to the generator, and which is connected
> to the consumers."

The original discussion explains the alternating two-battery design, which
keeps the generator and consumer circuits isolated:
[Serial Smart Battery Automation](https://forums.kleientertainment.com/forums/topic/87604-serial-smart-battery-automation/#findComment-1004188).

Source code: [hilarycheng/Oni-Mods-BatterySwitcher](https://github.com/hilarycheng/Oni-Mods-BatterySwitcher).

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
