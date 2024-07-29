﻿using HarmonyLib;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(GameFlowManager))]
    internal static class GameFlowManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameFlowManager.StartStoryModeGame))]
        private static void StartStoryModeGame_Prefix(GameFlowManager __instance, bool resetData = false)
        {
            if (!resetData)
            {
                GameDataManager gameDataManager = GameDataManager.Instance;
                if (gameDataManager)
                {
                    string levelId = gameDataManager._storyModeData.CurentLevelID;
                    if (!levelId.IsNullOrEmpty() && levelId.Contains("C5"))
                    {
                        ModGameUtils.overrideCurrentLevelId = levelId;
                        ModGameUtils.overrideActiveSections = gameDataManager._storyModeData.CurrentLevelSectionsVisible;
                    }
                }
            }
        }
    }
}
