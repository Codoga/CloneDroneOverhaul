﻿using CDOverhaul.Workshop;
using UnityEngine;
using UnityEngine.UI;

namespace CDOverhaul.HUD
{
    public class UIOverhaulVersionLabel : UIController
    {
        [OverhaulSetting(OverhaulSettingConstants.Categories.USER_INTERFACE, OverhaulSettingConstants.Sections.VERSION_LABEL, "Always show label")]
        public static bool AlwaysShow = true;

        [UIElementReference(0)]
        private readonly GameObject m_TitleScreenWatermark;
        [UIElementReference(1)]
        private readonly Text m_TitleScreenVersionLabel;
        [UIElementReference(5)]
        private readonly GameObject m_TitleScreenDebugLabel;

        [UIElementReference(2)]
        private readonly GameObject m_GameplayWatermark;
        [UIElementReference(3)]
        private readonly Text m_GameplayVersionLabel;
        [UIElementReference(4)]
        private readonly GameObject m_GameplayDebugLabel;

        [UIElementReference(6)]
        private readonly GameObject m_UpperButtonsContainer;

        [UIElementActionReference(nameof(onPatchNotesButtonClicked))]
        [UIElementReference(7)]
        private readonly Button m_PatchNotesButton;
        [UIElementActionReference(nameof(onTestUIClicked))]
        [UIElementReference(8)]
        private readonly Button m_TestUIButton;

        private Text m_TitleScreenUIVersionLabel;
        private GameObject m_TitleScreenRootButtons;

        protected override bool AutoCursorManagement() => false;

        public override void Initialize()
        {
            base.Initialize();
            m_TitleScreenWatermark.SetActive(false);
            m_TitleScreenVersionLabel.text = OverhaulVersion.hasToShowFullBuildTag ? OverhaulVersion.fullBuildTag : OverhaulVersion.Watermark;
            m_TitleScreenDebugLabel.SetActive(OverhaulVersion.IsDebugBuild);

            m_GameplayWatermark.gameObject.SetActive(false);
            m_GameplayVersionLabel.text = OverhaulVersion.hasToShowFullBuildTag ? OverhaulVersion.fullBuildTag : OverhaulVersion.ShortenedWatermark;
            m_GameplayDebugLabel.SetActive(OverhaulVersion.IsDebugBuild);

            m_TitleScreenUIVersionLabel = GameUIRoot.Instance.TitleScreenUI.VersionLabel;
            m_TitleScreenUIVersionLabel.gameObject.SetActive(false);
            m_TitleScreenRootButtons = GameUIRoot.Instance.TitleScreenUI.RootButtonsContainerBG;
        }

        protected override void OnDisposed()
        {
            if (m_TitleScreenVersionLabel)
                m_TitleScreenUIVersionLabel.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (Time.frameCount % 5 == 0)
                Refresh();
        }

        public void Refresh()
        {
            bool isOnTitleScreen = GameModeManager.IsOnTitleScreen();
            bool shouldShowOnTitleScreen = !OverhaulWorkshopBrowserUI.IsActive;
            bool shouldShowInGameplay = AlwaysShow;

            m_UpperButtonsContainer.gameObject.SetActive(m_TitleScreenRootButtons.activeInHierarchy);
            m_TitleScreenWatermark.SetActive(isOnTitleScreen && shouldShowOnTitleScreen);
            m_GameplayWatermark.SetActive(!isOnTitleScreen && shouldShowInGameplay);
        }

        private void onPatchNotesButtonClicked()
        {
            UIConstants.ShowChangelogPanel();
        }

        private void onTestUIClicked()
        {
            UIConstants.ShowUITest();
        }
    }
}