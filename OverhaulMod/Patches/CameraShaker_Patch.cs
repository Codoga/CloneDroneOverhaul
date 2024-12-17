﻿using HarmonyLib;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(CameraShaker))]
    internal static class CameraShaker_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CameraShaker.ShakeCamera))]
        private static bool ShakeCamera_Prefix(CameraShaker __instance)
        {
            return !ModCore.DisableScreenShaking || GameModeManager.IsInLevelEditor();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CameraShaker.Update))]
        private static void Update_Postfix(CameraShaker __instance)
        {
            Vector3 position;
            if (__instance._temporaryShakeStartTime < 0f)
            {
                position = Vector3.zero;
            }
            else
            {
                position = __instance.transform.localPosition;
            }

            CameraModeController cameraModeController = ModComponentCache.GetCameraModeController(__instance.transform);
            if (cameraModeController)
            {
                cameraModeController.ShakePositionOffset = position;
            }
        }
    }
}
