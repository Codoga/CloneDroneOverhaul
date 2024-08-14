using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UILocalizationEditor : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnSaveButtonClicked))]
        [UIElement("SaveButton")]
        private readonly Button m_saveButton;

        [UIElementAction(nameof(OnLanguagesDropdownChanged))]
        [UIElement("LanguagesDropdown")]
        private readonly Dropdown m_languagesDropdown;

        [UIElement("TranslationKeyDisplay", false)]
        private readonly ModdedObject m_translationKeyPrefab;
        [UIElement("AddTranslationKeyButton", false)]
        private readonly Button m_addTranslationKeyButtonPrefab;
        [UIElement("TextPrefab", false)]
        private readonly Text m_textPrefab;
        [UIElement("TranslationKeysListContainer")]
        private readonly Transform m_translationKeysContainer;
        [UIElement("LoadingIndicator", false)]
        private readonly GameObject m_loadingIndicatorObject;

        [UIElementAction(nameof(OnChangedTranslationKeyField))]
        [UIElement("EditingKeyText")]
        private readonly InputField m_translationKeyLabel;
        [UIElement("EditingValueInputField")]
        private readonly InputField m_translationValueInputField;

        [UIElementAction(nameof(OnToLowerButtonClicked))]
        [UIElement("ToLowerButton")]
        private readonly Button m_toLowerButton;
        [UIElementAction(nameof(OnDeleteTranslationButtonClicked))]
        [UIElement("DeleteButton")]
        private readonly Button m_deleteButton;

        [UIElement("ArialRText")]
        private readonly Text m_arialRFontPreviewText;
        [UIElement("ArialBText")]
        private readonly Text m_arialBFontPreviewText;
        [UIElement("OSRText")]
        private readonly Text m_openSansRFontPreviewText;
        [UIElement("OSBText")]
        private readonly Text m_openSansBFontPreviewText;
        [UIElement("EditUndoText")]
        private readonly Text m_editUndoFontPreviewText;
        [UIElement("PixelsSimpleText")]
        private readonly Text m_pixelsSimpleFontPreviewText;
        [UIElement("TriggeringFanfaresText")]
        private readonly Text m_triggeringFanfaresFontPreviewText;

        [UIElementAction(nameof(OnSearchBoxChanged))]
        [UIElement("SearchBox")]
        private readonly InputField m_searchBox;

        private Dictionary<string, GameObject> m_cachedInstantiatedKeyDisplays;

        public override bool hideTitleScreen => true;

        public string editingLangId
        {
            get
            {
                return m_languagesDropdown.options[m_languagesDropdown.value].text;
            }
        }

        public string editingTranslationKey
        {
            get;
            set;
        }

        public int siblingIndexOfTranslationKey
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            m_cachedInstantiatedKeyDisplays = new Dictionary<string, GameObject>();

            m_languagesDropdown.options = ModLocalizationManager.Instance.GetLanguageOptions(true);
            m_languagesDropdown.value = 0;
            m_translationValueInputField.onEndEdit.AddListener(delegate (string str)
            {
                if (editingLangId.IsNullOrEmpty() || editingTranslationKey.IsNullOrEmpty())
                    return;

                ModLocalizationManager.Instance.SetTranslation(editingLangId, editingTranslationKey, str);
            });
            m_translationValueInputField.onValueChanged.AddListener(delegate
            {
                RefreshPreview();
            });

            PopulateTranslations();
        }

        public void EditTranslation(string key)
        {
            editingTranslationKey = key;

            m_translationKeyLabel.text = key;
            m_translationValueInputField.text = ModLocalizationManager.Instance.GetTranslation(editingLangId, editingTranslationKey);
            RefreshPreview();
        }

        public void ChangeTranslation(string oldName, string newName)
        {
            if (m_cachedInstantiatedKeyDisplays.TryGetValue(oldName, out GameObject display))
            {
                editingTranslationKey = newName;

                ModLocalizationManager.Instance.ChangeTranslation(oldName, newName);
                try
                {
                    ModdedObject translationKey = display.GetComponent<ModdedObject>();
                    if (translationKey)
                    {
                        translationKey.GetObject<Text>(0).text = newName;
                    }

                    Button button = translationKey.GetComponent<Button>();
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(delegate
                    {
                        siblingIndexOfTranslationKey = translationKey.transform.GetSiblingIndex();
                        EditTranslation(newName);
                    });
                }
                catch { }

                _ = m_cachedInstantiatedKeyDisplays.Remove(oldName);
                m_cachedInstantiatedKeyDisplays.Add(newName, display);
            }
        }

        public void DeleteTranslation(string key)
        {
            editingTranslationKey = string.Empty;

            ModLocalizationManager.Instance.DeleteTranslation(key);
            _ = m_cachedInstantiatedKeyDisplays.Remove(key);

            m_translationKeyLabel.text = string.Empty;
            m_translationValueInputField.text = string.Empty;

            try
            {
                Transform translationKey = m_translationKeysContainer.GetChild(siblingIndexOfTranslationKey);
                if (translationKey)
                {
                    Destroy(translationKey.gameObject);
                }
            }
            catch { }
        }

        public void RefreshPreview()
        {
            string text = m_translationValueInputField.text;

            m_arialRFontPreviewText.text = text;
            m_arialBFontPreviewText.text = text;
            m_openSansRFontPreviewText.text = text;
            m_openSansBFontPreviewText.text = text;
            m_editUndoFontPreviewText.text = text;
            m_pixelsSimpleFontPreviewText.text = text;
            m_triggeringFanfaresFontPreviewText.text = text;
        }

        public ModdedObject InstantiateTranslationKeyDisplay(string key)
        {
            ModdedObject translationKey = Instantiate(m_translationKeyPrefab, m_translationKeysContainer);
            translationKey.gameObject.SetActive(true);
            translationKey.GetObject<Text>(0).text = key;
            Button button = translationKey.GetComponent<Button>();
            button.onClick.AddListener(delegate
            {
                siblingIndexOfTranslationKey = translationKey.transform.GetSiblingIndex();
                EditTranslation(key);
            });

            m_cachedInstantiatedKeyDisplays.Add(key, translationKey.gameObject);
            return translationKey;
        }

        public void PopulateTranslations()
        {
            m_cachedInstantiatedKeyDisplays.Clear();
            if (m_translationKeysContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_translationKeysContainer);

            ModLocalizationManager manager = ModLocalizationManager.Instance;
            if (!manager.CanLanguageBeTranslated(editingLangId))
            {
                Text text = Instantiate(m_textPrefab, m_translationKeysContainer);
                text.gameObject.SetActive(true);
                text.text = "This language cannot be translated now";
                return;
            }

            Dictionary<string, string> translationDictionary = manager.GetTranslationDictionary(editingLangId);
            if (translationDictionary != null && translationDictionary.Count != 0)
            {
                foreach (string key in translationDictionary.Keys)
                    _ = InstantiateTranslationKeyDisplay(key);
            }

            Button addTranslationButton = Instantiate(m_addTranslationKeyButtonPrefab, m_translationKeysContainer);
            addTranslationButton.gameObject.SetActive(true);
            addTranslationButton.onClick.AddListener(OnAddTranslationButtonClicked);
        }

        public void OnLanguagesDropdownChanged(int index)
        {
            PopulateTranslations();
            if (!editingTranslationKey.IsNullOrEmpty())
            {
                EditTranslation(editingTranslationKey);
            }
        }

        public void OnAddTranslationButtonClicked()
        {
            ModUIUtils.InputFieldWindow("Add translation", "Enter non-existing translation name", null, 0, 125f, delegate (string value)
            {
                ModLocalizationManager.Instance.AddTranslation(value);
                ModdedObject moddedObject = InstantiateTranslationKeyDisplay(value);
                moddedObject.transform.SetSiblingIndex(m_translationKeysContainer.childCount - 2);
                EditTranslation(value);
            });
        }

        public void OnSaveButtonClicked()
        {
            ModLocalizationManager.Instance.SaveInfo();
            LocalizationManager.Instance.SetCurrentLanguage(SettingsManager.Instance.GetCurrentLanguageID());
        }

        public void OnChangedTranslationKeyField(string text)
        {
            ChangeTranslation(editingTranslationKey, text);
        }

        public void OnToLowerButtonClicked()
        {
            OnChangedTranslationKeyField(editingTranslationKey.ToLower());
        }

        public void OnDeleteTranslationButtonClicked()
        {
            ModUIUtils.MessagePopup(true, "Delete translation?", $"\"{editingTranslationKey}\" will be deleted.", 125f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes, delete", "No", null, delegate
            {
                DeleteTranslation(editingTranslationKey);
            });
        }

        public void OnSearchBoxChanged(string text)
        {
            string lowerText = text.ToLower();
            bool forceSetEnabled = text.IsNullOrEmpty();

            foreach (KeyValuePair<string, GameObject> keyValue in m_cachedInstantiatedKeyDisplays)
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
    }
}
