﻿using OverhaulMod.Engine;
using OverhaulMod.Utils;
using System.Collections.Generic;
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
            InstantiateToggle("A menu with chapter section selection feature", ModSettingConstants.SHOW_CHAPTER_SELECTION_MENU_REWORK);
            InstantiateToggle("A menu with leaderboard and your current progress", ModSettingConstants.SHOW_ENDLESS_MODE_MENU);
            InstantiateToggle("TBA", ModSettingConstants.SHOW_CHALLENGES_MENU_REWORK);
        }

        public void InstantiateToggle(string description, string settingId)
        {
            ModSetting setting = ModSettingsManager.Instance.GetSetting(settingId);
            if (setting == null || setting.valueType != ModSetting.ValueType.Bool)
                return;

            ModdedObject moddedObject = Instantiate(m_uiDisplayPrefab, m_uiDisplayContainer);
            moddedObject.gameObject.SetActive(true);
            moddedObject.GetObject<Text>(0).text = LocalizationManager.Instance.GetTranslatedString(setting.name);
            moddedObject.GetObject<Text>(1).text = description;
            moddedObject.GetObject<Button>(2).onClick.AddListener(delegate
            {
                GUIUtility.systemCopyBuffer = setting.name;
                ModUIUtils.MessagePopupOK("Copied localization ID", "", false);
            });

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

            ModUIUtils.MessagePopup(true, "Enable all modded UIs?", "", 100f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                foreach (Toggle toggle in m_instantiatedToggles)
                    toggle.isOn = true;
            });
        }

        public void OnDisableAllButtonClicked()
        {
            if (m_instantiatedToggles.IsNullOrEmpty())
                return;

            ModUIUtils.MessagePopup(true, "Disable all modded UIs?", "", 100f, MessageMenu.ButtonLayout.EnableDisableButtons, "ok", "Yes", "No", null, delegate
            {
                foreach (Toggle toggle in m_instantiatedToggles)
                    toggle.isOn = false;
            });
        }
    }
}
