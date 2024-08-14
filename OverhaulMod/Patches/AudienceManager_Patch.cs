using HarmonyLib;
using OverhaulMod.Engine;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(AudienceManager))]
    internal static class AudienceManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(AudienceManager.InitializeAudienceDataForLevel))]
        private static void InitializeAudienceDataForLevel_Postfix()
        {
            ArenaAudienceManager arenaAudienceManager = ArenaAudienceManager.Instance;
            if (arenaAudienceManager && ArenaRemodelManager.EnableRemodel)
            {
                arenaAudienceManager.PatchAudienceRotation();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AudienceManager.addAudiencePositions))]
        private static bool addAudiencePositions_Prefix(AudienceManager __instance, List<Vector3> potentialAudiencePositions, AudiencePlacementLine audiencePlacementLine)
        {
            return audiencePlacementLine && audiencePlacementLine.StartPos && audiencePlacementLine.EndPos;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AudienceManager.PlayAudienceReaction))]
        private static bool PlayAudienceReaction_Prefix()
        {
            LevelManager levelManager = LevelManager.Instance;
            if (!levelManager || levelManager.IsCurrentLevelHidingTheArena())
                return false;

            return true;
        }
    }
}
