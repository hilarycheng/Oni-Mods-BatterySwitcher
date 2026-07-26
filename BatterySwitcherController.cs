using KSerialization;
using UnityEngine;

namespace BatterySwitcher
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class BatterySwitcherController : Generator
    {
        [Serialize]
        private float batteryAEnergy;

        private Battery inputBattery;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            inputBattery = GetComponent<Battery>();
            batteryAEnergy = ClampEnergy(batteryAEnergy);
            AssignJoulesAvailable(0f);
        }

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            batteryAEnergy = ClampEnergy(batteryAEnergy);

            float transferred = Mathf.Min(
                inputBattery.JoulesAvailable,
                BatterySwitcherConfig.BufferCapacity - batteryAEnergy);
            if (transferred > 0f)
            {
                inputBattery.ConsumeEnergy(transferred, false);
                batteryAEnergy += transferred;
            }

            inputBattery.chargeWattage =
                batteryAEnergy < BatterySwitcherConfig.BufferCapacity
                    ? BatterySwitcherConfig.InputWattage
                    : 0f;
            AssignJoulesAvailable(Mathf.Min(
                batteryAEnergy,
                BatterySwitcherConfig.OutputWattage * Mathf.Max(0f, dt)));
        }

        public override void ApplyDeltaJoules(float joules, bool doDisease)
        {
            if (joules < 0f)
                batteryAEnergy = ClampEnergy(batteryAEnergy + joules);
            base.ApplyDeltaJoules(joules, doDisease);
        }

        private static float ClampEnergy(float joules)
        {
            return float.IsNaN(joules) || float.IsInfinity(joules)
                ? 0f
                : Mathf.Clamp(joules, 0f, BatterySwitcherConfig.BufferCapacity);
        }
    }
}
