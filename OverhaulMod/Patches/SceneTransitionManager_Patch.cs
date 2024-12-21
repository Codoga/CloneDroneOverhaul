﻿using HarmonyLib;
using OverhaulMod.Engine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(SceneTransitionManager))]
    internal static class SceneTransitionManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneTransitionManager.DisconnectAndExitToMainMenu))]
        private static bool DisconnectAndExitToMainMenu_Prefix(SceneTransitionManager __instance)
        {
            if (!TransitionManager.OverhaulSceneTransitions)
                return true;

            TransitionManager transitionManager = TransitionManager.Instance;
            transitionManager.DoTransition(TransitionManager.SceneTransitionCoroutine(__instance), transitionManager.GetBlackScreenColor(), true, false);
            return false;
        }
    }
}