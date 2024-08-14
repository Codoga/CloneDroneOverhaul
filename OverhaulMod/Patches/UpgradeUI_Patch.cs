using HarmonyLib;
using OverhaulMod.Engine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(UpgradeUI))]
    internal static class UpgradeUI_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UpgradeUI.closeUpgradeUIIfLocalPlayerHasNoSkillPoints))]
        private static bool closeUpgradeUIIfLocalPlayerHasNoSkillPoints_Prefix(UpgradeUI __instance)
        {
            if (AutoBuildManager.Instance && AutoBuildManager.Instance.isInAutoBuildConfigurationMode)
                return false;

            return true;
        }
    }
}
