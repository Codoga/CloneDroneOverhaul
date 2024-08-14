using HarmonyLib;
using OverhaulMod.Utils;
using System.Collections.Generic;
using UnityEngine.UI;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(EndlessModeManager))]
    internal static class EndlessModeManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(EndlessModeManager.GetDifficultyTierDropdownOptions))]
        private static void GetDifficultyTierDropdownOptions_Postfix(EndlessModeManager __instance, ref List<Dropdown.OptionData> __result)
        {
            if (!ModBuildInfo.ENABLE_V5)
                return;

            __result = new List<Dropdown.OptionData>();
            for (int i = 0; i < __instance.TierDescriptions.Length; i++)
            {
                EndlessTierDescription tierDescription = __instance.TierDescriptions[i];
                if (tierDescription.Tier.IsModdedEnumValue() && GameModeManager.IsMultiplayer())
                    continue;

                __result.Add(new Dropdown.OptionData(LocalizationManager.Instance.GetTranslatedString(tierDescription.TierDescription)));
            }
        }
    }
}
