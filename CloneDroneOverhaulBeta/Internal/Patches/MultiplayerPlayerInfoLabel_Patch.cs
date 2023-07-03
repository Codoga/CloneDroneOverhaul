﻿using CDOverhaul.Gameplay.QualityOfLife;
using HarmonyLib;
using PicaVoxel;
using UnityEngine;

namespace CDOverhaul.Patches
{
    [HarmonyPatch(typeof(MultiplayerPlayerInfoLabel))]
    internal static class MultiplayerPlayerInfoLabel_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Initialize")]
        private static void Initialize_Postfix(MultiplayerPlayerInfoLabel __instance, MultiplayerPlayerInfoState playerState)
        {
            if (!OverhaulMod.IsModInitialized || !playerState || playerState.IsDetached())
                return;

            if (ModBotTagDisabler.DisableTags)
            {
                __instance.PlayerNameLabel.gameObject.AddComponent<ModBotTagRemoverBehaviour>().NormalUsername = playerState.state.DisplayName;
            }
        }
    }
}
