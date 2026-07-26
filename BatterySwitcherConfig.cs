using System;
using PeterHan.PLib.Buildings;
using STRINGS;
using TUNING;
using UnityEngine;

namespace BatterySwitcher
{
    public sealed class BatterySwitcherConfig : IBuildingConfig
    {
        public const string Id = "BatterySwitcher";
        internal const float BufferCapacity = 20000f;
        internal const float ChargeStopEnergy = BufferCapacity * 0.8f;
        internal const float DischargeStopEnergy = BufferCapacity * 0.3f;
        internal const float InputWattage = 1000f;
        internal const float SmartBatteryActiveHeat = 0.5f;
        internal const float SwitchingHeatMargin = SmartBatteryActiveHeat * 0.1f;

        internal static PBuilding Building { get; private set; }

        internal static PBuilding CreateBuilding()
        {
            Building = new PBuilding(Id, "Battery Switcher")
            {
                AddAfter = BatterySmartConfig.ID,
                Animation = "batteryswitcher_kanim",
                Category = new HashedString("Power"),
                ConstructionTime = 60f,
                Decor = TUNING.BUILDINGS.DECOR.PENALTY.TIER2,
                Description = "A switching device with two isolated internal battery sections.",
                EffectText = "Transfers power through two alternating internal 20 kJ energy buffers.",
                Height = 2,
                HP = 30,
                Placement = BuildLocationRule.OnFloor,
                PowerInput = new PowerRequirement(InputWattage, new CellOffset(-1, 0)),
                PowerOutput = new PowerRequirement(0f, new CellOffset(1, 0)),
                SubCategory = "batteries",
                Tech = "Acoustics",
                ViewMode = OverlayModes.Power.ID,
                Width = 3
            };
            Building.Ingredients.Add(new BuildIngredient(MATERIALS.REFINED_METALS, 4));
            return Building;
        }

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def;
            try
            {
                def = Building.CreateDef();
            }
            catch (MissingMemberException exception)
            {
                Debug.LogError($"[BatterySwitcher] Power ports disabled: required public power API is unavailable ({exception.GetType().Name}).");
                Building.PowerInput = null;
                Building.PowerOutput = null;
                def = Building.CreateDef();
            }
            def.GeneratorBaseCapacity = 2f * BufferCapacity;
            def.SelfHeatKilowattsWhenActive =
                2f * SmartBatteryActiveHeat + SwitchingHeatMargin;
            def.AddSearchTerms(SEARCH_TERMS.POWER);
            def.AddSearchTerms(SEARCH_TERMS.BATTERY);
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefabTag)
        {
            Building.ConfigureBuildingTemplate(go);
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Building.DoPostConfigureComplete(go);
            UnityEngine.Object.DestroyImmediate(go.GetComponent<EnergyGenerator>());
            go.GetComponent<EnergyConsumer>().BaseWattageRating = 0f;
            Battery input = go.AddOrGet<Battery>();
            input.capacity = InputWattage;
            input.chargeWattage = InputWattage;
            input.joulesLostPerSecond = 0f;
            go.AddOrGet<BatterySwitcherController>();
        }
    }
}
