using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIVersionLabel : OverhaulUIBehaviour
    {
        [ModSetting(ModSettingsConstants.SHOW_VERSION_LABEL, true)]
        public static bool ShowLabel;

        [UIElement("NewVersionLabel_TitleScreen")]
        private readonly GameObject m_watermark;

        [UIElement("DebugLabel_TitleScreen")]
        private readonly GameObject m_debugIcon;

        [UIElement("Watermark_TitleScreen")]
        private readonly Text m_versionText;

        [UIElement("NewVersionLabel_Gameplay")]
        private readonly GameObject m_gameplayWatermark;

        [UIElement("NewVersionLabel_Gameplay")]
        private readonly RectTransform m_gameplayWatermarkTransform;

        [UIElement("DebugLabel_Gameplay")]
        private readonly GameObject m_gameplayDebugIcon;

        [UIElement("Watermark_Gameplay")]
        private readonly Text m_gameplayVersionText;

        public bool ForceHide;

        private bool m_refreshWidth;

        public override bool closeOnEscapeButtonPress => false;

        public static UIVersionLabel instance
        {
            get;
            set;
        }

        public bool showWatermark
        {
            get
            {
                return ShowLabel && !GameModeManager.IsInLevelEditor() && !ModCache.photoManager.IsInPhotoMode();
            }
        }

        public bool showFullWatermark
        {
            get
            {
                return GameModeManager.IsOnTitleScreen();
            }
        }

        private float m_offsetX;
        public float offsetX
        {
            get
            {
                return m_offsetX;
            }
            set
            {
                m_offsetX = value;
                Vector2 anchoredPosition = m_gameplayWatermarkTransform.anchoredPosition;
                anchoredPosition.x = 5f + value;
                m_gameplayWatermarkTransform.anchoredPosition = anchoredPosition;
            }
        }

        protected override void OnInitialized()
        {
            instance = this;

            bool debug = ModBuildInfo.debug;
            _ = ModBuildInfo.fullVersionString;

            m_versionText.text = $"Overhaul {ModBuildInfo.fullVersionString}\nClone Drone {VersionNumberManager.Instance.GetVersionString()}";
            m_debugIcon.SetActive(debug);
            m_gameplayVersionText.text = $"overhaul {ModBuildInfo.versionString}";
            m_gameplayDebugIcon.SetActive(debug);
            m_refreshWidth = true;

            ModCache.titleScreenUI.VersionLabel.gameObject.SetActive(false);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            instance = null;
        }

        public override void Update()
        {
            if (m_refreshWidth)
            {
                m_refreshWidth = false;
                RectTransform rectTransform = m_gameplayWatermarkTransform;
                Vector2 sideDelta = rectTransform.sizeDelta;
                sideDelta.x = m_gameplayVersionText.preferredWidth + 15f;
                rectTransform.sizeDelta = sideDelta;
            }

            if (Time.frameCount % 10 != 0)
                return;

            bool show = !ForceHide && showWatermark;
            bool titleScreen = showFullWatermark;
            m_watermark.SetActive(show && ModCache.titleScreenUI.RootButtonsContainerBG.activeInHierarchy && titleScreen);
            m_gameplayWatermark.SetActive(show && !titleScreen);
        }

        public void OnOtherModsButtonClicked()
        {
            _ = ModUIConstants.ShowOtherModsMenu();
        }
    }
}
