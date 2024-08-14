using HarmonyLib;
using ModLibrary;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(CustomUpgradeManager))]
    internal static class CustomUpgradeManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CustomUpgradeManager.Update))]
        private static void Update_Postfix(CustomUpgradeManager __instance)
        {
            bool notOnTitleScreen = !GameModeManager.IsOnTitleScreen();

            GameObject backButton = __instance._backButton;
            if (backButton)
                backButton.SetActive(backButton.activeSelf && notOnTitleScreen);

            GameObject nextButton = __instance._nextButton;
            if (nextButton)
                nextButton.SetActive(nextButton.activeSelf && notOnTitleScreen);
        }
    }
}
