using OverhaulMod.Content;
using OverhaulMod.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIAddonsEditor : OverhaulUIBehaviour
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

        [UIElementAction(nameof(OnEditedAddonsDropdown))]
        [UIElement("AddonsDropdown")]
        private readonly Dropdown m_addonsDropdown;

        [UIElement("AddonNameInputField")]
        private readonly InputField m_addonNameField;

        [UIElement("AddonVersionField")]
        private readonly InputField m_addonVersionField;

        private List<ContentInfo> m_content;

        private bool m_disableDropdownCallbacks;

        public override bool hideTitleScreen => true;

        public string editingFile
        {
            get;
            private set;
        }

        public ContentInfo editingContentInfo
        {
            get;
            private set;
        }

        public override void Show()
        {
            base.Show();
            populateAddonsDropdown();
        }

        private void populateAddonsDropdown()
        {
            List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
            list.Clear();

            List<ContentInfo> content = ContentManager.Instance.GetContent();
            m_content = content;

            if (content.IsNullOrEmpty())
                return;

            foreach (ContentInfo c in content)
            {
                list.Add(new Dropdown.OptionData(c.DisplayName));
            }
            m_addonsDropdown.options = list;
        }

        public void OnEditedAddonsDropdown(int index)
        {
            if (m_disableDropdownCallbacks)
                return;

            List<ContentInfo> list = m_content;
            if (list.IsNullOrEmpty())
            {
                editingFile = string.Empty;
                return;
            }

            ContentInfo content = list[index];
            editingContentInfo = content;
            editingFile = Path.Combine(content.FolderPath, ContentManager.CONTENT_INFO_FILE);

            m_addonNameField.text = content.DisplayName;
            m_addonVersionField.text = content.Version.ToString();
        }

        public void OnSaveButtonClicked()
        {
            string file = editingFile;
            if (file.IsNullOrEmpty())
                return;

            ContentInfo contentInfo = editingContentInfo;
            if (contentInfo == null)
                return;

            contentInfo.DisplayName = m_addonNameField.text;
            contentInfo.Version = int.Parse(m_addonVersionField.text);

            ModJsonUtils.WriteStream(file, contentInfo);
        }

        public void OnCreateNewButtonClicked()
        {
            ModUIUtils.InputFieldWindow("Create new content folder", "Name it", null, 30, 125f, delegate (string value)
            {
                string folderName = value.Replace(" ", string.Empty);
                string folderPath = Path.Combine(ModCore.addonsFolder, folderName);
                string contentInfoFilePath = folderPath + ContentManager.CONTENT_INFO_FILE;

                _ = Directory.CreateDirectory(folderPath);
                using (File.CreateText(contentInfoFilePath))
                {
                    m_disableDropdownCallbacks = true;

                    editingContentInfo = new ContentInfo();
                    editingFile = contentInfoFilePath;
                    m_addonNameField.text = string.Empty;
                    m_addonVersionField.text = "0";
                    populateAddonsDropdown();

                    m_disableDropdownCallbacks = false;
                }
            });
        }
    }
}
