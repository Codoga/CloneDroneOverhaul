using HarmonyLib;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(GameplayAchievementManager))]
    internal static class GameplayAchievementManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameplayAchievementManager.SetAchievementProgress))]
        private static void SetAchievementProgress_Postfix(GameplayAchievement achievement, int progress, bool silentCompletion = false)
        {
            if (!GameModeManager.SupportsAchievementTracking())
                return;

            if (achievement && !silentCompletion)
            {
                _ = ModUIConstants.ShowAdvancementProgress(achievement);
            }
        }
    }
}
