using HarmonyLib;
using OverhaulMod.UI;
using OverhaulMod.Utils;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(MultiplayerErrorPopupBubble))]
    internal static class MultiplayerErrorPopupBubble_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MultiplayerErrorPopupBubble.AnimateShowError))]
        private static void AnimateShowError_Postfix(string errorLabel, string errorDetails, bool autoHide = true, bool showRulesButton = false, bool isWarning = false)
        {
            UITitleScreenRework titleScreenRework = ModUIManager.Instance.Get<UITitleScreenRework>(AssetBundleConstants.UI, ModUIConstants.UI_TITLE_SCREEN);
            if (titleScreenRework && titleScreenRework.visibleInHierarchy)
            {
                UIElementMultiplayerMessageBox em = titleScreenRework.ErrorMessage;
                if (!em.hasEverShowed)
                {
                    em.showedFromCode = true;
                    em.Show();
                }
                em.ShowError(errorLabel, errorDetails, showRulesButton, isWarning);
                em.showedFromCode = false;

                UIElementMultiplayerMessageButton eb = titleScreenRework.ErrorMessageButton;
                eb.Refresh();
            }
        }
    }
}
