using OverhaulMod.Content;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UINewsPanel : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnEditorButtonClicked))]
        [UIElement("EditorButton")]
        private readonly Button m_editorButton;

        [UIElementAction(nameof(OnRefreshButtonClicked))]
        [UIElement("RefreshButton")]
        private readonly Button m_refreshButton;

        [UIElement("LoadingIndicator", false)]
        private readonly GameObject m_loadingIndicator;

        [UIElement("NewsDisplayBig", false)]
        private readonly ModdedObject m_newsDisplayBig;

        [UIElement("NewsDisplaySmall", false)]
        private readonly ModdedObject m_newsDisplaySmall;

        [UIElement("Content")]
        private readonly Transform m_newsContainer;

        private bool m_isPopulating, m_hasEverPopulatedList;

        public override bool hideTitleScreen => true;

        public override void Show()
        {
            base.Show();
            if (!m_hasEverPopulatedList)
                Populate();

            m_editorButton.gameObject.SetActive(ModUserInfo.isDeveloper);
        }

        public void Populate()
        {
            if (m_isPopulating)
                return;

            clearList();
            setIsPopulating(true);

            NewsManager.Instance.DownloadNewsInfoFile(delegate (NewsInfoList newsInfoList)
            {
                m_hasEverPopulatedList = true;
                setIsPopulating(false);
                populateList(newsInfoList);
                NewsManager.Instance.SetHasSeenNews();
            }, delegate (string error)
            {
                ModUIUtils.MessagePopupOK("Error", error, "Ok", Hide, 200f, true);
                setIsPopulating(false);
            });
        }

        private void setIsPopulating(bool value)
        {
            m_isPopulating = value;
            m_refreshButton.interactable = !value;
            m_loadingIndicator.SetActive(value);
        }

        private void populateList(NewsInfoList newsInfoList)
        {
            int index = 0;
            foreach (NewsInfo news in newsInfoList.News)
            {
                ModdedObject moddedObject = Instantiate(index == 0 ? m_newsDisplayBig : m_newsDisplaySmall, m_newsContainer);
                moddedObject.gameObject.SetActive(true);
                moddedObject.GetObject<Text>(0).text = news.Title;

                Text descriptionLabel = moddedObject.GetObject<Text>(1);
                if (descriptionLabel)
                {
                    string shortDescription = news.Description;
                    if (shortDescription.Length > 128)
                    {
                        shortDescription = shortDescription.Remove(128) + "...";
                    }
                    descriptionLabel.text = shortDescription;
                }

                Button button = moddedObject.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    if (NewsManager.PrevNewsCount != newsInfoList.News.Count)
                        ModSettingsManager.SetIntValue(ModSettingsConstants.PREV_NEWS_COUNT, newsInfoList.News.Count);

                    ShowNewsDetails(news);
                });
                index++;
            }
        }

        private void clearList()
        {
            if (m_newsContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_newsContainer);
        }

        public void ShowNewsDetails(NewsInfo newsInfo)
        {
            _ = ModUIConstants.ShowNewsDetailsPanel(base.transform, newsInfo);
        }

        public void OnEditorButtonClicked()
        {
            _ = ModUIConstants.ShowNewsInfoEditor(base.transform);
        }

        public void OnRefreshButtonClicked()
        {
            Populate();
        }
    }
}
