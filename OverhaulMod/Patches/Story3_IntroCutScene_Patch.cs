using HarmonyLib;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(Story3_IntroCutScene))]
    internal static class Story3_IntroCutScene_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Story3_IntroCutScene.startMission))]
        private static void startMission_Postfix()
        {
            MetagameProgressManager.Instance.SetProgress(MetagameProgress.P6_EnteredFleetBeacon);
        }
    }
}
