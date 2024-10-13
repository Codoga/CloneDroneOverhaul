﻿using HarmonyLib;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine.UI;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(GenericKeyboardHint))]
    internal static class GenericKeyboardHint_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GenericKeyboardHint.Show))]
        private static bool Show_Prefix(GenericKeyboardHint __instance)
        {
            bool hideText = UseKeyTriggerManager.EnablePressButtonTriggerDescriptionRework;
            if (hideText)
            {
                Text t = __instance.DescriptionLabel;
                if (t && !t.text.IsNullOrEmpty())
                    t.text = null;
            }

            UseKeyTriggerManager.PatchKeyboardHint(__instance.transform);
            return !hideText;
        }
    }
}
