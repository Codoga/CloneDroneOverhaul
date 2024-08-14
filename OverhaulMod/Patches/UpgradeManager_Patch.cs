using HarmonyLib;
using OverhaulMod.Engine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(UpgradeManager))]
    internal static class UpgradeManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UpgradeManager.hideUI))]
        private static bool hideUI_Prefix(UpgradeManager __instance)
        {
            if (AutoBuildManager.Instance && AutoBuildManager.Instance.isInAutoBuildConfigurationMode)
            {
                __instance.SetUpgradeButtonsDisabled(false);
                return false;
            }

            return true;
        }
    }
}
