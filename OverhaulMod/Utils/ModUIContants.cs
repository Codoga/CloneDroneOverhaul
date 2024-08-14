using OverhaulMod.Content;
using OverhaulMod.UI;
using System.Collections.Generic;
using UnityEngine;

namespace OverhaulMod.Utils
{
    public static class ModUIConstants
    {
        public const string UI_VERSION_LABEL = "UI_VersionLabel";
        public const string UI_OTHER_MODS = "UI_OtherMods";
        public const string UI_ENDLESS_MODE = "UI_EndlessModeMenu";
        public const string UI_ENDLESS_MODE_LEADERBOARD = "UI_EndlessModeLeaderboard";
        public const string UI_SETTINGS_MENU = "UI_SettingsMenuRework";
        public const string UI_TITLE_SCREEN = "UI_TitleScreenRework";
        public const string UI_ADVANCEMENT_PROGRESS = "UI_AdvancementProgress";
        public const string UI_ADVANCEMENTS_MENU = "UI_AdvancementsMenuRework";
        public const string UI_WORKSHOP_BROWSER = "UI_WorkshopBrowserRework";
        public const string UI_MESSAGE_POPUP = "UI_MessagePopup";
        public const string UI_FEEDBACK_UI = "UI_FeedbackUIRework";
        public const string UI_COMMUNITY_HUB = "UI_CommunityHub";
        public const string UI_PAUSE_MENU = "UI_PauseMenuRework";
        public const string UI_CHAPTER_SELECT_MENU = "UI_ChapterSelectionMenu";
        public const string UI_CHAPTER_LEVEL_SELECT_MENU = "UI_ChapterLevelSelectionMenu";
        public const string UI_LOADING_SCREEN = "UI_LoadingScreen";
        public const string UI_EXCLUSIVE_CONTENT_MENU = "UI_ExclusiveContentMenu";
        public const string UI_EXCLUSIVE_CONTENT_EDITOR = "UI_ExclusiveContentEditor";
        public const string UI_CONNECT_SCREEN = "UI_ConnectScreen";
        public const string UI_NEWS_PANEL = "UI_NewsPanel";
        public const string UI_CONTENT_DOWNLOAD_WINDOW = "UI_ContentDownloadWindow";
        public const string UI_UPDATES_WINDOW = "UI_UpdatesWindow";
        public const string UI_PERSONALIZATION_ITEMS_BROWSER = "UI_PersonalizationItemsBrowser";
        public const string UI_UPDATE_INFO_EDITOR = "UI_UpdateInfoEditor";
        public const string UI_NEWS_INFO_EDITOR = "UI_NewsInfoEditor";
        public const string UI_RESTART_REQUIRED_SCREEN = "UI_RestartRequiredScreen";
        public const string UI_NEWS_DETAILS_PANEL = "UI_NewsDetailsPanel";
        public const string UI_INFORMATION_SELECT_WINDOW = "UI_InformationSelectWindow";
        public const string UI_OVERHAUL_MOD_INFO_WINDOW = "UI_OverhaulModInfoWindow";
        public const string UI_WORKSHOP_ITEM_PAGE_WINDOW = "UI_WorkshopItemPageWindow";
        public const string UI_MESSAGE_POPUP_FULL_SCREEN = "UI_MessagePopupFullScreen";
        public const string UI_OVERHAUL_UI_MANAGEMENT_PANEL = "UI_OverhaulUIsManagementPanel";
        public const string UI_LEVEL_DESCRIPTION_LIST_EDITOR = "UI_LevelDescriptionListEditor";
        public const string UI_PERSONALIZATION_EDITOR = "UI_PersonalizationEditor";
        public const string UI_MULTIPLAYER_GAMEMODE_SELECT_SCREEN = "UI_MPSelectScreen";
        public const string UI_CREDITS_MENU = "UI_CreditsMenu";
        public const string UI_ADDONS_MENU = "UI_AddonsMenu";
        public const string UI_ADDONS_EDITOR = "UI_AddonsEditor";
        public const string UI_CHALLENGES_MENU_REWORK = "UI_ChallengesMenuRework";
        public const string UI_LOCALIZATION_EDITOR = "UI_LocalizationEditor";
        public const string UI_GENERIC_INPUT_FIELD_WINDOW = "UI_GenericInputWindow";
        public const string UI_ADDONS_DOWNLOAD_EDITOR = "UI_AddonsDownloadEditor";
        public const string UI_TITLE_SCREEN_CUSTOMIZATION_PANEL = "UI_TitleScreenCustomizationPanel";
        public const string UI_LEVEL_DESCRIPTION_BROWSER = "UI_LevelDescriptionBrowser";
        public const string UI_TOOLTIPS = "UI_Tooltips";
        public const string UI_IMAGE_EFFECTS = "UI_ImageEffects";
        public const string UI_PHOTO_MODE_UI_REWORK = "UI_PhotoModeUIRework";
        public const string UI_DEVELOPMENT_GALLERY = "UI_DevelopmentGallery";
        public const string UI_SET_KEY_BIND_WINDOW = "UI_SetKeyBindWindow";
        public const string UI_PERSONALIZATION_EDITOR_ITEMS_BROWSER = "UI_PersonalizationEditorItemsBrowser";
        public const string UI_PERSONALIZATION_EDITOR_VERIFICATION_MENU = "UI_PersonalizationEditorVerificationMenu";
        public const string UI_DEBUG_MENU = "UI_DebugMenu";
        public const string UI_CRASH_SCREEN = "UI_CrashScreen";
        public const string UI_DAMAGE_INDICATOR = "UI_DamageIndicator";
        public const string UI_GENERIC_COLOR_PICKER = "UI_GenericColorPicker";
        public const string UI_GENERIC_IMAGE_VIEWER = "UI_GenericImageViewer";
        public const string UI_PERSONALIZATION_EDITOR_AUTHORS_EDIT_MENU = "UI_PersonalizationEditorAuthorsEditMenu";
        public const string UI_GAME_LOSS_WINDOW = "UI_GameLossWindow";
        public const string UI_ENDLESS_GAME_LOSS_WINDOW = "UI_EndlessGameLossWindow";
        public const string UI_DUEL_INVITE_MENU_REWORK = "UI_DuelInviteMenuRework";
        public const string UI_SCREEN_TOOLTIPS = "UI_ScreenTooltips";
        public const string UI_PERSONALIZATION_EDITOR_HELP_MENU = "UI_PersonalizationEditorHelpMenu";
        public const string UI_FILE_EXPLORER = "UI_FileExplorer";
        public const string UI_PERSONALIZATION_EDITOR_OBJECT_BROWSER = "UI_PersonalizationEditorObjectBrowser";
        public const string UI_CINEMATIC_EFFECTS = "UI_CinematicEffects";
        public const string UI_IMAGE_EXPLORER = "UI_ImageExplorer";
        public const string UI_AUTO_BUILD_MENU = "UI_AutoBuildMenu";
        public const string UI_DOWNLOAD_PERSONALIZATION_ASSETS_MENU = "UI_DownloadPersonalizationAssetsMenu";
        public const string UI_PERSONALIZATION_SETTINGS_MENU = "UI_PersonalizationSettingsMenu";
        public const string UI_DISCORD_SERVER_MENU = "UI_DiscordServerMenu";
        public const string UI_PERSONALIZATION_EDITOR_EXCLUSIVITY_EDIT_MENU = "UI_PersonalizationEditorExclusivityEditMenu";
        public const string UI_PERSONALIZATION_EDITOR_ABOUT_DIALOG = "UI_PersonalizationEditorAboutDialog";
        public const string UI_ASSET_BUNDLE_ASSETS_BROWSER = "UI_AssetBundleAssetsBrowser";
        public const string UI_PATCH_NOTES = "UI_PatchNotes";
        public const string UI_PERSONALIZATION_EDITOR_ITEM_CREATION_DIALOG = "UI_PersonalizationEditorItemCreationDialog";
        public const string UI_PRESS_ACTION_KEY_DESCRIPTION = "UI_PressActionKeyDescription";
        public const string UI_SUBTITLE_TEXT_FIELD_REWORK = "UI_SubtitleTextFieldRework";
        public const string UI_AUTO_BUILD_SELECTION_MENU = "UI_AutoBuildSelectionMenu";
        public const string UI_PERSONALIZATION_EDITOR_PLAYTEST_HUD = "UI_PersonalizationEditorPlaytestHUD";
        public const string UI_PERSONALIZATION_EDITOR_ITEM_IMPORT_DIALOG = "UI_PersonalizationEditorItemImportDialog";

        public static UIVersionLabel ShowVersionLabel()
        {
            return ModUIManager.Instance.Show<UIVersionLabel>(AssetBundleConstants.UI, UI_VERSION_LABEL, ModUIManager.UILayer.BeforeCrashScreen);
        }

        public static UIOtherMods ShowOtherModsMenu()
        {
            return ModUIManager.Instance.Show<UIOtherMods>(AssetBundleConstants.UI, UI_OTHER_MODS, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIEndlessModeMenu ShowEndlessModeMenu()
        {
            return ModUIManager.Instance.Show<UIEndlessModeMenu>(AssetBundleConstants.UI, UI_ENDLESS_MODE, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIEndlessModeLeaderboard ShowLeaderboard(Transform parent, List<HighScoreData> list, string fileName)
        {
            UIEndlessModeLeaderboard leaderboard = ModUIManager.Instance.Show<UIEndlessModeLeaderboard>(AssetBundleConstants.UI, UI_ENDLESS_MODE_LEADERBOARD, parent);
            leaderboard.Populate(list, fileName);
            return leaderboard;
        }

        public static UISettingsMenuRework ShowSettingsMenuRework(bool setup)
        {
            UISettingsMenuRework settingsMenuRework = ModUIManager.Instance.Show<UISettingsMenuRework>(AssetBundleConstants.UI, UI_SETTINGS_MENU, ModUIManager.UILayer.AfterTitleScreen);
            if (setup)
                settingsMenuRework.ShowSetupElements();
            else
                settingsMenuRework.ShowRegularElements();
            return settingsMenuRework;
        }

        public static UITitleScreenRework ShowTitleScreenRework()
        {
            return ModUIManager.Instance.Show<UITitleScreenRework>(AssetBundleConstants.UI, UI_TITLE_SCREEN, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static void HideTitleScreenRework()
        {
            _ = ModUIManager.Instance.Hide(AssetBundleConstants.UI, UI_TITLE_SCREEN);
        }

        public static UIAdvancementProgress ShowAdvancementProgress(GameplayAchievement gameplayAchievement)
        {
            UIAdvancementProgress panel = ModUIManager.Instance.Show<UIAdvancementProgress>(AssetBundleConstants.UI, UI_ADVANCEMENT_PROGRESS, ModUIManager.UILayer.AfterTitleScreen);
            if (panel)
            {
                panel.ShowProgress(gameplayAchievement);
            }
            return panel;
        }

        public static UIAdvancementsMenu ShowAdvancementsMenuRework()
        {
            return ModUIManager.Instance.Show<UIAdvancementsMenu>(AssetBundleConstants.UI, UI_ADVANCEMENTS_MENU, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIWorkshopBrowser ShowWorkshopBrowserRework()
        {
            return ModUIManager.Instance.Show<UIWorkshopBrowser>(AssetBundleConstants.UI, UI_WORKSHOP_BROWSER, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIMessagePopup ShowMessagePopup()
        {
            return ModUIManager.Instance.Show<UIMessagePopup>(AssetBundleConstants.UI, UI_MESSAGE_POPUP, ModUIManager.UILayer.Last);
        }

        public static UIMessagePopup ShowFullScreenMessagePopup()
        {
            return ModUIManager.Instance.Show<UIMessagePopup>(AssetBundleConstants.UI, UI_MESSAGE_POPUP_FULL_SCREEN, ModUIManager.UILayer.Last);
        }

        public static UIFeedbackMenu ShowFeedbackUIRework(bool showExitButton)
        {
            UIFeedbackMenu feedbackMenu = ModUIManager.Instance.Show<UIFeedbackMenu>(AssetBundleConstants.UI, UI_FEEDBACK_UI, ModUIManager.UILayer.BeforeCrashScreen);
            feedbackMenu.SetExitButtonVisible(showExitButton);
            return feedbackMenu;
        }

        public static UICommunityHub ShowCommunityHub()
        {
            return ModUIManager.Instance.Show<UICommunityHub>(AssetBundleConstants.UI, UI_COMMUNITY_HUB, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIPauseMenuRework ShowPauseMenuRework()
        {
            return ModUIManager.Instance.Show<UIPauseMenuRework>(AssetBundleConstants.UI, UI_PAUSE_MENU, ModUIManager.UILayer.AfterEscMenu);
        }

        public static UIChapterSelectMenuRework ShowChapterSelectMenu()
        {
            return ModUIManager.Instance.Show<UIChapterSelectMenuRework>(AssetBundleConstants.UI, UI_CHAPTER_SELECT_MENU, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIChapterSectionSelectMenu ShowChapterLevelSelectMenu(Transform parent, int chapterIndex)
        {
            UIChapterSectionSelectMenu chapterLevelSelectMenu = ModUIManager.Instance.Show<UIChapterSectionSelectMenu>(AssetBundleConstants.UI, UI_CHAPTER_LEVEL_SELECT_MENU, parent);
            chapterLevelSelectMenu.PopulateChapter(chapterIndex);
            return chapterLevelSelectMenu;
        }

        public static UILoadingScreenRework ShowLoadingScreen()
        {
            return ModUIManager.Instance.Show<UILoadingScreenRework>(AssetBundleConstants.UI, UI_LOADING_SCREEN, ModUIManager.UILayer.Last);
        }

        public static void HideLoadingScreen()
        {
            _ = ModUIManager.Instance.Hide(AssetBundleConstants.UI, UI_LOADING_SCREEN);
        }

        public static UIExclusiveContentMenu ShowExclusiveContentMenu()
        {
            return ModUIManager.Instance.Show<UIExclusiveContentMenu>(AssetBundleConstants.UI, UI_EXCLUSIVE_CONTENT_MENU, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static void ShowExclusiveContentEditor(Transform parent)
        {
            _ = ModUIManager.Instance.Show<UIExclusiveContentEditor>(AssetBundleConstants.UI, UI_EXCLUSIVE_CONTENT_EDITOR, parent);
        }

        public static UIMultiplayerConnectScreen ShowMultiplayerConnectScreen()
        {
            return ModUIManager.Instance.Show<UIMultiplayerConnectScreen>(AssetBundleConstants.UI, UI_CONNECT_SCREEN, ModUIManager.UILayer.AfterMultiplayerConnectScreen);
        }

        public static UINewsPanel ShowNewsPanel()
        {
            return ModUIManager.Instance.Show<UINewsPanel>(AssetBundleConstants.UI, UI_NEWS_PANEL, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static void ShowContentDownloadWindow()
        {
            _ = ModUIManager.Instance.Show<UIContentDownloadWindow>(AssetBundleConstants.UI, UI_CONTENT_DOWNLOAD_WINDOW, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIUpdatesWindow ShowUpdatesWindow()
        {
            return ModUIManager.Instance.Show<UIUpdatesWindow>(AssetBundleConstants.UI, UI_UPDATES_WINDOW, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIPersonalizationItemBrowser ShowPersonalizationItemsBrowser()
        {
            return ModUIManager.Instance.Show<UIPersonalizationItemBrowser>(AssetBundleConstants.UI, UI_PERSONALIZATION_ITEMS_BROWSER, ModUIManager.UILayer.BeforeEscMenu);
        }

        public static UIUpdateInfoEditor ShowUpdateInfoEditor(Transform parent)
        {
            return ModUIManager.Instance.Show<UIUpdateInfoEditor>(AssetBundleConstants.UI, UI_UPDATE_INFO_EDITOR, parent);
        }

        public static UINewsInfoEditor ShowNewsInfoEditor(Transform parent)
        {
            return ModUIManager.Instance.Show<UINewsInfoEditor>(AssetBundleConstants.UI, UI_NEWS_INFO_EDITOR, parent);
        }

        public static UIRestartRequiredScreen ShowRestartRequiredScreen(bool allowIgnoring)
        {
            UIRestartRequiredScreen screen = ModUIManager.Instance.Show<UIRestartRequiredScreen>(AssetBundleConstants.UI, UI_RESTART_REQUIRED_SCREEN, ModUIManager.UILayer.AfterCrashScreen);
            screen.SetAllowIgnoring(allowIgnoring);
            return screen;
        }

        public static UINewsDetailsPanel ShowNewsDetailsPanel(Transform parent, NewsInfo newsInfo)
        {
            UINewsDetailsPanel panel = ModUIManager.Instance.Show<UINewsDetailsPanel>(AssetBundleConstants.UI, UI_NEWS_DETAILS_PANEL, parent);
            panel.Populate(newsInfo);
            return panel;
        }

        public static UIInformationSelectWindow ShowInformationSelectMenu()
        {
            return ModUIManager.Instance.Show<UIInformationSelectWindow>(AssetBundleConstants.UI, UI_INFORMATION_SELECT_WINDOW, ModUIManager.UILayer.BeforeCrashScreen);
        }

        public static UIOverhaulInfoWindow ShowOverhaulModInfoMenu(Transform parent)
        {
            return ModUIManager.Instance.Show<UIOverhaulInfoWindow>(AssetBundleConstants.UI, UI_OVERHAUL_MOD_INFO_WINDOW, parent);
        }

        public static UIWorkshopItemPageWindow ShowWorkshopItemPageWindow(Transform parent)
        {
            return ModUIManager.Instance.Show<UIWorkshopItemPageWindow>(AssetBundleConstants.UI, UI_WORKSHOP_ITEM_PAGE_WINDOW, parent);
        }

        public static UIOverhaulUIManagementPanel ShowOverhaulUIManagementPanel(Transform parent)
        {
            return ModUIManager.Instance.Show<UIOverhaulUIManagementPanel>(AssetBundleConstants.UI, UI_OVERHAUL_UI_MANAGEMENT_PANEL, parent);
        }

        public static UILevelDescriptionListEditor ShowLevelDescriptionListEditor()
        {
            return ModUIManager.Instance.Show<UILevelDescriptionListEditor>(AssetBundleConstants.UI, UI_LEVEL_DESCRIPTION_LIST_EDITOR, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIPersonalizationEditor ShowPersonalizationEditorUI()
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditor>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR, ModUIManager.UILayer.BeforeEscMenu);
        }

        public static UIMultiplayerGameModeSelectScreen ShowMultiplayerGameModeSelectScreen()
        {
            return ModUIManager.Instance.Show<UIMultiplayerGameModeSelectScreen>(AssetBundleConstants.UI, UI_MULTIPLAYER_GAMEMODE_SELECT_SCREEN, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIAllCreditsMenu ShowCreditsMenu(Transform parent)
        {
            return ModUIManager.Instance.Show<UIAllCreditsMenu>(AssetBundleConstants.UI, UI_CREDITS_MENU, parent);
        }

        public static UIAddonsMenu ShowAddonsMenu()
        {
            return ModUIManager.Instance.Show<UIAddonsMenu>(AssetBundleConstants.UI, UI_ADDONS_MENU, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIAddonsEditor ShowAddonsEditor(Transform parent)
        {
            return ModUIManager.Instance.Show<UIAddonsEditor>(AssetBundleConstants.UI, UI_ADDONS_EDITOR, parent);
        }

        public static UIChallengesMenuRework ShowChallengesMenuRework(bool coop, bool privateMatches)
        {
            UIChallengesMenuRework challengesMenuRework = ModUIManager.Instance.Show<UIChallengesMenuRework>(AssetBundleConstants.UI, UI_CHALLENGES_MENU_REWORK, ModUIManager.UILayer.AfterTitleScreen);
            challengesMenuRework.Populate(coop, privateMatches);
            return challengesMenuRework;
        }

        public static UILocalizationEditor ShowLocalizationEditor()
        {
            return ModUIManager.Instance.Show<UILocalizationEditor>(AssetBundleConstants.UI, UI_LOCALIZATION_EDITOR, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIGenericInputFieldWindow ShowGenericInputFieldWindow()
        {
            return ModUIManager.Instance.Show<UIGenericInputFieldWindow>(AssetBundleConstants.UI, UI_GENERIC_INPUT_FIELD_WINDOW, ModUIManager.UILayer.Last);
        }

        public static UIAddonsDownloadEditor ShowAddonsDownloadEditor(Transform parent)
        {
            return ModUIManager.Instance.Show<UIAddonsDownloadEditor>(AssetBundleConstants.UI, UI_ADDONS_DOWNLOAD_EDITOR, parent);
        }

        public static UITitleScreenCustomizationPanel ShowTitleScreenCustomizationPanel(Transform parent)
        {
            return ModUIManager.Instance.Show<UITitleScreenCustomizationPanel>(AssetBundleConstants.UI, UI_TITLE_SCREEN_CUSTOMIZATION_PANEL, parent);
        }

        public static UILevelDescriptionBrowser ShowLevelDescriptionBrowser()
        {
            return ModUIManager.Instance.Show<UILevelDescriptionBrowser>(AssetBundleConstants.UI, UI_LEVEL_DESCRIPTION_BROWSER, ModUIManager.UILayer.Last);
        }

        public static UITooltips ShowTooltips()
        {
            return ModUIManager.Instance.Show<UITooltips>(AssetBundleConstants.UI, UI_TOOLTIPS, ModUIManager.UILayer.First);
        }

        public static UIImageEffects ShowImageEffects()
        {
            return ModUIManager.Instance.Show<UIImageEffects>(AssetBundleConstants.UI, UI_IMAGE_EFFECTS, ModUIManager.UILayer.First);
        }

        public static UIPhotoModeUIRework ShowPhotoModeUIRework()
        {
            return ModUIManager.Instance.Show<UIPhotoModeUIRework>(AssetBundleConstants.UI, UI_PHOTO_MODE_UI_REWORK, ModUIManager.UILayer.BeforeEscMenu);
        }

        public static void HidePhotoModeUIRework()
        {
            _ = ModUIManager.Instance.Hide(AssetBundleConstants.UI, UI_PHOTO_MODE_UI_REWORK);
        }

        public static UIDevelopmentGallery ShowDevelopmentGallery(Transform parent)
        {
            return ModUIManager.Instance.Show<UIDevelopmentGallery>(AssetBundleConstants.UI, UI_DEVELOPMENT_GALLERY, parent);
        }

        public static UISetKeyBindWindow ShowSetKeyBindWindow(Transform parent)
        {
            return ModUIManager.Instance.Show<UISetKeyBindWindow>(AssetBundleConstants.UI, UI_SET_KEY_BIND_WINDOW, parent);
        }

        public static UIPersonalizationEditorItemBrowser ShowPersonalizationEditorItemsBrowser(Transform parent)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorItemBrowser>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_ITEMS_BROWSER, parent);
        }

        public static UIPersonalizationEditorVerificationMenu ShowPersonalizationEditorVerificationMenu(Transform parent)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorVerificationMenu>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_VERIFICATION_MENU, parent);
        }

        public static UIDebugMenu ShowDebugMenu()
        {
            return ModUIManager.Instance.Show<UIDebugMenu>(AssetBundleConstants.UI, UI_DEBUG_MENU, ModUIManager.UILayer.Last);
        }

        public static void HideDebugMenu()
        {
            _ = ModUIManager.Instance.Hide(AssetBundleConstants.UI, UI_DEBUG_MENU);
        }

        public static UICrashScreen ShowCrashScreen(string errorMessage)
        {
            UICrashScreen crashScreen = ModUIManager.Instance.Show<UICrashScreen>(AssetBundleConstants.UI, UI_CRASH_SCREEN, ModUIManager.UILayer.AfterCrashScreen);
            crashScreen.SetStackTraceText(errorMessage);
            return crashScreen;
        }

        public static void HideCrashScreen()
        {
            _ = ModUIManager.Instance.Hide(AssetBundleConstants.UI, UI_CRASH_SCREEN);
        }

        public static UIDamageIndicator ShowDamageIndicator()
        {
            return ModUIManager.Instance.Show<UIDamageIndicator>(AssetBundleConstants.UI, UI_DAMAGE_INDICATOR, ModUIManager.UILayer.BeforeEscMenu);
        }

        public static UIGenericColorPicker ShowGenericColorPicker(Transform parent)
        {
            return ModUIManager.Instance.Show<UIGenericColorPicker>(AssetBundleConstants.UI, UI_GENERIC_COLOR_PICKER, parent);
        }

        public static void HideGenericColorPicker()
        {
            _ = ModUIManager.Instance.Hide(AssetBundleConstants.UI, UI_GENERIC_COLOR_PICKER);
        }

        public static UIGenericImageViewer ShowGenericImageViewer(Transform parent)
        {
            return ModUIManager.Instance.Show<UIGenericImageViewer>(AssetBundleConstants.UI, UI_GENERIC_IMAGE_VIEWER, parent);
        }

        public static UIPersonalizationEditorAuthorsEditMenu ShowPersonalizationEditorAuthorsEditMenu(Transform parent)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorAuthorsEditMenu>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_AUTHORS_EDIT_MENU, parent);
        }

        public static UIGameLossWindow ShowGameLossWindow()
        {
            return ModUIManager.Instance.Show<UIGameLossWindow>(AssetBundleConstants.UI, UI_GAME_LOSS_WINDOW, ModUIManager.UILayer.BeforeEscMenu);
        }

        public static UIEndlessGameLossWindow ShowEndlessGameLossWindow()
        {
            return ModUIManager.Instance.Show<UIEndlessGameLossWindow>(AssetBundleConstants.UI, UI_ENDLESS_GAME_LOSS_WINDOW, ModUIManager.UILayer.BeforeEscMenu);
        }

        public static UIDuelInviteMenuRework ShowDuelInviteMenuRework(GameMode gameMode)
        {
            UIDuelInviteMenuRework duelInviteMenuRework = ModUIManager.Instance.Show<UIDuelInviteMenuRework>(AssetBundleConstants.UI, UI_DUEL_INVITE_MENU_REWORK, ModUIManager.UILayer.AfterTitleScreen);
            duelInviteMenuRework.Populate(gameMode);
            return duelInviteMenuRework;
        }

        public static UIScreenTooltips ShowScreenTooltips()
        {
            return ModUIManager.Instance.Show<UIScreenTooltips>(AssetBundleConstants.UI, UI_SCREEN_TOOLTIPS, ModUIManager.UILayer.Last);
        }

        public static UIPersonalizationEditorHelpMenu ShowPersonalizationEditorHelpMenu(Transform parent)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorHelpMenu>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_HELP_MENU, parent);
        }

        public static UIFileExplorer ShowFileExplorer(Transform parent)
        {
            return ModUIManager.Instance.Show<UIFileExplorer>(AssetBundleConstants.UI, UI_FILE_EXPLORER, parent);
        }

        public static UIPersonalizationEditorObjectBrowser ShowPersonalizationEditorObjectBrowser(Transform parent)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorObjectBrowser>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_OBJECT_BROWSER, parent);
        }

        public static UICinematicEffects ShowCinematicEffects()
        {
            return ModUIManager.Instance.Show<UICinematicEffects>(AssetBundleConstants.UI, UI_CINEMATIC_EFFECTS, ModUIManager.UILayer.AfterEnergyUI);
        }

        public static UIImageExplorer ShowImageExplorer(Transform parent)
        {
            return ModUIManager.Instance.Show<UIImageExplorer>(AssetBundleConstants.UI, UI_IMAGE_EXPLORER, parent);
        }

        public static UIAutoBuildMenu ShowAutoBuildMenu()
        {
            return ModUIManager.Instance.Show<UIAutoBuildMenu>(AssetBundleConstants.UI, UI_AUTO_BUILD_MENU, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIDownloadPersonalizationAssetsMenu ShowDownloadPersonalizationAssetsMenu(Transform transform)
        {
            return ModUIManager.Instance.Show<UIDownloadPersonalizationAssetsMenu>(AssetBundleConstants.UI, UI_DOWNLOAD_PERSONALIZATION_ASSETS_MENU, transform);
        }

        public static UIPersonalizationSettingsMenu ShowPersonalizationSettingsMenu(Transform transform)
        {
            return ModUIManager.Instance.Show<UIPersonalizationSettingsMenu>(AssetBundleConstants.UI, UI_PERSONALIZATION_SETTINGS_MENU, transform);
        }

        public static UIDiscordServerMenu ShowDiscordServerMenu(Transform transform)
        {
            return ModUIManager.Instance.Show<UIDiscordServerMenu>(AssetBundleConstants.UI, UI_DISCORD_SERVER_MENU, transform);
        }

        public static UIPersonalizationEditorExclusivityEditMenu ShowPersonalizationEditorExclusivityEditMenu(Transform transform)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorExclusivityEditMenu>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_EXCLUSIVITY_EDIT_MENU, transform);
        }

        public static UIPersonalizationEditorAboutDialog ShowPersonalizationEditorAboutDialog(Transform transform)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorAboutDialog>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_ABOUT_DIALOG, transform);
        }

        public static UIAssetBundleAssetsBrowser ShowAssetBundleAssetsBrowser(Transform transform)
        {
            return ModUIManager.Instance.Show<UIAssetBundleAssetsBrowser>(AssetBundleConstants.UI, UI_ASSET_BUNDLE_ASSETS_BROWSER, transform);
        }

        public static UIPatchNotes ShowPatchNotes()
        {
            return ModUIManager.Instance.Show<UIPatchNotes>(AssetBundleConstants.UI, UI_PATCH_NOTES, ModUIManager.UILayer.AfterTitleScreen, 1);
        }

        public static UIPersonalizationEditorItemCreationDialog ShowPersonalizationEditorItemCreationDialog(Transform transform)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorItemCreationDialog>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_ITEM_CREATION_DIALOG, transform);
        }

        public static UIPressActionKeyDescription ShowPressActionKeyDescription()
        {
            return ModUIManager.Instance.Show<UIPressActionKeyDescription>(AssetBundleConstants.UI, UI_PRESS_ACTION_KEY_DESCRIPTION, ModUIManager.UILayer.AfterEnergyUI);
        }

        public static UISubtitleTextFieldRework ShowSubtitleTextFieldRework()
        {
            return ModUIManager.Instance.Show<UISubtitleTextFieldRework>(AssetBundleConstants.UI, UI_SUBTITLE_TEXT_FIELD_REWORK, ModUIManager.UILayer.AfterEnergyUI);
        }

        public static UIAutoBuildSelectionMenu ShowAutoBuildSelectionMenu()
        {
            return ModUIManager.Instance.Show<UIAutoBuildSelectionMenu>(AssetBundleConstants.UI, UI_AUTO_BUILD_SELECTION_MENU, ModUIManager.UILayer.AfterTitleScreen);
        }

        public static UIPersonalizationEditorPlaytestHUD ShowPersonalizationEditorPlaytestHUD()
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorPlaytestHUD>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_PLAYTEST_HUD, ModUIManager.UILayer.AfterEnergyUI);
        }

        public static UIPersonalizationEditorItemImportDialog ShowPersonalizationEditorItemImportDialog(Transform transform)
        {
            return ModUIManager.Instance.Show<UIPersonalizationEditorItemImportDialog>(AssetBundleConstants.UI, UI_PERSONALIZATION_EDITOR_ITEM_IMPORT_DIALOG, transform);
        }
    }
}
