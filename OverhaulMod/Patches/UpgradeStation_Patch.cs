using HarmonyLib;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(UpgradeStation))]
    internal static class UpgradeStation_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UpgradeStation.Start))]
        private static void Start_Prefix(UpgradeStation __instance)
        {
            PressActionKeyObjectManager.PatchKeyboardHint(__instance.UpgradePressEPrefab.transform);
        }
    }
}
