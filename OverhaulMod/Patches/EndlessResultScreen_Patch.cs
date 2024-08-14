using HarmonyLib;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(EndlessResultScreen))]
    internal static class EndlessResultScreen_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EndlessResultScreen.Show))]
        private static bool Show_Prefix(EndlessResultScreen __instance)
        {
            //ModUIConstants.ShowEndlessGameLossWindow();
            return true;
        }
    }
}
