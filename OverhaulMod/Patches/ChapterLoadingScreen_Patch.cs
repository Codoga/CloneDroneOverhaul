using HarmonyLib;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(ChapterLoadingScreen))]
    internal static class ChapterLoadingScreen_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ChapterLoadingScreen.Show))]
        private static void Show_Postfix(ChapterLoadingScreen __instance)
        {
            _ = ModUIConstants.ShowLoadingScreen();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ChapterLoadingScreen.Hide))]
        private static void Hide_Prefix(ChapterLoadingScreen __instance)
        {
            ModUIConstants.HideLoadingScreen();
        }
    }
}
