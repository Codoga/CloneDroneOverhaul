using HarmonyLib;
using OverhaulMod.Engine;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(TitleScreenUI))]
    internal static class TitleScreenUI_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TitleScreenUI.OnCloneDroneLogoClicked))]
        private static bool OnCloneDroneLogoClicked_Prefix(ChapterLoadingScreen __instance)
        {
            TitleScreenCustomizationManager titleScreenCustomizationManager = TitleScreenCustomizationManager.Instance;
            if (titleScreenCustomizationManager && titleScreenCustomizationManager.overrideLevelDescription != null)
            {
                if (!titleScreenCustomizationManager.disallowClickingLogo)
                {
                    GlobalEventManager.Instance.Dispatch("SimpleAchivementEvent", SimpleAchievementEvent.ClickedCloneDroneLogo);
                    ArenaCameraManager.Instance.TitleScreenLogoCamera.GetComponent<Animator>().Play("RobotClicked", 0, 0f);
                    _ = AudioManager.Instance.PlayClipGlobal(AudioLibrary.Instance.DogVoteUpZap, 0f, false, 1f, 0f);
                    titleScreenCustomizationManager.disallowClickingLogo = true;
                    DelegateScheduler.Instance.Schedule(delegate
                    {
                        if (titleScreenCustomizationManager)
                            titleScreenCustomizationManager.disallowClickingLogo = false;
                    }, 0.7f);
                }
                return false;
            }
            return true;
        }
    }
}
