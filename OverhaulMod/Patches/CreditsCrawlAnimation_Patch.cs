using HarmonyLib;
using OverhaulMod.Content;
using OverhaulMod.Engine;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(CreditsCrawlAnimation))]
    internal static class CreditsCrawlAnimation_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CreditsCrawlAnimation.Hide))]
        private static bool Hide_Prefix(CreditsCrawlAnimation __instance)
        {
            __instance.gameObject.SetActive(false);
            if (__instance._isShowing)
            {
                TimeManager.Instance.SetBaseTimeScale(1f);
                if (GameModeManager.IsOnTitleScreen())
                {
                    GameFlowManager.Instance.SwapOutTitleScreenLevel();
                    AudioManager.Instance.FadeOutMusic(2f);
                    TitleScreenCustomizationManager titleScreenCustomizationManager = TitleScreenCustomizationManager.Instance;
                    if (titleScreenCustomizationManager)
                        titleScreenCustomizationManager.RefreshMusicTrackDelayed(2f);
                }
            }
            __instance._isShowing = false;
            return false;
        }
    }
}
