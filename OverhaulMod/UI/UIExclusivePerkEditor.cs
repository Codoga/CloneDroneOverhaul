using OverhaulMod.Content;
using OverhaulMod.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIExclusivePerkEditor : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElementAction(nameof(OnDataFolderButtonClicked))]
        [UIElement("DataFolderButton")]
        private readonly Button m_dataFolderButton;

        [UIElementAction(nameof(OnSaveButtonClicked))]
        [UIElement("SaveButton")]
        private readonly Button m_saveButton;

        [UIElementAction(nameof(OnNewButtonClicked))]
        [UIElement("NewButton")]
        private readonly Button m_newButton;

        [UIElement("NameInputField")]
        private readonly InputField m_contentNameInputField;

        [UIElement("SteamIDInputField")]
        private readonly InputField m_steamIdInputField;

        [UIElement("PlayFabIDInputField")]
        private readonly InputField m_playFabIdInputField;

        [UIElement("TypeDropdown")]
        private readonly Dropdown m_perkTypeDropdown;

        [UIElement("Content")]
        private readonly Transform m_container;

        protected override void OnInitialized()
        {
            List<Dropdown.OptionData> contentTypeOptions = new List<Dropdown.OptionData>();
            foreach (string str in typeof(ExclusiveContentType).GetEnumNames())
            {
                contentTypeOptions.Add(new Dropdown.OptionData() { text = str });
            }
            m_perkTypeDropdown.options = contentTypeOptions;
        }

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }

        public void OnDataFolderButtonClicked()
        {
            _ = ModFileUtils.OpenFileExplorer(ModCache.dataRepository.GetRootDataPath(false));
        }

        public void OnSaveButtonClicked()
        {
            ModUIUtils.MessagePopupOK("Content file saved!", string.Empty);
        }

        public void OnNewButtonClicked()
        {

        }
    }
}
