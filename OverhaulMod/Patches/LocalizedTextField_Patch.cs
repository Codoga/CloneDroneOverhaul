using HarmonyLib;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(LocalizedTextField))]
    internal static class LocalizedTextField_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LocalizedTextField.tryLocalizeUnityTextField))]
        private static void tryLocalizeUnityTextField_Postfix(LocalizedTextField __instance)
        {
            if (!__instance.DisableWrappingForLogographicLanguages || !__instance._textLabel)
                return;

            LocalizationManager localizationManager = LocalizationManager.Instance;
            if (localizationManager && localizationManager.GetCurrentLanguageCode() == "ru")
                __instance._textLabel.resizeTextMaxSize = __instance._originalMaxFontSize;
        }
    }
}
