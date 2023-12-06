﻿using HarmonyLib;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(SelectableUI))]
    internal static class SelectableUI_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        private static void Start_Prefix(SelectableUI __instance)
        {
            GameUIThemeData gameUIThemeData = __instance.GameThemeData;
            if (gameUIThemeData && !ModCache.gameUIThemeData)
            {
                gameUIThemeData.ButtonBackground[0].Color = new Color(0.19f, 0.37f, 0.88f, 1);
                gameUIThemeData.ButtonBackground[1].Color = new Color(0.3f, 0.5f, 1, 1f);
                gameUIThemeData.ButtonTextOutline[0].Color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
                gameUIThemeData.ButtonTextOutline[1].Color = new Color(0.1f, 0.1f, 0.1f, 0.6f);
                ModCache.gameUIThemeData = __instance.GameThemeData;
            }
            if (!gameUIThemeData && ModCache.gameUIThemeData)
            {
                __instance.GameThemeData = ModCache.gameUIThemeData;
            }
        }
    }
}
