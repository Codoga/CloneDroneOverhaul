﻿using HarmonyLib;

namespace CDOverhaul.Patches
{
    [HarmonyPatch(typeof(CharacterModel))]
    internal static class CharacterModel_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetManualUpperAnimationEnabled")]
        private static void SetManualUpperAnimationEnabled_Postfix(CharacterModel __instance, bool animationIsManual)
        {
            if (!OverhaulMod.IsCoreCreated)
            {
                return;
            }
        }
    }
}