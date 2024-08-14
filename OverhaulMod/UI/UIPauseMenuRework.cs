using InternalModBot;
using OverhaulMod.Content;
using OverhaulMod.Utils;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPauseMenuRework : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(OnLegacyUIButtonClicked))]
        [UIElement("LegacyUIButton")]
        private readonly Button m_legacyUIButton;

        [UIElementAction(nameof(OnResumeButtonClicked))]
        [UIElement("ResumeButton")]
        private readonly Button m_resumeButton;

        [UIElementAction(nameof(OnAchievementsButtonClicked))]
        [UIElement("AchievementsButton")]
        private readonly Button m_achievementsButton;

        [UIElementAction(nameof(OnCustomizationButtonClicked))]
        [UIElement("CustomizationButton")]
        private readonly Button m_customizationButton;

        [UIElementAction(nameof(OnSettingsButtonClicked))]
        [UIElement("SettingsButton")]
        private readonly Button m_settingsButton;

        [UIElementAction(nameof(OnModsButtonClicked))]
        [UIElement("ModsButton")]
        private readonly Button m_modsButton;

        [UIElementAction(nameof(OnGiveFeedbackButtonClicked))]
        [UIElement("FeedbackButton")]
        private readonly Button m_feedbackButton;

        [UIElement("LogoEn")]
        private readonly GameObject m_logoEn;

        [UIElement("LogoCh")]
        private readonly GameObject m_logoCh;

        [UIElement("LogoJa")]
        private readonly GameObject m_logoJa;

        [UIElement("LogoKo")]
        private readonly GameObject m_logoKo;

        [UIElement("ExitDialogue", false)]
        private readonly GameObject m_exitDialogue;

        [UIElementAction(nameof(OnExitGameButtonClicked), true)]
        [UIElement("ExitGameButton")]
        private readonly Button m_exitGameButton;

        [UIElementAction(nameof(OnMainMenuButtonClicked), true)]
        [UIElement("MainMenuButton")]
        private readonly Button m_mainMenuButton;

        [UIElementAction(nameof(OnConfirmExitGameButtonClicked), false)]
        [UIElement("ConfirmExitGameButton")]
        private readonly Button m_confirmExitGameButton;

        [UIElementAction(nameof(OnConfirmMainMenuButtonClicked), false)]
        [UIElement("ConfirmMainMenuButton")]
        private readonly Button m_confirmMainMenuButton;

        [UIElementAction(nameof(OnReturnToLevelEditorButtonClicked), false)]
        [UIElement("ReturnToLevelEditorButton")]
        private readonly Button m_returnToLevelEditorButton;

        [UIElementAction(nameof(OnStartMatchButtonClicked), false)]
        [UIElement("StartMatchButton")]
        private readonly Button m_startMatchButton;

        [UIElement("StartMatchButtonText")]
        private readonly Text m_startMatchButtonText;

        [UIElementAction(nameof(OnSkipLevelButtonClicked), false)]
        [UIElement("SkipLevelButton")]
        private readonly Button m_skipLevelButton;

        [UIElementAction(nameof(OnReconnectButtonClicked), false)]
        [UIElement("ReconnectButton")]
        private readonly Button m_reconnectButton;

        [UIElement("ConfirmExitGameText", false)]
        private readonly GameObject m_confirmExitGameTextObject;

        [UIElement("ConfirmMainMenuText", false)]
        private readonly GameObject m_confirmMainMenuTextObject;

        [UIElement("WorkshopPanel")]
        private readonly GameObject m_workshopPanelObject;

        [UIElement("WorkshopPanel")]
        private readonly RectTransform m_workshopPanelTransform;

        [UIElement("LevelName")]
        private readonly Text m_workshopLevelTitleText;

        [UIElement("LevelCreator")]
        private readonly Text m_workshopLevelCreatorText;

        [UIElementAction(nameof(OnWorkshopLevelUpVoteButtonClicked))]
        [UIElement("UpVoteButton")]
        private readonly Button m_workshopLevelUpVoteButton;

        [UIElementAction(nameof(OnWorkshopLevelDownVoteButtonClicked))]
        [UIElement("DownVoteButton")]
        private readonly Button m_workshopLevelDownVoteButton;

        [UIElementAction(nameof(OnWorkshopLevelInfoButtonClicked))]
        [UIElement("SteamPageButton")]
        private readonly Button m_workshopLevelPageButton;

        [UIElement("PlayerList", false)]
        private readonly GameObject m_playerInfoListObject;

        [UIElement("PlayerList", false)]
        private readonly RectTransform m_playerInfoListTransform;

        [UIElement("PlayerListScrollRect")]
        private readonly RectTransform m_playerListScrollRectTransform;

        [UIElement("PlayerSpecialStatLabel")]
        private readonly Text m_playerInfoSpecialStatLabel;

        [UIElement("PlayerInfoDisplay", false)]
        private readonly ModdedObject m_playerInfoDisplayPrefab;

        [UIElement("PlayerInfoDisplayContainer")]
        private readonly Transform m_playerInfoDisplayContainer;

        [UIElement("ExtrasContentDownload", typeof(UIElementAddonEmbed))]
        private readonly UIElementAddonEmbed m_extrasAddonEmbed;

        [UIElement("CodePanel", false)]
        private readonly GameObject m_codePanelObject;

        [UIElement("CodeField")]
        private readonly InputField m_codeField;

        [UIElementAction(nameof(OnCopyCodeButtonClicked))]
        [UIElement("CopyCodeButton")]
        private readonly Button m_copyCodeButton;

        [UIElementAction(nameof(OnRevealCodeButtonClicked))]
        [UIElement("RevealCodeButton")]
        private readonly Button m_revealCodeButton;

        [UIElement("PlayerIconLabel")]
        private readonly GameObject m_playerIconLabelObject;

        private ulong m_refreshedWorkshopPanelForItem;

        public override bool enableCursor
        {
            get
            {
                return true;
            }
        }

        public static bool disableOverhauledVersion { get; set; }

        protected override void OnInitialized()
        {
            m_extrasAddonEmbed.addonId = ContentManager.EXTRAS_CONTENT_FOLDER_NAME;
            m_extrasAddonEmbed.RefreshDisplays();
            m_extrasAddonEmbed.onContentDownloaded.AddListener(refreshPlayers);
        }

        public override void Show()
        {
            base.Show();
            TimeManager.Instance.OnGamePaused();
            _ = AudioManager.Instance.PlayClipGlobal(AudioLibrary.Instance.UISelectionPress, 0f, 1f, 0f);

            refreshLogo();
            refreshButtons();
            refreshPlayers();
            refreshWorkshopPanel();
            refreshCodePanel();
        }

        public override void Hide()
        {
            base.Hide();
            TimeManager.Instance.OnGameUnPaused();
            _ = AudioManager.Instance.PlayClipGlobal(AudioLibrary.Instance.UISelectionBack, 0f, 1f, 0f);
        }

        private void refreshLogo()
        {
            string langId = LocalizationManager.Instance.GetCurrentLanguageCode();
            m_logoEn.SetActive(langId != "ko" && langId != "ja" && langId != "zh-CN" && langId != "zh-TW");
            m_logoCh.SetActive(langId == "zh-CN" || langId == "zh-TW");
            m_logoKo.SetActive(langId == "ko");
            m_logoJa.SetActive(langId == "ja");
        }

        private void refreshButtons()
        {
            m_customizationButton.interactable = !GameModeManager.Is((GameMode)2500) && !GameModeManager.IsInLevelEditor() && CharacterTracker.Instance.GetPlayer();

            ArenaCoopManager arenaCoopManager = ArenaCoopManager.Instance;
            BattleRoyaleManager battleRoyaleManager = BattleRoyaleManager.Instance;
            bool isBattleRoyale = battleRoyaleManager;
            bool isCoop = arenaCoopManager;

            bool isBattleRoyaleWaitingArea = isBattleRoyale && battleRoyaleManager.IsProgress(BattleRoyaleMatchProgress.InWaitingArea);
            bool isBattleRoyaleFightStarted = isBattleRoyale && battleRoyaleManager.IsProgress(BattleRoyaleMatchProgress.FightingStarted);
            bool isCoopMatchNotStarted = isCoop && !arenaCoopManager.IsMatchStarted();

            m_startMatchButton.gameObject.SetActive(MultiplayerMatchmakingManager.Instance.IsLocalPlayerHostOfCustomMatch() && (isBattleRoyaleWaitingArea || isBattleRoyaleFightStarted || isCoopMatchNotStarted));
            if (isCoopMatchNotStarted || isBattleRoyaleWaitingArea)
                m_startMatchButtonText.text = LocalizationManager.Instance.GetTranslatedString("Start Match!");
            else if (isBattleRoyaleFightStarted)
                m_startMatchButtonText.text = LocalizationManager.Instance.GetTranslatedString("Final Zone!");

            m_skipLevelButton.gameObject.SetActive(GameModeManager.CanSkipCurrentLevel());
            m_returnToLevelEditorButton.gameObject.SetActive(WorkshopLevelManager.Instance.IsPlaytestActive());
            m_reconnectButton.gameObject.SetActive(GameModeManager.IsBattleRoyale() && MultiplayerMatchmakingManager.LastDuelRequest.GameType == GameRequestType.RandomBattleRoyale);

            m_confirmExitGameTextObject.SetActive(false);
            m_confirmMainMenuTextObject.SetActive(false);
            m_confirmExitGameButton.gameObject.SetActive(false);
            m_confirmMainMenuButton.gameObject.SetActive(false);
            m_exitGameButton.gameObject.SetActive(true);
            m_mainMenuButton.gameObject.SetActive(true);
        }

        private void refreshPlayers()
        {
            bool isInMultiplayer = GameModeManager.IsMultiplayer();
            m_playerInfoListObject.SetActive(isInMultiplayer);
            if (!isInMultiplayer)
                return;

            bool lostContentModEnabled = ModSpecialUtils.IsModEnabled("cool-hidden-content");
            Vector2 sizeDelta = m_playerInfoListTransform.sizeDelta;
            sizeDelta.y = lostContentModEnabled ? -145f : -110f;
            m_playerInfoListTransform.sizeDelta = sizeDelta;

            Vector2 sizeDelta2 = m_playerListScrollRectTransform.sizeDelta;
            sizeDelta2.y = m_extrasAddonEmbed.ShouldBeHidden() ? -55f : -105f;
            m_playerListScrollRectTransform.sizeDelta = sizeDelta2;

            if (m_playerInfoDisplayContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_playerInfoDisplayContainer);

            MultiplayerPlayerInfoManager multiplayerPlayerInfoManager = MultiplayerPlayerInfoManager.Instance;
            if (!multiplayerPlayerInfoManager)
                return;

            System.Collections.Generic.List<MultiplayerPlayerInfoState> infoStates = multiplayerPlayerInfoManager.GetAllPlayerInfoStates();
            if (infoStates.IsNullOrEmpty())
                return;

            bool canShowIcons = m_extrasAddonEmbed.ShouldBeHidden();
            bool canShowWins = BattleRoyaleManager.Instance;
            Color killsColor = ModParseUtils.TryParseToColor("EC2711", Color.red);
            Color winsColor = Color.white;

            Text specialStatLabel = m_playerInfoSpecialStatLabel;
            if (canShowWins)
            {
                specialStatLabel.color = winsColor;
                specialStatLabel.text = "Wins";
            }
            else
            {
                specialStatLabel.color = killsColor;
                specialStatLabel.text = "Kills";
            }
            specialStatLabel.enabled = GameModeManager.IsBattleRoyale() || GameModeManager.IsMultiplayerDuel();

            m_playerIconLabelObject.SetActive(canShowIcons);

            int index = -1;
            foreach (MultiplayerPlayerInfoState infoState in infoStates)
            {
                index++;
                if (!infoState || infoState.IsDetached())
                    continue;

                IPlayerInfoState playerInfoState;
                try
                {
                    playerInfoState = infoState.state;
                }
                catch
                {
                    continue;
                }

                if (playerInfoState.IsDisconnected)
                    continue;

                ModdedObject playerDisplay = Instantiate(m_playerInfoDisplayPrefab, m_playerInfoDisplayContainer);
                playerDisplay.gameObject.SetActive(true);
                playerDisplay.GetObject<Text>(0).text = playerInfoState.DisplayName;
                Text countLabel = playerDisplay.GetObject<Text>(1);
                if (canShowWins)
                {
                    countLabel.color = winsColor;
                    countLabel.text = playerInfoState.LastBotStandingWins.ToString();
                }
                else
                {
                    countLabel.color = killsColor;
                    countLabel.text = playerInfoState.Kills.ToString();
                }
                countLabel.enabled = GameModeManager.IsBattleRoyale() || GameModeManager.IsMultiplayerDuel();
                playerDisplay.GetObject<Text>(2).text = GetPlatformString((PlayFab.ClientModels.LoginIdentityProvider)playerInfoState.PlatformID);
                playerDisplay.GetObject<RawImage>(3).enabled = canShowIcons;
                playerDisplay.GetObject<GameObject>(6).SetActive(canShowIcons);
                playerDisplay.GetObject<GameObject>(7).SetActive(!canShowIcons);

                UIElementPlayerInfoDisplay playerInfoDisplay = playerDisplay.gameObject.AddComponent<UIElementPlayerInfoDisplay>();
                playerInfoDisplay.InitializeElement();
                playerInfoDisplay.LoadRobotHead(playerInfoState.CharacterModelIndex, playerInfoState.FavouriteColor);
            }
        }

        private void refreshWorkshopPanel()
        {
            SteamWorkshopItem item;
            try
            {
                item = WorkshopLevelManager.Instance.GetCurrentLevelWorkshopItem();
            }
            catch
            {
                item = null;
            }

            bool shouldShowPanel = item != null;

            bool lostContentModEnabled = ModSpecialUtils.IsModEnabled("cool-hidden-content");
            Vector2 anchoredPosition = m_workshopPanelTransform.anchoredPosition;
            anchoredPosition.y = lostContentModEnabled ? 50f : 15f;
            m_workshopPanelTransform.anchoredPosition = anchoredPosition;
            m_workshopPanelObject.SetActive(shouldShowPanel);

            if (!shouldShowPanel)
                return;

            ulong itemId = (ulong)item.WorkshopItemID;
            if (itemId != m_refreshedWorkshopPanelForItem)
            {
                m_refreshedWorkshopPanelForItem = itemId;

                Text titleText = m_workshopLevelTitleText;
                titleText.text = item.Title;
                if (item.Title.Contains("color="))
                    titleText.color = Color.white;
                else
                    titleText.color = ModParseUtils.TryParseToColor("#FF4040", Color.red);

                if (!item.CreatorName.IsNullOrEmpty() && item.CreatorName != "[unknown]")
                    m_workshopLevelCreatorText.text = $"By {item.CreatorName}";
                else
                    m_workshopLevelCreatorText.text = $"By {item.CreatorID}";

                m_workshopLevelUpVoteButton.gameObject.SetActive(false);
                m_workshopLevelDownVoteButton.gameObject.SetActive(false);

                ModSteamUGCUtils.GetUserVote(item.WorkshopItemID, delegate (WorkshopItemVote workshopItemVote)
                {
                    SteamWorkshopItem item2 = WorkshopLevelManager.Instance.GetCurrentLevelWorkshopItem();
                    if (item != item2)
                        return;

                    m_workshopLevelUpVoteButton.gameObject.SetActive(true);
                    m_workshopLevelDownVoteButton.gameObject.SetActive(true);

                    if (!workshopItemVote.HasVoted)
                    {
                        m_workshopLevelUpVoteButton.interactable = true;
                        m_workshopLevelDownVoteButton.interactable = true;
                        return;
                    }
                    m_workshopLevelUpVoteButton.interactable = !workshopItemVote.VoteValue;
                    m_workshopLevelDownVoteButton.interactable = workshopItemVote.VoteValue;
                });
            }
        }

        private void refreshCodePanel()
        {
            if (MultiplayerMatchmakingManager.Instance.IsLocalPlayerHostOfCustomMatch())
            {
                m_codePanelObject.SetActive(true);
                m_revealCodeButton.gameObject.SetActive(true);
                m_codeField.text = MultiplayerMatchmakingManager.Instance.GetLastInviteCode();
            }
            else
            {
                m_codePanelObject.SetActive(false);
            }
        }

        public void OnResumeButtonClicked()
        {
            Hide();
        }

        public void OnAchievementsButtonClicked()
        {
            _ = ModUIConstants.ShowAdvancementsMenuRework();
        }

        public void OnCustomizationButtonClicked()
        {
            Hide();
            _ = ModUIConstants.ShowPersonalizationItemsBrowser();
        }

        public void OnSettingsButtonClicked()
        {
            _ = ModUIConstants.ShowSettingsMenuRework(false);
        }

        public void OnModsButtonClicked()
        {
            ModsPanelManager.Instance.openModsMenu();
        }

        public void OnGiveFeedbackButtonClicked()
        {
            _ = ModUIConstants.ShowFeedbackUIRework(false);
        }

        public void OnMainMenuButtonClicked()
        {
            if (GameModeManager.Is((GameMode)2500) || (GameModeManager.IsInLevelEditor() && LevelEditorDataManager.Instance.CurrentLevelNeedsSaving()))
            {
                ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("dialog_exit_editor"), LocalizationManager.Instance.GetTranslatedString("dialog_exit_editor_desc"), 150f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, OnConfirmMainMenuButtonClicked, null);
                return;
            }

            m_confirmExitGameTextObject.SetActive(false);
            m_confirmMainMenuTextObject.SetActive(true);
            m_confirmExitGameButton.gameObject.SetActive(false);
            m_confirmMainMenuButton.gameObject.SetActive(true);
            m_mainMenuButton.gameObject.SetActive(false);
            m_exitGameButton.gameObject.SetActive(true);
        }

        public void OnConfirmMainMenuButtonClicked()
        {
            WorkshopLevelManager workshopLevelManager = WorkshopLevelManager.Instance;
            workshopLevelManager.ClearPlaytestData();
            workshopLevelManager.SetPlaytestActive(false);

            SceneTransitionManager.Instance.DisconnectAndExitToMainMenu();
        }

        public void OnExitGameButtonClicked()
        {
            if (GameModeManager.Is((GameMode)2500) || (GameModeManager.IsInLevelEditor() && LevelEditorDataManager.Instance.CurrentLevelNeedsSaving()))
            {
                ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("dialog_exit_editor"), LocalizationManager.Instance.GetTranslatedString("dialog_exit_editor_desc"), 150f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, OnConfirmExitGameButtonClicked, null);
                return;
            }

            m_confirmExitGameTextObject.SetActive(true);
            m_confirmMainMenuTextObject.SetActive(false);
            m_confirmExitGameButton.gameObject.SetActive(true);
            m_confirmMainMenuButton.gameObject.SetActive(false);
            m_mainMenuButton.gameObject.SetActive(true);
            m_exitGameButton.gameObject.SetActive(false);
        }

        public void OnConfirmExitGameButtonClicked()
        {
            Application.Quit();
        }

        public void OnReturnToLevelEditorButtonClicked()
        {
            GameUIRoot.Instance.EscMenu.OnBackToLevelEditorButtonClicked();
        }

        public void OnStartMatchButtonClicked()
        {
            GameUIRoot.Instance.EscMenu.OnStartBattleRoyaleLevelClicked();
            Hide();
        }

        public void OnSkipLevelButtonClicked()
        {
            Hide();
            GameUIRoot.Instance.EscMenu.OnSkipWorkshopLevelClicked();
        }

        public void OnLegacyUIButtonClicked()
        {
            Hide();
            ModUIUtils.ShowVanillaEscMenu();
        }

        public void OnWorkshopLevelUpVoteButtonClicked()
        {
            SteamWorkshopItem item = WorkshopLevelManager.Instance.GetCurrentLevelWorkshopItem();
            if (item == null)
                return;

            m_workshopLevelUpVoteButton.interactable = false;
            ModSteamUGCUtils.SetUserVote(item.WorkshopItemID, true, delegate (SetUserItemVoteResult_t t, bool ioError)
            {
                SteamWorkshopItem item2 = WorkshopLevelManager.Instance.GetCurrentLevelWorkshopItem();
                if (item != item2)
                    return;

                if (ioError || t.m_eResult != EResult.k_EResultOK)
                    ModUIUtils.MessagePopupOK("Vote error", $"Error code: {t.m_eResult} (ioError: {ioError})", 150f, true);
                else
                {
                    m_workshopLevelDownVoteButton.interactable = t.m_bVoteUp;
                    m_workshopLevelUpVoteButton.interactable = !t.m_bVoteUp;
                    return;
                }

                m_workshopLevelUpVoteButton.interactable = true;
            });
        }

        public void OnWorkshopLevelDownVoteButtonClicked()
        {
            SteamWorkshopItem item = WorkshopLevelManager.Instance.GetCurrentLevelWorkshopItem();
            if (item == null)
                return;

            m_workshopLevelDownVoteButton.interactable = false;
            ModSteamUGCUtils.SetUserVote(item.WorkshopItemID, false, delegate (SetUserItemVoteResult_t t, bool ioError)
            {
                SteamWorkshopItem item2 = WorkshopLevelManager.Instance.GetCurrentLevelWorkshopItem();
                if (item != item2)
                    return;

                if (ioError || t.m_eResult != EResult.k_EResultOK)
                    ModUIUtils.MessagePopupOK("Vote error", $"Error code: {t.m_eResult} (ioError: {ioError})", 150f, true);
                else
                {
                    m_workshopLevelDownVoteButton.interactable = t.m_bVoteUp;
                    m_workshopLevelUpVoteButton.interactable = !t.m_bVoteUp;
                    return;
                }

                m_workshopLevelDownVoteButton.interactable = true;
            });
        }

        public void OnWorkshopLevelInfoButtonClicked()
        {
            SteamWorkshopItem item = WorkshopLevelManager.Instance.GetCurrentLevelWorkshopItem();
            if (item == null)
                return;

            string link = item.GetURL();
            if (SteamManager.Instance && SteamManager.Instance.Initialized && SteamUtils.IsOverlayEnabled())
                SteamFriends.ActivateGameOverlayToWebPage(link);
            else
                Application.OpenURL(link);
        }

        public void OnRevealCodeButtonClicked()
        {
            m_revealCodeButton.gameObject.SetActive(false);
        }

        public void OnCopyCodeButtonClicked()
        {
            GUIUtility.systemCopyBuffer = m_codeField.text;
        }

        public void OnReconnectButtonClicked()
        {
            MultiplayerMatchmakingManager.ShouldStartBattleRoyaleGameOnLoad = true;
            SceneTransitionManager.Instance.DisconnectAndExitToMainMenu();
        }

        public static string GetPlatformString(PlayFab.ClientModels.LoginIdentityProvider login, bool colored = true)
        {
            if (!colored)
            {
                switch (login)
                {
                    case PlayFab.ClientModels.LoginIdentityProvider.Custom:
                    case PlayFab.ClientModels.LoginIdentityProvider.CustomServer:
                        return "Custom";

                    case PlayFab.ClientModels.LoginIdentityProvider.NintendoSwitch:
                    case PlayFab.ClientModels.LoginIdentityProvider.NintendoSwitchAccount:
                        return "Switch";
                    case PlayFab.ClientModels.LoginIdentityProvider.PlayFab:
                        return "PlayFab";
                    case PlayFab.ClientModels.LoginIdentityProvider.PSN:
                        return "PSN";
                    case PlayFab.ClientModels.LoginIdentityProvider.Steam:
                        return "Steam";
                    case PlayFab.ClientModels.LoginIdentityProvider.Twitch:
                        return "Twitch";
                    case PlayFab.ClientModels.LoginIdentityProvider.XBoxLive:
                        return "XBOX";
                }
                return "N/A";
            }

            switch (login)
            {
                case PlayFab.ClientModels.LoginIdentityProvider.Custom:
                case PlayFab.ClientModels.LoginIdentityProvider.CustomServer:
                    return "<color=#cacaca>Custom</color>";

                case PlayFab.ClientModels.LoginIdentityProvider.NintendoSwitch:
                case PlayFab.ClientModels.LoginIdentityProvider.NintendoSwitchAccount:
                    return "<color=#FF3B26>Switch</color>";
                case PlayFab.ClientModels.LoginIdentityProvider.PlayFab:
                    return "<color=#ffffff>PlayFab</color>";
                case PlayFab.ClientModels.LoginIdentityProvider.PSN:
                    return "<color=#4F85FF>PSN</color>";
                case PlayFab.ClientModels.LoginIdentityProvider.Steam:
                    return "<color=#2769E5>Steam</color>";
                case PlayFab.ClientModels.LoginIdentityProvider.Twitch:
                    return "<color=#A426E4>Twitch</color>";
                case PlayFab.ClientModels.LoginIdentityProvider.XBoxLive:
                    return "<color=#0DB30F>XBOX</color>";
            }
            return "<color=#ffffff>N/A</color>";
        }
    }
}
