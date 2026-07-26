# Battery Switcher

Battery Switcher is an idea for an *Oxygen Not Included* mod that adds a
standalone power building with two isolated internal batteries.

One 20 kJ buffer charges from an isolated 1 kW input while the other supplies
an isolated output circuit. Charging stops at 80%, discharging stops at 30%,
and the buffers exchange roles when both thresholds are reached. Output is
limited by usable stored energy and ONI's normal wire rules, not an artificial
building wattage cap. The input wire reports its 1 kW charging load through
ONI's native circuit accounting. The 3×2 building costs 400 kg of refined
metal and generates 1.05 kDTU/s while supplying output power.
