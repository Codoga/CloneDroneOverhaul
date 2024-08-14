using HarmonyLib;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(LoadingScreen))]
    internal static class LoadingScreen_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LoadingScreen.Show))]
        private static void Show_Postfix()
        {
            _ = ModUIConstants.ShowLoadingScreen();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LoadingScreen.Hide))]
        private static void Hide_Postfix()
        {
            ModUIConstants.HideLoadingScreen();
        }
    }
}
