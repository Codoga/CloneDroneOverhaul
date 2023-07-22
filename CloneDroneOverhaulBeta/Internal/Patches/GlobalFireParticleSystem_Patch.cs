﻿using HarmonyLib;
using OverhaulAPI;
using UnityEngine;

namespace CDOverhaul.Patches
{
    [HarmonyPatch(typeof(GlobalFireParticleSystem))]
    internal static class GlobalFireParticleSystem_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CreateGroundImpactVFX")]
        private static void CreateGroundImpactVFX_Postfix(Vector3 positon)
        {
            if (!OverhaulMod.IsModInitialized)
                return;

            PooledPrefabController.SpawnEntry<PooledPrefabInstanceBase>(Graphics.OverhaulVFXController.FIRE_VFX, positon, Vector3.zero);
        }
    }
}