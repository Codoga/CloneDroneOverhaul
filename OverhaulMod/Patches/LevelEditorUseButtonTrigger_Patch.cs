using HarmonyLib;
using OverhaulMod.Engine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(LevelEditorUseButtonTrigger))]
    internal static class LevelEditorUseButtonTrigger_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LevelEditorUseButtonTrigger.Start))]
        private static void Start_Postfix(LevelEditorUseButtonTrigger __instance)
        {
            PressActionKeyObjectManager.Instance.SetTriggerRegistered(__instance, true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LevelEditorUseButtonTrigger.OnDestroy))]
        private static void OnDestroy_Postfix(LevelEditorUseButtonTrigger __instance)
        {
            PressActionKeyObjectManager.Instance.SetTriggerRegistered(__instance, false);
        }
    }
}
