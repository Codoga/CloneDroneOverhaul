using HarmonyLib;
using OverhaulMod.Utils;
using System.Collections.Generic;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(GameDataManager))]
    internal static class GameDataManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameDataManager.GetSectionsVisible))]
        private static void GetSectionsVisible_Postfix(ref List<string> __result)
        {
            if (GameModeManager.Is(GameMode.Story))
            {
                List<string> list = ModGameUtils.overrideActiveSections;
                if (list != null)
                    __result = list;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameDataManager.GetCurrentLevelID))]
        private static void GetCurrentLevelID_Postfix(ref string __result)
        {
            if (GameModeManager.Is(GameMode.Story))
            {
                string str = ModGameUtils.overrideCurrentLevelId;
                if (str != null)
                    __result = str;
            }
        }
    }
}
