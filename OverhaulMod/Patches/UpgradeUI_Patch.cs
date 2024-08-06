﻿using HarmonyLib;
using OverhaulMod.Combat;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(UpgradeUI))]
    internal static class UpgradeUI_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UpgradeUI.closeUpgradeUIIfLocalPlayerHasNoSkillPoints))]
        private static bool closeUpgradeUIIfLocalPlayerHasNoSkillPoints_Prefix(UpgradeUI __instance)
        {
            if(AutoBuildManager.Instance && AutoBuildManager.Instance.isInAutoBuildConfigurationMode)
                return false;

            return true;
        }
    }
}
