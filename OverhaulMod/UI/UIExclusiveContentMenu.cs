using OverhaulMod.Content;
using OverhaulMod.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIExclusiveContentMenu : OverhaulUIBehaviour
    {
        [UIElement("StatusBarText")]
        private readonly Text m_statusBarText;

        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnEditorButtonClicked))]
        [UIElement("EditorButton")]
        private readonly Button m_editorButton;

        [UIElementAction(nameof(OnRetrieveButtonClicked))]
        [UIElement("RetrieveDataButton")]
        private readonly Button m_retrieveDataButton;

        [UIElementAction(nameof(OnRefreshButtonClicked))]
        [UIElement("RefreshButton")]
        private readonly Button m_refreshButton;

        [UIElement("UnlockedItemDisplayPrefab", false)]
        private readonly ModdedObject m_unlockedItemDisplayPrefab;

        [UIElement("Content")]
        private readonly Transform m_container;

        [UIElement("NothingIndicator")]
        private readonly GameObject m_nothingIndicator;

        public override bool hideTitleScreen => true;

        protected override void OnInitialized()
        {
            m_statusBarText.text = "Idle";
        }

        public override void Show()
        {
            base.Show();
            Populate();

            string error = ExclusiveContentManager.Instance.error;
            if (error != null)
            {
                m_statusBarText.text = "Error";
                ModUIUtils.MessagePopup(true, "Could not get data. Retry?", error, 150f, MessageMenu.ButtonLayout.EnableDisableButtons, "Ok", "Retry", "No", null, delegate
                {
                    OnRetrieveButtonClicked();
                }, null);
            }

            m_editorButton.gameObject.SetActive(ModUserInfo.isDeveloper);
        }

        public void Populate()
        {
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            System.Collections.Generic.List<ExclusiveContentInfo> list = ExclusiveContentManager.Instance.GetAllUnlockedContent();
            if (list.IsNullOrEmpty())
            {
                m_nothingIndicator.SetActive(true);
            }
            else
            {
                m_nothingIndicator.SetActive(false);
                foreach (Content.ExclusiveContentInfo contentInfo in list)
                {
                    ModdedObject moddedObject = Instantiate(m_unlockedItemDisplayPrefab, m_container);
                    moddedObject.gameObject.SetActive(true);
                    moddedObject.GetObject<Text>(0).text = contentInfo.Name;
                }
            }
        }

        public void OnEditorButtonClicked()
        {
            ModUIConstants.ShowExclusiveContentEditor(base.transform);
        }

        public void OnRetrieveButtonClicked()
        {
            m_retrieveDataButton.interactable = false;
            m_statusBarText.text = "Retrieving data...";
            ExclusiveContentManager.Instance.RetrieveData(delegate
            {
                m_statusBarText.text = "Successfully retrieved data!";
                Populate();
            }, delegate (string error)
            {
                m_retrieveDataButton.interactable = true;
                m_statusBarText.text = "Error: " + error;
            }, false);
        }

        public void OnRefreshButtonClicked()
        {
            Populate();
        }
    }
}
