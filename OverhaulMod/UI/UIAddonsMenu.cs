using OverhaulMod.Content;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIAddonsMenu : OverhaulUIBehaviour
    {
        private static ContentListInfo s_contentLiftInfo;

        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnLocalAddonsEditorButtonClicked))]
        [UIElement("EditorButton")]
        private readonly Button m_localAddonsEditorButton;

        [UIElementAction(nameof(OnNetworkAddonsEditorButtonClicked))]
        [UIElement("NetworkEditorButton")]
        private readonly Button m_networkAddonsEditorButton;

        [UIElement("LocalAddons")]
        private readonly ModdedObject m_localAddonsTab;
        [UIElement("NetworkAddons")]
        private readonly ModdedObject m_networkAddonsTab;

        [TabManager(typeof(UIElementTab), null, null, null, nameof(OnTabSelected))]
        private readonly TabManager m_tabs;

        [UIElement("LocalContentDisplay", false)]
        private readonly ModdedObject m_localContentDisplay;
        [UIElement("NetworkContentDisplay", false)]
        private readonly ModdedObject m_networkContentDisplay;

        [UIElement("Content")]
        private readonly Transform m_container;

        [UIElement("LoadingIndicator", false)]
        private readonly GameObject m_loadingIndicator;

        private bool m_shouldSuggestGameRestart;

        public override bool hideTitleScreen => true;

        protected override void OnInitialized()
        {
            m_tabs.AddTab(m_localAddonsTab.gameObject, "local addons");
            m_tabs.AddTab(m_networkAddonsTab.gameObject, "network addons");
            m_tabs.SelectTab("local addons");

            GlobalEventManager.Instance.AddEventListener<string>(ContentManager.CONTENT_DOWNLOAD_DONE_EVENT, onContentDownloaded);

            if (TitleScreenCustomizationManager.IntroduceCustomization)
            {
                ModSettingsManager.SetBoolValue(ModSettingsConstants.INTRODUCE_TITLE_SCREEN_CUSTOMIZATION, false);
            }
        }

        public override void Show()
        {
            base.Show();

            m_localAddonsEditorButton.gameObject.SetActive(ModUserInfo.isDeveloper);
            m_networkAddonsEditorButton.gameObject.SetActive(ModUserInfo.isDeveloper);
        }

        public override void Hide()
        {
            base.Hide();
            if (m_shouldSuggestGameRestart)
            {
                _ = ModUIConstants.ShowRestartRequiredScreen(true);
                m_shouldSuggestGameRestart = false;
            }
        }

        private void onContentDownloaded(string error)
        {
            if (!error.IsNullOrEmpty())
            {
                ModUIUtils.MessagePopupOK("Content download error", "Details:\n" + error, 350f, true);
            }
            else
            {
                m_shouldSuggestGameRestart = true;
            }
        }

        public void OnTabSelected(UIElementTab elementTab)
        {
            bool local = elementTab.tabId == "local addons";

            UIElementTab oldTab = m_tabs.prevSelectedTab;
            UIElementTab newTab = m_tabs.selectedTab;
            if (oldTab)
            {
                RectTransform rt = oldTab.transform as RectTransform;
                Vector2 vector = rt.sizeDelta;
                vector.y = 25f;
                rt.sizeDelta = vector;
            }
            if (newTab)
            {
                RectTransform rt = newTab.transform as RectTransform;
                Vector2 vector = rt.sizeDelta;
                vector.y = 30f;
                rt.sizeDelta = vector;
            }

            if (local)
                populateLocalContent();
            else
                populateNetworkContent();
        }

        private void populateLocalContent()
        {
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            System.Collections.Generic.List<ContentInfo> list = ContentManager.Instance.GetContent();
            if (list.IsNullOrEmpty())
                return;

            foreach (ContentInfo content in list)
            {
                string translationKey = $"addons_addon_{content.DisplayName.ToLower().Replace(' ', '_')}";
                string displayName = LocalizationManager.Instance.GetTranslatedString($"{translationKey}_name");

                ModdedObject moddedObject = Instantiate(m_localContentDisplay, m_container);
                moddedObject.gameObject.SetActive(true);
                moddedObject.GetObject<Text>(0).text = displayName;
                moddedObject.GetObject<Button>(1).onClick.AddListener(delegate
                {
                    ModUIUtils.MessagePopup(true, $"{LocalizationManager.Instance.GetTranslatedString("addons_confirmdelete_header")} \"{displayName}\"?", LocalizationManager.Instance.GetTranslatedString("action_cannot_be_undone"), 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
                    {
                        if (moddedObject && moddedObject.gameObject)
                        {
                            Directory.Delete(content.FolderPath, true);
                            ContentManager.Instance.RefreshContent(true);
                            Destroy(moddedObject.gameObject);
                        }
                    });
                });
            }
        }

        private void populateNetworkContent()
        {
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            if (s_contentLiftInfo != null)
            {
                populate(s_contentLiftInfo);
                return;
            }

            m_loadingIndicator.SetActive(true);
            m_tabs.interactable = false;
            ContentManager.Instance.DownloadContentList(out _, populate, delegate (string error)
            {
                ModUIUtils.MessagePopupOK("Error", error, true);

                m_loadingIndicator.SetActive(false);
                m_tabs.interactable = true;
                m_tabs.SelectTab("local addons");
            });
        }

        private void populate(ContentListInfo contentListInfo)
        {
            s_contentLiftInfo = contentListInfo;
            foreach (ContentDownloadInfo content in contentListInfo.ContentToDownload)
            {
                if (content == null || content.File.IsNullOrEmpty() || content.DisplayName.IsNullOrEmpty())
                    continue;

                string translationKey = $"addons_addon_{content.DisplayName.ToLower().Replace(' ', '_')}";

                ModdedObject moddedObject = Instantiate(m_networkContentDisplay, m_container);
                moddedObject.gameObject.SetActive(true);
                moddedObject.GetObject<Text>(0).text = LocalizationManager.Instance.GetTranslatedString($"{translationKey}_name");
                moddedObject.GetObject<Text>(7).text = content.Description.IsNullOrEmpty() ? LocalizationManager.Instance.GetTranslatedString("no_description_provided") : LocalizationManager.Instance.GetTranslatedString($"{translationKey}_description");
                moddedObject.GetObject<Text>(1).text = (Mathf.RoundToInt(Mathf.Round(content.Size / 1048576f * 10f)) / 10f).ToString() + " Megabytes";

                List<string> list = content.GetImages().ToList();
                if (!list.IsNullOrEmpty())
                    for (int i = 0; i < list.Count; i++)
                        list[i] = RepositoryManager.REPOSITORY_URL + "images/" + list[i];

                UIElementNetworkAddonDisplay networkAddonDisplay = moddedObject.gameObject.AddComponent<UIElementNetworkAddonDisplay>();
                networkAddonDisplay.contentFile = content.File;
                networkAddonDisplay.contentName = content.DisplayName;
                networkAddonDisplay.isSupported = content.IsSupported();
                networkAddonDisplay.minModVersion = content.MinModVersion;
                networkAddonDisplay.isLarge = content.Size > 52428800L;
                networkAddonDisplay.imageExplorerParentTransform = base.transform;
                networkAddonDisplay.images = list;
                networkAddonDisplay.InitializeElement();
            }

            m_loadingIndicator.SetActive(false);
            m_tabs.interactable = true;
        }

        public void OnLocalAddonsEditorButtonClicked()
        {
            _ = ModUIConstants.ShowAddonsEditor(base.transform);
        }

        public void OnNetworkAddonsEditorButtonClicked()
        {
            _ = ModUIConstants.ShowAddonsDownloadEditor(base.transform);
        }
    }
}
