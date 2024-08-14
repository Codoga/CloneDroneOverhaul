using HarmonyLib;
using OverhaulMod.Utils;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(LevelEditorDataManager))]
    internal static class LevelEditorDataManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(LevelEditorDataManager.DeserializeInto))]
        private static void DeserializeInto_Prefix(Transform levelRoot, ref LevelEditorLevelData currentLevelData, bool isAsync = false)
        {
            ModGameUtils.currentLevelMetaData = currentLevelData?.ModdedMetadata;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(LevelEditorDataManager.LevelHasAtLeastOneEnemyForDifficulty))]
        private static bool LevelHasAtLeastOneEnemyForDifficulty_Prefix(ref bool __result, Transform currentLevelTransform, int currentWorkshopLevelDifficultyIndex)
        {
            if (GameModeManager.IsOnTitleScreen())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
