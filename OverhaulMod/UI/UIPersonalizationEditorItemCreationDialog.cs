using OverhaulMod.Content.Personalization;
using OverhaulMod.Utils;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPersonalizationEditorItemCreationDialog : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnDoneButtonClicked))]
        [UIElement("DoneButton")]
        private readonly Button m_doneButton;

        [UIElementAction(nameof(OnItemNameChanged))]
        [UIElement("ItemNameField")]
        private readonly InputField m_itemNameField;

        [UIElementAction(nameof(OnItemFolderNameChanged))]
        [UIElement("ItemFolderNameField")]
        private readonly InputField m_itemFolderNameField;

        [UIElementAction(nameof(OnTemplateDropdownChanged))]
        [UIElement("TemplateDropdown")]
        private readonly Dropdown m_templateDropdown;

        [UIElement("StatusText")]
        private readonly Text m_statusText;

        public string TargetDirectory;

        public bool UsePersistentFolder;

        public Action ItemCreatedCallback;

        private float m_timeLeftToRefreshStatus;

        private bool m_hasToRefreshStatus;

        protected override void OnInitialized()
        {
            System.Collections.Generic.List<Dropdown.OptionData> options = m_templateDropdown.options;
            options.Clear();
            options.Add(new Dropdown.OptionData("None"));

            PersonalizationItemInfo[] templates = PersonalizationEditorTemplateManager.Instance.GetTemplates();
            if (templates != null)
                foreach (PersonalizationItemInfo template in templates)
                {
                    if (!template.Corrupted)
                        options.Add(new DropdownPersonalizationItemInfo(template));
                }

            m_templateDropdown.options = options;
            m_templateDropdown.value = 0;
        }

        public override void Show()
        {
            base.Show();

            m_templateDropdown.value = 0;
            m_itemNameField.text = string.Empty;

            m_timeLeftToRefreshStatus = 0f;
            m_hasToRefreshStatus = true;
        }

        public override void Update()
        {
            if (m_hasToRefreshStatus)
            {
                m_timeLeftToRefreshStatus -= Time.unscaledDeltaTime;
                if (m_timeLeftToRefreshStatus <= 0f)
                {
                    m_hasToRefreshStatus = false;
                    m_timeLeftToRefreshStatus = 0f;

                    RefreshStatus();
                }
            }
        }

        public void StartRefreshingStatus()
        {
            SetStatusText("Please wait...", Color.gray);
            m_doneButton.interactable = false;

            m_timeLeftToRefreshStatus = 1f;
            m_hasToRefreshStatus = true;
        }

        public void RefreshStatus()
        {
            if (m_itemNameField.text.IsNullOrEmpty())
            {
                SetStatusText("Item name is empty.", Color.red);
                m_doneButton.interactable = false;
                return;
            }

            if (m_itemNameField.text.IsNullOrWhiteSpace())
            {
                SetStatusText("Folder name is a whitespace.", Color.red);
                m_doneButton.interactable = false;
                return;
            }

            if (m_itemNameField.text.EndsWith(" "))
            {
                SetStatusText("Item name ends with whitespace.", Color.red);
                m_doneButton.interactable = false;
                return;
            }

            if (m_itemFolderNameField.text.IsNullOrEmpty())
            {
                SetStatusText("Folder name is empty.", Color.red);
                m_doneButton.interactable = false;
                return;
            }

            if (m_itemFolderNameField.text.IsNullOrWhiteSpace())
            {
                SetStatusText("Folder name is a whitespace.", Color.red);
                m_doneButton.interactable = false;
                return;
            }

            if (m_itemFolderNameField.text.Contains(" "))
            {
                SetStatusText("Folder name contains whitespaces.", Color.red);
                m_doneButton.interactable = false;
                return;
            }

            foreach (char c in Path.GetInvalidFileNameChars())
                if (m_itemFolderNameField.text.Contains(c))
                {
                    SetStatusText($"Folder name contains invalid character: {c}", Color.red);
                    m_doneButton.interactable = false;
                    return;
                }

            string path = Path.Combine(TargetDirectory, m_itemFolderNameField.text);
            if (Directory.Exists(path))
            {
                SetStatusText("A folder with the same name already exists.", Color.red);
                m_doneButton.interactable = false;
                return;
            }

            SetStatusText("You can create the item.", Color.green);
            m_doneButton.interactable = true;
        }

        public void SetStatusText(string text, Color color)
        {
            m_statusText.text = text;
            m_statusText.color = color;
        }

        public void OnDoneButtonClicked()
        {
            Hide();

            PersonalizationItemInfo template = null;
            if (m_templateDropdown.options[m_templateDropdown.value] is DropdownPersonalizationItemInfo dropdownPersonalizationItemInfo)
                template = dropdownPersonalizationItemInfo.ItemInfo;

            if (PersonalizationEditorManager.Instance.CreateItem(m_itemFolderNameField.text, m_itemNameField.text, UsePersistentFolder, template, out PersonalizationItemInfo personalizationItem))
            {
                UIPersonalizationEditor.instance.ShowEverything();
                PersonalizationEditorManager.Instance.EditItem(personalizationItem, personalizationItem.FolderPath);
                Hide();
                if (ItemCreatedCallback != null)
                {
                    ItemCreatedCallback();
                    ItemCreatedCallback = null;
                }
            }
            else
            {
                ModUIUtils.MessagePopupOK("Item creation error", "A folder with the name has been already created.\nTry giving your folder an alternate name.", true);
            }
        }

        public void OnItemNameChanged(string value)
        {
            StartRefreshingStatus();
            string itemName = value;

            bool isDone = false;
            while (!isDone)
            {
                int index = itemName.IndexOf(' ');
                if (index == -1)
                    isDone = true;
                else
                {
                    itemName = itemName.Remove(index, 1);
                    if (index < itemName.Length)
                    {
                        string upperChar = itemName[index].ToString().ToUpper();
                        itemName = itemName.Remove(index, 1);
                        itemName = itemName.Insert(index, upperChar);
                    }
                }
            }
            m_itemFolderNameField.text = itemName;
        }

        public void OnItemFolderNameChanged(string value)
        {
            StartRefreshingStatus();
        }

        public void OnTemplateDropdownChanged(int value)
        {

        }
    }
}
