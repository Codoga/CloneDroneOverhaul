using OverhaulMod.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIChallengesMenuRework : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnLegacyUIButtonClicked))]
        [UIElement("OldUIButton")]
        private readonly Button m_legacyUIButton;

        [UIElementAction(nameof(OnDebugSoloChallengesButtonClicked))]
        [UIElement("DebugSoloButton")]
        private readonly Button m_debugSoloChallengesButton;
        [UIElementAction(nameof(OnDebugPublicCoopChallengesButtonClicked))]
        [UIElement("DebugPubCoopButton")]
        private readonly Button m_debugPublicCoopChallengesButton;
        [UIElementAction(nameof(OnDebugPrivateCoopChallengesButtonClicked))]
        [UIElement("DebugPrivCoopButton")]
        private readonly Button m_debugPrivateCoopChallengesButton;

        [UIElementAction(nameof(OnPlayRandomButtonClicked))]
        [UIElement("RandomSoloChallengeButton")]
        private readonly Button m_soloPlayRandomButton;
        [UIElementAction(nameof(OnPlayUndefeatedButtonClicked))]
        [UIElement("UndefeatedSoloChallengeButton")]
        private readonly Button m_soloPlayUndefeatedButton;

        [UIElementAction(nameof(OnPlayRandomButtonClicked))]
        [UIElement("RandomCoopChallengeButton")]
        private readonly Button m_coopPlayRandomButton;
        [UIElementAction(nameof(OnPlayUndefeatedButtonClicked))]
        [UIElement("UndefeatedCoopChallengeButton")]
        private readonly Button m_coopPlayUndefeatedButton;

        [UIElementAction(nameof(OnGetMoreChallengesButtonClicked))]
        [UIElement("WorkshopChallengesButton")]
        private readonly Button m_getMoreChallengesButton;

        [UIElement("SoloButtons", true)]
        private readonly GameObject m_soloButtonsContainerObject;
        [UIElement("CoopButtons", false)]
        private readonly GameObject m_coopButtonsContainerObject;

        [UIElement("PlayUndefeatedChallengeButtonHolder", true)]
        private readonly GameObject m_soloPlayUndefeatedChallengeButtonHolder;
        [UIElement("JoinUndefeatedChallengeButtonHolder", true)]
        private readonly GameObject m_coopPlayUndefeatedChallengeButtonHolder;

        [UIElement("ChallengeDisplay", false)]
        private readonly ModdedObject m_challengeDisplayPrefab;
        [UIElement("Content")]
        private readonly Transform m_challengesContainer;

        public override bool refreshOnlyCursor => true;

        public List<ChallengeDefinition> displayingChallenges
        {
            get;
            private set;
        }

        public bool showsCoopChallenges
        {
            get;
            private set;
        }

        public bool startPrivateMatch
        {
            get;
            private set;
        }

        protected override void OnInitialized()
        {
            displayingChallenges = new List<ChallengeDefinition>();
        }

        public override void Show()
        {
            base.Show();

            TitleScreenUI titleScreenUI = ModCache.titleScreenUI;
            if (titleScreenUI.SingleplayerModeSelectScreen.gameObject.activeInHierarchy)
                titleScreenUI.SetSinglePlayerModeSelectButtonsVisibile(false);
            else if (titleScreenUI.MultiplayerModeSelectScreen.gameObject.activeInHierarchy)
                titleScreenUI.SetMultiplayerPlayerModeSelectButtonsVisibile(false);
        }

        public override void Hide()
        {
            base.Hide();

            TitleScreenUI titleScreenUI = ModCache.titleScreenUI;
            if (titleScreenUI.SingleplayerModeSelectScreen.gameObject.activeInHierarchy)
                titleScreenUI.SetSinglePlayerModeSelectButtonsVisibile(true);
            else if (titleScreenUI.MultiplayerModeSelectScreen.gameObject.activeInHierarchy)
                titleScreenUI.SetMultiplayerPlayerModeSelectButtonsVisibile(true);
        }

        public void Populate(bool isCoop, bool isPrivate)
        {
            showsCoopChallenges = isCoop;
            startPrivateMatch = isPrivate;

            displayingChallenges.Clear();
            if (m_challengesContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_challengesContainer);

            m_coopButtonsContainerObject.SetActive(isCoop);
            m_soloButtonsContainerObject.SetActive(!isCoop);

            ChallengeManager manager = ChallengeManager.Instance;
            ChallengeDefinition[] challenges = manager.GetChallenges(isCoop);
            if (challenges.IsNullOrEmpty())
                return;

            bool hasUndefeatedChallenge = false;
            foreach (ChallengeDefinition challengeDefinition in challenges)
            {
                displayingChallenges.Add(challengeDefinition);
                bool hasCompleted = manager.HasCompletedChallenge(challengeDefinition.ChallengeID);

                void action()
                {
                    OnChallengeClicked(challengeDefinition);
                }

                void leaderboardAction()
                {
                    string fileName = $"{GameDataManager.Instance.getChallengeHighScoreSavePath(challengeDefinition.ChallengeID)}.json";
                    List<HighScoreData> highScores;
                    try
                    {
                        highScores = ModJsonUtils.DeserializeStream<List<HighScoreData>>(DataRepository.Instance.GetFullPath(fileName));
                    }
                    catch
                    {
                        highScores = new List<HighScoreData>();
                    }
                    _ = ModUIConstants.ShowLeaderboard(base.transform, highScores, fileName);
                }

                CharacterModelCustomizationEntry characterModelCustomizationEntry = getCharacterModelUnlockedByChallenge(challengeDefinition.ChallengeID);
                Sprite sprite = characterModelCustomizationEntry != null ? characterModelCustomizationEntry.ImageSprite : challengeDefinition.ImageSprite;

                string rewardText = string.Empty;
                UpgradeDescription upgradeUnlockedByChallenge = UpgradeManager.Instance.GetUpgradeUnlockedByChallenge(challengeDefinition.ChallengeID);
                if (upgradeUnlockedByChallenge)
                {
                    rewardText = LocalizationManager.Instance.GetTranslatedString(upgradeUnlockedByChallenge.UpgradeName);
                }
                else
                {
                    EmoteDefinition emoteUnlockedByChallenge = getEmoteUnlockedByChallenge(challengeDefinition.ChallengeID);
                    rewardText = characterModelCustomizationEntry != null
                        ? LocalizationManager.Instance.GetTranslatedString(characterModelCustomizationEntry.Name) + " " + LocalizationManager.Instance.GetTranslatedString("skin")
                        : emoteUnlockedByChallenge != null
                            ? LocalizationManager.Instance.GetTranslatedString("Emote:") + " " + LocalizationManager.Instance.GetTranslatedString(emoteUnlockedByChallenge.EmoteID, -1)
                            : LocalizationManager.Instance.GetTranslatedString("A nice trophy!", -1);
                }

                ModdedObject moddedObject = Instantiate(m_challengeDisplayPrefab, m_challengesContainer);
                moddedObject.gameObject.SetActive(true);
                moddedObject.GetObject<Text>(0).text = LocalizationManager.Instance.GetTranslatedString(challengeDefinition.ChallengeName);
                moddedObject.GetObject<Text>(1).text = LocalizationManager.Instance.GetTranslatedString(challengeDefinition.ChallengeDescription);
                moddedObject.GetObject<Image>(2).sprite = sprite;
                moddedObject.GetObject<Text>(3).text = rewardText;
                moddedObject.GetObject<Button>(4).onClick.AddListener(action);
                moddedObject.GetObject<GameObject>(5).SetActive(hasCompleted);
                moddedObject.GetObject<GameObject>(6).SetActive(!hasCompleted);
                moddedObject.GetObject<Button>(7).onClick.AddListener(leaderboardAction);
                moddedObject.GetObject<Button>(7).interactable = challengeDefinition.UseEndlessLevels && !isCoop;
                UIElementShowTooltipOnHightLight showTooltipOnHightLight = moddedObject.GetObject<Button>(7).gameObject.AddComponent<UIElementShowTooltipOnHightLight>();
                showTooltipOnHightLight.InitializeElement();
                showTooltipOnHightLight.tooltipText = "leaderboard";
                showTooltipOnHightLight.tooltipShowDuration = 1f;
                showTooltipOnHightLight.textIsLocalizationId = true;

                if (!hasUndefeatedChallenge)
                {
                    if (!manager.HasCompletedChallenge(challengeDefinition.ChallengeID))
                        hasUndefeatedChallenge = true;
                }
            }

            m_soloPlayUndefeatedButton.interactable = hasUndefeatedChallenge;
            m_coopPlayUndefeatedButton.interactable = hasUndefeatedChallenge;
        }

        private CharacterModelCustomizationEntry getCharacterModelUnlockedByChallenge(string challengeID)
        {
            CompleteChallengeAchievement completeChallengeAchievement = GameplayAchievementManager.Instance.GetCompleteChallengeAchievement(challengeID);
            return completeChallengeAchievement
                ? MultiplayerCharacterCustomizationManager.Instance.GetCharacterModelUnlockedByAchievement(completeChallengeAchievement.AchievementID)
                : null;
        }

        private EmoteDefinition getEmoteUnlockedByChallenge(string challengeID)
        {
            CompleteChallengeAchievement completeChallengeAchievement = GameplayAchievementManager.Instance.GetCompleteChallengeAchievement(challengeID);
            return completeChallengeAchievement
                ? EmoteManager.Instance.GetEmoteUnlockedByAchievement(completeChallengeAchievement.AchievementID)
                : null;
        }

        public void OnChallengeClicked(ChallengeDefinition challengeDefinition)
        {
            Hide();
            if (showsCoopChallenges)
            {
                ModUIManager modUIManager = ModUIManager.Instance;
                if (modUIManager)
                    _ = modUIManager.Hide(AssetBundleConstants.UI, ModUIConstants.UI_DUEL_INVITE_MENU_REWORK);

                if (startPrivateMatch)
                    ChallengeManager.Instance.CreatePrivateCoopChallenge(challengeDefinition.ChallengeID);
                else
                    ChallengeManager.Instance.JoinPublicCoopChallenge(challengeDefinition.ChallengeID);
            }
            else
                ChallengeManager.Instance.StartChallenge(challengeDefinition, false);
        }

        public void OnLegacyUIButtonClicked()
        {
            TitleScreenUI titleScreenUI = ModCache.titleScreenUI;
            if (titleScreenUI)
            {
                Hide();
                titleScreenUI.OnChallengeButtonClicked();
            }
        }

        public void OnGetMoreChallengesButtonClicked()
        {
            Hide();

            TitleScreenUI titleScreenUI = ModCache.titleScreenUI;
            if (!titleScreenUI)
                return;

            WorkshopBrowserUI workshopBrowserUI = titleScreenUI.WorkshopBrowserUI;
            if (!workshopBrowserUI)
                return;

            titleScreenUI.CoopChallengesInviteMenu.Hide();
            titleScreenUI.SetSinglePlayerModeSelectButtonsVisibile(true);
            titleScreenUI.SingleplayerModeSelectScreen.Hide();
            titleScreenUI.SetMultiplayerPlayerModeSelectButtonsVisibile(true);
            titleScreenUI.MultiplayerModeSelectScreen.Hide();
            titleScreenUI.setLogoAndRootButtonsVisible(true);

            workshopBrowserUI.Show();
        }

        public void OnDebugSoloChallengesButtonClicked()
        {
            Populate(false, false);
        }

        public void OnDebugPublicCoopChallengesButtonClicked()
        {
            Populate(true, false);
        }

        public void OnDebugPrivateCoopChallengesButtonClicked()
        {
            Populate(true, true);
        }

        public void OnPlayRandomButtonClicked()
        {
            List<ChallengeDefinition> list = displayingChallenges;
            if (list.IsNullOrEmpty())
                return;

            ChallengeDefinition challengeDefinition = list[UnityEngine.Random.Range(0, list.Count)];
            if (challengeDefinition == null)
                return;

            OnChallengeClicked(challengeDefinition);
        }

        public void OnPlayUndefeatedButtonClicked()
        {
            List<ChallengeDefinition> list = displayingChallenges;
            if (list.IsNullOrEmpty())
                return;

            ChallengeDefinition challengeDefinition = null;
            foreach (ChallengeDefinition cd in list)
            {
                if (!ChallengeManager.Instance.HasCompletedChallenge(cd.ChallengeID))
                {
                    challengeDefinition = cd;
                    break;
                }
            }

            if (challengeDefinition == null)
            {
                ModUIUtils.MessagePopupOK("You have all challenges defeated", "Well done", true);
                return;
            }

            OnChallengeClicked(challengeDefinition);
            /*
            ModUIUtils.MessagePopup(true, $"Play {LocalizationManager.Instance.GetTranslatedString(challengeDefinition.ChallengeName)}?", "You haven't beaten it yet", 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                OnChallengeClicked(challengeDefinition);
            });*/
        }
    }
}
