﻿using HarmonyLib;
using OverhaulMod.Engine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(LevelManager))]
    internal static class LevelManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetCurrentLevelDescription")]
        private static bool GetCurrentLevelDescription_Prefix(ref LevelDescription __result)
        {
            if (GameModeManager.IsOnTitleScreen())
            {
                TitleScreenCustomizationManager titleScreenCustomizationManager = TitleScreenCustomizationManager.Instance;
                if (titleScreenCustomizationManager && titleScreenCustomizationManager.overrideLevelDescription != null)
                {
                    __result = titleScreenCustomizationManager.overrideLevelDescription;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("setCurrentDifficultyIndex")]
        private static bool setCurrentDifficultyIndex_Prefix(LevelManager __instance, LevelDescription levelDescription, LevelEditorLevelData levelData)
        {
            if (levelDescription != null && levelDescription.LevelID == "customTitleScreenLevel")
            {
                __instance._currentWorkshopLevelDifficultyIndex = 0;
                return false;
            }
            return true;
        }
    }
}
