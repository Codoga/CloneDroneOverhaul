using OverhaulMod.Content;
using OverhaulMod.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OverhaulMod.Utils
{
    public static class ModUIUtils
    {
        public static void ShowNewUpdateMessageOrChangelog(float delay, bool dontShowNewUpdateMessage = false, bool dontShowPatchNotes = false)
        {
            bool showChangelog = false;

            string lastBuildChangelogWasShownOn = UIPatchNotes.LastBuildChangelogWasShownOn;
            if (lastBuildChangelogWasShownOn.IsNullOrEmpty())
                showChangelog = true;
            else if (Version.TryParse(lastBuildChangelogWasShownOn, out Version prevVersion) && prevVersion < ModBuildInfo.version)
                showChangelog = true;

            if (!dontShowPatchNotes && showChangelog)
            {
                DelegateScheduler.Instance.Schedule(delegate
                {
                    _ = ModUIConstants.ShowPatchNotes();
                }, delay);
            }
            else if (!dontShowNewUpdateMessage)
            {
                DelegateScheduler.Instance.Schedule(delegate
                {
                    bool isTester = ModBuildInfo.isPrereleaseBuild || ExclusiveContentManager.Instance.IsLocalUserTheTester();
                    string savedVersion = isTester ? UpdateManager.SavedTestingVersion : UpdateManager.SavedReleaseVersion;
                    if (savedVersion.IsNullOrEmpty())
                        return;

                    if (!System.Version.TryParse(savedVersion, out System.Version newVersion))
                        return;

                    if (newVersion > ModBuildInfo.version && GameModeManager.IsOnTitleScreen())
                    {
                        MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("update_available_header"), string.Format(LocalizationManager.Instance.GetTranslatedString("update_available_description"), newVersion), 150f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
                        {
                            UI.UIUpdatesWindow window = ModUIConstants.ShowUpdatesWindow();
                            window.SelectBranchAndSearchForUpdates(isTester ? 1 : 0);
                        });
                    }
                }, delay);
            }
        }

        public static void ImageExplorer(List<string> imagePaths, Transform parent)
        {
            UIImageExplorer imageExplorer = ModUIConstants.ShowImageExplorer(parent);
            imageExplorer.Populate(imagePaths);
        }

        public static void FileExplorer(Transform parent, bool selectMode, Action<string> callback, string initialFolder, string searchPattern = null)
        {
            UIFileExplorer fileExplorer = ModUIConstants.ShowFileExplorer(parent);

            if (initialFolder.IsNullOrEmpty() || !Directory.Exists(initialFolder))
                fileExplorer.OnDownloadsFolderButtonClicked();
            else
                fileExplorer.currentFolder = initialFolder;

            fileExplorer.searchPattern = searchPattern;
            fileExplorer.callback = callback;
        }

        public static void Tooltip(string text, float duration = 2f)
        {
            UIScreenTooltips screenTooltips = UIScreenTooltips.instance ?? ModUIConstants.ShowScreenTooltips();
            screenTooltips.ShowText(text, duration);
        }

        public static void ImageViewer(Texture2D texture, Transform parent, Action closedCallback = null)
        {
            UIGenericImageViewer genericImageViewer = ModUIConstants.ShowGenericImageViewer(parent);
            genericImageViewer.Populate(texture, closedCallback);
        }

        public static void ColorPicker(Color currentColor, bool showAlphaChannel, Action<Color> callback, Transform parent)
        {
            UIGenericColorPicker genericColorPicker = ModUIConstants.ShowGenericColorPicker(parent);
            genericColorPicker.Populate(currentColor, showAlphaChannel, callback);
        }

        public static void KeyBinder(string name, KeyCode defaultKey, Action<KeyCode> callback, Transform parent)
        {
            UISetKeyBindWindow setKeyBindWindow = ModUIConstants.ShowSetKeyBindWindow(parent);
            setKeyBindWindow.callBack = callback;
            setKeyBindWindow.SetContents(name, defaultKey);
        }

        public static void LevelDescriptionBrowser(List<LevelDescription> levelDescriptions, Action<LevelDescription> callback)
        {
            UILevelDescriptionBrowser levelDescriptionBrowser = ModUIConstants.ShowLevelDescriptionBrowser();
            levelDescriptionBrowser.callback = callback;
            levelDescriptionBrowser.Populate(levelDescriptions);
        }

        public static void InputFieldWindow(string header, string description, string initialText = null, int limit = 0, float height = 125f, Action<string> doneAction = null)
        {
            if (initialText == null)
                initialText = string.Empty;

            UIGenericInputFieldWindow genericInputFieldWindow = ModUIConstants.ShowGenericInputFieldWindow();
            genericInputFieldWindow.SetTexts(header, description);
            genericInputFieldWindow.SetHeight(height);
            genericInputFieldWindow.SetInputFieldText(initialText, limit);
            genericInputFieldWindow.doneAction = doneAction;
        }

        public static void MessagePopup(bool fullScreen, string header, string description, float height = 125f, MessageMenu.ButtonLayout buttonLayout = MessageMenu.ButtonLayout.OkButton, string okText = null, string yesText = null, string noText = null, Action okAction = null, Action yesAction = null, Action noAction = null)
        {
            UIMessagePopup messagePopup = fullScreen ? ModUIConstants.ShowFullScreenMessagePopup() : ModUIConstants.ShowMessagePopup();
            messagePopup.SetTexts(header, description);
            messagePopup.SetHeight(height);
            messagePopup.SetButtonLayout(buttonLayout);
            messagePopup.SetButtonActions(okAction, yesAction, noAction);
            messagePopup.SetButtonTexts(okText, yesText, noText);
        }

        public static void MessagePopupOK(string header, string description, bool fullScreen = false)
        {
            MessagePopup(fullScreen, header, description, 150f, MessageMenu.ButtonLayout.OkButton, "ok");
        }

        public static void MessagePopupOK(string header, string description, float height, bool fullScreen = false)
        {
            MessagePopup(fullScreen, header, description, height, MessageMenu.ButtonLayout.OkButton, "ok");
        }

        public static void MessagePopupOK(string header, string description, string buttonText, float height, bool fullScreen = false)
        {
            MessagePopup(fullScreen, header, description, height, MessageMenu.ButtonLayout.OkButton, buttonText);
        }

        public static void MessagePopupOK(string header, string description, string buttonText, Action buttonAction, float height, bool fullScreen = false)
        {
            MessagePopup(fullScreen, header, description, height, MessageMenu.ButtonLayout.OkButton, buttonText, null, null, buttonAction);
        }

        public static void MessagePopupNotImplemented()
        {
            MessagePopupOK(LocalizationManager.Instance.GetTranslatedString("wip_header"), LocalizationManager.Instance.GetTranslatedString("wip_text"), false);
        }

        public static void ShowVanillaEscMenu()
        {
            UIPauseMenuRework.disableOverhauledVersion = true;
            ModCache.gameUIRoot.EscMenu.Show();
            ModCache.gameUIRoot.RefreshCursorEnabled();
            UIPauseMenuRework.disableOverhauledVersion = false;
        }

        public static bool IsEscKeyDown()
        {
            return Input.GetKeyDown(KeyCode.Escape);
        }
    }
}
