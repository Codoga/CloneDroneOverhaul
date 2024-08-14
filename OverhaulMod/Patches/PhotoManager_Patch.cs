using HarmonyLib;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(PhotoManager))]
    internal static class PhotoManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PhotoManager.Update))]
        private static bool Update_Prefix(PhotoManager __instance)
        {
            if (__instance._isInPhotoMode)
            {
                if (UIManager.Instance.IsMouseOverUIElement())
                    return false;

                if (Input.GetMouseButtonUp(0))
                    AdvancedPhotoModeManager.Instance.RefreshLightingWithEditedInfo();
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PhotoManager.TriggerPhotoModeOnOff))]
        private static bool TriggerPhotoModeOnOff_Prefix(PhotoManager __instance)
        {
            if (__instance._isInPhotoMode)
                return true;

            ModUIManager modUIManager = ModUIManager.Instance;
            if (modUIManager && modUIManager.IsUIVisible(AssetBundleConstants.UI, ModUIConstants.UI_PAUSE_MENU))
                return false;

            GameUIRoot gameUIRoot = ModCache.gameUIRoot;
            return !gameUIRoot || !gameUIRoot.EscMenu || !gameUIRoot.EscMenu.gameObject.activeInHierarchy;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PhotoManager.Update))]
        private static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction ci = codes[i];
                if (ci.opcode == OpCodes.Call && ci.Calls(ModCache.unityInputGetMouseButtonMethod))
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, ModCache.unityInputGetMouseButtonDownMethod);
                }
            }

            return codes.AsEnumerable();
        }
    }
}
