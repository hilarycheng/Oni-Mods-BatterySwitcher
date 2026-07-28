using KSerialization;
using PeterHan.PLib.UI;
using TMPro;
using UnityEngine;

namespace BatterySwitcher
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class BatterySwitcherController : PowerTransformer, ISimEveryTick
    {
        [Serialize]
        private float batteryAEnergy;

        [Serialize]
        private float batteryBEnergy;

        [Serialize]
        private bool batteryAIsCharging = true;

        [Serialize]
        private int batteryALowPercent = BatterySwitcherConfig.DefaultLowPercent;

        [Serialize]
        private int batteryAHighPercent = BatterySwitcherConfig.DefaultHighPercent;

        [Serialize]
        private int batteryBLowPercent = BatterySwitcherConfig.DefaultLowPercent;

        [Serialize]
        private int batteryBHighPercent = BatterySwitcherConfig.DefaultHighPercent;

        private static StatusItem bufferStatus;
        private Battery inputBattery;
        private bool outputEnergyTransferred;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            inputBattery = GetComponent<Battery>();
            batteryAEnergy = ClampEnergy(batteryAEnergy);
            batteryBEnergy = ClampEnergy(batteryBEnergy);
            RepairThresholds();
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
            outputEnergyTransferred = false;
            batteryAEnergy = ClampEnergy(batteryAEnergy);
            batteryBEnergy = ClampEnergy(batteryBEnergy);
            RepairThresholds();

            while (inputBattery.JoulesAvailable > 0f)
            {
                SwitchAtBoundary();
                float transferred = Mathf.Min(
                    inputBattery.JoulesAvailable,
                    ChargingHighEnergy - ChargingEnergy);
                if (transferred <= 0f)
                    break;

                inputBattery.ConsumeEnergy(transferred, false);
                ChargingEnergy = ClampEnergy(ChargingEnergy + transferred);
            }

            SwitchAtBoundary();
            inputBattery.chargeWattage =
                ChargingEnergy < ChargingHighEnergy
                    ? BatterySwitcherConfig.InputWattage
                    : 0f;
            float outputAvailable = Mathf.Max(
                0f,
                SupplyingEnergy - SupplyingLowEnergy);
            if (ChargingEnergy >= ChargingHighEnergy)
                outputAvailable += Mathf.Max(
                    0f,
                    ChargingEnergy - ChargingLowEnergy);
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
                            SupplyingEnergy - SupplyingLowEnergy));
                    if (transferred <= 0f)
                        break;

                    SupplyingEnergy = ClampEnergy(SupplyingEnergy - transferred);
                    remaining -= transferred;
                    outputEnergyTransferred = true;
                }
                SwitchAtBoundary();
            }
            AssignJoulesAvailable(Mathf.Clamp(
                JoulesAvailable + joules,
                0f,
                doDisease ? float.MaxValue : Capacity));
        }

        public void SimEveryTick(float _)
        {
            operational.SetActive(outputEnergyTransferred);
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

        private float ChargingHighEnergy => PercentToEnergy(
            batteryAIsCharging ? batteryAHighPercent : batteryBHighPercent);

        private float ChargingLowEnergy => PercentToEnergy(
            batteryAIsCharging ? batteryALowPercent : batteryBLowPercent);

        private float SupplyingLowEnergy => PercentToEnergy(
            batteryAIsCharging ? batteryBLowPercent : batteryALowPercent);

        private void SwitchAtBoundary()
        {
            if (ChargingEnergy >= ChargingHighEnergy &&
                SupplyingEnergy <= SupplyingLowEnergy)
                batteryAIsCharging = !batteryAIsCharging;
        }

        private static string ResolveBufferStatus(string _, object data)
        {
            BatterySwitcherController controller = (BatterySwitcherController)data;
            string capacity = GameUtil.GetFormattedJoules(BatterySwitcherConfig.BufferCapacity);
            return $"A {controller.batteryALowPercent}–{controller.batteryAHighPercent}% " +
                $"({GetState(controller.batteryAIsCharging, controller.batteryAEnergy, controller.batteryALowPercent, controller.batteryAHighPercent)}): " +
                $"{GameUtil.GetFormattedJoules(controller.batteryAEnergy)} / {capacity}\n" +
                $"B {controller.batteryBLowPercent}–{controller.batteryBHighPercent}% " +
                $"({GetState(!controller.batteryAIsCharging, controller.batteryBEnergy, controller.batteryBLowPercent, controller.batteryBHighPercent)}): " +
                $"{GameUtil.GetFormattedJoules(controller.batteryBEnergy)} / {capacity}\n" +
                $"Total: {GameUtil.GetFormattedJoules(controller.batteryAEnergy + controller.batteryBEnergy)} / " +
                GameUtil.GetFormattedJoules(2f * BatterySwitcherConfig.BufferCapacity);
        }

        private static string GetState(bool charging, float energy, int lowPercent, int highPercent)
        {
            if (charging)
                return energy >= PercentToEnergy(highPercent)
                    ? "charging stopped"
                    : "charging";
            return energy <= PercentToEnergy(lowPercent)
                ? "discharging stopped"
                : "discharging";
        }

        internal int BatteryALowPercent
        {
            get => batteryALowPercent;
            set => batteryALowPercent = Mathf.Clamp(value, 0, batteryAHighPercent - 1);
        }

        internal int BatteryAHighPercent
        {
            get => batteryAHighPercent;
            set => batteryAHighPercent = Mathf.Clamp(value, batteryALowPercent + 1, 100);
        }

        internal int BatteryBLowPercent
        {
            get => batteryBLowPercent;
            set => batteryBLowPercent = Mathf.Clamp(value, 0, batteryBHighPercent - 1);
        }

        internal int BatteryBHighPercent
        {
            get => batteryBHighPercent;
            set => batteryBHighPercent = Mathf.Clamp(value, batteryBLowPercent + 1, 100);
        }

        private void RepairThresholds()
        {
            RepairRange(ref batteryALowPercent, ref batteryAHighPercent);
            RepairRange(ref batteryBLowPercent, ref batteryBHighPercent);
        }

        private static void RepairRange(ref int lowPercent, ref int highPercent)
        {
            if (lowPercent < 0 || highPercent > 100 || lowPercent >= highPercent)
            {
                lowPercent = BatterySwitcherConfig.DefaultLowPercent;
                highPercent = BatterySwitcherConfig.DefaultHighPercent;
            }
        }

        private static float PercentToEnergy(int percent)
        {
            return BatterySwitcherConfig.BufferCapacity * percent / 100f;
        }

        private static float ClampEnergy(float joules)
        {
            return float.IsNaN(joules) || float.IsInfinity(joules)
                ? 0f
                : Mathf.Clamp(joules, 0f, BatterySwitcherConfig.BufferCapacity);
        }
    }

    internal sealed class BatterySwitcherThresholdSideScreen : SideScreenContent
    {
        private readonly TMP_InputField[] valueFields = new TMP_InputField[4];
        private BatterySwitcherController target;

        public BatterySwitcherThresholdSideScreen()
        {
            PGridPanel panel = new PGridPanel("BatterySwitcherThresholds")
            {
                FlexSize = Vector2.right,
                Margin = new RectOffset(8, 8, 6, 6)
            };
            panel.AddColumn(new GridColumnSpec(flex: 1f));
            panel.AddColumn(new GridColumnSpec(width: 56f));
            for (int row = 0; row < 5; row++)
                panel.AddRow(new GridRowSpec());
            panel.AddChild(new PLabel
            {
                Text = "Whole percentages: 0–100.\nLow must stay below high.",
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                FlexSize = Vector2.right
            }, new GridComponentSpec(0, 0)
            {
                Alignment = TextAnchor.MiddleLeft,
                ColumnSpan = 2,
                Margin = new RectOffset(0, 0, 0, 6)
            });
            AddRow(panel, 0, "Battery A low (%)");
            AddRow(panel, 1, "Battery A high (%)");
            AddRow(panel, 2, "Battery B low (%)");
            AddRow(panel, 3, "Battery B high (%)");
            ContentContainer = panel.AddTo(gameObject);
        }

        public override string GetTitle()
        {
            return "Battery thresholds";
        }

        public override bool IsValidForTarget(GameObject candidate)
        {
            return candidate != null &&
                candidate.GetComponent<BatterySwitcherController>() != null;
        }

        public override void SetTarget(GameObject candidate)
        {
            BatterySwitcherController next =
                candidate.GetComponent<BatterySwitcherController>();
            bool changed = target != next;
            target = next;
            RefreshValues(changed);
        }

        public override void ClearTarget()
        {
            target = null;
        }

        private void AddRow(PGridPanel panel, int index, string label)
        {
            panel.AddChild(new PLabel
            {
                Text = label,
                TextAlignment = TextAnchor.MiddleLeft,
                TextStyle = PUITuning.Fonts.TextDarkStyle,
                FlexSize = Vector2.right
            }, new GridComponentSpec(index + 1, 0)
            {
                Alignment = TextAnchor.MiddleLeft,
                Margin = new RectOffset(0, 8, 2, 2)
            });
            panel.AddChild(new PTextField($"BatterySwitcherThresholdValue{index}")
            {
                Text = (index % 2 == 0
                    ? BatterySwitcherConfig.DefaultLowPercent
                    : BatterySwitcherConfig.DefaultHighPercent).ToString(),
                Type = PTextField.FieldType.Integer,
                MaxLength = 3,
                OnTextChanged = (_, text) => SetValue(index, text),
                ToolTip = "Enter a whole percentage from 0 to 100."
            }.SetKleiBlueStyle().AddOnRealize(realized =>
                valueFields[index] = realized.GetComponent<TMP_InputField>()),
                new GridComponentSpec(index + 1, 1)
                {
                    Alignment = TextAnchor.MiddleRight,
                    Margin = new RectOffset(0, 0, 2, 2)
                });
        }

        private void SetValue(int index, string text)
        {
            if (target == null || !int.TryParse(text, out int value))
            {
                RefreshValues();
                return;
            }

            switch (index)
            {
                case 0:
                    target.BatteryALowPercent = value;
                    break;
                case 1:
                    target.BatteryAHighPercent = value;
                    break;
                case 2:
                    target.BatteryBLowPercent = value;
                    break;
                case 3:
                    target.BatteryBHighPercent = value;
                    break;
            }
            RefreshValues(true);
        }

        private void RefreshValues(bool force = false)
        {
            if (target == null)
                return;

            int[] values =
            {
                target.BatteryALowPercent,
                target.BatteryAHighPercent,
                target.BatteryBLowPercent,
                target.BatteryBHighPercent
            };
            for (int index = 0; index < values.Length; index++)
                if (valueFields[index] != null &&
                    (force || !valueFields[index].isFocused))
                    valueFields[index].SetTextWithoutNotify(values[index].ToString());
        }
    }
}
