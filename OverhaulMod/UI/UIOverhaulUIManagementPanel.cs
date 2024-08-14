using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace OverhaulMod.UI
{
    public class UIOverhaulUIManagementPanel : OverhaulUIBehaviour
    {
        [UIElementAction(nameof(Hide))]
        [UIElement("CloseButton")]
        private readonly Button m_exitButton;

        [UIElement("UIDisplay", false)]
        private readonly ModdedObject m_uiDisplayPrefab;

        [UIElement("Content")]
        private readonly Transform m_uiDisplayContainer;

        [UIElementAction(nameof(OnEnableAllButtonClicked))]
        [UIElement("EnableAllButton")]
        private readonly Button m_enableAllButton;

        [UIElementAction(nameof(OnDisableAllButtonClicked))]
        [UIElement("DisableAllButton")]
        private readonly Button m_disableAllButton;

        private List<Toggle> m_instantiatedToggles;

        protected override void OnInitialized()
        {
            m_instantiatedToggles = new List<Toggle>();
        }

        public override void Show()
        {
            base.Show();

            if (m_uiDisplayContainer.childCount != 0)
                TransformUtils.DestroyAllChildren(m_uiDisplayContainer);

            m_instantiatedToggles.Clear();
            instantiateToggles();
        }

        private void instantiateToggles()
        {
            foreach (ModSetting setting in ModSettingsManager.Instance.GetSettings(ModSetting.Tag.UISetting))
            {
                InstantiateToggle(setting.name);
            }
        }

        public void InstantiateToggle(string settingId)
        {
            ModSetting setting = ModSettingsManager.Instance.GetSetting(settingId);
            if (setting == null || setting.valueType != ModSetting.ValueType.Bool)
                return;

            ModdedObject moddedObject = Instantiate(m_uiDisplayPrefab, m_uiDisplayContainer);
            moddedObject.gameObject.SetActive(true);
            moddedObject.GetObject<Text>(0).text = LocalizationManager.Instance.GetTranslatedString(setting.name);
            moddedObject.GetObject<Button>(2).onClick.AddListener(delegate
            {
                GUIUtility.systemCopyBuffer = setting.name;
                ModUIUtils.MessagePopupOK("Copied localization ID", "", false);
            });

            UIElementOverhaulUIInfo info = moddedObject.gameObject.AddComponent<UIElementOverhaulUIInfo>();
            info.PreviewFile = Path.Combine(ModCore.texturesFolder, "uiPreviews", $"{setting.name.Replace("ModUI_UI", string.Empty).Replace("Rework", string.Empty)}.png");
            info.InitializeElement();

            bool isOn = (bool)setting.GetFieldValue();

            Toggle toggle = moddedObject.GetComponent<Toggle>();
            toggle.isOn = isOn;
            toggle.onValueChanged.AddListener(delegate (bool value)
            {
                setting.SetBoolValue(value);
            });

            m_instantiatedToggles.Add(toggle);
        }

        public void OnEnableAllButtonClicked()
        {
            if (m_instantiatedToggles.IsNullOrEmpty())
                return;

            ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("settings_configure_overhaul_mod_uis_header_enable_all_modded_uis"), "", 100f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                foreach (Toggle toggle in m_instantiatedToggles)
                    toggle.isOn = true;
            });
        }

        public void OnDisableAllButtonClicked()
        {
            if (m_instantiatedToggles.IsNullOrEmpty())
                return;

            ModUIUtils.MessagePopup(true, LocalizationManager.Instance.GetTranslatedString("settings_configure_overhaul_mod_uis_header_disable_all_modded_uis"), "", 100f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                foreach (Toggle toggle in m_instantiatedToggles)
                    toggle.isOn = false;
            });
        }
    }
}
