using HarmonyLib;
using KMod;
using PeterHan.PLib.Buildings;
using PeterHan.PLib.Core;

namespace BatterySwitcher
{
    public sealed class Mod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            new PBuildingManager().Register(BatterySwitcherConfig.CreateBuilding());
        }
    }
}
