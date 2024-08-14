using ICSharpCode.SharpZipLib.Zip;
using OverhaulMod.Content.Personalization;
using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIPersonalizationEditorItemBrowser : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnReloadButtonClicked))]
        [UIElement("ReloadButton")]
        private readonly Button m_reloadButton;

        [UIElementAction(nameof(OnFolderButtonClicked))]
        [UIElement("FolderButton")]
        private readonly Button m_folderButton;

        [UIElementAction(nameof(OnCreateNewButtonClicked))]
        [UIElement("CreateNewButton")]
        private readonly Button m_createNewButton;

        [UIElementAction(nameof(OnImportButtonClicked))]
        [UIElement("ImportButton")]
        private readonly Button m_importButton;

        [UIElementAction(nameof(OnViewAllItemsToggleChanged))]
        [UIElement("ViewAllItemsToggle")]
        private readonly Toggle m_viewAllItemsToggle;

        [UIElement("UsePersistentDirectoryToggle")]
        private readonly Toggle m_usePersistentDirectoryToggle;

        [UIElement("ItemDisplayPrefab", false)]
        private readonly ModdedObject m_itemDisplayPrefab;

        [UIElement("LoadErrorDisplayPrefab", false)]
        private readonly ModdedObject m_itemLoadErrorDisplayPrefab;

        [UIElement("TextPrefab", false)]
        private readonly Text m_textPrefab;

        [UIElement("Content")]
        private readonly Transform m_container;

        [UIElementAction(nameof(OnSearchBoxChanged))]
        [UIElement("SearchBox")]
        private readonly InputField m_searchBox;

        [UIElement("ExportVersionField")]
        private readonly InputField m_exportVersionField;

        [UIElementAction(nameof(OnExportAllButtonClicked))]
        [UIElement("ExportAllButton")]
        private readonly Button m_exportAllButton;

        private Dictionary<string, GameObject> m_cachedInstantiatedDisplays;

        protected override void OnInitialized()
        {
            bool isDev = PersonalizationEditorManager.Instance.canVerifyItems;

            m_cachedInstantiatedDisplays = new Dictionary<string, GameObject>();
            m_viewAllItemsToggle.gameObject.SetActive(PersonalizationEditorManager.Instance.canEditNonOwnItems);
            m_usePersistentDirectoryToggle.gameObject.SetActive(isDev);
            m_usePersistentDirectoryToggle.isOn = true;
            m_importButton.interactable = isDev;
            m_exportAllButton.gameObject.SetActive(isDev);
            m_exportVersionField.gameObject.SetActive(isDev);
            if (isDev)
                m_exportVersionField.text = PersonalizationManager.Instance.localAssetsInfo.AssetsVersion.ToString();
        }

        public override void Show()
        {
            base.Show();
            Populate();

            m_searchBox.ActivateInputField();
        }

        public void Populate()
        {
            bool getAll = m_viewAllItemsToggle.isOn && PersonalizationEditorManager.Instance.canEditNonOwnItems;

            PersonalizationItemList itemList = PersonalizationManager.Instance.itemList;
            if (itemList == null || itemList.LoadError != null)
                return;

            List<PersonalizationItemInfo> list = new List<PersonalizationItemInfo>();
            foreach (PersonalizationItemInfo item in itemList.Items)
                if (item.CanBeEdited() || getAll)
                    list.Add(item);

            populate(list, itemList.ItemLoadErrors);
        }

        private void populate(List<PersonalizationItemInfo> list, Dictionary<string, System.Exception> exceptions)
        {
            m_cachedInstantiatedDisplays.Clear();
            if (m_container.childCount != 0)
                TransformUtils.DestroyAllChildren(m_container);

            if (list != null)
            {
                foreach (PersonalizationItemInfo item in list)
                {
                    ModdedObject moddedObject = Instantiate(m_itemDisplayPrefab, m_container);
                    moddedObject.gameObject.SetActive(true);
                    moddedObject.GetObject<Text>(0).text = item.Name;
                    moddedObject.GetObject<Text>(1).text = PersonalizationItemInfo.GetCategoryString(item.Category);
                    moddedObject.GetObject<Text>(2).text = item.GetSpecialInfoString();
                    moddedObject.GetObject<GameObject>(3).SetActive(item.IsVerified);
                    moddedObject.GetObject<Text>(4).text = item.GetAuthorsString(false);
                    moddedObject.GetObject<GameObject>(5).SetActive(item.IsExclusive());

                    Button button = moddedObject.GetComponent<Button>();
                    button.onClick.AddListener(delegate
                    {
                        UIPersonalizationEditor.instance.ShowEverything();
                        PersonalizationEditorManager.Instance.EditItem(item, item.FolderPath);
                        Hide();
                    });

                    string text = item.Name.ToLower();
                    while (m_cachedInstantiatedDisplays.ContainsKey(text))
                        text += "_1";

                    m_cachedInstantiatedDisplays.Add(text, moddedObject.gameObject);
                }

                Text textComponent = Instantiate(m_textPrefab, m_container);
                textComponent.gameObject.SetActive(true);
                textComponent.text = $"{list.Count} items";
            }

            if (exceptions != null)
                foreach (KeyValuePair<string, System.Exception> keyValue in exceptions)
                {
                    ModdedObject moddedObject = Instantiate(m_itemLoadErrorDisplayPrefab, m_container);
                    moddedObject.gameObject.SetActive(true);
                    moddedObject.GetObject<Text>(0).text = $"Item load error: {keyValue.Key}";
                    moddedObject.GetObject<Text>(1).text = keyValue.Value.ToString();
                }
        }

        public void OnViewAllItemsToggleChanged(bool value)
        {
            Populate();
        }

        public void OnReloadButtonClicked()
        {
            PersonalizationManager.Instance.itemList.Load();
            Populate();
        }

        public void OnFolderButtonClicked()
        {
            _ = ModIOUtils.OpenFileExplorer(m_usePersistentDirectoryToggle.isOn ? ModCore.customizationPersistentFolder : ModCore.customizationFolder);
        }

        public void OnCreateNewButtonClicked()
        {
            UIPersonalizationEditorItemCreationDialog panel = ModUIConstants.ShowPersonalizationEditorItemCreationDialog(base.transform);
            panel.TargetDirectory = m_usePersistentDirectoryToggle.isOn ? ModCore.customizationPersistentFolder : ModCore.customizationFolder;
            panel.UsePersistentFolder = m_usePersistentDirectoryToggle.isOn;
            panel.ItemCreatedCallback = Hide;

            /*
            ModUIUtils.InputFieldWindow("Create new item", "Enter folder name", 150f, delegate (string str)
            {
                string rootDirectory = m_usePersistentDirectoryToggle.isOn ? ModCore.customizationPersistentFolder : ModCore.customizationFolder;
                string directoryName = str.Replace(" ", string.Empty);
                string directoryPath = $"{Path.Combine(rootDirectory, directoryName)}/";
                if (Directory.Exists(directoryPath))
                {
                    ModUIUtils.MessagePopupOK("Directory with the same name already exists", "Try naming the folder differently", true);
                    return;
                }

                if (PersonalizationEditorManager.Instance.CreateItem(str, m_usePersistentDirectoryToggle.isOn, out PersonalizationItemInfo personalizationItem))
                {
                    UIPersonalizationEditor.instance.ShowEverything();
                    PersonalizationEditorManager.Instance.EditItem(personalizationItem, personalizationItem.FolderPath);
                    Hide();
                }
                else
                {
                    ModUIUtils.MessagePopupOK("Item creation error", "A folder with the name has been already created.\nTry giving your folder an alternate name.", true);
                }
            });*/
        }

        public void OnSearchBoxChanged(string text)
        {
            string lowerText = text.ToLower();
            bool forceSetEnabled = text.IsNullOrEmpty();

            foreach (KeyValuePair<string, GameObject> keyValue in m_cachedInstantiatedDisplays)
            {
                if (forceSetEnabled)
                {
                    keyValue.Value.SetActive(true);
                }
                else
                {
                    keyValue.Value.SetActive(keyValue.Key.ToLower().Contains(lowerText));
                }
            }
        }

        public void OnImportButtonClicked()
        {
            ModUIUtils.FileExplorer(base.transform, true, delegate (string path)
            {
                var dialog = ModUIConstants.ShowPersonalizationEditorItemImportDialog(base.transform);
                dialog.ItemBrowser = this;
                dialog.FilePath = path;
            }, null, "*.zip");
        }

        public void OnExportAllButtonClicked()
        {
            if (!Version.TryParse(m_exportVersionField.text, out Version version))
            {
                ModUIUtils.MessagePopupOK("Error", "Could not parse text from version input field");
                return;
            }

            FastZip fastZip = new FastZip();
            fastZip.CreateZip(Path.Combine(ModCore.savesFolder, "customization.zip"), ModCore.customizationFolder, true, string.Empty);

            PersonalizationAssetsInfo personalizationAssetsInfo = new PersonalizationAssetsInfo
            {
                AssetsVersion = version
            };
            _ = PersonalizationManager.Instance.SetLocalAssetsVersion(version);
            ModJsonUtils.WriteStream(Path.Combine(ModCore.savesFolder, PersonalizationManager.ASSETS_VERSION_FILE), personalizationAssetsInfo);

            _ = ModIOUtils.OpenFileExplorer(ModCore.savesFolder);
        }
    }
}
