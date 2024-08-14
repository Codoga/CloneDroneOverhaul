using HarmonyLib;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(MultiplayerMatchManager))]
    internal static class MultiplayerMatchManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MultiplayerMatchManager.OnClientStartedConnecting))]
        private static void OnClientStartedConnecting_Postfix(MultiplayerMatchManager __instance)
        {
            ModUIConstants.HideTitleScreenRework();
        }
    }
}
