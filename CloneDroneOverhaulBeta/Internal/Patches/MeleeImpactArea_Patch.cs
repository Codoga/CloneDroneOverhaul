﻿/*using HarmonyLib;
using UnityEngine;

namespace CDOverhaul.Patches
{
    /// <summary>
    /// This patch should fix damage areas offsets i guess
    /// </summary>
    [HarmonyPatch(typeof(MeleeImpactArea))]
    internal static class MeleeImpactArea_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Postfix(MeleeImpactArea __instance)
        {
            if (!OverhaulMod.IsModInitialized)
                return;

            Rigidbody b = __instance.GetComponent<Rigidbody>();
            if (b != null)
            {
                b.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }
    }
}*/