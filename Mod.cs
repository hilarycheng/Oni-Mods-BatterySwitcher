using HarmonyLib;
using KMod;
using PeterHan.PLib.Buildings;
using PeterHan.PLib.Core;
using PeterHan.PLib.PatchManager;
using PeterHan.PLib.UI;
using UnityEngine;

namespace BatterySwitcher
{
    public sealed class Mod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            new PPatchManager(harmony).RegisterPatchClass(typeof(Mod));
            new PBuildingManager().Register(BatterySwitcherConfig.CreateBuilding());
        }

        [PLibMethod(RunAt.OnDetailsScreenInit)]
        private static void RegisterThresholdSideScreen()
        {
            try
            {
                PUIUtils.AddSideScreenContent<BatterySwitcherThresholdSideScreen>(null);
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    $"[BatterySwitcher] Threshold controls disabled; default thresholds remain active ({exception.GetType().Name}).");
            }
        }
    }
}
