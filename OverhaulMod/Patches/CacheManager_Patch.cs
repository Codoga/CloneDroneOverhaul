using HarmonyLib;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(CacheManager))]
    internal static class CacheManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CacheManager.CreateOrClearInstance))]
        private static void CreateOrClearInstance_Postfix()
        {
            ModComponentCache.ClearCache();
        }
    }
}
