using KSerialization;
using UnityEngine;

namespace BatterySwitcher
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class BatterySwitcherController : Generator
    {
        [Serialize]
        private float batteryAEnergy;

        [Serialize]
        private float batteryBEnergy;

        [Serialize]
        private bool batteryAIsCharging = true;

        private static StatusItem bufferStatus;
        private Battery inputBattery;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            inputBattery = GetComponent<Battery>();
            batteryAEnergy = ClampEnergy(batteryAEnergy);
            batteryBEnergy = ClampEnergy(batteryBEnergy);
            AssignJoulesAvailable(0f);

            if (bufferStatus == null)
            {
                bufferStatus = new StatusItem(
                    "BatterySwitcherBuffers",
                    "Battery buffers",
                    "Battery buffer charge and roles.",
                    "",
                    StatusItem.IconType.Info,
                    NotificationType.Neutral,
                    false,
                    OverlayModes.None.ID,
                    resolve_string_callback: ResolveBufferStatus);
                bufferStatus.resolveTooltipCallback = ResolveBufferStatus;
            }
            GetComponent<KSelectable>().AddStatusItem(bufferStatus, this);
        }

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            batteryAEnergy = ClampEnergy(batteryAEnergy);
            batteryBEnergy = ClampEnergy(batteryBEnergy);

            while (inputBattery.JoulesAvailable > 0f)
            {
                SwitchAtBoundary();
                float transferred = Mathf.Min(
                    inputBattery.JoulesAvailable,
                    BatterySwitcherConfig.ChargeStopEnergy - ChargingEnergy);
                if (transferred <= 0f)
                    break;

                inputBattery.ConsumeEnergy(transferred, false);
                ChargingEnergy = ClampEnergy(ChargingEnergy + transferred);
            }

            SwitchAtBoundary();
            inputBattery.chargeWattage =
                ChargingEnergy < BatterySwitcherConfig.ChargeStopEnergy
                    ? BatterySwitcherConfig.InputWattage
                    : 0f;
            float outputAvailable = Mathf.Max(
                0f,
                SupplyingEnergy - BatterySwitcherConfig.DischargeStopEnergy);
            if (ChargingEnergy >= BatterySwitcherConfig.ChargeStopEnergy)
                outputAvailable += Mathf.Max(
                    0f,
                    ChargingEnergy - BatterySwitcherConfig.DischargeStopEnergy);
            AssignJoulesAvailable(outputAvailable);
        }

        public override void ApplyDeltaJoules(float joules, bool doDisease)
        {
            if (joules < 0f)
            {
                float remaining = -joules;
                while (remaining > 0f)
                {
                    SwitchAtBoundary();
                    float transferred = Mathf.Min(
                        remaining,
                        Mathf.Max(
                            0f,
                            SupplyingEnergy - BatterySwitcherConfig.DischargeStopEnergy));
                    if (transferred <= 0f)
                        break;

                    SupplyingEnergy = ClampEnergy(SupplyingEnergy - transferred);
                    remaining -= transferred;
                }
                SwitchAtBoundary();
            }
            base.ApplyDeltaJoules(joules, doDisease);
        }

        private float ChargingEnergy
        {
            get => batteryAIsCharging ? batteryAEnergy : batteryBEnergy;
            set
            {
                if (batteryAIsCharging)
                    batteryAEnergy = value;
                else
                    batteryBEnergy = value;
            }
        }

        private float SupplyingEnergy
        {
            get => batteryAIsCharging ? batteryBEnergy : batteryAEnergy;
            set
            {
                if (batteryAIsCharging)
                    batteryBEnergy = value;
                else
                    batteryAEnergy = value;
            }
        }

        private void SwitchAtBoundary()
        {
            if (ChargingEnergy >= BatterySwitcherConfig.ChargeStopEnergy &&
                SupplyingEnergy <= BatterySwitcherConfig.DischargeStopEnergy)
                batteryAIsCharging = !batteryAIsCharging;
        }

        private static string ResolveBufferStatus(string _, object data)
        {
            BatterySwitcherController controller = (BatterySwitcherController)data;
            string capacity = GameUtil.GetFormattedJoules(BatterySwitcherConfig.BufferCapacity);
            return $"A ({GetState(controller.batteryAIsCharging, controller.batteryAEnergy)}): " +
                $"{GameUtil.GetFormattedJoules(controller.batteryAEnergy)} / {capacity}\n" +
                $"B ({GetState(!controller.batteryAIsCharging, controller.batteryBEnergy)}): " +
                $"{GameUtil.GetFormattedJoules(controller.batteryBEnergy)} / {capacity}\n" +
                $"Total: {GameUtil.GetFormattedJoules(controller.batteryAEnergy + controller.batteryBEnergy)} / " +
                GameUtil.GetFormattedJoules(2f * BatterySwitcherConfig.BufferCapacity);
        }

        private static string GetState(bool charging, float energy)
        {
            if (charging)
                return energy >= BatterySwitcherConfig.ChargeStopEnergy
                    ? "charging stopped"
                    : "charging";
            return energy <= BatterySwitcherConfig.DischargeStopEnergy
                ? "discharging stopped"
                : "discharging";
        }

        private static float ClampEnergy(float joules)
        {
            return float.IsNaN(joules) || float.IsInfinity(joules)
                ? 0f
                : Mathf.Clamp(joules, 0f, BatterySwitcherConfig.BufferCapacity);
        }
    }
}
