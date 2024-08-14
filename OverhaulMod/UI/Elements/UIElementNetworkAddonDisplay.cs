using OverhaulMod.Content;
using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIElementNetworkAddonDisplay : OverhaulUIBehaviour
    {
        [UIElement("ContentSize")]
        private readonly GameObject m_contentSizeText;

        [UIElementAction(nameof(OnDownloadButtonClicked))]
        [UIElement("DownloadButton")]
        private readonly Button m_downloadButton;

        [UIElementAction(nameof(OnImagesButtonClicked))]
        [UIElement("ImagesButton")]
        private readonly Button m_imagesButton;

        [UIElement("ProgressBar")]
        private readonly GameObject m_progressBar;

        [UIElement("NotSupportedText", false)]
        private readonly GameObject m_notSupportedText;

        [UIElement("MinVersionText")]
        private readonly Text m_minVersionText;

        [UIElement("Fill")]
        private readonly Image m_progressBarFill;

        private ContentManager m_contentManager;

        private bool m_hasImages;

        public string contentName
        {
            get;
            set;
        }

        public string contentFile
        {
            get;
            set;
        }

        public bool isSupported
        {
            get;
            set;
        }

        public bool isLarge
        {
            get;
            set;
        }

        public Version minModVersion
        {
            get;
            set;
        }

        public List<string> images
        {
            get;
            set;
        }

        public Transform imageExplorerParentTransform
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            m_contentManager = ContentManager.Instance;

            string versionString = "N/A";
            if (minModVersion != null)
                versionString = minModVersion.ToString();

            m_minVersionText.enabled = !isSupported;
            m_minVersionText.text = $"{LocalizationManager.Instance.GetTranslatedString("addon_requires_version")} {versionString}";

            m_hasImages = !images.IsNullOrEmpty();
        }

        public override void Update()
        {
            ContentManager contentManager = m_contentManager;
            string file = contentFile;

            bool isDownloadingContent = contentManager.IsDownloading(file);
            bool hasDownloaded = contentManager.HasContent(contentName, true);

            m_progressBar.SetActive(isDownloadingContent);
            m_contentSizeText.SetActive(!isDownloadingContent && isSupported);
            m_downloadButton.gameObject.SetActive(!isDownloadingContent && isSupported);
            m_downloadButton.interactable = !hasDownloaded;
            m_imagesButton.gameObject.SetActive(m_hasImages && !isDownloadingContent && isSupported);

            if (isDownloadingContent)
                m_progressBarFill.fillAmount = contentManager.GetDownloadProgress(file);
        }

        public void OnDownloadButtonClicked()
        {
            if (isLarge)
            {
                ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("addons_largeaddon_header"), LocalizationManager.Instance.GetTranslatedString("addons_largeaddon_text"), 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
                {
                    _ = m_contentManager.DownloadContent(contentName, contentFile, delegate (string error)
                    {
                        if (error != null)
                            ModUIUtils.MessagePopupOK("Addon download error", error, true);
                    });
                });
                return;
            }
            _ = m_contentManager.DownloadContent(contentName, contentFile, delegate (string error)
            {
                if (error != null)
                    ModUIUtils.MessagePopupOK("Addon download error", error, true);
            });
        }

        public void OnImagesButtonClicked()
        {
            ModUIUtils.ImageExplorer(images, imageExplorerParentTransform);
        }
    }
}
