using HarmonyLib;
using OverhaulMod.Content.Personalization;
using OverhaulMod.Engine;
using OverhaulMod.UI;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.Patches
{
    [HarmonyPatch(typeof(GameUIRoot))]
    internal static class GameUIRoot_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameUIRoot.RefreshCursorEnabled))]
        private static bool RefreshCursorEnabled_Prefix()
        {
            if (PersonalizationEditorCamera.IsControllingTheCamera)
                return false;

#if DEBUG
            if (ModDebug.forceDisableCursor)
            {
                InputManager.Instance.SetCursorEnabled(false);
                return false;
            }
#endif
            ModUIManager manager = ModUIManager.Instance;
            if (!manager)
                return true;

            /*
            Debug.Log($"ShouldEnableCursor: {manager.ShouldEnableCursor()}");
            Debug.Log($"UIVisible: {manager.IsUIVisible(AssetBundleConstants.UI, ModUIConstants.UI_PHOTO_MODE_UI_REWORK) && AdvancedPhotoModeManager.Instance.IsInPhotoMode() && UIManager.Instance.IsMouseOverUIElement()}");
            */

            if (manager.ShouldEnableCursor() || (manager.IsUIVisible(AssetBundleConstants.UI, ModUIConstants.UI_PHOTO_MODE_UI_REWORK) && AdvancedPhotoModeManager.Instance.IsInPhotoMode() && UIManager.Instance.IsMouseOverUIElement()))
            {
                InputManager.Instance.SetCursorEnabled(true);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameUIRoot.CloseCurrentMenu))]
        private static bool CloseCurrentMenu_Prefix(GameUIRoot __instance, bool uiCancelDown, bool pauseDown, bool force = false)
        {
            bool flag = false;
            GameObject nativeControllerSelectTarget = InputManager.Instance.GetNativeControllerSelectTarget();
            if (nativeControllerSelectTarget)
            {
                Dropdown dropdownComponent = nativeControllerSelectTarget.GetComponent<Dropdown>();
                ControllerDropdownItem controllerDropdownItem = nativeControllerSelectTarget.GetComponent<ControllerDropdownItem>();
                if (dropdownComponent != null)
                {
                    if (dropdownComponent.transform.Find("Dropdown List"))
                        flag = true;
                }
                else if (controllerDropdownItem)
                {
                    controllerDropdownItem.CloseParentDropdown();
                    flag = true;
                }
            }

            if (!uiCancelDown || !flag)
            {
                ModUIManager modUIManager = ModUIManager.Instance;
                if (!modUIManager || modUIManager.skipHidingCustomUIs)
                    return true;

                if (modUIManager.TryInvokeAction())
                    return false;

                OverhaulUIBehaviour lastShownUi = modUIManager.GetLastShownUI();
                if (!lastShownUi)
                {
                    SettingsMenu settingsMenu = ModCache.gameUIRoot?.SettingsMenu;
                    if (settingsMenu && settingsMenu.gameObject.activeInHierarchy)
                    {
                        settingsMenu.Hide();
                        return false;
                    }
                    return true;
                }

                if (lastShownUi is UIAutoBuildMenu autoBuildMenu)
                {
                    if (autoBuildMenu.isShowingUpgradeUI)
                    {
                        autoBuildMenu.OnCloseUpgradeUIButtonClicked();
                        return false;
                    }
                    autoBuildMenu.Hide();
                    return false;
                }
                else if (lastShownUi is UIDownloadPersonalizationAssetsMenu downloadPersonalizationAssetsMenu)
                {
                    if (!downloadPersonalizationAssetsMenu.CanExit())
                        return false;

                    downloadPersonalizationAssetsMenu.Hide();
                    return false;
                }
                else if (lastShownUi is UIPersonalizationEditorVerificationMenu personalizationEditorVerificationMenu)
                {
                    if (!personalizationEditorVerificationMenu.CanExit())
                        return false;

                    personalizationEditorVerificationMenu.Hide();
                    return false;
                }
                else if (lastShownUi is UISettingsMenuRework settingsMenuRework)
                {
                    EmoteSettingsUI menu = __instance.EmoteSettingsUI;
                    if (menu.EmoteSelectionUI.activeInHierarchy)
                    {
                        menu.HideEmoteLibraryUI();
                        return false;
                    }

                    if (menu.gameObject.activeInHierarchy)
                    {
                        menu.Hide();
                        return false;
                    }

                    if (!settingsMenuRework.disallowUsingKey)
                        settingsMenuRework.Hide();

                    return false;
                }
                else
                {
                    lastShownUi.Hide();
                    return false;
                }
            }
            return true;
        }
    }
}
