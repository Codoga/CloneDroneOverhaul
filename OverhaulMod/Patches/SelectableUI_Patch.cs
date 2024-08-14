using HarmonyLib;
using OverhaulMod.Utils;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(SelectableUI))]
    internal static class SelectableUI_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SelectableUI.Start))]
        private static void Start_Prefix(SelectableUI __instance)
        {
            /*
            GameUIThemeData gameUIThemeData = __instance.GameThemeData;
            if (gameUIThemeData && !ModCache.gameUIThemeData)
            {
                gameUIThemeData.ButtonBackground[0].Color = new Color(0.19f, 0.37f, 0.88f, 1);
                gameUIThemeData.ButtonBackground[1].Color = new Color(0.3f, 0.5f, 1, 1f);
                gameUIThemeData.ButtonTextOutline[0].Color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
                gameUIThemeData.ButtonTextOutline[1].Color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
                ModCache.gameUIThemeData = __instance.GameThemeData;
            }
            else if (!gameUIThemeData && ModCache.gameUIThemeData)
            {
                __instance.GameThemeData = ModCache.gameUIThemeData;
            }*/

            if (!__instance.GameThemeData)
            {
                GameUIThemeData gameUIThemeData = ModCache.gameUIThemeData;
                if (gameUIThemeData)
                    __instance.GameThemeData = gameUIThemeData;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SelectableUI.onStateEnter))]
        private static bool onStateEnter_Prefix(SelectableUI __instance, UISelectionState stateEntering)
        {
            __instance.updateColorsToState(stateEntering);
            switch (stateEntering)
            {
                case UISelectionState.Selected:
                    if (!__instance.SkipSelectionArrows && __instance.GameThemeData && __instance.GameThemeData.SelectionCornerPrefab)
                    {
                        Animator animator = __instance.getEnabledCornersAnimator();
                        if (animator)
                        {
                            animator.Play("ButtonSelected");
                        }
                    }
                    break;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SelectableUI.onStateExit))]
        private static bool onStateExit_Prefix(SelectableUI __instance, UISelectionState stateExiting)
        {
            switch (stateExiting)
            {
                case UISelectionState.Selected:
                    if (!__instance.SkipSelectionArrows && __instance.GameThemeData && __instance.GameThemeData.SelectionCornerPrefab)
                    {
                        Animator animator = __instance.getEnabledCornersAnimator();
                        if (animator)
                        {
                            animator.Play("ButtonDeselected");
                        }
                    }
                    break;
            }
            return false;
        }
    }
}
