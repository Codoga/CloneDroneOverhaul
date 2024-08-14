using OverhaulMod.Content;
using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIAddonsDownloadEditor : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnSaveButtonClicked))]
        [UIElement("SaveButton")]
        private readonly Button m_saveButton;

        [UIElementAction(nameof(OnCreateNewButtonClicked))]
        [UIElement("CreateNewButton")]
        private readonly Button m_createNewButton;

        [UIElementAction(nameof(OnGetDataFromServersButtonClicked))]
        [UIElement("GetDataFromServersButton")]
        private readonly Button m_getDataFromServersButton;

        [UIElementAction(nameof(OnEditedInfosDropdown))]
        [UIElement("AddonsDropdown")]
        private readonly Dropdown m_infosDropdown;

        [UIElement("AddonNameInputField")]
        private readonly InputField m_contentNameField;
        [UIElement("AddonSizeInputField")]
        private readonly InputField m_contentSizeField;
        [UIElement("AddonFilePathInputField")]
        private readonly InputField m_contentPathField;
        [UIElement("AddonMinModVersionInputField")]
        private readonly InputField m_contentMinModVersionField;
        [UIElement("AddonDescriptionField")]
        private readonly InputField m_contentDescriptionField;
        [UIElement("AddonImagesField")]
        private readonly InputField m_contentImagesField;
        [UIElement("AddonVersionField")]
        private readonly InputField m_contentVersionField;

        [UIElement("FileDisplay", false)]
        private readonly ModdedObject m_fileDisplayPrefab;
        [UIElement("AddFileButton", false)]
        private readonly Button m_addFileButton;

        [UIElement("Content")]
        private readonly Transform m_filesContainer;

        public ContentListInfo editingContentList
        {
            get;
            private set;
        }

        public ContentDownloadInfo editingContentDownloadInfo
        {
            get;
            private set;
        }

        public override bool hideTitleScreen => true;

        public override void Show()
        {
            base.Show();
        }

        private void populateDropdown()
        {
            List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();

            ContentListInfo cl = editingContentList;
            if (cl == null || cl.ContentToDownload.IsNullOrEmpty())
                return;

            foreach (ContentDownloadInfo c in cl.ContentToDownload)
            {
                list.Add(new Dropdown.OptionData(c.DisplayName));
            }
            m_infosDropdown.options = list;
        }

        private void populateFields()
        {
            ContentDownloadInfo contentDownloadInfo = editingContentDownloadInfo;
            if (contentDownloadInfo == null)
                return;

            m_contentNameField.text = contentDownloadInfo.DisplayName;
            m_contentSizeField.text = contentDownloadInfo.Size.ToString();
            m_contentPathField.text = contentDownloadInfo.File;
            m_contentMinModVersionField.text = contentDownloadInfo.MinModVersion?.ToString();
            m_contentDescriptionField.text = contentDownloadInfo.Description;
            m_contentImagesField.text = contentDownloadInfo.Images;
            m_contentVersionField.text = contentDownloadInfo.Version.ToString();
        }

        private void readFileFromDisk()
        {
            ModDataManager manager = ModDataManager.Instance;
            ContentListInfo contentListInfo;
            if (manager.FileExists(ContentManager.CONTENT_LIST_REPOSITORY_FILE, true))
            {
                string rawData = ModDataManager.Instance.ReadFile(ContentManager.CONTENT_LIST_REPOSITORY_FILE, true);
                contentListInfo = ModJsonUtils.Deserialize<ContentListInfo>(rawData);
                contentListInfo.FixValues();
            }
            else
            {
                contentListInfo = new ContentListInfo();
                contentListInfo.FixValues();
            }

            editingContentList = contentListInfo;
            m_getDataFromServersButton.interactable = true;

            populateDropdown();
        }

        public void OnEditedInfosDropdown(int index)
        {
            ContentListInfo cl = editingContentList;
            if (cl == null || cl.ContentToDownload.IsNullOrEmpty())
                return;

            editingContentDownloadInfo = cl.ContentToDownload[index];
            populateFields();
        }

        public void OnSaveButtonClicked()
        {
            if (!Version.TryParse(m_contentMinModVersionField.text, out Version version))
            {
                ModUIUtils.MessagePopupOK("Could not parse version", "check if version is typed correctly", false);
                return;
            }

            ContentDownloadInfo contentDownloadInfo = editingContentDownloadInfo;
            if (contentDownloadInfo != null)
            {
                contentDownloadInfo.DisplayName = m_contentNameField.text;
                contentDownloadInfo.Size = long.Parse(m_contentSizeField.text);
                contentDownloadInfo.File = m_contentPathField.text;
                contentDownloadInfo.MinModVersion = version;
                contentDownloadInfo.Version = int.Parse(m_contentVersionField.text);
                contentDownloadInfo.Images = m_contentImagesField.text;
                contentDownloadInfo.Description = m_contentDescriptionField.text;
            }

            ContentListInfo contentListInfo = editingContentList;
            if (contentListInfo != null)
            {
                ModDataManager.Instance.SerializeToFile(ContentManager.CONTENT_LIST_REPOSITORY_FILE, contentListInfo, true);
            }

            populateDropdown();
        }

        public void OnCreateNewButtonClicked()
        {
            ModUIUtils.InputFieldWindow("Create new content folder", "Name it", null, 30, 125f, delegate (string value)
            {
                ContentListInfo contentListInfo = editingContentList;
                if (contentListInfo != null)
                {
                    ContentDownloadInfo contentDownloadInfo = new ContentDownloadInfo()
                    {
                        DisplayName = value,
                        Size = 1,
                        Files = new List<string>()
                    };
                    contentListInfo.ContentToDownload.Add(contentDownloadInfo);

                    editingContentDownloadInfo = contentDownloadInfo;
                    populateDropdown();
                    populateFields();
                }
            });
        }

        public void OnGetDataFromServersButtonClicked()
        {
            m_getDataFromServersButton.interactable = false;

            ContentManager.Instance.DownloadContentList(out _, delegate (ContentListInfo contentListInfo)
            {
                editingContentList = contentListInfo;
                m_getDataFromServersButton.interactable = true;

                populateDropdown();
            }, delegate (string error)
            {
                readFileFromDisk();
            });
        }
    }
}
