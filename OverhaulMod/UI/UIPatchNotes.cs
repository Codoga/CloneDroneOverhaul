using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPatchNotes : OverhaulUIBehaviour
    {
        [ModSetting(ModSettingsConstants.LAST_BUILD_CHANGELOG_WAS_SHOWN, null)]
        public static string LastBuildChangelogWasShownOn;

        [UIElementAction(nameof(OnCloseButtonClicked))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnFeedbackButtonClicked))]
        [UIElement("FeedbackButton")]
        private readonly Button m_feedbackButton;

        [UIElement("VersionDisplay", false)]
        private readonly ModdedObject m_versionDisplay;

        [UIElement("Content")]
        private readonly Transform m_container;

        [UIElement("Separator", false)]
        private readonly GameObject m_separator;

        [UIElement("Header")]
        private readonly Text m_headerText;

        [UIElement("Description")]
        private readonly Text m_descriptionText;

        public override bool hideTitleScreen => true;

        private bool m_shouldRefreshText;

        private Button m_previousButtonClicked;

        protected override void OnInitialized()
        {
            m_descriptionText.fontSize = 10;

            string path = Path.Combine(ModCore.dataFolder, "changelogs");
            if (!Directory.Exists(path))
                return;

            List<Version> versions = new List<Version>();
            foreach (string directory in Directory.GetDirectories(path))
            {
                string dirName = ModIOUtils.GetDirectoryName(directory);
                if (!Version.TryParse(dirName, out Version version))
                    version = new Version(0, 0, 0, 0);

                versions.Add(version);
            }

            if (versions.IsNullOrEmpty())
                return;

            Button firstButton = null;

            versions.Sort(CompareByVersion);

            int minorVersion = ModBuildInfo.versionMinor;
            int buildVersion = ModBuildInfo.versionBuild;
            foreach (Version version in versions)
            {
                if ((version.Minor != minorVersion || version.Build != buildVersion) && firstButton)
                {
                    GameObject separator = Instantiate(m_separator, m_container);
                    separator.SetActive(true);
                    minorVersion = version.Minor;
                    buildVersion = version.Build;
                }

                ModdedObject display = Instantiate(m_versionDisplay, m_container);
                display.gameObject.SetActive(true);

                string verString = version.ToString();
                string verHeader = getBuildDisplayVersion(verString);
                display.GetObject<Text>(0).text = verHeader;

                Button button = display.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    Button pb = m_previousButtonClicked;
                    if (pb)
                        pb.interactable = true;

                    button.interactable = false;
                    m_previousButtonClicked = button;

                    PopulateChangelog(verHeader, verString);
                });

                if (!firstButton)
                {
                    firstButton = button;
                }
            }

            if (firstButton)
                firstButton.Press();
        }

        public override void Hide()
        {
            base.Hide();

            ModSettingsManager.SetStringValue(ModSettingsConstants.LAST_BUILD_CHANGELOG_WAS_SHOWN, ModBuildInfo.versionString);
            ModSettingsDataManager.Instance.Save();
        }

        public override void Update()
        {
            base.Update();
            if (m_shouldRefreshText)
            {
                m_shouldRefreshText = false;

                Vector2 sizeDelta = m_descriptionText.rectTransform.sizeDelta;
                sizeDelta.y = m_descriptionText.preferredHeight + 30f;
                m_descriptionText.rectTransform.sizeDelta = sizeDelta;
            }
        }

        public void PopulateChangelog(string header, string folderName)
        {
            string path = Path.Combine(ModCore.dataFolder, "changelogs", folderName);
            string langCode = LocalizationManager.Instance.GetCurrentLanguageCode();
            if (langCode != "ru" && langCode != "en")
                langCode = "en";

            string file = Path.Combine(path, $"changelog_{langCode}.txt");
            string text;
            if (File.Exists(file))
                text = ModIOUtils.ReadText(file);
            else
            {
                if (langCode != "en")
                {
                    file = Path.Combine(path, $"changelog_en.txt");
                    if (File.Exists(file))
                        text = ModIOUtils.ReadText(file);
                    else
                        text = "Changelog file read error.";
                }
                else
                {
                    text = "Changelog file read error.";
                }
            }

            m_headerText.text = header;
            m_descriptionText.text = text;
            m_shouldRefreshText = true;
        }

        private string getBuildDisplayVersion(string folder)
        {
            switch (folder)
            {
                case "0.2.0.13":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 2 (0.2.0.13)";
                case "0.2.0.22":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 2 Patch (0.2.0.22)";
                case "0.2.10.22":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 2 HotFix (0.2.10.22)";
                case "0.3.0.345":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 3 (0.3.0.345)";
                case "0.3.1.0":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 3.1 (0.3.1.0)";
                case "0.3.1.8":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 3.1 HotFix (0.3.1.8)";
                case "0.4.0.200":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 4 Test (0.4.0.200)";
                case "0.4.0.227":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 4 (0.4.0.227)";
                case "0.4.1.13":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 4.1 (0.4.1.13)";
                case "0.4.2.0":
                    return $"{LocalizationManager.Instance.GetTranslatedString("changelog_update")} 4.2 (0.4.2.0)";
                default:
                    return folder;
            }
        }

        public void OnCloseButtonClicked()
        {
            Hide();
        }

        public void OnFeedbackButtonClicked()
        {
            _ = ModUIConstants.ShowFeedbackUIRework(false);
        }

        public static int CompareByVersion(Version a, Version b)
        {
            if (b > a)
            {
                return 1;
            }
            if (b < a)
            {
                return -1;
            }
            return 0;
        }
    }
}
