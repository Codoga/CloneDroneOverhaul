using OverhaulMod.Combat;
using OverhaulMod.Utils;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIChapterSelectMenuRework : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnRestartChapter1ButtonClicked))]
        [UIElement("RestartC1")]
        private readonly Button m_chapter1Button;

        [UIElementAction(nameof(OnRestartChapter2ButtonClicked))]
        [UIElement("RestartC2")]
        private readonly Button m_chapter2Button;

        [UIElementAction(nameof(OnRestartChapter3ButtonClicked))]
        [UIElement("RestartC3")]
        private readonly Button m_chapter3Button;

        [UIElementAction(nameof(OnRestartChapter4ButtonClicked))]
        [UIElement("RestartC4")]
        private readonly Button m_chapter4Button;

        [UIElementAction(nameof(OnRestartChapter5ButtonClicked))]
        [UIElement("RestartC5")]
        private readonly Button m_chapter5Button;

        [UIElementAction(nameof(OnContinueButtonClicked))]
        [UIElement("ContinueButton")]
        private readonly Button m_continueButton;

        [UIElementAction(nameof(OnLegacyUIButtonClicked))]
        [UIElement("OldUIButton")]
        private readonly Button m_legacyUIButton;

        [UIElementAction(nameof(OnDifficultyDropdownEdit))]
        [UIElement("DifficultyDropdown")]
        private readonly Dropdown m_difficultyDropdown;

        [UIElementAction(nameof(OnEnableGreatSwordsToggleEdit))]
        [UIElement("EnableGreatswordsToggle")]
        private readonly Toggle m_enableGreatSwordsToggle;

        [UIElement("ProgressText")]
        private readonly Text m_progressText;

        [UIElement("ContinueText")]
        private readonly Text m_continueButtonText;

        public override bool refreshOnlyCursor => true;

        public override void Show()
        {
            base.Show();
            ModCache.titleScreenUI.SetSinglePlayerModeSelectButtonsVisibile(false);
            m_difficultyDropdown.options = SettingsManager.Instance.GetDifficultyOptions();
            m_difficultyDropdown.value = SettingsManager.Instance.GetStoryDifficultyIndex();
            m_enableGreatSwordsToggle.isOn = ModGameModifiersManager.Instance.forceEnableGreatSwords;
            m_enableGreatSwordsToggle.gameObject.SetActive(ModFeatures.IsEnabled(ModFeatures.FeatureType.StoryModeModifiers));
            RefreshProgressText();
        }

        public override void Hide()
        {
            base.Hide();
            if (!ModCache.titleScreenUI.ChapterSelectUI.gameObject.activeInHierarchy)
                ModCache.titleScreenUI.SetSinglePlayerModeSelectButtonsVisibile(true);
        }

        public void RefreshProgressText()
        {
            m_progressText.text = string.Empty;

            int currentChapter = MetagameProgressManager.Instance.CurrentProgressHasReached(MetagameProgress.P2_FirstHumanEscaped) ? 2 : 1;
            int numLevels = GameDataManager.Instance.GetNumberOfStoryLevelsWon() + 1;
            if (currentChapter <= 1 && numLevels <= 1)
                m_continueButtonText.text = LocalizationManager.Instance.GetTranslatedString("New Game");
            else
            {
                string chapterText = LocalizationManager.Instance.GetTranslatedString("Chapter");
                string levelText = LocalizationManager.Instance.GetTranslatedString("Level");

                m_continueButtonText.text = LocalizationManager.Instance.GetTranslatedString("Continue");
                m_progressText.text = Singleton<MetagameProgressManager>.Instance.CurrentProgressHasReached(MetagameProgress.P10_ConqueredBattlecruiser)
                    ? $"{chapterText} 5"
                    : Singleton<MetagameProgressManager>.Instance.CurrentProgressHasReached(MetagameProgress.P7_CompletedTowerAssault)
                    ? $"{chapterText} 4"
                    : Singleton<MetagameProgressManager>.Instance.CurrentProgressHasReached(MetagameProgress.P5_DestroyedAlphaCentauri)
                    ? $"{chapterText} 3"
                    : $"{chapterText} {currentChapter}, {levelText} {numLevels}";
            }

            m_chapter1Button.gameObject.SetActive(true);
            m_chapter2Button.gameObject.SetActive(MetagameProgressManager.Instance.HasBeatChapter1());
            m_chapter3Button.gameObject.SetActive(MetagameProgressManager.Instance.HasBeatChapter2());
            m_chapter4Button.gameObject.SetActive(MetagameProgressManager.Instance.HasBeatChapter3());
            m_chapter5Button.gameObject.SetActive(MetagameProgressManager.Instance.HasBeatChapter4());
        }

        public void ShowChapterLevelSelectionMenu(int chapterIndex)
        {
            _ = ModUIConstants.ShowChapterLevelSelectMenu(base.transform, chapterIndex);
        }

        public void OnRestartChapter1ButtonClicked()
        {
            ShowChapterLevelSelectionMenu(1);
        }

        public void OnRestartChapter2ButtonClicked()
        {
            ShowChapterLevelSelectionMenu(2);
        }

        public void OnRestartChapter3ButtonClicked()
        {
            ShowChapterLevelSelectionMenu(3);
        }

        public void OnRestartChapter4ButtonClicked()
        {
            ShowChapterLevelSelectionMenu(4);
        }

        public void OnRestartChapter5ButtonClicked()
        {
            ShowChapterLevelSelectionMenu(5);
        }

        public void OnContinueButtonClicked()
        {
            Hide();
            GameFlowManager.Instance.StartStoryModeGame();
        }

        public void OnDifficultyDropdownEdit(int index)
        {
            SettingsManager.Instance.SetStoryDifficultyIndex(index);
        }

        public void OnEnableGreatSwordsToggleEdit(bool value)
        {
            ModGameModifiersManager.Instance.forceEnableGreatSwords = value;
        }

        public void OnLegacyUIButtonClicked()
        {
            TitleScreenUI titleScreenUI = ModCache.titleScreenUI;
            if (titleScreenUI)
            {
                titleScreenUI.ChapterSelectUI.Show();
                Hide();
            }
        }
    }
}
