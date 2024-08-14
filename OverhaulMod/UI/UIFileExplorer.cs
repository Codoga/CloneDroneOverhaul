using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIFileExplorer : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [ShowTooltipOnHighLight("Go up")]
        [UIElementAction(nameof(OnGoUpButtonClicked))]
        [UIElement("GoToParentFolderButton")]
        private readonly Button m_goUpButton;

        [UIElementAction(nameof(OnDoneButtonClicked))]
        [UIElement("DoneButton")]
        private readonly Button m_doneButton;

        [UIElementAction(nameof(OnCancelButtonClicked))]
        [UIElement("CancelButton")]
        private readonly Button m_cancelButton;

        [UIElementAction(nameof(OnDownloadsFolderButtonClicked))]
        [UIElement("DownloadsFolderButton")]
        private readonly Button m_downloadsFolderButton;

        [UIElement("ItemDisplayPrefab", false)]
        private readonly ModdedObject m_itemDisplayPrefab;

        [UIElement("Content")]
        private readonly Transform m_itemDisplayContainer;

        [UIElement("ErrorWindow", false)]
        private readonly GameObject m_errorWindow;

        [UIElement("ErrorDescription")]
        private readonly Text m_errorText;

        [UIElement("SearchPatternText")]
        private readonly Text m_searchPatternText;

        [UIElementAction(nameof(OnEditedDriveDropdown))]
        [UIElement("DriveDropdown")]
        private readonly Dropdown m_driveDropdown;

        [UIElementCallback(true)]
        [UIElementAction(nameof(OnEditedPathField))]
        [UIElement("PathField")]
        private readonly InputField m_pathField;

        [UIElementAction(nameof(OnRevealPathButtonClicked))]
        [UIElement("RevealPathButton")]
        private readonly Button m_revealPathButton;

        private bool m_populateNextFrame;

        private string m_selectedFile;

        private GameObject m_prevSelectedIndicator;

        public override bool enableCursor => true;

        public Action<string> callback
        {
            get;
            set;
        }
        public DirectoryInfo currentFolderInfo { get; private set; }

        public string currentFolder
        {
            get
            {
                return currentFolderInfo == null ? null : currentFolderInfo.FullName;
            }
            set
            {
                if (value.IsNullOrEmpty())
                {
                    currentFolderInfo = new DirectoryInfo("C:\\");
                }
                else if (value.Length == 1)
                {
                    currentFolderInfo = new DirectoryInfo($"{value.ToUpper()}:\\");
                }
                else
                {
                    currentFolderInfo = new DirectoryInfo(value);
                }

                m_populateNextFrame = true;
                m_pathField.text = currentFolderInfo.FullName;

                string rootName = currentFolderInfo.Root.Name;
                for (int i = 0; i < m_driveDropdown.options.Count; i++)
                {
                    if (m_driveDropdown.options[i].text == rootName)
                    {
                        m_driveDropdown.value = i;
                        break;
                    }
                }
            }
        }

        private string m_searchPattern;
        public string searchPattern
        {
            get
            {
                return m_searchPattern;
            }
            set
            {
                m_searchPattern = value;
                m_populateNextFrame = true;

                if (value.IsNullOrEmpty())
                    m_searchPatternText.text = "All";
                else
                    m_searchPatternText.text = value;
            }
        }

        protected override void OnInitialized()
        {
            m_doneButton.interactable = false;

            m_driveDropdown.options.Clear();
            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                Sprite sprite = ModResources.Sprite(AssetBundleConstants.UI, d.Name == "C:\\" ? "SysDrive-Mini-16x16" : "Drive-Mini-16x16");
                m_driveDropdown.options.Add(new Dropdown.OptionData(d.Name, sprite));
            }
            m_driveDropdown.RefreshShownValue();
        }

        public override void Hide()
        {
            base.Hide();

            m_doneButton.interactable = false;
            m_selectedFile = null;
            callback = null;
        }

        public override void Show()
        {
            base.Show();

            m_pathField.interactable = false;
            m_revealPathButton.gameObject.SetActive(true);
        }

        public override void Update()
        {
            base.Update();
            if (m_populateNextFrame)
            {
                m_populateNextFrame = false;
                Populate();
            }
        }

        public void Populate()
        {
            m_errorWindow.SetActive(false);

            if (m_itemDisplayContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_itemDisplayContainer);

            string currentDirectory = currentFolder;
            if (currentDirectory.IsNullOrEmpty())
                return;

            string sp = searchPattern;
            if (sp.IsNullOrEmpty())
                sp = "*";

            DirectoryInfo directoryInfo = currentFolderInfo;
            m_goUpButton.interactable = directoryInfo.Parent != null;

            List<FileSystemInfo> list = new List<FileSystemInfo>();
            try
            {
                list.AddRange(directoryInfo.GetDirectories());
                list.AddRange(directoryInfo.GetFiles(sp));
            }
            catch (Exception exc)
            {
                ShowError(exc);
                return;
            }

            if (list.IsNullOrEmpty())
                return;

            foreach (FileSystemInfo info in list)
            {
                spawnItem(info);
            }
        }

        private void spawnItem(FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo.Attributes.HasFlag(FileAttributes.Hidden))
                return;

            bool isFolder = fileSystemInfo is DirectoryInfo;
            bool isSelected = fileSystemInfo.FullName == m_selectedFile;

            ModdedObject moddedObject = Instantiate(m_itemDisplayPrefab, m_itemDisplayContainer);
            moddedObject.gameObject.SetActive(true);
            moddedObject.GetObject<Text>(0).text = fileSystemInfo.Name;
            moddedObject.GetObject<GameObject>(1).SetActive(!isFolder);
            moddedObject.GetObject<GameObject>(2).SetActive(isFolder);
            moddedObject.GetObject<GameObject>(3).SetActive(isSelected);
            if (isSelected)
                m_prevSelectedIndicator = moddedObject.GetObject<GameObject>(3);

            UIElementFileExplorerItemDisplay itemDisplay = moddedObject.gameObject.AddComponent<UIElementFileExplorerItemDisplay>();
            itemDisplay.InitializeElement();
            itemDisplay.isFolder = isFolder;
            itemDisplay.fullName = fileSystemInfo.FullName;
            itemDisplay.displayName = fileSystemInfo.Name;
            itemDisplay.clickAction = onItemClicked;
            itemDisplay.doubleClickAction = onItemDoubleClicked;
        }

        public void ShowError(Exception exception)
        {
            m_errorWindow.SetActive(true);
            if (exception == null)
            {
                m_errorText.text = "Unknown error.";
                return;
            }

            if (exception is SecurityException || exception is UnauthorizedAccessException)
            {
                m_errorText.text = "Access denied.";
            }
            else if (exception is DirectoryNotFoundException)
            {
                m_errorText.text = "Directory not found.";
            }
            else
            {
                m_errorText.text = $"{exception.GetType().Name}.";
            }
        }

        private void onItemClicked(UIElementFileExplorerItemDisplay itemDisplay)
        {
            if (itemDisplay.isFolder)
                return;

            if (m_prevSelectedIndicator)
                m_prevSelectedIndicator.SetActive(false);

            m_prevSelectedIndicator = itemDisplay.moddedObjectReference.GetObject<GameObject>(3);
            m_prevSelectedIndicator.SetActive(true);
            m_selectedFile = itemDisplay.fullName;
            m_doneButton.interactable = true;
        }

        private void onItemDoubleClicked(UIElementFileExplorerItemDisplay itemDisplay)
        {
            if (itemDisplay.isFolder)
            {
                currentFolder = itemDisplay.fullName;
            }
            else
            {
                OnDoneButtonClicked();
            }
        }

        public void OnEditedDriveDropdown(int value)
        {
            if (m_populateNextFrame)
                return;

            currentFolder = m_driveDropdown.options[value].text;
        }

        public void OnEditedPathField(string path)
        {
            if (m_populateNextFrame)
                return;

            currentFolder = path.Replace("'", string.Empty).Replace("\"", string.Empty);
        }

        public void OnDownloadsFolderButtonClicked()
        {
            currentFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        }

        public void OnGoUpButtonClicked()
        {
            if (currentFolderInfo == null || currentFolderInfo.Parent == null)
                return;

            currentFolder = currentFolderInfo.Parent.FullName;
        }

        public void OnDoneButtonClicked()
        {
            callback?.Invoke(m_selectedFile);
            callback = null;
            Hide();
        }

        public void OnRevealPathButtonClicked()
        {
            m_pathField.interactable = true;
            m_revealPathButton.gameObject.SetActive(false);
        }

        public void OnCancelButtonClicked()
        {
            callback?.Invoke(null);
            callback = null;
            Hide();
        }
    }
}
