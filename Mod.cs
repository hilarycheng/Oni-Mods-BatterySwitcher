using HarmonyLib;
using KMod;
using UnityEngine;

namespace BatterySwitcher
{
    public sealed class Mod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            Debug.Log("[BatterySwitcher] Mod loaded");
        }
    }
}
