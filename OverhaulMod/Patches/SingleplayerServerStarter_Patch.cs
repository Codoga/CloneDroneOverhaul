using HarmonyLib;
using OverhaulMod.Engine;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(SingleplayerServerStarter))]
    internal static class SingleplayerServerStarter_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SingleplayerServerStarter.StartServerThenCall))]
        private static void StartServerThenCall_Postfix()
        {
            TitleScreenCustomizationManager.Instance.StopTitleScreenMusic();
            ModUIConstants.HideTitleScreenRework();
        }
    }
}
