using HarmonyLib;
using OverhaulMod.Engine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(SettingsMenu))]
    internal static class SettingsMenu_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SettingsMenu.refreshResolutionOptions))]
        private static void refreshResolutionOptions_Postfix(SettingsMenu __instance)
        {
            ModSettingsManager.ExtraResolutionLength = 0;
            Resolution resolution = new Resolution
            {
                width = Screen.width,
                height = Screen.height,
                refreshRate = -1
            };

            List<SettingsResolution> list = __instance._uniqueResolutionOptions;
            foreach (SettingsResolution r in list)
                if (r.Resolution.width == resolution.width && r.Resolution.height == resolution.height)
                    return;

            resolution.refreshRate = list[list.Count - 1].Resolution.refreshRate;
            list.Add(new SettingsResolution(resolution));
            ModSettingsManager.ExtraResolutionLength = 1;
            __instance._uniqueResolutionOptions = list.OrderBy(s => s.Resolution.width).ThenBy(s => s.Resolution.height).ToList();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SettingsMenu.OnScreenResolutionChanged))]
        private static bool OnScreenResolutionChanged_Prefix(SettingsMenu __instance)
        {
            if (Time.realtimeSinceStartup - __instance._timeOpened < 0.1f || __instance.ScreenResolutionDropDown.value >= (Screen.resolutions.Length + ModSettingsManager.ExtraResolutionLength))
                return false;

            SettingsResolution settingsResolution = __instance._uniqueResolutionOptions[__instance.ScreenResolutionDropDown.value];
            SettingsManager.Instance.SetResolution(settingsResolution.Resolution.width, settingsResolution.Resolution.height);
            return false;
        }
    }
}