using HarmonyLib;
using OverhaulMod.Engine;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(SkyBoxManager))]
    internal static class SkyBoxManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SkyBoxManager.RefreshSkyboxAmbientLightAndFog))]
        private static void RefreshSkyboxAmbientLightAndFog_Prefix(SkyBoxManager __instance, LevelLightSettings lightSettings)
        {
            ModLevelManager modLevelManager = ModLevelManager.Instance;
            if (modLevelManager && lightSettings)
                modLevelManager.currentSkyBoxIndex = lightSettings.SkyboxIndex;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SkyBoxManager.RefreshSkyboxAmbientLightAndFog))]
        private static void RefreshSkyboxAmbientLightAndFog_Postfix(SkyBoxManager __instance, LevelLightSettings lightSettings)
        {
            if (GameModeManager.IsStoryChapter4())
                RenderSettings.skybox = __instance.LevelConfigurableSkyboxes[7];

            RealisticLightSettings realisticLightSettings = lightSettings.GetComponent<RealisticLightSettings>();
            if (!realisticLightSettings)
                return;

            RealisticLightingManager.Instance.SetSkybox(realisticLightSettings.RealisticSkyBoxIndex);
        }
    }
}
