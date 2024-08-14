using HarmonyLib;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using Rewired;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(FlyingCameraController))]
    internal static class FlyingCameraController_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FlyingCameraController.Update))]
        private static bool Update_Prefix(FlyingCameraController __instance)
        {
            if (!PhotoManager.Instance.IsInPhotoMode() || __instance._isMovementDisabled)
                return true;

            InputManager inputManager = InputManager.Instance;
            if (UIManager.Instance.IsMouseOverUIElement())
            {
                inputManager.SetCursorEnabled(true);
                return false;
            }

            ModUIManager modUIManager = ModUIManager.Instance;
            Player player = ReInput.players.GetPlayer(0);
            if (player != null)
            {
                bool rmbHeld = player.GetButton(3) || (!AdvancedPhotoModeManager.RequireHoldingRMBWhenUIIsHidden && modUIManager && !modUIManager.IsUIVisible(AssetBundleConstants.UI, ModUIConstants.UI_PHOTO_MODE_UI_REWORK));

                inputManager.SetCursorEnabled(!rmbHeld);
            }
            return true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(FlyingCameraController.Update))]
        private static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction ci = codes[i];
                if (ci.opcode == OpCodes.Call && ci.Calls(ModCache.unityTimeFixedUnscaledDeltaTimePropertyGetter))
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, ModCache.unityTimeUnscaledDeltaTimePropertyGetter);
                }
            }

            return codes.AsEnumerable();
        }
    }
}
