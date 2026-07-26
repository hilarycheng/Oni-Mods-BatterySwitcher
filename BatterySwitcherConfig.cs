using PeterHan.PLib.Buildings;
using STRINGS;
using TUNING;
using UnityEngine;

namespace BatterySwitcher
{
    public sealed class BatterySwitcherConfig : IBuildingConfig
    {
        public const string Id = "BatterySwitcher";

        internal static PBuilding Building { get; private set; }

        internal static PBuilding CreateBuilding()
        {
            Building = new PBuilding(Id, "Battery Switcher")
            {
                AddAfter = BatterySmartConfig.ID,
                Animation = "smartbattery_kanim",
                Category = new HashedString("Power"),
                ConstructionTime = 60f,
                Decor = TUNING.BUILDINGS.DECOR.PENALTY.TIER2,
                Description = "A switching device with two isolated internal battery sections.",
                EffectText = "Phase 1 prototype. It has no power connections or electrical behavior.",
                Height = 2,
                HP = 30,
                Placement = BuildLocationRule.OnFloor,
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
            BuildingDef def = Building.CreateDef();
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
        }
    }
}
