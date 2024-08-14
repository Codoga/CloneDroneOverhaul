using HarmonyLib;
using OverhaulMod.Combat;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(GameModeManager))]
    internal static class GameModeManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameModeManager.UsesMultiplayerSpeedMultiplier))]
        private static void UsesMultiplayerSpeedMultiplier_Postfix(ref bool __result)
        {
            if (!ModBuildInfo.ENABLE_V5)
                return;

            if (ModFeatures.IsEnabled(ModFeatures.FeatureType.StoryModeModifiers) && GameModeManager.Is(GameMode.Story))
            {
                __result = ModGameModifiersManager.Instance.forceEnableGreatSwords && LevelManager.Instance.GetCurrentLevelID() != "StoryC5_5";
            }
        }

#if DEBUG
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameModeManager.CanLevelsModifyTimeScale))]
        private static void CanLevelsModifyTimeScale_Postfix(ref bool __result)
        {
            if (!__result && GameModeManager.IsLevelPlaytest())
            {
                __result = true;
            }
        }
#endif
    }
}
